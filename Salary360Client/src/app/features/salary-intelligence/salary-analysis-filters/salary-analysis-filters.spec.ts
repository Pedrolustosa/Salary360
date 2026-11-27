import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideTranslateService } from '@ngx-translate/core';
import { SalaryBasis } from '../enums/salary-basis.enum';
import { SalaryAnalysisFilter } from '../interfaces/salary-analysis-filter.interface';
import { SalaryAnalysisFilters } from './salary-analysis-filters';

describe('SalaryAnalysisFilters', () => {
  let fixture: ComponentFixture<SalaryAnalysisFilters>;

  function root(): HTMLElement {
    return fixture.nativeElement as HTMLElement;
  }

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [SalaryAnalysisFilters],
      providers: [provideTranslateService({ fallbackLang: 'pt-BR' })],
    }).compileComponents();
    fixture = TestBed.createComponent(SalaryAnalysisFilters);
    fixture.componentRef.setInput('value', {
      from: null,
      to: null,
      basis: SalaryBasis.Net,
    } satisfies SalaryAnalysisFilter);
    fixture.detectChanges();
  });

  it('should emit basis changes', () => {
    const changes: SalaryAnalysisFilter[] = [];
    fixture.componentInstance.filterChange.subscribe((value) => changes.push(value));

    const buttons = Array.from(root().querySelectorAll<HTMLButtonElement>('button'));
    const gross = buttons.find((button) => button.textContent?.includes('gross') || button.textContent?.includes('Gross') || button.textContent?.includes('Bruto'));
    gross!.click();

    expect(changes.at(-1)?.basis).toBe(SalaryBasis.Gross);
  });

  it('should emit last 12 months shortcut', () => {
    const changes: SalaryAnalysisFilter[] = [];
    fixture.componentInstance.filterChange.subscribe((value) => changes.push(value));

    const buttons = Array.from(root().querySelectorAll<HTMLButtonElement>('button'));
    const last12 = buttons.find((button) => button.textContent?.includes('last12') || button.textContent?.includes('12'));
    last12!.click();

    expect(changes.at(-1)?.from).toMatch(/^\d{4}-\d{2}$/);
    expect(changes.at(-1)?.to).toMatch(/^\d{4}-\d{2}$/);
  });
});
