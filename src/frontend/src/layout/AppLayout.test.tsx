import { screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { http, HttpResponse } from 'msw';
import { describe, expect, it } from 'vitest';
import { buildUser, renderWithProviders } from '../test/renderWithProviders';
import { apiUrl, server } from '../test/server';
import { AppLayout } from './AppLayout';

describe('AppLayout', () => {
  it('lleva el foco al menú al abrirlo y lo devuelve al cerrarlo con Escape', async () => {
    server.use(http.get(apiUrl('/api/notificaciones/no-leidas/total'), () => HttpResponse.json(0)));
    renderWithProviders(<AppLayout />, { user: buildUser('Analyst') });
    const menuToggle = screen.getByRole('button', { name: 'Abrir menú' });

    await userEvent.click(menuToggle);
    expect(menuToggle).toHaveAttribute('aria-expanded', 'true');
    expect(screen.getByRole('link', { name: 'Inicio' })).toHaveFocus();

    await userEvent.keyboard('{Escape}');

    expect(menuToggle).toHaveAttribute('aria-expanded', 'false');
    expect(menuToggle).toHaveFocus();
  });
});
