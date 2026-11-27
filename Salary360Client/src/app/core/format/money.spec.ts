import { clampMoney, formatMoney, parseMoneyDigits, roundToEven } from './money';

describe('money', () => {
  it('should round midpoints to even (bankers rounding)', () => {
    expect(roundToEven(1.225)).toBe(1.22);
    expect(roundToEven(1.235)).toBe(1.24);
    expect(roundToEven(2.5, 0)).toBe(2);
    expect(roundToEven(3.5, 0)).toBe(4);
  });

  it('should round ordinary values to two decimals', () => {
    expect(roundToEven(1.234)).toBe(1.23);
    expect(roundToEven(1.236)).toBe(1.24);
  });

  it('should clamp to the allowed money range', () => {
    expect(clampMoney(-10)).toBe(0);
    expect(clampMoney(1_000_000_000)).toBe(999_999_999.99);
  });

  it('should format BRL with the active locale', () => {
    expect(formatMoney(1234.5, 'pt-BR')).toMatch(/1\.234,50/);
    expect(formatMoney(null, 'en-US')).toBe('');
  });

  it('should parse digit-only input as cents', () => {
    expect(parseMoneyDigits('12345')).toBe(123.45);
    expect(parseMoneyDigits('')).toBeNull();
    expect(parseMoneyDigits('abc')).toBeNull();
  });
});
