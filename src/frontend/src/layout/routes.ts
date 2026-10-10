export const ROUTES = {
  login: '/login',
  home: '/',
  serviceRequests: '/solicitudes',
  newServiceRequest: '/solicitudes/nueva',
  serviceRequestDetail: (id: string) => `/solicitudes/${id}`,
  governmentEntities: '/entidades-gubernamentales',
  newGovernmentEntity: '/entidades-gubernamentales/nueva',
  editGovernmentEntity: (id: number) => `/entidades-gubernamentales/${id}/editar`,
  users: '/administracion/usuarios',
  catalogs: '/administracion/catalogos',
} as const;

export interface RouteHandle {
  title: string;
}
