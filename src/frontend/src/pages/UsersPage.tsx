import { keepPreviousData, useQuery, useQueryClient } from '@tanstack/react-query';
import { useState } from 'react';
import { ActiveBadge } from '../components/Badges';
import { Card } from '../components/Card';
import { FilterPanel } from '../components/FilterPanel';
import { Dialog } from '../components/Dialog';
import { Pagination } from '../components/Pagination';
import { EmptyState, ErrorState, LoadingState } from '../components/States';
import { CreateUserForm, UpdateUserForm } from '../components/UserForms';
import { useAuth } from '../hooks/useAuth';
import { useClampedPage } from '../hooks/useClampedPage';
import { SEARCH_PARAMETER, readOption, useListSearchParameters, useSearchText } from '../hooks/useListSearchParameters';
import { useToast } from '../hooks/useToast';
import type { CreateUserFormValues, UpdateUserFormValues } from '../schemas/userSchemas';
import { SEARCH_MAXIMUM_LENGTH } from '../schemas/validationLimits';
import { QUERY_KEYS } from '../services/queryKeys';
import { userService } from '../services/userService';
import { USER_ROLES } from '../types/api';
import type { UserResponse } from '../types/api';
import { formatDate } from '../utils/formatting';
import { ROLE_LABELS, roleLabel } from '../utils/labels';

const ACTIVE_FILTER_VALUES = { all: '', active: 'activos', inactive: 'inactivos' } as const;
const ROLE_PARAMETER = 'rol';
const ACTIVE_PARAMETER = 'estado';
const ACTIVE_FILTER_OPTIONS = [ACTIVE_FILTER_VALUES.active, ACTIVE_FILTER_VALUES.inactive] as const;

