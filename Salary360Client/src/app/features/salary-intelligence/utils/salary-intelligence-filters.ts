import { MonthYearValue } from '../../../shared/month-year-picker/interfaces/month-year-value.interface';

export const MAX_WINDOW_MONTHS = 120;

export function formatCompetence(value: MonthYearValue): string {
  return `${value.year}-${String(value.month).padStart(2, '0')}`;
}

export function parseCompetence(value: string | null | undefined): MonthYearValue | null {
  if (!value) return null;
  const match = /^(\d{4})-(\d{2})$/.exec(value);
  if (!match) return null;
  const year = Number(match[1]);
  const month = Number(match[2]);
  if (!Number.isInteger(year) || !Number.isInteger(month) || month < 1 || month > 12) return null;
  return { year, month };
}

export function monthsBetween(from: MonthYearValue, to: MonthYearValue): number {
  return (to.year - from.year) * 12 + (to.month - from.month) + 1;
}

export function addMonths(value: MonthYearValue, offset: number): MonthYearValue {
  const index = value.year * 12 + (value.month - 1) + offset;
  return { year: Math.floor(index / 12), month: (index % 12) + 1 };
}

export function currentCompetence(now = new Date()): MonthYearValue {
  return { year: now.getFullYear(), month: now.getMonth() + 1 };
}

export function last12Months(now = new Date()): { from: string; to: string } {
  const to = currentCompetence(now);
  const from = addMonths(to, -(12 - 1));
  return { from: formatCompetence(from), to: formatCompetence(to) };
}

export function currentYearPeriod(now = new Date()): { from: string; to: string } {
  const to = currentCompetence(now);
  return { from: formatCompetence({ year: to.year, month: 1 }), to: formatCompetence(to) };
}

export function allPeriod(now = new Date()): { from: string; to: string } {
  const to = currentCompetence(now);
  const from = addMonths(to, -(MAX_WINDOW_MONTHS - 1));
  return { from: formatCompetence(from), to: formatCompetence(to) };
}

export function validatePeriod(from: string | null, to: string | null, now = new Date()): string | null {
  const fromProvided = !!from;
  const toProvided = !!to;
  if (fromProvided !== toProvided) return 'invalid_period';
  if (!fromProvided) return null;

  const fromValue = parseCompetence(from);
  const toValue = parseCompetence(to);
  if (!fromValue || !toValue) return 'invalid_period';
  if (monthsBetween(fromValue, toValue) < 1) return 'invalid_period';
  if (monthsBetween(fromValue, toValue) > MAX_WINDOW_MONTHS) return 'period_too_long';

  const current = currentCompetence(now);
  if (monthsBetween(fromValue, current) < 1 || monthsBetween(toValue, current) < 1) {
    return 'future_competence';
  }

  return null;
}
