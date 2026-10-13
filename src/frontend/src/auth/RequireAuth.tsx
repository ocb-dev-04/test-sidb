import type { ReactNode } from 'react';
import { Navigate, useLocation } from 'react-router-dom';
import { useAuth } from '../hooks/useAuth';
import type { UserRole } from '../types/api';
import { ROUTES } from '../layout/routes';

interface RequireAuthProps {
  children: ReactNode;
  roles?: readonly UserRole[];
}

export function RequireAuth({ children, roles }: RequireAuthProps) {
  const { user } = useAuth();
  const location = useLocation();

  if (!user) {
    return <Navigate to={ROUTES.login} replace state={{ from: `${location.pathname}${location.search}` }} />;
  }
  if (roles && !roles.includes(user.role)) {
    return <Navigate to={ROUTES.home} replace />;
  }
  return <>{children}</>;
}