export function UsersPage() {
  const { user: currentUser } = useAuth();
  const { showToast } = useToast();
  const queryClient = useQueryClient();
  const listParameters = useListSearchParameters();
  const { searchParameters, page, pageSize, activeFilterCount, updateParameters } = listParameters;
  const search = searchParameters.get(SEARCH_PARAMETER) ?? '';
  const role = readOption(USER_ROLES, searchParameters.get(ROLE_PARAMETER));
  const activeFilter = readOption(ACTIVE_FILTER_OPTIONS, searchParameters.get(ACTIVE_PARAMETER));
  const [searchText, setSearchText] = useSearchText(search, updateParameters);
  const [isCreating, setIsCreating] = useState(false);
  const [userToEdit, setUserToEdit] = useState<UserResponse | null>(null);

  const query = {
    search,
    role,
    isActive: activeFilter === undefined ? undefined : activeFilter === ACTIVE_FILTER_VALUES.active,
    page,
    pageSize,
  };
  const users = useQuery({
    queryKey: QUERY_KEYS.userList(query),
    queryFn: () => userService.list(query),
    placeholderData: keepPreviousData,
  });
  useClampedPage(users.data, listParameters.setPage);

  const refreshUsers = () =>
    Promise.all([
      queryClient.invalidateQueries({ queryKey: QUERY_KEYS.users }),
      queryClient.invalidateQueries({ queryKey: QUERY_KEYS.assignees }),
    ]);

  const createUser = async (values: CreateUserFormValues) => {
    await userService.create(values);
    await refreshUsers();
    setIsCreating(false);
    showToast(`Usuario ${values.email} creado.`);
  };

  const updateUser = async (values: UpdateUserFormValues) => {
    if (!userToEdit) {
      return;
    }
    await userService.update(userToEdit.id, values);
    await refreshUsers();
    setUserToEdit(null);
    showToast('Usuario actualizado.');
  };

  return (
    <Card
      title="Usuarios"
      subtitle="Administración de cuentas y roles del sistema."
      actions={
        <button type="button" className="button button--accent" onClick={() => setIsCreating(true)}>
          Nuevo usuario
        </button>
      }
    >
      <FilterPanel label="Filtros de usuarios" activeFilterCount={activeFilterCount}>
        <label className="field filters__search">
          <span className="field__label">Buscar</span>
          <input
            className="input"
            type="search"
            placeholder="Nombre o correo"
            maxLength={SEARCH_MAXIMUM_LENGTH}
            value={searchText}
            onChange={(event) => setSearchText(event.target.value)}
          />
        </label>
        <label className="field">
          <span className="field__label">Rol</span>
          <select
            className="select"
            value={role ?? ''}
            onChange={(event) => updateParameters({ [ROLE_PARAMETER]: event.target.value })}
          >
            <option value="">Todos</option>
            {USER_ROLES.map((roleOption) => (
              <option key={roleOption} value={roleOption}>
                {ROLE_LABELS[roleOption]}
              </option>
            ))}
          </select>
        </label>
        <label className="field">
          <span className="field__label">Estado</span>
          <select
            className="select"
            value={activeFilter ?? ACTIVE_FILTER_VALUES.all}
            onChange={(event) => updateParameters({ [ACTIVE_PARAMETER]: event.target.value })}
          >
            <option value={ACTIVE_FILTER_VALUES.all}>Todos</option>
            <option value={ACTIVE_FILTER_VALUES.active}>Activos</option>
            <option value={ACTIVE_FILTER_VALUES.inactive}>Inactivos</option>
          </select>
        </label>
        <div className="filters__actions">
          <button
            type="button"
            className="button button--ghost"
            onClick={() => listParameters.clearFilters()}
            disabled={activeFilterCount === 0}
          >
            Limpiar filtros
          </button>
        </div>
      </FilterPanel>

      {users.isPending && <LoadingState message="Cargando usuarios…" />}
      {users.isError && <ErrorState error={users.error} onRetry={() => users.refetch()} />}
      {users.data && users.data.items.length === 0 && <EmptyState title="No se encontraron usuarios" />}
      {users.data && users.data.items.length > 0 && (
        <>
          <div className="table-wrapper">
            <table className="table">
              <thead>
                <tr>
                  <th scope="col">Nombre</th>
                  <th scope="col">Correo</th>
                  <th scope="col">Rol</th>
                  <th scope="col">Estado</th>
                  <th scope="col">Creado</th>
                  <th scope="col">
                    <span className="visually-hidden">Acciones</span>
                  </th>
                </tr>
              </thead>
              <tbody>
                {users.data.items.map((user) => (
                  <tr key={user.id}>
                    <td>
                      <strong>{user.fullName}</strong>
                      {user.id === currentUser?.id && <div className="table__muted">Usted</div>}
                    </td>
                    <td>{user.email}</td>
                    <td>{roleLabel(user.role)}</td>
                    <td>
                      <ActiveBadge isActive={user.isActive} />
                    </td>
                    <td className="table__muted">{formatDate(user.createdOnUtc)}</td>
                    <td>
                      <div className="table__actions">
                        <button
                          type="button"
                          className="button button--secondary button--small"
                          aria-label={`Editar ${user.fullName}`}
                          onClick={() => setUserToEdit(user)}
                        >
                          Editar
                        </button>
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
          <Pagination
            page={users.data.page}
            pageSize={users.data.pageSize}
            totalCount={users.data.totalCount}
            totalPages={users.data.totalPages}
            onPageChange={listParameters.setPage}
            onPageSizeChange={listParameters.setPageSize}
          />
        </>
      )}

      {isCreating && (
        <Dialog title="Nuevo usuario" onClose={() => setIsCreating(false)}>
          <CreateUserForm onSubmit={createUser} onCancel={() => setIsCreating(false)} />
        </Dialog>
      )}
      {userToEdit && (
        <Dialog title={`Editar ${userToEdit.fullName}`} onClose={() => setUserToEdit(null)}>
          <p className="text-muted">{userToEdit.email}</p>
          <UpdateUserForm
            defaultValues={{ fullName: userToEdit.fullName, role: userToEdit.role, isActive: userToEdit.isActive }}
            onSubmit={updateUser}
            onCancel={() => setUserToEdit(null)}
          />
        </Dialog>
      )}
    </Card>
  );
}
