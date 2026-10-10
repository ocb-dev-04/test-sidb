import { screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { describe, expect, it, vi } from 'vitest';
import { ApiError } from '../services/apiError';
import { renderWithProviders } from '../test/renderWithProviders';
import { LoginPage } from './LoginPage';

const HTTP_STATUS_UNAUTHORIZED = 401;

describe('LoginPage', () => {
  it('muestra los mensajes de validación sin llamar a la API', async () => {
    const login = vi.fn();
    renderWithProviders(<LoginPage />, { login });

    await userEvent.click(screen.getByRole('button', { name: 'Ingresar' }));

    expect(await screen.findByText('Ingrese su correo electrónico.')).toBeInTheDocument();
    expect(screen.getByText('Ingrese su contraseña.')).toBeInTheDocument();
    expect(login).not.toHaveBeenCalled();
  });

  it('valida el formato del correo', async () => {
    renderWithProviders(<LoginPage />);

    await userEvent.type(screen.getByLabelText(/Correo electrónico/), 'no-es-correo');
    await userEvent.type(screen.getByLabelText(/Contraseña/), 'Demo2026*');
    await userEvent.click(screen.getByRole('button', { name: 'Ingresar' }));

    expect(await screen.findByText('Ingrese un correo electrónico válido.')).toBeInTheDocument();
  });

  it('muestra el error traducido que devuelve la API', async () => {
    const login = vi
      .fn()
      .mockRejectedValue(new ApiError(HTTP_STATUS_UNAUTHORIZED, { code: 'user.invalidCredentials' }));
    renderWithProviders(<LoginPage />, { login });

    await userEvent.type(screen.getByLabelText(/Correo electrónico/), 'admin@solicitudes.local');
    await userEvent.type(screen.getByLabelText(/Contraseña/), 'incorrecta');
    await userEvent.click(screen.getByRole('button', { name: 'Ingresar' }));

    expect(await screen.findByRole('alert')).toHaveTextContent('El correo o la contraseña son incorrectos.');
    expect(login).toHaveBeenCalledWith({ email: 'admin@solicitudes.local', password: 'incorrecta' });
  });
});
