import {
  allPeriod,
  currentYearPeriod,
  last12Months,
  validatePeriod,
} from './salary-intelligence-filters';

describe('salary-intelligence-filters', () => {
  const now = new Date(2024, 5, 15);

  it('should build last 12 months shortcut', () => {
    expect(last12Months(now)).toEqual({ from: '2023-07', to: '2024-06' });
  });

  it('should build current year shortcut', () => {
    expect(currentYearPeriod(now)).toEqual({ from: '2024-01', to: '2024-06' });
  });

  it('should build all period within max window', () => {
    expect(allPeriod(now)).toEqual({ from: '2014-07', to: '2024-06' });
  });

  it('should validate period rules', () => {
    expect(validatePeriod(null, null, now)).toBeNull();
    expect(validatePeriod('2024-01', null, now)).toBe('invalid_period');
    expect(validatePeriod('2024-06', '2024-01', now)).toBe('invalid_period');
    expect(validatePeriod('2010-01', '2024-06', now)).toBe('period_too_long');
    expect(validatePeriod('2024-01', '2025-01', now)).toBe('future_competence');
    expect(validatePeriod('2024-01', '2024-06', now)).toBeNull();
  });
});
