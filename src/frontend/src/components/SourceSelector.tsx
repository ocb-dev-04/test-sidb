import type { GovernmentEntityDataSource, GovernmentEntityDataSourceResponse } from '../types/api';
import { dataSourceLabel } from '../utils/labels';
import { Badge } from './Badges';

interface SourceSelectorProps {
  sources: GovernmentEntityDataSourceResponse[];
  selected: GovernmentEntityDataSource | undefined;
  onSelect: (source: GovernmentEntityDataSource) => void;
  writableOnly?: boolean;
}

export function SourceSelector({ sources, selected, onSelect, writableOnly = false }: SourceSelectorProps) {
  const options = writableOnly ? sources.filter((source) => !source.isReadOnly) : sources;

  return (
    <div className="inline">
      <span className="field__label" id="source-selector-label">
        Fuente de datos
      </span>
      <div className="source-switch" role="group" aria-labelledby="source-selector-label">
        {options.map((option) => (
          <button
            key={option.source}
            type="button"
            className="source-switch__option"
            aria-pressed={option.source === selected}
            onClick={() => onSelect(option.source)}
          >
            {dataSourceLabel(option.source)}
            {option.isDefault && <Badge tone="info">Predeterminada</Badge>}
          </button>
        ))}
      </div>
    </div>
  );
}
