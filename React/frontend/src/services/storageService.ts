export const storageService = {
  setItem(key: string, value: unknown): void {
    const stringValue = JSON.stringify(value)
    sessionStorage.setItem(key, stringValue)
  },

  getItem<T>(key: string): T | null {
    const storedValue = sessionStorage.getItem(key)
    if (!storedValue) {
      return null
    }
    try {
      return JSON.parse(storedValue) as T
    } catch (error) {
      console.error(`Failed to parse item from storage: ${key}`, error)
      sessionStorage.removeItem(key)
      return null
    }
  },

  removeItem(key: string): void {
    sessionStorage.removeItem(key)
  },
} 