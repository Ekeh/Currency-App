export function formatRate(rate: number): string {
  // Small rates (e.g. NGN -> USD) need more precision than large ones (USD -> NGN).
  const digits = rate >= 100 ? 2 : rate >= 1 ? 4 : 6;
  return rate.toLocaleString("en-US", { minimumFractionDigits: 2, maximumFractionDigits: digits });
}

export function formatAmount(value: number): string {
  return value.toLocaleString("en-US", { minimumFractionDigits: 2, maximumFractionDigits: 2 });
}

export function formatTimestamp(iso: string): string {
  const date = new Date(iso);
  if (Number.isNaN(date.getTime())) return "unknown";
  return date.toLocaleString(undefined, { dateStyle: "medium", timeStyle: "short" });
}
