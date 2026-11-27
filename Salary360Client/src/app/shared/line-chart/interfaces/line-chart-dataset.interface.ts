export interface LineChartDataset {
  label: string;
  data: (number | null)[];
  color: string;
  hidden?: boolean;
  borderDash?: number[];
}
