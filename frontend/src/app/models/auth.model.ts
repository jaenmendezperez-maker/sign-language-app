/** Debe coincidir con SignLanguageApp.Domain.Enums.UserRole. */
export type UserRole = 'Administrator' | 'MedicalStaff';

export interface LoginResult {
  token: string;
  expiresAt: string;
  name: string;
  role: UserRole;
}
