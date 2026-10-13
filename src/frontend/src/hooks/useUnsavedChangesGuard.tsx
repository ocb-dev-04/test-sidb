import { useContext, useEffect } from 'react';
import type { ReactNode } from 'react';
import { useBlocker } from 'react-router-dom';
import { ConfirmDialog } from '../components/Dialog';
import { DialogContext } from '../context/dialogContext';

interface UnsavedChangesGuard {
  cancel: () => void;
  prompt: ReactNode;
}

// Dentro de un Dialog avisa al diálogo; en una página bloquea la navegación a otra ruta y el cierre de la pestaña.
export function useUnsavedChangesGuard(
  isDirty: boolean,
  isSubmitting: boolean,
  onCancel: () => void,
): UnsavedChangesGuard {
  const dialog = useContext(DialogContext);
  const shouldGuard = isDirty && !isSubmitting;

  useEffect(() => {
    dialog?.reportDirty(shouldGuard);
  }, [dialog, shouldGuard]);

  useEffect(() => {
    if (!shouldGuard) {
      return;
    }
    const warnBeforeUnload = (event: BeforeUnloadEvent) => event.preventDefault();
    window.addEventListener('beforeunload', warnBeforeUnload);
    return () => window.removeEventListener('beforeunload', warnBeforeUnload);
  }, [shouldGuard]);

  const blocker = useBlocker(
    ({ currentLocation, nextLocation }) =>
      dialog === null && shouldGuard && currentLocation.pathname !== nextLocation.pathname,
  );

  const prompt =
    blocker.state === 'blocked' ? (
      <ConfirmDialog
        title="Cambios sin guardar"
        message="Hay cambios sin guardar. ¿Desea salir y descartarlos?"
        confirmLabel="Descartar cambios"
        cancelLabel="Seguir editando"
        onConfirm={() => blocker.proceed()}
        onCancel={() => blocker.reset()}
      />
    ) : null;

  return { cancel: dialog ? dialog.requestClose : onCancel, prompt };
}
