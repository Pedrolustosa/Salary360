export function categoryDisplayName(
  name: string | null | undefined,
  systemKey: string | null | undefined,
  translate: (key: string) => string,
): string {
  if (name?.trim()) return name.trim();
  if (systemKey) return translate(`expenses.systemCategories.${systemKey}`);
  return '';
}
