import { MonthYearValue } from '../../../shared/month-year-picker/interfaces/month-year-value.interface';

export function shiftCompetence(value: MonthYearValue, delta: number): MonthYearValue {
  const date = new Date(value.year, value.month - 1 + delta, 1);
  return { year: date.getFullYear(), month: date.getMonth() + 1 };
}

export function competenceRangePreview(
  start: MonthYearValue,
  count: number,
  locale: string,
): string {
  if (count < 1) return '';
  const end = shiftCompetence(start, count - 1);
  const formatter = new Intl.DateTimeFormat(locale, { month: 'short', year: 'numeric' });
  const format = (value: MonthYearValue): string => {
    const label = formatter.format(new Date(value.year, value.month - 1, 1));
    return label.charAt(0).toUpperCase() + label.slice(1);
  };
  return `${format(start)} – ${format(end)}`;
}

export function seriesFromMonths(to: MonthYearValue, months: number): { from: MonthYearValue; to: MonthYearValue } {
  return { from: shiftCompetence(to, -(months - 1)), to };
}
