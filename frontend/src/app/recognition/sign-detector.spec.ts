import { SignDetector } from './sign-detector';
import { hand, syntheticHand } from './test-hands';

const still = () => [hand(syntheticHand([0, 60, 60, 60, 60]))];
const movedTo = (dx: number) => [hand(syntheticHand([0, 60, 60, 60, 60], { dx }))];

describe('SignDetector', () => {
  it('emits once when the hand stays still for the required frames', () => {
    const d = new SignDetector({ stableFrames: 5 });
    const results = Array.from({ length: 12 }, () => d.push(still()));
    expect(results.filter(Boolean).length).toBe(1);
    expect(results[5]?.handCount).toBe(1); // primer cuadro fija la referencia + 5 quietos
  });

  it('does not emit while the hand keeps moving', () => {
    const d = new SignDetector({ stableFrames: 3 });
    const results = Array.from({ length: 10 }, (_, i) => d.push(movedTo(0.3 + i * 0.05)));
    expect(results.every((r) => r === null)).toBe(true);
  });

  it('can emit again after the hand moves', () => {
    const d = new SignDetector({ stableFrames: 3 });
    const emitted: unknown[] = [];
    for (let i = 0; i < 6; i++) emitted.push(d.push(movedTo(0.5)));
    emitted.push(d.push(movedTo(0.7))); // movimiento grande: nueva seña
    for (let i = 0; i < 6; i++) emitted.push(d.push(movedTo(0.7)));
    expect(emitted.filter(Boolean).length).toBe(2);
  });

  it('resets when the hands leave the frame', () => {
    const d = new SignDetector({ stableFrames: 3 });
    for (let i = 0; i < 6; i++) d.push(still());
    expect(d.push([])).toBeNull();
    const again = Array.from({ length: 6 }, () => d.push(still()));
    expect(again.filter(Boolean).length).toBe(1);
  });
});
