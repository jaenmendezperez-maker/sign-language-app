import { ScorePipe } from './score.pipe';

describe('ScorePipe', () => {
  const pipe = new ScorePipe();
  it('formats the 0–100 score', () => expect(pipe.transform(72.456)).toBe('72 %'));
  it('clamps', () => expect(pipe.transform(130)).toBe('100 %'));
  it('dash without value', () => expect(pipe.transform(null)).toBe('—'));
});
