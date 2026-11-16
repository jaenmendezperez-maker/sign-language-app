# Dataset de señas LSM para consulta médica

Guía para armar el dataset de señas que reconoce la app. Explica el objetivo, cómo guarda la app las señas, qué datasets públicos existen y qué falta grabar.

## 1. Objetivo

La app ayuda al personal médico a atender a pacientes sordos que usan **Lengua de Señas Mexicana (LSM)**. El paciente hace una seña frente a la cámara y la app muestra la palabra en español. Por eso el vocabulario es de consulta: síntomas, partes del cuerpo, urgencias y respuestas cortas, no conversación general.

## 2. Lo mínimo que hay que saber de la LSM

- **No es español con las manos.** Tiene vocabulario y gramática propios. Casi todo se comunica con **señas por palabra o concepto**. Deletrear letra por letra (*dactilología*) solo se usa para nombres propios o palabras sin seña.
- **No hay una seña por frase.** Las frases se arman combinando señas, más el espacio y la expresión facial (por ejemplo, para negar se frunce el ceño mientras se gira la cabeza). La app trabaja con **señas aisladas**, una palabra a la vez.
- **Cada seña tiene 5 partes:** forma de la mano, ubicación, movimiento, orientación y rasgos no manuales (la cara). Ejemplos:
  - *vino* y *whisky* solo cambian en la forma de la mano;
  - *él/ella* y *ellos* solo cambian en el movimiento.
- **Hay variación regional.** Una misma palabra puede hacerse distinto según la región. Conviene validar las señas con una persona sorda o un intérprete de la zona.

