import { angleAt, averageFeatures, handFeatures, singleHandFeatures } from './hand-features';
import { ANGLES_PER_HAND, COORDINATES_PER_HAND, DISTANCES_PER_HAND } from './hand-geometry';
import { hand, mirror, syntheticHand } from './test-hands';

const close = (a: number[], b: number[], eps = 1e-6) => a.every((v, i) => Math.abs(v - b[i]) < eps);

describe('hand features (RF-03)', () => {
  it('produces 12 distances, 15 angles and 63 coordinates per hand', () => {
    const f = singleHandFeatures(syntheticHand());
    expect(f.distances.length).toBe(DISTANCES_PER_HAND);
    expect(f.angles.length).toBe(ANGLES_PER_HAND);
    expect(f.normalized.length).toBe(COORDINATES_PER_HAND);
  });

  it('does not depend on position or distance to the camera', () => {
    const base = singleHandFeatures(syntheticHand([10, 20, 30, 40, 50]));
    const movedAndCloser = singleHandFeatures(syntheticHand([10, 20, 30, 40, 50], { scale: 0.25, dx: 0.2, dy: 0.3 }));

    expect(close(base.distances, movedAndCloser.distances)).toBe(true);
    expect(close(base.angles, movedAndCloser.angles)).toBe(true);
  });

  it('gives the same distances and angles for the mirrored hand', () => {
    const right = singleHandFeatures(syntheticHand([0, 60, 60, 0, 0]));
    const left = singleHandFeatures(mirror(syntheticHand([0, 60, 60, 0, 0])));
    expect(close(right.distances, left.distances)).toBe(true);
    expect(close(right.angles, left.angles)).toBe(true);
  });

  it('measures a straight finger as ~180° and a bent one as less', () => {
    expect(angleAt({ x: 0, y: 0, z: 0 }, { x: 1, y: 0, z: 0 }, { x: 2, y: 0, z: 0 })).toBeCloseTo(180);
    const straight = singleHandFeatures(syntheticHand([0, 0, 0, 0, 0])).angles[4]; // índice, articulación media
    const bent = singleHandFeatures(syntheticHand([0, 70, 0, 0, 0])).angles[4];
    expect(straight).toBeGreaterThan(bent);
  });

  it('orders two hands left first and averages samples', () => {
    const two = handFeatures([hand(syntheticHand([0, 0, 0, 0, 0]), 'Right'), hand(syntheticHand([60, 60, 60, 60, 60]), 'Left')]);
    const leftOnly = handFeatures([hand(syntheticHand([60, 60, 60, 60, 60]), 'Left')]);
    expect(two?.handCount).toBe(2);
    expect(two?.distances.slice(0, DISTANCES_PER_HAND)).toEqual(leftOnly?.distances);

    const avg = averageFeatures([leftOnly!, leftOnly!]);
    expect(close(avg.angles, leftOnly!.angles)).toBe(true);
  });

  it('uses world landmarks so a 16:9 image does not distort the geometry', () => {
    const real = syntheticHand([0, 70, 70, 0, 0]);
    const stretched = real.map((p) => ({ ...p, x: p.x * (9 / 16) })); // x normalizada por el ancho
    const fromImage = handFeatures([hand(stretched)])!;
    const fromWorld = handFeatures([{ side: 'Right', landmarks: stretched, world: real }])!;
    const expected = handFeatures([hand(real)])!;

    expect(close(fromWorld.angles, expected.angles)).toBe(true);
    expect(close(fromImage.angles, expected.angles)).toBe(false);
  });

  it('returns null without hands', () => {
    expect(handFeatures([])).toBeNull();
  });
});
