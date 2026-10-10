import { useId, useState } from 'react';
import type { ReactNode } from 'react';

interface FilterPanelProps {
  label: string;
  activeFilterCount?: number;
  children: ReactNode;
}

// En pantallas angostas los filtros se pliegan para no desplazar el listado fuera de la vista.
export function FilterPanel({ label, activeFilterCount = 0, children }: FilterPanelProps) {
  const [isExpanded, setIsExpanded] = useState(false);
  const panelId = useId();

  return (
    <>
      <button
        type="button"
        className="button button--secondary filters-toggle"
        aria-expanded={isExpanded}
        aria-controls={panelId}
        onClick={() => setIsExpanded((current) => !current)}
      >
        {isExpanded ? 'Ocultar filtros' : 'Mostrar filtros'}
        {activeFilterCount > 0 && ` (${activeFilterCount})`}
      </button>
      <form
        id={panelId}
        className={`filters ${isExpanded ? 'filters--expanded' : ''}`.trim()}
        onSubmit={(event) => event.preventDefault()}
        aria-label={label}
      >
        {children}
      </form>
    </>
  );
}
