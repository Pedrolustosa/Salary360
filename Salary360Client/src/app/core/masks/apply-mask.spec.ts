import { applyMask } from './apply-mask';

describe('applyMask', () => {
  it('should format digits following the pattern', () => {
    expect(applyMask('53020000', '00000-000')).toBe('53020-000');
  });

  it('should stop at the end of the pattern', () => {
    expect(applyMask('530200001234', '00000-000')).toBe('53020-000');
  });

  it('should not append the literal before there is a next digit', () => {
    expect(applyMask('53020', '00000-000')).toBe('53020');
  });

  it('should skip letters in digit positions', () => {
    expect(applyMask('5a3b0', '000')).toBe('530');
  });

  it('should uppercase letters in letter positions', () => {
    expect(applyMask('sw1a1aa', 'AA0A 0AA')).toBe('SW1A 1AA');
  });

  it('should return an empty string for an empty value', () => {
    expect(applyMask('', '00000-000')).toBe('');
  });
});
