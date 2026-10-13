import { render, screen } from '@testing-library/react';
import { createMemoryRouter, RouterProvider } from 'react-router-dom';
import { afterEach, describe, expect, it, vi } from 'vitest';
import { RouteErrorPage } from './RouteErrorPage';

function BrokenPage(): never {
  throw new Error('Fallo de render');
}

function renderRoute(element: React.ReactNode) {
  const router = createMemoryRouter([{ path: '/rota', element, errorElement: <RouteErrorPage /> }], {
    initialEntries: ['/rota'],
  });
  return render(<RouterProvider router={router} />);
}

afterEach(() => {
  vi.restoreAllMocks();
});

describe('RouteErrorPage', () => {
  it('muestra un mensaje en español con opciones para recuperarse', () => {
    vi.spyOn(console, 'error').mockImplementation(() => undefined);
    vi.spyOn(console, 'warn').mockImplementation(() => undefined);

    renderRoute(<BrokenPage />);

    expect(screen.getByRole('alert')).toHaveTextContent('Algo salió mal');
    expect(screen.getByRole('button', { name: 'Recargar' })).toBeInTheDocument();
    expect(screen.getByRole('link', { name: 'Ir al inicio' })).toHaveAttribute('href', '/');
  });
});
