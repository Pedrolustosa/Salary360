import { EvolutionMode } from '../enums/evolution-mode.enum';

export interface SalaryEvolutionQuery {
  mode: EvolutionMode;
  month?: number | null;
  fromYear: number;
  toYear: number;
}
