import { Pipe, PipeTransform } from '@angular/core';

/** 72.456 → "72 %" */
@Pipe({ name: 'score' })
export class ScorePipe implements PipeTransform {
  transform(value: number | null | undefined): string {
    if (value == null || Number.isNaN(value)) return '—';
    return `${Math.round(Math.min(Math.max(value, 0), 100))} %`;
  }
}
