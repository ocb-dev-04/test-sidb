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
import { useDebouncedValue } from '../hooks/useDebouncedValue';
import { useToast } from '../hooks/useToast';
import type { CreateUserFormValues, UpdateUserFormValues } from '../schemas/userSchemas';
import { SEARCH_MAXIMUM_LENGTH } from '../schemas/validationLimits';
import { QUERY_KEYS } from '../services/queryKeys';
import { userService } from '../services/userService';
import { USER_ROLES } from '../types/api';
import type { UserResponse, UserRole } from '../types/api';
import { formatDate } from '../utils/formatting';
import { ROLE_LABELS, roleLabel } from '../utils/labels';
import { DEFAULT_PAGE_SIZE, FIRST_PAGE } from '../utils/pagination';

const ACTIVE_FILTER_VALUES = { all: '', active: 'true', inactive: 'false' } as const;

export function UsersPage() {
  const { user: currentUser } = useAuth();
  const { showToast } = useToast();
  const queryClient = useQueryClient();
  const [searchText, setSearchText] = useState('');
  const debouncedSearch = useDebouncedValue(searchText.trim());
  const [role, setRole] = useState<UserRole | ''>('');
  const [activeFilter, setActiveFilter] = useState<string>(ACTIVE_FILTER_VALUES.all);
  const [page, setPage] = useState<number>(FIRST_PAGE);
  const [pageSize, setPageSize] = useState<number>(DEFAULT_PAGE_SIZE);
  const [isCreating, setIsCreating] = useState(false);
  const [userToEdit, setUserToEdit] = useState<UserResponse | null>(null);

  const query = {
    search: debouncedSearch,
    role: role || undefined,
    isActive: activeFilter === ACTIVE_FILTER_VALUES.all ? undefined : activeFilter === ACTIVE_FILTER_VALUES.active,
    page,
    pageSize,
  };
  const users = useQuery({
    queryKey: QUERY_KEYS.userList(query),
    queryFn: () => userService.list(query),
    placeholderData: keepPreviousData,
  });

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
      <FilterPanel
        label="Filtros de usuarios"
        activeFilterCount={[searchText, role, activeFilter].filter(Boolean).length}
      >
        <label className="field filters__search">
          <span className="field__label">Buscar</span>
          <input
            className="input"
            type="search"
            placeholder="Nombre o correo"
            maxLength={SEARCH_MAXIMUM_LENGTH}
            value={searchText}
            onChange={(event) => {
              setSearchText(event.target.value);
              setPage(FIRST_PAGE);
            }}
          />
        </label>
        <label className="field">
          <span className="field__label">Rol</span>
          <select
            className="select"
            value={role}
            onChange={(event) => {
              setRole(event.target.value as UserRole | '');
              setPage(FIRST_PAGE);
            }}
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
            value={activeFilter}
            onChange={(event) => {
              setActiveFilter(event.target.value);
              setPage(FIRST_PAGE);
            }}
          >
            <option value={ACTIVE_FILTER_VALUES.all}>Todos</option>
            <option value={ACTIVE_FILTER_VALUES.active}>Activos</option>
            <option value={ACTIVE_FILTER_VALUES.inactive}>Inactivos</option>
          </select>
        </label>
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
            onPageChange={setPage}
            onPageSizeChange={(newPageSize) => {
              setPageSize(newPageSize);
              setPage(FIRST_PAGE);
            }}
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
