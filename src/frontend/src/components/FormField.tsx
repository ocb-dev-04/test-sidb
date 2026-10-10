import { cloneElement, isValidElement, useId } from 'react';
import type { ReactElement } from 'react';

interface FormFieldProps {
  label: string;
  error?: string;
  hint?: string;
  required?: boolean;
  className?: string;
  children: ReactElement<Record<string, unknown>>;
}

export function FormField({ label, error, hint, required, className = '', children }: FormFieldProps) {
  const generatedId = useId();
  const controlId = (children.props.id as string | undefined) ?? generatedId;
  const messageId = `${controlId}-message`;
  const control = isValidElement(children)
    ? cloneElement(children, {
        id: controlId,
        'aria-invalid': error ? true : undefined,
        'aria-describedby': error || hint ? messageId : undefined,
      })
    : children;

  return (
    <div className={`field ${className}`.trim()}>
      <label className="field__label" htmlFor={controlId}>
        {label}
        {required && (
          <span className="field__required" aria-hidden="true">
            *
          </span>
        )}
      </label>
      {control}
      {error ? (
        <span id={messageId} className="field__error" role="alert">
          {error}
        </span>
      ) : (
        hint && (
          <span id={messageId} className="field__hint">
            {hint}
          </span>
        )
      )}
    </div>
  );
}
