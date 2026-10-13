import { zodResolver } from '@hookform/resolvers/zod';
import { useState } from 'react';
import { useForm } from 'react-hook-form';
import { Navigate, useLocation, useNavigate } from 'react-router-dom';
import logo from '../assets/SUPERINTENDENCIA_DE_BANCOS.png';
import { FormField } from '../components/FormField';
import { Alert } from '../components/States';
import { useAuth } from '../hooks/useAuth';
import { ROUTES } from '../layout/routes';
import { LOGIN_SCHEMA } from '../schemas/authSchemas';
import type { LoginFormValues } from '../schemas/authSchemas';
import { errorMessage } from '../utils/errorMessages';

export function LoginPage() {
  const { login, isAuthenticated, sessionExpired } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();
  const [submitError, setSubmitError] = useState<string | null>(null);
  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
  } = useForm<LoginFormValues>({ resolver: zodResolver(LOGIN_SCHEMA), defaultValues: { email: '', password: '' } });

  const redirectTo = (location.state as { from?: string } | null)?.from ?? ROUTES.home;

  if (isAuthenticated) {
    return <Navigate to={redirectTo} replace />;
  }

  const onSubmit = async (values: LoginFormValues) => {
    setSubmitError(null);
    try {
      await login(values);
      navigate(redirectTo, { replace: true });
    } catch (error) {
      setSubmitError(errorMessage(error));
    }
  };

  return (
    <div className="login-page">
      <div className="login-page__brand">
        <img src={logo} alt="Superintendencia de Bancos - República Dominicana" />
        <p>Sistema de gestión de solicitudes internas.</p>
      </div>
      <div className="login-page__panel">
        <div className="card login-card">
          <h1>Iniciar sesión</h1>
          <p className="text-muted">Ingrese sus credenciales institucionales para continuar.</p>
          <form className="stack" onSubmit={handleSubmit(onSubmit)} noValidate>
            {submitError && <Alert tone="error">{submitError}</Alert>}
            {sessionExpired && !submitError && (
              <Alert tone="info">Su sesión expiró. Inicie sesión de nuevo para continuar.</Alert>
            )}
            <FormField label="Correo electrónico" error={errors.email?.message} required>
              <input className="input" type="email" autoComplete="username" {...register('email')} />
            </FormField>
            <FormField label="Contraseña" error={errors.password?.message} required>
              <input className="input" type="password" autoComplete="current-password" {...register('password')} />
            </FormField>
            <button type="submit" className="button button--primary" disabled={isSubmitting}>
              {isSubmitting ? 'Ingresando…' : 'Ingresar'}
            </button>
          </form>
        </div>
      </div>
    </div>
  );
}
