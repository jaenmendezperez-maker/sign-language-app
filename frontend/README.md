# frontend · Angular

Captura la cámara, detecta las manos con **MediaPipe Hands en el navegador** y reconoce la seña
comparando distancias y ángulos contra el catálogo de referencias (sin IA propia). Al backend solo
llegan la palabra, el puntaje y el nivel de certeza; el video nunca sale del equipo.

```bash
npm install
npm start          # http://localhost:4200 (necesita el backend en http://localhost:5090)
npm test           # pruebas unitarias (Vitest)
npm run build
```

| Carpeta | Contenido |
|---|---|
| `src/app/recognition/` | Algoritmo por reglas: `hand-features` (12 distancias + 15 ángulos por mano), `similarity` (puntaje 0–100), `certainty` (semáforo), `sign-detector` (cuándo se completa una seña). TypeScript puro, con pruebas. |
| `src/app/core/` | Servicios (API, MediaPipe, voz, sesión), interceptores y guard por rol. |
| `src/app/features/` | Pantallas: `translate` (consulta), `vocabulary`, `admin` (registrar seña), `consultations`, `login`. |
| `src/app/shared/` | Cámara con dibujo de landmarks, semáforo, dibujo de la mano, pipe de puntaje. |

- Las tolerancias del algoritmo están en `src/app/recognition/similarity.ts` (`SIMILARITY`); se ajustan con las pruebas de precisión (RNF-03).
- Los umbrales del semáforo vienen del backend (`Certainty` en `appsettings.json`).
- El wasm de MediaPipe se copia desde `node_modules` al compilar; el modelo `hand_landmarker.task` se descarga de `storage.googleapis.com/mediapipe-models` la primera vez.
