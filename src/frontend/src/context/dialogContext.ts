import { createContext } from 'react';

export interface DialogContextValue {
  reportDirty: (isDirty: boolean) => void;
  requestClose: () => void;
}

export const DialogContext = createContext<DialogContextValue | null>(null);
