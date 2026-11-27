import { formatPhone } from './phone-format';

describe('formatPhone', () => {
  it('should format E.164 numbers internationally', () => {
    expect(formatPhone('+5581999990001')).toBe('+55 81 99999 0001');
  });

  it('should return empty for missing values and the raw value when unparseable', () => {
    expect(formatPhone(null)).toBe('');
    expect(formatPhone('abc')).toBe('abc');
  });
});