Fuentes: [fonética de la LSM](https://pressbooks.pub/linghispintro/chapter/la-fonetica-de-la-lengua-de-las-senas-mexicanas), [rasgos no manuales (Redalyc)](https://www.redalyc.org/pdf/5119/511951372005.pdf), [Diccionario LSM CDMX (PDF)](https://pdh.cdmx.gob.mx/storage/app/media/banner/Dic_LSM%202.pdf), [Wikipedia: Mexican Sign Language](https://en.wikipedia.org/wiki/Mexican_Sign_Language).

## 3. Cómo guarda y reconoce la app una seña

Hay que entender esto antes de buscar datos, porque **limita qué señas puede reconocer la app**.

- La cámara usa **MediaPipe HandLandmarker**, que da **21 puntos por mano** con coordenadas x, y, z ([hand-tracker.service.ts](../frontend/src/app/core/services/hand-tracker.service.ts)).
- De esos puntos se calculan **12 distancias y 15 ángulos por mano**, y se guardan las 63 coordenadas normalizadas ([hand-geometry.ts](../frontend/src/app/recognition/hand-geometry.ts), [hand-features.ts](../frontend/src/app/recognition/hand-features.ts)).
- El administrador registra cada seña con **5 capturas** desde la pantalla *Registrar seña* ([register-sign.ts](../frontend/src/app/features/admin/register-sign.ts)). Al reconocer, la app compara lo que ve la cámara contra esa referencia por similitud.
- **Cada captura es una postura fija, no un movimiento.** Por eso funcionan bien las señas que son una forma de la mano quieta (la mayoría de las letras y los números). Las señas cuyo significado depende del movimiento (*tos*, *dolor*, *respirar*) pueden confundirse entre sí. Lo verás en las pruebas del paso 4 del plan (sección 7).
- Formato de las señas de demostración, útil si se importa algo por código: [demo-signs.json](../SignLanguageApp.backend/SignLanguageApp.Infrastructure/Persistence/Seed/demo-signs.json) y [DevelopmentSeeder.cs](../SignLanguageApp.backend/SignLanguageApp.Infrastructure/Persistence/DevelopmentSeeder.cs).

### Categorías que ya existen en la app

Definidas en [DevelopmentSeeder.cs](../SignLanguageApp.backend/SignLanguageApp.Infrastructure/Persistence/DevelopmentSeeder.cs):

| Categoría | Ejemplos previstos |
|---|---|
| Síntomas | dolor, fiebre, mareo… |
| Urgencias | ayuda, emergencia, ambulancia… |
| Respuestas | sí, no, gracias… |
| Partes del cuerpo | cabeza, pecho, estómago… |
| Lugares | hospital, farmacia, baño… |

## 4. Datasets públicos encontrados

### 4.1 Vocabulario médico (lo más relevante)

| Dataset | Contenido | Personas que señan | Formato | Licencia |
|---|---|---|---|---|
| **MSL-150** · [GitHub](https://github.com/armandobecerril/MSL-150-Dataset) · [Zenodo](https://zenodo.org/records/17783312) · [paper (Sensors 2026)](https://www.mdpi.com/1424-8220/26/1/27) | 150 señas aisladas: **47 médicas**, 19 partes del cuerpo, números del 1 al 10, tiempo, preguntas, verbos | 1, nativa (800 secuencias por seña: 1 original + 799 alteradas por software) | Solo puntos de **MediaPipe Holistic**: 30 cuadros × 226 valores (mano derecha 63, mano izquierda 63, cuerpo 100). CSV de 13.9 GB o npy de 4.4 GB | CC BY 4.0 para los puntos. **Los videos no se pueden reutilizar** |
| **LSM Dynamic Signs (121 señas)** · [Zenodo](https://zenodo.org/records/18330565) · [paper (Frontiers 2026)](https://www.frontiersin.org/journals/artificial-intelligence/articles/10.3389/frai.2026.1794923/full) · [código](https://github.com/XxANTONIO73xX/LSM-DynamicSigns-DeepLearning) | 121 señas de emergencias, accidentes, síntomas y cortesía (DOLOR, AYUDA, HOSPITAL, OPRESIÓN_EN_PECHO, RESPIRAR, TEMBLOR, INFECCIÓN, FARMACIA…). Salieron de 110 frases definidas por intérpretes | 12: 1 intérprete, 1 persona sorda y 10 personas sin experiencia que imitan | Video .mp4 en HD, un archivo por seña y persona (.7z de 2.7 GB) | CC BY 4.0 |

**Vocabulario médico de MSL-150 (47):** ambulancia, jarabe, virus, aborto, accidente, hospital, doctor, enfermera, paciente, dolor, enfermo, terapia, pastillas, inyección, contagiar, revisar, calentura, cáncer, infección, infarto, lesión, embarazo, sangre, gripa, garganta, tos, débil, huesos, farmacia, emergencia, inflamación, análisis, coronavirus, cita, fractura, urgencia, orina, popó, mareo, vómito, convulsiones, gases, diarrea, moco, sed.

**Partes del cuerpo de MSL-150 (19):** ojo, nariz, oreja, boca, cuello, hombro, espalda, brazo, codo, muñeca, mano, panza, cintura, pene, vagina, piernas, rodilla, tobillo, pie.

**Otras de MSL-150 útiles en consulta:**
- básicas: sí, no, bien, mal, duda, pregunta;
- adjetivos: frío, caliente, mejor, peor, débil, cansado, estresado;
- tiempo: ayer, hoy, ahora, mañana, siempre, nunca, diario;
- números del 1 al 10;
- preguntas: cómo, cuántos, por qué, para qué;
- personas: mamá, papá, esposo, esposa, hijo, hija;
- verbos: comer, beber, dormir, caminar…

### 4.2 Vocabulario cotidiano

| Dataset | Contenido | Personas que señan | Formato | Licencia |
|---|---|---|---|---|
| **22 señas** · [Zenodo](https://zenodo.org/records/23744039) | ahora, ayudar, bien, casa, comer, cómo, cuándo, dónde, él, gracias, hola, hoy, ir, mal, no, perdón, querer, saber, sí, tener, tú, yo | 5 | Puntos .npy de MediaPipe Hand Landmarker + Pose (45 MB) | CC BY 4.0 |

### 4.3 Alfabeto (dactilología): ya estaban guardados en el equipo

| Dataset | Contenido | Formato | Licencia |
|---|---|---|---|
| [Kaggle: LSM ~100,000 fotos](https://www.kaggle.com/datasets/osvalmontesinos/lengua-de-seas-mexicana-100000-fotos) | Alfabeto: 27 letras + 2 señas especiales (lo usa Dactimex) | Imágenes | Revisar en Kaggle |
| [Dactimex (M0ndares/dactilologiaMexicana)](https://github.com/M0ndares/dactilologiaMexicana) | App que reconoce el alfabeto con la cámara: MediaPipe recorta la mano y un modelo EfficientNetB2 clasifica (~97.9 % de exactitud en validación). Usa el dataset de Kaggle | Código Flask + TFLite | Sin licencia publicada |
| [smams (anthonyibrail)](https://github.com/anthonyibrail/smams/tree/main/dataSet) | Alfabeto A–Z + clase `0`, separado en entrenamiento y prueba (~300 imágenes por letra). Red neuronal convolucional con 82 % de exactitud | Imágenes .jpg | Sin licencia publicada |
| [Alfabeto LSM estático 3D (PMC)](https://pmc.ncbi.nlm.nih.gov/articles/PMC13241763) | Alfabeto con 67 puntos 3D de MediaPipe | Puntos | Revisar en el artículo |

El alfabeto sirve de respaldo: si el paciente deletrea su nombre o una palabra que no está en el catálogo. Además son posturas fijas, que es justo lo que la app reconoce mejor.

> Los dos repos de GitHub **no tienen licencia**. Úsalos solo como referencia o para pruebas internas, no para redistribuir sus imágenes.

### 4.4 Otras referencias

- [Diccionario LSM CDMX (PDF)](https://pdh.cdmx.gob.mx/storage/app/media/banner/Dic_LSM%202.pdf): organizado por forma de la mano, con secciones de dactilología y expresión facial. Sirve para confirmar cómo se hace una seña.
- [Interpretación de LSM en tiempo real con CNN y HMM (IPN)](https://rcs.cic.ipn.mx/2021_150_6/Interpretacion%20en%20tiempo%20real%20de%20lengua%20de%20senas%20mexicana%20con%20CNN%20y%20HMM.pdf): ejemplo de cómo reconocer señas con movimiento.
- [Gaceta UNAM: LSM en medicina](https://www.gaceta.unam.mx/lengua-de-senas-util-e-inclusiva-en-medicina/): contexto de por qué hace falta.
- [Generalización entre personas que señan en LSM (MDPI 2026)](https://www.mdpi.com/2076-3417/16/22/11248): explica por qué un modelo entrenado con una persona falla con otras.

No encontré un glosario oficial de salud en LSM del IMSS, la Secretaría de Salud o CONADIS. Si alguien lo encuentra, se agrega aquí.

## 5. Cobertura de nuestras categorías

| Categoría | Ya disponible (fuente) | **Falta, hay que grabarlo** |
|---|---|---|
| Síntomas | dolor, calentura, tos, mareo, vómito, diarrea, convulsiones, gripa, débil, inflamación, sangre, sed, moco, gases, infección (MSL-150); opresión en el pecho, respirar, temblor (121) | comezón, ardor, náuseas*, cansancio (cansado está en MSL-150) |
| Urgencias | ambulancia, emergencia, urgencia, accidente, infarto, fractura, lesión (MSL-150); ayuda (121, 22) | — |
| Respuestas | sí, no, bien, mal (MSL-150, 22); gracias, perdón (22); bueno (121) | más o menos, no sé (saber está en 22) |
| Partes del cuerpo | 19 de MSL-150 + garganta, huesos | **cabeza, pecho, corazón, estómago** (panza puede servir) |
| Lugares | hospital, farmacia (MSL-150); casa (22) | **baño** |

\* Revisar si "náuseas" y "vómito" son la misma seña en la región.

**Categorías que conviene agregar** (el vocabulario ya existe en MSL-150):
- **Tiempo**, para "¿desde cuándo?": ayer, hoy, ahora, siempre, diario.
- **Intensidad / números**, para "del 1 al 10, ¿cuánto duele?": números del 1 al 10, mejor, peor.
- **Preguntas**: cómo, cuántos, por qué.

## 6. Problemas de los datasets

1. **Tienen movimiento y la app no.** MSL-150, el de 121 y el de 22 son secuencias. Para usarlos con la app hay que tomar un cuadro representativo, o varios, como posturas fijas. En señas que dependen del movimiento eso pierde información.
2. **Formato distinto pero compatible.** MSL-150 trae los 21 puntos por mano en el mismo orden que MediaPipe HandLandmarker (el que usa la app). Hay que extraer las columnas de la mano, quitar los cuadros con ceros (mano no detectada) y pasarlas por el mismo cálculo de distancias y ángulos de [hand-features.ts](../frontend/src/app/recognition/hand-features.ts).
3. **Una sola persona en MSL-150.** Las 800 muestras salen de una persona con alteraciones por software. Sirve para saber cómo es la seña, pero no garantiza que funcione con otras personas.
4. **Personas sin experiencia en el dataset de 121.** 10 de los 12 participantes imitan a los expertos. Para referencia, usa a los sujetos 0 (intérprete) y 11 (persona sorda).
5. **Licencias.** Atribuir los datasets CC BY 4.0 en el README. No redistribuir los videos de MSL-150 ni las imágenes de los repos sin licencia.

## 7. Plan de trabajo propuesto

1. **Definir la lista final de palabras** (unas 40–60 para empezar): sección 5 + lo que el médico necesite preguntar. Priorizar dolor, partes del cuerpo y sí/no.
2. **Descargar MSL-150** (el zip npy es más ligero que el CSV) y el **dataset de 121** (videos). Con ellos se aprende cómo se hace cada seña y se revisa la lista completa de 121.
3. **Validar las señas** con una persona sorda o un intérprete: confirmar que la forma sea la que se usa en nuestra región.
4. **Prueba piloto con la app:** registrar unas 10 señas médicas con *Registrar seña* (5 capturas cada una) y medir cuántas reconoce bien y cuáles se confunden. Esto dice si basta con posturas fijas o si hace falta reconocer movimiento.
5. **Grabar lo que falta** (cabeza, pecho, corazón, baño…), con **al menos 3 personas distintas**: distinta mano, tono de piel e iluminación.
6. **(Opcional) Importar por código** desde MSL-150 en lugar de grabar todo a mano. Se haría un script que convierta los puntos de la mano al formato de la referencia (distancias, ángulos, puntos normalizados) para poblar la base.
7. **Documentar** en una tabla cada seña: palabra, categoría, fuente (dataset o grabación propia), una o dos manos, si tiene movimiento (sí/no) y resultado de la prueba.

## 8. Entregables

- [ ] Lista final de palabras por categoría (CSV o tabla en este documento).
- [ ] Señas validadas por una persona sorda o un intérprete.
- [ ] Señas registradas en la app (o script de importación).
- [ ] Resultados de la prueba piloto: exactitud por seña y pares que se confunden.
- [ ] Atribución de los datasets usados en el README.
