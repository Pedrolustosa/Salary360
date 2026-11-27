import { DEFAULT_POSTAL_MAX_LENGTH } from './constants/postal-code-formats';
import { postalCodeFormat, postalCodeMaxLength } from './postal-code';

describe('postal-code', () => {
  it('should find formats regardless of case', () => {
    expect(postalCodeFormat('br')?.pattern).toBe('00000-000');
    expect(postalCodeFormat('US')?.minLength).toBe(5);
  });

  it('should return null for unknown countries', () => {
    expect(postalCodeFormat('ZZ')).toBeNull();
  });

  it('should limit the length by the country pattern or use the default', () => {
    expect(postalCodeMaxLength('BR')).toBe(9);
    expect(postalCodeMaxLength('ZZ')).toBe(DEFAULT_POSTAL_MAX_LENGTH);
  });
});
