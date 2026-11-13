/**
 * Genera las señas de DEMOSTRACIÓN que el backend siembra en desarrollo.
 *
 * Son posturas simples de la mano construidas con proporciones anatómicas aproximadas,
 * NO señas oficiales de LSM. Sirven para probar el sistema sin registrar nada; las señas
 * reales se registran con la cámara (Administrador → Registrar seña).
 *
 * Usa exactamente el mismo cálculo de características que el reconocedor del navegador.
 *
 *   npm run demo-signs
 *
 * Salida: SignLanguageApp.backend/SignLanguageApp.Infrastructure/Persistence/Seed/demo-signs.json
 */
import { writeFileSync } from 'node:fs';
import { resolve } from 'node:path';

import { DetectedHand, handFeatures, HandFeatures, Point3 } from '../src/app/recognition/hand-features';
import { similarity } from '../src/app/recognition/similarity';

type Flex = [number, number, number]; // grados de flexión en MCP, PIP, DIP (pulgar: CMC, MCP, IP)

interface Pose {
  word: string;
  description: string;
  fingers: [thumb: Flex, index: Flex, middle: Flex, ring: Flex, pinky: Flex];
  /** Separación extra de índice y medio (seña en V). */
  spread?: number;
}

const OPEN: Flex = [5, 5, 5];
const CURLED: Flex = [85, 100, 65];
const THUMB_OUT: Flex = [0, 5, 5];
const THUMB_TUCKED: Flex = [55, 55, 45];

const POSES: Pose[] = [
  { word: 'ayuda', description: 'mano abierta, cinco dedos extendidos', fingers: [THUMB_OUT, OPEN, OPEN, OPEN, OPEN] },
  { word: 'no', description: 'puño cerrado', fingers: [THUMB_TUCKED, CURLED, CURLED, CURLED, CURLED] },
  { word: 'sí', description: 'pulgar arriba, demás dedos cerrados', fingers: [THUMB_OUT, CURLED, CURLED, CURLED, CURLED] },
  { word: 'dolor', description: 'índice extendido (señalar)', fingers: [THUMB_TUCKED, OPEN, CURLED, CURLED, CURLED] },
  { word: 'bien', description: 'índice y medio en V', fingers: [THUMB_TUCKED, OPEN, OPEN, CURLED, CURLED], spread: 12 },
  { word: 'llamar', description: 'pulgar y meñique extendidos (teléfono)', fingers: [THUMB_OUT, CURLED, CURLED, CURLED, OPEN] },
];

// Proporciones aproximadas de la mano, en unidades de "muñeca → base del dedo medio" = 1.
// base: ángulo de la base del dedo respecto a la vertical (+ hacia el meñique); dist: muñeca → base.
const FINGERS = [
  { base: -48, dist: 0.3, segs: [0.33, 0.28, 0.23] }, // pulgar (desde CMC)
  { base: -14, dist: 0.93, segs: [0.43, 0.25, 0.21] }, // índice
  { base: 0, dist: 1.0, segs: [0.47, 0.29, 0.23] }, // medio
  { base: 13, dist: 0.94, segs: [0.44, 0.27, 0.22] }, // anular
  { base: 27, dist: 0.84, segs: [0.34, 0.2, 0.19] }, // meñique
];

const rad = (d: number) => (d * Math.PI) / 180;
const add = (a: Point3, b: Point3, k = 1): Point3 => ({ x: a.x + b.x * k, y: a.y + b.y * k, z: a.z + b.z * k });
const unit = (v: Point3): Point3 => {
  const n = Math.hypot(v.x, v.y, v.z);
  return { x: v.x / n, y: v.y / n, z: v.z / n };
};

/**
 * 21 puntos en 3D (y hacia abajo, como MediaPipe). Cada dedo se flexiona en el plano formado
 * por su dirección y la normal de la palma (hacia la cámara), como una mano real; el pulgar se
 * flexiona cruzando la palma.
 */
function buildHand(pose: Pose): Point3[] {
  const pts: Point3[] = Array.from({ length: 21 }, () => ({ x: 0, y: 0, z: 0 }));
  const palmNormal: Point3 = { x: 0, y: 0, z: -1 };

  FINGERS.forEach((f, i) => {
    const spread = pose.spread && (i === 1 || i === 2) ? (i === 1 ? -pose.spread : pose.spread) : 0;
    const base = rad(f.base);
    const dir: Point3 = { x: Math.sin(rad(f.base + spread)), y: -Math.cos(rad(f.base + spread)), z: 0 };
    const bendTowards = i === 0 ? unit({ x: 0.8, y: 0, z: -0.6 }) : palmNormal; // el pulgar cruza la palma
    const first = 1 + i * 4; // 1, 5, 9, 13, 17

    let p: Point3 = { x: Math.sin(base) * f.dist, y: -Math.cos(base) * f.dist, z: 0 };
    pts[first] = p;
    let phi = 0;
    f.segs.forEach((len, s) => {
      phi += rad(pose.fingers[i][s]);
      const d = add({ x: dir.x * Math.cos(phi), y: dir.y * Math.cos(phi), z: 0 }, bendTowards, Math.sin(phi));
      p = add(p, d, len);
      pts[first + s + 1] = p;
    });
  });
  return pts;
}

const features = (pose: Pose): HandFeatures => {
  const hand: DetectedHand = { side: 'Right', landmarks: buildHand(pose), world: buildHand(pose) };
  return handFeatures([hand])!;
};

// ---------- generar y verificar que las posturas se distinguen entre sí ----------
const all = POSES.map((p) => ({ pose: p, f: features(p) }));
const round = (v: number[]) => v.map((x) => Math.round(x * 1e4) / 1e4);

console.log('Similitud entre posturas (debe ser alta solo en la diagonal):\n');
console.log('          ' + all.map((a) => a.pose.word.padStart(8)).join(''));
let worst = 0;
for (const a of all) {
  const row = all.map((b) => {
    const s = similarity(a.f, b.f);
    if (a !== b) worst = Math.max(worst, s);
    return s.toFixed(0).padStart(8);
  });
  console.log(a.pose.word.padEnd(10) + row.join(''));
}
console.log(`\nMayor similitud entre posturas distintas: ${worst.toFixed(1)}`);
if (worst >= 60) {
  console.error('Dos posturas quedarían en amarillo o verde entre sí: ajusta las posturas.');
  process.exit(1);
}

const output = all.map(({ pose, f }) => ({
  word: pose.word,
  description: pose.description,
  handCount: f.handCount,
  distances: round(f.distances),
  angles: round(f.angles),
  normalizedLandmarks: round(f.normalized),
  sampleCount: 5,
}));

// npm ejecuta el script desde frontend/
const target = resolve(process.cwd(), '../SignLanguageApp.backend/SignLanguageApp.Infrastructure/Persistence/Seed/demo-signs.json');
writeFileSync(target, JSON.stringify(output, null, 2) + '\n');
console.log(`\n${output.length} señas de demostración → ${target}`);
