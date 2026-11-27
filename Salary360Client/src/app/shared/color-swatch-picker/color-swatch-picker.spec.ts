import { ComponentFixture, TestBed } from '@angular/core/testing';
import { CategoryColor } from '../../features/expenses/enums/category-color.enum';
import { ColorSwatchPicker } from './color-swatch-picker';

describe('ColorSwatchPicker', () => {
  let fixture: ComponentFixture<ColorSwatchPicker>;
  let component: ColorSwatchPicker;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ColorSwatchPicker],
    }).compileComponents();

    fixture = TestBed.createComponent(ColorSwatchPicker);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should write and emit color values via CVA', () => {
    let value: CategoryColor | null = null;
    component.registerOnChange((next) => (value = next));
    component.writeValue(CategoryColor.Blue);
    fixture.detectChanges();

    const buttons = fixture.nativeElement.querySelectorAll('button');
    const blue = Array.from(buttons as NodeListOf<HTMLButtonElement>).find(
      (button) => button.getAttribute('aria-label') === CategoryColor.Blue,
    );
    expect(blue?.getAttribute('aria-checked')).toBe('true');

    const pink = Array.from(buttons as NodeListOf<HTMLButtonElement>).find(
      (button) => button.getAttribute('aria-label') === CategoryColor.Pink,
    );
    pink?.click();
    expect(value).toBe(CategoryColor.Pink);
  });
});
