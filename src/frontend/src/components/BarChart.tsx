import { formatNumber } from '../utils/formatting';

export interface BarChartItem {
  key: string;
  label: string;
  value: number;
  color: string;
}

const FULL_PERCENTAGE = 100;

export function BarChart({ items, label }: { items: BarChartItem[]; label: string }) {
  const maximumValue = Math.max(...items.map((item) => item.value), 1);

  return (
    <ul className="bar-chart" aria-label={label}>
      {items.map((item) => (
        <li key={item.key} className="bar-chart__row">
          <span>{item.label}</span>
          <span className="bar-chart__track" aria-hidden="true">
            <span
              className="bar-chart__fill"
              style={{
                display: 'block',
                width: `${(item.value / maximumValue) * FULL_PERCENTAGE}%`,
                ['--bar-color' as string]: item.color,
              }}
            />
          </span>
          <span className="bar-chart__value">{formatNumber(item.value)}</span>
        </li>
      ))}
    </ul>
  );
}
