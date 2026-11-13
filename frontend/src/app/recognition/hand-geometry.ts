/**
 * Geometría de la mano de MediaPipe Hands (21 puntos por mano).
 * Los tamaños deben coincidir con SignLanguageApp.Domain.ValueObjects.HandGeometry.
 *
 *  0 muñeca · 1-4 pulgar · 5-8 índice · 9-12 medio · 13-16 anular · 17-20 meñique
 */
export const LANDMARKS_PER_HAND = 21;
export const DISTANCES_PER_HAND = 12;
export const ANGLES_PER_HAND = 15;
export const COORDINATES_PER_HAND = LANDMARKS_PER_HAND * 3;

export const WRIST = 0;
export const MIDDLE_MCP = 9;
export const FINGERTIPS = [4, 8, 12, 16, 20] as const;

/** Cadenas de cada dedo, de la base a la punta (con la muñeca al inicio). */
export const FINGER_CHAINS = [
  [0, 1, 2, 3, 4], // pulgar
  [0, 5, 6, 7, 8], // índice
  [0, 9, 10, 11, 12], // medio
  [0, 13, 14, 15, 16], // anular
  [0, 17, 18, 19, 20], // meñique
] as const;

/** Pares de puntos cuyas distancias forman el vector (12 por mano). */
export const DISTANCE_PAIRS: ReadonlyArray<readonly [number, number]> = [
  ...FINGERTIPS.map((t) => [WRIST, t] as const), // 5: puntas → muñeca (qué tan extendido está cada dedo)
  [4, 8], [8, 12], [12, 16], [16, 20], // 4: entre puntas consecutivas (separación de los dedos)
  [4, 12], [4, 16], [4, 20], // 3: pulgar → demás puntas (contacto del pulgar)
];

/** Conexiones para dibujar el esqueleto de la mano. */
export const HAND_CONNECTIONS: ReadonlyArray<readonly [number, number]> = [
  ...FINGER_CHAINS.flatMap((chain) => chain.slice(1).map((p, i) => [chain[i], p] as const)),
  [5, 9], [9, 13], [13, 17], // palma
];
