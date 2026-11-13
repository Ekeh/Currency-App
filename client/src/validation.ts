// Client-side mirrors of the server's FluentValidation rules, for fast feedback.
// The server remains the source of truth and its messages are always shown too.

const EMAIL_PATTERN = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;

export function validateEmail(email: string): string | null {
  if (!email.trim()) return "Email is required";
  if (!EMAIL_PATTERN.test(email.trim())) return "Invalid email format";
  return null;
}

export function validatePassword(password: string): string | null {
  if (!password) return "Password is required";
  if (password.length < 6) return "Password must be at least 6 characters";
  if (!/[A-Z]/.test(password)) return "Password must contain at least one uppercase letter";
  if (!/[a-z]/.test(password)) return "Password must contain at least one lowercase letter";
  if (!/[0-9]/.test(password)) return "Password must contain at least one digit";
  return null;
}

export function validateName(label: string, value: string): string | null {
  if (!value.trim()) return `${label} is required`;
  if (value.trim().length > 50) return `${label} cannot exceed 50 characters`;
  return null;
}
