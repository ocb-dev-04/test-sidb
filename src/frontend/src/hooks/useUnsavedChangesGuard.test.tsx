import { screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { useForm } from 'react-hook-form';
import { Link, useLocation } from 'react-router-dom';
import { describe, expect, it, vi } from 'vitest';
import { Dialog } from '../components/Dialog';
import { renderWithProviders } from '../test/renderWithProviders';
import { useUnsavedChangesGuard } from './useUnsavedChangesGuard';

const FORM_ROUTE = '/formulario';
const OTHER_ROUTE = '/otra-ruta';

function GuardedForm({ onCancel }: { onCancel: () => void }) {
  const {
    register,
    formState: { isDirty, isSubmitting },
  } = useForm({ defaultValues: { name: '' } });
  const { cancel, prompt } = useUnsavedChangesGuard(isDirty, isSubmitting, onCancel);
  return (
    <form>
      <label>
        Nombre
        <input {...register('name')} />
      </label>
      <button type="button" onClick={cancel}>
        Cancelar
      </button>
      {prompt}
    </form>
  );
}

function CurrentPath() {
  return <span data-testid="ruta">{useLocation().pathname}</span>;
}

describe('useUnsavedChangesGuard dentro de un Dialog', () => {
  function renderDialog(onClose: () => void) {
    renderWithProviders(
      <Dialog title="Editar" onClose={onClose}>
        <GuardedForm onCancel={onClose} />
      </Dialog>,
    );
  }

  it('cierra directamente si no hay cambios', async () => {
    const onClose = vi.fn();
    renderDialog(onClose);

    await userEvent.keyboard('{Escape}');

    expect(onClose).toHaveBeenCalledOnce();
  });

  it('pide confirmación con Escape o Cancelar cuando hay cambios', async () => {
    const onClose = vi.fn();
    renderDialog(onClose);
    await userEvent.type(screen.getByLabelText('Nombre'), 'Cambio');

    await userEvent.keyboard('{Escape}');
    expect(screen.getByRole('alert')).toHaveTextContent('Hay cambios sin guardar');
    await userEvent.click(screen.getByRole('button', { name: 'Seguir editando' }));
    expect(screen.queryByRole('alert')).not.toBeInTheDocument();
    await userEvent.click(screen.getByRole('button', { name: 'Cancelar' }));

    await userEvent.click(screen.getByRole('button', { name: 'Descartar cambios' }));

    expect(onClose).toHaveBeenCalledOnce();
    expect(screen.getByLabelText('Nombre')).toHaveValue('Cambio');
  });
});

describe('useUnsavedChangesGuard en una página', () => {
  function renderPage() {
    renderWithProviders(
      <>
        <CurrentPath />
        <Link to={OTHER_ROUTE}>Salir</Link>
        <GuardedForm onCancel={vi.fn()} />
      </>,
      { route: FORM_ROUTE },
    );
  }

  it('deja navegar si no hay cambios', async () => {
    renderPage();

    await userEvent.click(screen.getByRole('link', { name: 'Salir' }));

    expect(screen.getByTestId('ruta')).toHaveTextContent(OTHER_ROUTE);
  });

  it('bloquea la navegación con cambios hasta confirmar', async () => {
    renderPage();
    await userEvent.type(screen.getByLabelText('Nombre'), 'Cambio');

    await userEvent.click(screen.getByRole('link', { name: 'Salir' }));
    expect(screen.getByRole('dialog', { name: 'Cambios sin guardar' })).toBeInTheDocument();
    await userEvent.click(screen.getByRole('button', { name: 'Seguir editando' }));
    expect(screen.getByTestId('ruta')).toHaveTextContent(FORM_ROUTE);

    await userEvent.click(screen.getByRole('link', { name: 'Salir' }));
    await userEvent.click(screen.getByRole('button', { name: 'Descartar cambios' }));

    expect(screen.getByTestId('ruta')).toHaveTextContent(OTHER_ROUTE);
  });
});
