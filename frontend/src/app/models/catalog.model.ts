export interface Category {
  id: number;
  name: string;
  description: string | null;
}

export interface GeometricReference {
  id: string;
  distances: number[];
  angles: number[];
  /** 21 puntos (x, y, z) por mano, normalizados; para dibujar la mano en el catálogo. */
  normalizedLandmarks: number[];
  sampleCount: number;
  capturedAt: string;
}

export interface Sign {
  id: string;
  word: string;
  categoryId: number;
  categoryName: string;
  handCount: 1 | 2;
  visualUrl: string | null;
  references: GeometricReference[];
}

export interface RegisterReferenceRequest {
  word: string;
  categoryId: number;
  handCount: 1 | 2;
  visualUrl: string | null;
  distances: number[];
  angles: number[];
  normalizedLandmarks: number[];
  sampleCount: number;
}
