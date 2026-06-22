export function getLocalDateKey(p_date: Date = new Date()): string {
    const year = p_date.getFullYear();
    const month = String(p_date.getMonth() + 1).padStart(2, "0");
    const day = String(p_date.getDate()).padStart(2, "0");

    return `${year}-${month}-${day}`;
}
