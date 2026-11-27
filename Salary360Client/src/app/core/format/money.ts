const MAX_AMOUNT = 999_999_999.99;

export function roundToEven(value: number, decimals = 2): number {
  if (!Number.isFinite(value)) return 0;
  const factor = 10 ** decimals;
  const scaled = Number((Math.abs(value) * factor).toFixed(8));
  const floored = Math.floor(scaled);
  const fraction = scaled - floored;
  const isMidpoint = Math.abs(fraction - 0.5) < 1e-8;

  let rounded: number;
  if (isMidpoint) {
    rounded = floored % 2 === 0 ? floored : floored + 1;
  } else {
    rounded = Math.round(scaled);
  }

  const result = rounded / factor;
  return value < 0 ? -result : result;
}

export function clampMoney(value: number): number {
  if (!Number.isFinite(value)) return 0;
  return Math.min(Math.max(0, roundToEven(value)), MAX_AMOUNT);
}

export function formatMoney(value: number | null | undefined, locale: string): string {
  if (value === null || value === undefined || !Number.isFinite(value)) return '';
  return new Intl.NumberFormat(locale, {
    style: 'currency',
    currency: 'BRL',
    minimumFractionDigits: 2,
    maximumFractionDigits: 2,
  }).format(value);
}

export function parseMoneyDigits(digits: string): number | null {
  const cleaned = digits.replace(/\D/g, '');
  if (!cleaned) return null;
  const cents = Number(cleaned);
  if (!Number.isFinite(cents)) return null;
  return clampMoney(cents / 100);
}
