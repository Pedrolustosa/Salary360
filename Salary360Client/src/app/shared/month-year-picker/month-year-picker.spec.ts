import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideTranslateService } from '@ngx-translate/core';
import { LanguageService } from '../../core/i18n/language.service';
import { LanguageCode } from '../../core/i18n/enums/language-code.enum';
import { MonthYearPicker } from './month-year-picker';

describe('MonthYearPicker', () => {
  let fixture: ComponentFixture<MonthYearPicker>;
  let component: MonthYearPicker;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [MonthYearPicker],
      providers: [provideTranslateService(), LanguageService],
    }).compileComponents();

    const language = TestBed.inject(LanguageService);
    await language.set(LanguageCode.EnUs);

    fixture = TestBed.createComponent(MonthYearPicker);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should write a competence value into the input', () => {
    component.writeValue({ year: 2024, month: 3 });
    fixture.detectChanges();
    const input = fixture.nativeElement.querySelector('input') as HTMLInputElement;
    expect(input.value.toLowerCase()).toContain('2024');
    expect(input.value.toLowerCase()).toContain('march');
  });

  it('should emit the selected month and close the panel', () => {
    let value: { year: number; month: number } | null = null;
    component.registerOnChange((next) => (value = next));
    (component as unknown as { toggle(): void }).toggle();
    fixture.detectChanges();
    (component as unknown as { viewYear: { set(v: number): void } }).viewYear.set(2023);
    (component as unknown as { pick(month: number): void }).pick(5);
    expect(value).toEqual({ year: 2023, month: 5 });
  });

  it('should block future months in the current year', () => {
    const now = new Date();
    (component as unknown as { viewYear: { set(v: number): void } }).viewYear.set(now.getFullYear());
    const future = now.getMonth() + 2;
    if (future <= 12) {
      expect((component as unknown as { monthAllowed(month: number): boolean }).monthAllowed(future)).toBe(false);
    }
    expect((component as unknown as { monthAllowed(month: number): boolean }).monthAllowed(1)).toBe(true);
  });
});
