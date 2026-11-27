import { Competence } from './competence.interface';

export interface SalaryIntelligenceSummary {
  hasEnoughData: boolean;
  requiredRecords: number;
  recordCount: number;
  from: Competence | null;
  to: Competence | null;
  currentGross: number | null;
  currentNet: number | null;
  totalBenefits: number | null;
  totalDeductions: number | null;
  grossChangeAbsolute: number | null;
  grossChangePercent: number | null;
  netChangeAbsolute: number | null;
  netChangePercent: number | null;
}
