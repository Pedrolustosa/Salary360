import { SalaryRecord } from './salary-record.interface';

export interface PagedSalaries {
  items: SalaryRecord[];
  page: number;
  pageSize: number;
  totalCount: number;
}
