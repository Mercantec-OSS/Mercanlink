import CryptoJS from "crypto-js"

// WARNING: Storing a secret key in client-side code provides obfuscation, not true security.
// A determined attacker can still reverse-engineer the code to find the key.
// For higher security, consider server-side session management or more advanced token handling strategies.
const SECRET_KEY =
  import.meta.env.VITE_STORAGE_SECRET_KEY || "default-super-secret-key"

function encrypt(data: string): string {
  return CryptoJS.AES.encrypt(data, SECRET_KEY).toString()
}

function decrypt(ciphertext: string): string {
  const bytes = CryptoJS.AES.decrypt(ciphertext, SECRET_KEY)
  return bytes.toString(CryptoJS.enc.Utf8)
}

export const storageService = {
  setItem(key: string, value: unknown): void {
    const stringValue = JSON.stringify(value)
    localStorage.setItem(key, encrypt(stringValue))
  },

  getItem<T>(key: string): T | null {
    const storedValue = localStorage.getItem(key)
    if (!storedValue) {
      return null
    }
    try {
      const decryptedValue = decrypt(storedValue)
      return JSON.parse(decryptedValue) as T
    } catch (error) {
      console.error(`Failed to decrypt or parse item from storage: ${key}`, error)
      // If decryption fails, it's safer to remove the corrupted item.
      localStorage.removeItem(key)
      return null
    }
  },

  removeItem(key: string): void {
    localStorage.removeItem(key)
  },
} 