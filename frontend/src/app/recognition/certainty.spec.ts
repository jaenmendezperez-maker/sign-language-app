import { DEFAULT_CERTAINTY, LEVEL_INFO, levelFor } from './certainty';

describe('certainty (RF-06)', () => {
  it.each([
    [100, 'High'], [80, 'High'], [79.99, 'Medium'], [72, 'Medium'], [60, 'Medium'], [59.99, 'Low'], [45, 'Low'],
  ] as const)('score %d → %s', (score, level) => {
    expect(levelFor(score, DEFAULT_CERTAINTY)).toBe(level);
  });

  it('uses the configured thresholds', () => {
    expect(levelFor(85, { high: 90, medium: 70, confirmationSeconds: 10 })).toBe('Medium');
  });

  it('every level has text and icon besides color (RNF-08)', () => {
    expect(Object.values(LEVEL_INFO).map((i) => i.label)).toEqual(['Alta', 'Media', 'Baja']);
    expect(Object.values(LEVEL_INFO).every((i) => i.icon.length > 0)).toBe(true);
  });
});
