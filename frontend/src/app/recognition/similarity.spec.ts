import { Sign } from '../models/catalog.model';
import { handFeatures } from './hand-features';
import { bestMatch, similarity } from './similarity';
import { hand, syntheticHand } from './test-hands';

const features = (flex: number[]) => handFeatures([hand(syntheticHand(flex))])!;

const sign = (word: string, flex: number[], handCount: 1 | 2 = 1): Sign => {
  const f = features(flex);
  return {
    id: word, word, categoryId: 1, categoryName: 'Síntomas', handCount, visualUrl: null,
    references: [{ id: `${word}-ref`, distances: f.distances, angles: f.angles, normalizedLandmarks: f.normalized, sampleCount: 5, capturedAt: '' }],
  };
};

describe('similarity (RF-03)', () => {
  it('scores an identical sign as 100', () => {
    const f = features([0, 30, 60, 60, 60]);
    expect(similarity(f, f)).toBe(100);
  });

  it('scores a very different sign low', () => {
    expect(similarity(features([0, 0, 0, 0, 0]), features([80, 85, 85, 85, 85]))).toBeLessThan(60);
  });

  it('stays between 0 and 100', () => {
    const s = similarity(features([0, 0, 0, 0, 0]), { distances: Array(12).fill(99), angles: Array(15).fill(0) });
    expect(s).toBe(0);
  });

  it('picks the reference with the highest score', () => {
    const catalog = [sign('ayuda', [0, 0, 0, 0, 0]), sign('dolor', [0, 80, 80, 80, 80]), sign('fiebre', [70, 0, 70, 70, 70])];
    const match = bestMatch(features([0, 75, 82, 78, 80]), catalog);
    expect(match.sign?.word).toBe('dolor');
    expect(match.score).toBeGreaterThan(80);
  });

  it('only compares against signs with the same number of hands', () => {
    const match = bestMatch(features([0, 0, 0, 0, 0]), [sign('dos-manos', [0, 0, 0, 0, 0], 2)]);
    expect(match.sign).toBeNull();
    expect(match.score).toBe(0);
  });
});
