import { Competence } from './competence.interface';
import { InflationCoverage } from './inflation-coverage.interface';
import { PurchasingPowerPoint } from './purchasing-power-point.interface';

export interface PurchasingPower {
  hasEnoughData: boolean;
  requiredRecords: number;
  recordCount: number;
  from: Competence | null;
  to: Competence | null;
  inflationAvailable: boolean;
  isStale: boolean;
  inflationCoverage: InflationCoverage | null;
  nominalChangePercent: number | null;
  ipcaAccumulatedPercent: number | null;
  realGainPercent: number | null;
  realGainAbsolute: number | null;
  series: PurchasingPowerPoint[];
}
