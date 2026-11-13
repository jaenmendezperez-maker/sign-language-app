import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { firstValueFrom } from 'rxjs';

import { environment } from '../../../environments/environment';
import { Category, RegisterReferenceRequest, Sign } from '../../models/catalog.model';

@Injectable({ providedIn: 'root' })
export class CatalogApiService {
  private readonly http = inject(HttpClient);
  private readonly api = `${environment.apiUrl}/api`;

  categories(): Promise<Category[]> {
    return firstValueFrom(this.http.get<Category[]>(`${this.api}/categories`));
  }

  /** Mismo catálogo para el reconocedor y para la pantalla de vocabulario (RF-10). */
  catalog(categoryId?: number): Promise<Sign[]> {
    const params: Record<string, number> = categoryId ? { categoryId } : {};
    return firstValueFrom(this.http.get<Sign[]>(`${this.api}/catalog`, { params }));
  }

  registerReference(request: RegisterReferenceRequest): Promise<Sign> {
    return firstValueFrom(this.http.post<Sign>(`${this.api}/catalog/references`, request));
  }
}
