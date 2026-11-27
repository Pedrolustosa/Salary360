import { formatDateOnly, formatDateTime, todayIsoDate } from './date-format';

describe('date-format', () => {
  it('should format a date-only value without timezone shifts', () => {
    expect(formatDateOnly('2026-03-15', 'en-US')).toBe('3/15/2026');
  });

  it('should return empty strings for missing values', () => {
    expect(formatDateOnly(null, 'pt-BR')).toBe('');
    expect(formatDateTime(undefined, 'pt-BR')).toBe('');
  });

  it('should treat timestamps without offset as UTC', () => {
    expect(formatDateTime('2026-03-15T10:00:00', 'en-US')).toBe(formatDateTime('2026-03-15T10:00:00Z', 'en-US'));
  });

  it('should return today in ISO format', () => {
    expect(todayIsoDate()).toMatch(/^\d{4}-\d{2}-\d{2}$/);
  });
});
