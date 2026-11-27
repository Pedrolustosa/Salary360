export interface CopyExpensesRequest {
  fromYear: number;
  fromMonth: number;
  toYear: number;
  toMonth: number;
  onlyFixed: boolean;
}
