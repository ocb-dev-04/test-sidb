import { useCallback, useMemo, useState } from 'react';
import type { ReactNode } from 'react';
import { ToastContext } from '../context/toastContext';
import type { ToastTone } from '../context/toastContext';

const TOAST_DURATION_MILLISECONDS = 4000;

interface ToastMessage {
  id: number;
  message: string;
  tone: ToastTone;
}

let nextToastId = 0;

export function ToastProvider({ children }: { children: ReactNode }) {
  const [toasts, setToasts] = useState<ToastMessage[]>([]);

  const showToast = useCallback((message: string, tone: ToastTone = 'success') => {
    nextToastId += 1;
    const toast: ToastMessage = { id: nextToastId, message, tone };
    setToasts((current) => [...current, toast]);
    window.setTimeout(
      () => setToasts((current) => current.filter((item) => item.id !== toast.id)),
      TOAST_DURATION_MILLISECONDS,
    );
  }, []);

  const value = useMemo(() => ({ showToast }), [showToast]);

  return (
    <ToastContext.Provider value={value}>
      {children}
      <div className="toast-region" aria-live="polite">
        {toasts.map((toast) => (
          <div key={toast.id} className={`toast toast--${toast.tone}`} role="status">
            {toast.message}
          </div>
        ))}
      </div>
    </ToastContext.Provider>
  );
}
