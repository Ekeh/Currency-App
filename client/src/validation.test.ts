import { validateEmail, validateName, validatePassword } from "./validation";

describe("validateEmail", () => {
  it("requires a value", () => expect(validateEmail("  ")).toBe("Email is required"));
  it("rejects malformed addresses", () => expect(validateEmail("not-an-email")).toBe("Invalid email format"));
  it("accepts a valid address", () => expect(validateEmail("ada@example.com")).toBeNull());
});

describe("validatePassword (mirrors the server rules)", () => {
  it.each([
    ["", "Password is required"],
    ["Ab1", "Password must be at least 6 characters"],
    ["lowercase1", "Password must contain at least one uppercase letter"],
    ["UPPERCASE1", "Password must contain at least one lowercase letter"],
    ["NoDigitsHere", "Password must contain at least one digit"],
  ])("rejects %j", (password, message) => expect(validatePassword(password)).toBe(message));

  it("accepts a compliant password", () => expect(validatePassword("Passw0rd")).toBeNull());
});

describe("validateName", () => {
  it("requires a value", () => expect(validateName("First name", " ")).toBe("First name is required"));
  it("caps the length at 50", () =>
    expect(validateName("Last name", "x".repeat(51))).toBe("Last name cannot exceed 50 characters"));
  it("accepts a normal name", () => expect(validateName("Last name", "Okafor")).toBeNull());
});
