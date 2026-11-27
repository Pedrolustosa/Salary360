import { IconName } from '../../../shared/icon-paths';
import { CategoryIcon } from '../enums/category-icon.enum';

const CATEGORY_ICON_NAMES: Record<CategoryIcon, IconName> = {
  [CategoryIcon.Home]: 'home',
  [CategoryIcon.ShoppingBag]: 'shopping-bag',
  [CategoryIcon.Truck]: 'truck',
  [CategoryIcon.Heart]: 'heart',
  [CategoryIcon.AcademicCap]: 'academic-cap',
  [CategoryIcon.Sparkles]: 'sparkles',
  [CategoryIcon.CreditCard]: 'credit-card',
  [CategoryIcon.ReceiptPercent]: 'receipt-percent',
  [CategoryIcon.Banknotes]: 'banknotes',
  [CategoryIcon.EllipsisHorizontal]: 'ellipsis-horizontal',
};

export function categoryIconName(icon: CategoryIcon): IconName {
  return CATEGORY_ICON_NAMES[icon] ?? 'ellipsis-horizontal';
}

export const CATEGORY_ICON_OPTIONS: readonly CategoryIcon[] = Object.values(CategoryIcon);
