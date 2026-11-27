import { StatTone } from '../enums/stat-tone.enum';
import { StatToneClasses } from '../interfaces/stat-tone-classes.interface';

export const STAT_TONES: Record<StatTone, StatToneClasses> = {
  [StatTone.Brand]: {
    icon: 'bg-brand-100 text-brand-700 dark:bg-brand-900/50 dark:text-brand-100',
    value: 'text-slate-900 dark:text-white',
    bar: 'bg-brand-600',
    glow: 'from-brand-500/10',
  },
  [StatTone.Emerald]: {
    icon: 'bg-emerald-100 text-emerald-700 dark:bg-emerald-900/50 dark:text-emerald-200',
    value: 'text-emerald-700 dark:text-emerald-300',
    bar: 'bg-emerald-500',
    glow: 'from-emerald-500/10',
  },
  [StatTone.Slate]: {
    icon: 'bg-slate-200 text-slate-600 dark:bg-slate-700 dark:text-slate-300',
    value: 'text-slate-600 dark:text-slate-300',
    bar: 'bg-slate-400',
    glow: 'from-slate-500/10',
  },
  [StatTone.Violet]: {
    icon: 'bg-violet-100 text-violet-700 dark:bg-violet-900/50 dark:text-violet-200',
    value: 'text-violet-700 dark:text-violet-300',
    bar: 'bg-violet-500',
    glow: 'from-violet-500/10',
  },
  [StatTone.Rose]: {
    icon: 'bg-rose-100 text-rose-700 dark:bg-rose-900/50 dark:text-rose-200',
    value: 'text-rose-700 dark:text-rose-300',
    bar: 'bg-rose-500',
    glow: 'from-rose-500/10',
  },
};
