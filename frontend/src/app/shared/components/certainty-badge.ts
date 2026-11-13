import { Component, computed, input } from '@angular/core';

import { CertaintyLevel } from '../../models/consultation.model';
import { LEVEL_INFO } from '../../recognition/certainty';

/** Semáforo de certeza: color + icono + texto, para no depender solo del color (RF-06, RNF-08). */
@Component({
  selector: 'app-certainty-badge',
  template: `<span class="badge" [attr.data-level]="level()" [attr.title]="info().description">
    <span class="badge-icon" aria-hidden="true">{{ info().icon }}</span>{{ info().label }}
  </span>`,
})
export class CertaintyBadge {
  readonly level = input.required<CertaintyLevel>();
  protected readonly info = computed(() => LEVEL_INFO[this.level()]);
}
