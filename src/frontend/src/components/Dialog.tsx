import { useCallback, useEffect, useId, useMemo, useRef, useState } from 'react';
import type { ReactNode } from 'react';
import { DialogContext } from '../context/dialogContext';
import type { DialogContextValue } from '../context/dialogContext';

const INITIAL_FOCUS_SELECTOR = 'input:not([disabled]), select:not([disabled]), textarea:not([disabled])';
const FOCUSABLE_SELECTOR = [
  'a[href]',
  'button:not([disabled])',
  INITIAL_FOCUS_SELECTOR,
  '[tabindex]:not([tabindex="-1"])',
].join(', ');

interface DialogProps {
  title: string;
  children: ReactNode;
  onClose: () => void;
  wide?: boolean;
}

function keepFocusInside(event: KeyboardEvent, container: HTMLElement): void {
  const focusable = Array.from(container.querySelectorAll<HTMLElement>(FOCUSABLE_SELECTOR));
  const first = focusable.at(0);
  const last = focusable.at(-1);
  if (!first || !last) {
    event.preventDefault();
    container.focus();
    return;
  }

  const active = document.activeElement;
  const isOutside = !container.contains(active);
  if (event.shiftKey && (active === first || active === container || isOutside)) {
    event.preventDefault();
    last.focus();
  } else if (!event.shiftKey && (active === last || isOutside)) {
    event.preventDefault();
    first.focus();
  }
}

export function Dialog({ title, children, onClose, wide = false }: DialogProps) {
  const titleId = useId();
  const dialogReference = useRef<HTMLDivElement>(null);
  // El padre suele pasar una función nueva en cada render; si el efecto dependiera de ella, el foco saltaría.
  const onCloseReference = useRef(onClose);
  onCloseReference.current = onClose;
  const isDirtyReference = useRef(false);
  const [isConfirmingDiscard, setIsConfirmingDiscard] = useState(false);
  const isConfirmingDiscardReference = useRef(isConfirmingDiscard);
  isConfirmingDiscardReference.current = isConfirmingDiscard;

  const requestClose = useCallback(() => {
    if (isDirtyReference.current) {
      setIsConfirmingDiscard(true);
    } else {
      onCloseReference.current();
    }
  }, []);
  const contextValue = useMemo<DialogContextValue>(
    () => ({
      reportDirty: (isDirty) => {
        isDirtyReference.current = isDirty;
      },
      requestClose,
    }),
    [requestClose],
  );

  useEffect(() => {
    const dialog = dialogReference.current;
    const previouslyFocused = document.activeElement as HTMLElement | null;
    const previousBodyOverflow = document.body.style.overflow;
    document.body.style.overflow = 'hidden';
    const initialField = dialog?.querySelector<HTMLElement>(INITIAL_FOCUS_SELECTOR);
    (initialField ?? dialog)?.focus();

    const handleKeyDown = (event: KeyboardEvent) => {
      if (event.key === 'Escape') {
        if (isConfirmingDiscardReference.current) {
          setIsConfirmingDiscard(false);
        } else {
          requestClose();
        }
      } else if (event.key === 'Tab' && dialog) {
        keepFocusInside(event, dialog);
      }
    };
    document.addEventListener('keydown', handleKeyDown);
    return () => {
      document.removeEventListener('keydown', handleKeyDown);
      document.body.style.overflow = previousBodyOverflow;
      previouslyFocused?.focus();
    };
  }, [requestClose]);

  return (
    <div
      className="dialog-backdrop"
      onMouseDown={(event) => {
        if (event.target === event.currentTarget) {
          requestClose();
        }
      }}
    >
      <div
        ref={dialogReference}
        className={`dialog ${wide ? 'dialog--wide' : ''}`.trim()}
        role="dialog"
        aria-modal="true"
        aria-labelledby={titleId}
        tabIndex={-1}
      >
        <h2 id={titleId} className="dialog__title">
          {title}
        </h2>
        {isConfirmingDiscard && (
          <div className="alert alert--warning dialog__discard" role="alert">
            <span>Hay cambios sin guardar. ¿Desea descartarlos?</span>
            <span className="inline">
              <button
                type="button"
                className="button button--secondary button--small"
                autoFocus
                onClick={() => setIsConfirmingDiscard(false)}
              >
                Seguir editando
              </button>
              <button type="button" className="button button--danger button--small" onClick={() => onClose()}>
                Descartar cambios
              </button>
            </span>
          </div>
        )}
        <DialogContext.Provider value={contextValue}>{children}</DialogContext.Provider>
      </div>
    </div>
  );
}

interface ConfirmDialogProps {
  title: string;
  message: ReactNode;
  confirmLabel?: string;
  cancelLabel?: string;
  isProcessing?: boolean;
  onConfirm: () => void;
  onCancel: () => void;
}

export function ConfirmDialog({
  title,
  message,
  confirmLabel = 'Confirmar',
  cancelLabel = 'Cancelar',
  isProcessing = false,
  onConfirm,
  onCancel,
}: ConfirmDialogProps) {
  return (
    <Dialog title={title} onClose={onCancel}>
      <div className="dialog__body">{message}</div>
      <div className="form-actions">
        <button type="button" className="button button--secondary" onClick={onCancel} disabled={isProcessing}>
          {cancelLabel}
        </button>
        <button type="button" className="button button--danger" onClick={onConfirm} disabled={isProcessing}>
          {isProcessing ? 'Procesando…' : confirmLabel}
        </button>
      </div>
    </Dialog>
  );
}
