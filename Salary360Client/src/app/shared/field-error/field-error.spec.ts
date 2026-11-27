import { ComponentFixture, TestBed } from '@angular/core/testing';
import { FormControl } from '@angular/forms';
import { provideTranslateService } from '@ngx-translate/core';
import { SERVER_ERROR_KEY } from '../../core/forms/server-errors';
import { FieldError } from './field-error';

describe('FieldError', () => {
  let fixture: ComponentFixture<FieldError>;
  let control: FormControl;

  beforeEach(async () => {
    TestBed.configureTestingModule({ providers: [provideTranslateService({ fallbackLang: 'pt-BR' })] });
    control = new FormControl('');
    fixture = TestBed.createComponent(FieldError);
    fixture.componentRef.setInput('control', control);
    await fixture.whenStable();
  });

  it('should render nothing without a server error', () => {
    expect((fixture.nativeElement as HTMLElement).querySelector('.field-error')).toBeNull();
  });

  it('should render the message of the server error and hide it when it is cleared', async () => {
    control.setErrors({ [SERVER_ERROR_KEY]: 'required' });
    await fixture.whenStable();

    expect((fixture.nativeElement as HTMLElement).querySelector('.field-error')?.textContent).toContain('errors.fields.required');

    control.setValue('x');
    await fixture.whenStable();

    expect((fixture.nativeElement as HTMLElement).querySelector('.field-error')).toBeNull();
  });
});
