export function isExpenseOverdue(
  year: number,
  month: number,
  dueDay: number | null | undefined,
  isPaid: boolean,
  today: Date = new Date(),
): boolean {
  if (isPaid || dueDay == null || dueDay < 1) return false;
  const due = new Date(year, month - 1, dueDay);
  const startOfToday = new Date(today.getFullYear(), today.getMonth(), today.getDate());
  return due < startOfToday;
}
