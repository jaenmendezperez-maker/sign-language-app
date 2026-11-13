import { CertaintyConfig, CertaintyLevel } from '../models/consultation.model';

export const DEFAULT_CERTAINTY: CertaintyConfig = { high: 80, medium: 60, confirmationSeconds: 10 };

/** Mismo cálculo que el backend (RN-01): Alta ≥ high, Media ≥ medium, Baja en otro caso. */
export function levelFor(score: number, config: CertaintyConfig): CertaintyLevel {
  if (score >= config.high) return 'High';
  if (score >= config.medium) return 'Medium';
  return 'Low';
}

/** RF-06 / RNF-08: además del color, cada nivel tiene icono y texto. */
export const LEVEL_INFO: Record<CertaintyLevel, { label: string; icon: string; description: string }> = {
  High: { label: 'Alta', icon: '✓', description: 'Certeza alta' },
  Medium: { label: 'Media', icon: '?', description: 'Certeza media: confirme o descarte' },
  Low: { label: 'Baja', icon: '✕', description: 'Certeza baja: repita la seña' },
};
