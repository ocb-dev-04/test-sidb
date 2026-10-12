import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { describe, expect, it, vi } from 'vitest';
import { Dialog } from './Dialog';

function DialogWithFields({ onClose }: { onClose: () => void }) {
  return (
    <Dialog title="Editar registro" onClose={onClose}>
      <label>
        Nombre
        <input />
      </label>
      <label>
        Descripción
        <input />
      </label>
      <button type="button">Guardar</button>
    </Dialog>
  );
}

describe('Dialog', () => {
  it('enfoca el primer campo al abrirse', () => {
    render(<DialogWithFields onClose={vi.fn()} />);

    expect(screen.getByLabelText('Nombre')).toHaveFocus();
  });

  it('conserva el foco cuando el padre vuelve a renderizar con otro onClose', async () => {
    const { rerender } = render(<DialogWithFields onClose={vi.fn()} />);
    await userEvent.click(screen.getByLabelText('Descripción'));

    rerender(<DialogWithFields onClose={vi.fn()} />);

    expect(screen.getByLabelText('Descripción')).toHaveFocus();
  });

  it('mantiene el foco dentro del diálogo con Tab y Shift+Tab', async () => {
    render(<DialogWithFields onClose={vi.fn()} />);
    screen.getByRole('button', { name: 'Guardar' }).focus();

    await userEvent.tab();
    expect(screen.getByLabelText('Nombre')).toHaveFocus();

    await userEvent.tab({ shift: true });
    expect(screen.getByRole('button', { name: 'Guardar' })).toHaveFocus();
  });

  it('cierra con Escape usando el onClose más reciente', async () => {
    const firstOnClose = vi.fn();
    const latestOnClose = vi.fn();
    const { rerender } = render(<DialogWithFields onClose={firstOnClose} />);
    rerender(<DialogWithFields onClose={latestOnClose} />);

    await userEvent.keyboard('{Escape}');

    expect(latestOnClose).toHaveBeenCalledOnce();
    expect(firstOnClose).not.toHaveBeenCalled();
  });

  it('bloquea el scroll de la página mientras está abierto', () => {
    const { unmount } = render(<DialogWithFields onClose={vi.fn()} />);
    expect(document.body.style.overflow).toBe('hidden');

    unmount();

    expect(document.body.style.overflow).toBe('');
  });
});
