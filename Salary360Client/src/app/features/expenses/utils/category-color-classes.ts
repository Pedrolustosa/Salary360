import { CategoryColor } from '../enums/category-color.enum';

export interface CategoryColorClasses {
  bg: string;
  text: string;
  ring: string;
  chart: string;
}

export const CATEGORY_COLOR_CLASSES: Record<CategoryColor, CategoryColorClasses> = {
  [CategoryColor.Slate]: {
    bg: 'bg-slate-400',
    text: 'text-slate-600 dark:text-slate-300',
    ring: 'ring-slate-400',
    chart: '#94a3b8',
  },
  [CategoryColor.Red]: {
    bg: 'bg-red-500',
    text: 'text-red-600 dark:text-red-400',
    ring: 'ring-red-500',
    chart: '#ef4444',
  },
  [CategoryColor.Orange]: {
    bg: 'bg-orange-500',
    text: 'text-orange-600 dark:text-orange-400',
    ring: 'ring-orange-500',
    chart: '#f97316',
  },
  [CategoryColor.Amber]: {
    bg: 'bg-amber-500',
    text: 'text-amber-600 dark:text-amber-400',
    ring: 'ring-amber-500',
    chart: '#f59e0b',
  },
  [CategoryColor.Green]: {
    bg: 'bg-green-500',
    text: 'text-green-600 dark:text-green-400',
    ring: 'ring-green-500',
    chart: '#22c55e',
  },
  [CategoryColor.Teal]: {
    bg: 'bg-teal-500',
    text: 'text-teal-600 dark:text-teal-400',
    ring: 'ring-teal-500',
    chart: '#14b8a6',
  },
  [CategoryColor.Blue]: {
    bg: 'bg-blue-500',
    text: 'text-blue-600 dark:text-blue-400',
    ring: 'ring-blue-500',
    chart: '#3b82f6',
  },
  [CategoryColor.Violet]: {
    bg: 'bg-violet-500',
    text: 'text-violet-600 dark:text-violet-400',
    ring: 'ring-violet-500',
    chart: '#8b5cf6',
  },
  [CategoryColor.Pink]: {
    bg: 'bg-pink-500',
    text: 'text-pink-600 dark:text-pink-400',
    ring: 'ring-pink-500',
    chart: '#ec4899',
  },
};

export function categoryColorClasses(color: CategoryColor): CategoryColorClasses {
  return CATEGORY_COLOR_CLASSES[color] ?? CATEGORY_COLOR_CLASSES[CategoryColor.Slate];
}
