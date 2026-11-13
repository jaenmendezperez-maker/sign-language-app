import { Component, computed, inject, OnInit, signal } from '@angular/core';

import { CatalogApiService } from '../../core/services/catalog-api.service';
import { Category, Sign } from '../../models/catalog.model';
import { HandSketch } from '../../shared/components/hand-sketch';

/** CU-12 Consultar vocabulario soportado (sin login), filtrable por categoría. */
@Component({
  selector: 'app-vocabulary',
  imports: [HandSketch],
  templateUrl: './vocabulary.html',
})
export class Vocabulary implements OnInit {
  private readonly api = inject(CatalogApiService);

  protected readonly categories = signal<Category[]>([]);
  protected readonly signs = signal<Sign[]>([]);
  protected readonly selected = signal<number | null>(null);
  protected readonly loading = signal(true);
  protected readonly error = signal<string | null>(null);

  protected readonly visible = computed(() => {
    const id = this.selected();
    return id === null ? this.signs() : this.signs().filter((s) => s.categoryId === id);
  });

  async ngOnInit(): Promise<void> {
    try {
      const [categories, signs] = await Promise.all([this.api.categories(), this.api.catalog()]);
      this.categories.set(categories);
      this.signs.set(signs);
    } catch (err) {
      this.error.set((err as Error).message);
    } finally {
      this.loading.set(false);
    }
  }

  protected isImage(url: string): boolean {
    return /\.(png|jpe?g|gif|webp|svg)(\?.*)?$/i.test(url);
  }
}
