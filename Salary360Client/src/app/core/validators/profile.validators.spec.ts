import { FormControl, FormGroup } from '@angular/forms';
import {
  addressValidator,
  birthDateValidator,
  phoneValidator,
  pictureUrlValidator,
  postalCodeValidator,
} from './profile.validators';

describe('profile validators', () => {
  describe('birthDateValidator', () => {
    it('should accept empty and past dates', () => {
      expect(birthDateValidator(new FormControl(''))).toBeNull();
      expect(birthDateValidator(new FormControl('1990-03-15'))).toBeNull();
    });

    it('should reject future, very old and malformed dates', () => {
      const future = new Date();
      future.setFullYear(future.getFullYear() + 1);

      expect(birthDateValidator(new FormControl(future.toISOString().slice(0, 10)))).toEqual({ birthDate: true });
      expect(birthDateValidator(new FormControl('1899-12-31'))).toEqual({ birthDate: true });
      expect(birthDateValidator(new FormControl('nope'))).toEqual({ birthDate: true });
    });
  });

  describe('phoneValidator', () => {
    it('should accept empty and valid international numbers', () => {
      expect(phoneValidator(new FormControl(''))).toBeNull();
      expect(phoneValidator(new FormControl('+5581999990001'))).toBeNull();
    });

    it('should reject invalid numbers', () => {
      expect(phoneValidator(new FormControl('+55123'))).toEqual({ phone: true });
    });
  });

  describe('pictureUrlValidator', () => {
    it('should accept empty, http and https URLs', () => {
      expect(pictureUrlValidator(new FormControl(''))).toBeNull();
      expect(pictureUrlValidator(new FormControl('https://example.com/a.png'))).toBeNull();
      expect(pictureUrlValidator(new FormControl('http://example.com/a.png'))).toBeNull();
    });

    it.each(['ftp://example.com/a.png', 'not a url', '/relative.png'])('should reject %s', (value) => {
      expect(pictureUrlValidator(new FormControl(value))).toEqual({ pictureUrl: true });
    });
  });

  describe('addressValidator', () => {
    const fields = ['postalCode', 'street', 'number', 'complement', 'neighborhood', 'city', 'state', 'country'];

    function address(values: Record<string, string>): FormGroup {
      return new FormGroup(Object.fromEntries(fields.map((field) => [field, new FormControl(values[field] ?? '')])));
    }

    it('should accept an empty address', () => {
      expect(addressValidator(address({}))).toBeNull();
    });

    it('should accept a complete address without complement', () => {
      const complete = { postalCode: '53020-000', street: 'Rua', number: '1', neighborhood: 'Centro', city: 'Olinda', state: 'PE', country: 'BR' };

      expect(addressValidator(address(complete))).toBeNull();
    });

    it('should reject a partially filled address', () => {
      expect(addressValidator(address({ city: 'Olinda' }))).toEqual({ addressIncomplete: true });
    });
  });

  describe('postalCodeValidator', () => {
    function postal(country: string, value: string): FormControl {
      const control = new FormControl(value);
      new FormGroup({ postalCode: control, country: new FormControl(country) });
      return control;
    }

    it('should require the full length for countries with a known format', () => {
      expect(postalCodeValidator(postal('BR', '53020'))).toEqual({ postalCode: true });
      expect(postalCodeValidator(postal('BR', '53020-000'))).toBeNull();
    });

    it('should not validate empty values or unknown countries', () => {
      expect(postalCodeValidator(postal('BR', ''))).toBeNull();
      expect(postalCodeValidator(postal('ZZ', '1'))).toBeNull();
    });
  });
});
