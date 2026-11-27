import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideTranslateService } from '@ngx-translate/core';
import { LanguageService } from '../../core/i18n/language.service';
import { LanguageCode } from '../../core/i18n/enums/language-code.enum';
import { MoneyInput } from './money-input';

describe('MoneyInput', () => {
  let fixture: ComponentFixture<MoneyInput>;
  let component: MoneyInput;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [MoneyInput],
      providers: [provideTranslateService(), LanguageService],
    }).compileComponents();

    const language = TestBed.inject(LanguageService);
    await language.set(LanguageCode.PtBr);

    fixture = TestBed.createComponent(MoneyInput);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should write and format a numeric value', () => {
    component.writeValue(1234.5);
    fixture.detectChanges();
    const input = fixture.nativeElement.querySelector('input') as HTMLInputElement;
    expect(input.value).toMatch(/1\.234,50/);
  });

  it('should emit cents-based values from digit input', () => {
    let value: number | null = 0;
    component.registerOnChange((next) => (value = next));
    const input = fixture.nativeElement.querySelector('input') as HTMLInputElement;
    input.value = '12345';
    input.dispatchEvent(new Event('input'));
    expect(value).toBe(123.45);
  });

  it('should clear to null on empty input', () => {
    let value: number | null = 1;
    component.registerOnChange((next) => (value = next));
    const input = fixture.nativeElement.querySelector('input') as HTMLInputElement;
    input.value = '';
    input.dispatchEvent(new Event('input'));
    expect(value).toBeNull();
  });
});
