export function applyMask(value: string, pattern: string): string {
  const chars = value.replace(/[^a-zA-Z0-9]/g, '');
  let index = 0;
  let result = '';

  for (const token of pattern) {
    if (index >= chars.length) break;

    if (token === '0') {
      while (index < chars.length && !/\d/.test(chars[index])) index++;
      if (index >= chars.length) break;
      result += chars[index++];
    } else if (token === 'A') {
      while (index < chars.length && !/[a-zA-Z]/.test(chars[index])) index++;
      if (index >= chars.length) break;
      result += chars[index++].toUpperCase();
    } else {
      result += token;
    }
  }

  return result;
}
