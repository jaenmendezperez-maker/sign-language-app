"""Genera los diagramas de docs/ con Graphviz.

Uso:  python3 docs/generar_diagramas.py      (requiere `dot` y Pillow)

Salida: modelo-er.dot/.png/.svg y casos-de-uso.dot/.png/.svg
El modelo ER se define una sola vez aquí (ENTIDADES / RELACIONES) y debe coincidir
con las configuraciones de EF Core en SignLanguageApp.Infrastructure/Persistence.
"""

import subprocess
from pathlib import Path

from PIL import Image, ImageDraw

DOCS = Path(__file__).resolve().parent
AZUL = "#1F4E79"
GRIS = "#5c6670"

# ------------------------------------------------------------------ Modelo ER
# (clave, columna, tipo)  clave: PK, FK, UQ, CK (restricción CHECK) o ""
ENTIDADES: dict[str, list[tuple[str, str, str]]] = {
    "USUARIO": [
        ("PK", "id_usuario", "uuid"),
        ("", "nombre", "varchar(120)"),
        ("UQ", "correo", "varchar(256)"),
        ("", "hash_contrasena", "text"),
        ("CK", "rol", "varchar(20)"),
        ("", "activo", "boolean"),
        ("", "fecha_registro", "timestamptz"),
    ],
    "CATEGORIA": [
        ("PK", "id_categoria", "integer"),
        ("UQ", "nombre", "varchar(60)"),
        ("", "descripcion", "varchar(250)"),
    ],
    "SENA": [
        ("PK", "id_sena", "uuid"),
        ("UQ", "palabra", "varchar(60)"),
        ("FK", "id_categoria", "integer"),
        ("CK", "num_manos", "smallint"),
        ("", "recurso_visual_url", "varchar(500) NULL"),
        ("", "activa", "boolean"),
        ("", "fecha_registro", "timestamptz"),
        ("FK", "id_admin_registro", "uuid"),
    ],
    "REFERENCIA_GEOMETRICA": [
        ("PK", "id_referencia", "uuid"),
        ("FK", "id_sena", "uuid"),
        ("", "distancias", "jsonb"),
        ("", "angulos", "jsonb"),
        ("", "puntos_normalizados", "jsonb"),
        ("CK", "num_muestras", "smallint"),
        ("", "fecha_captura", "timestamptz"),
        ("FK", "id_admin", "uuid"),
    ],
    "CONSULTA": [
        ("PK", "id_consulta", "uuid"),
        ("FK", "id_medico", "uuid"),
        ("", "fecha_consulta", "date"),
        ("", "hora_inicio", "timestamptz"),
        ("", "hora_fin", "timestamptz NULL"),
        ("CK", "estado", "varchar(20)"),
    ],
    "TRADUCCION_REGISTRADA": [
        ("PK", "id_traduccion", "uuid"),
        ("FK", "id_consulta", "uuid"),
        ("FK", "id_sena", "uuid NULL"),
        ("", "palabra", "varchar(60) NULL"),
        ("", "hora_exacta", "timestamptz"),
        ("CK", "puntaje_similitud", "numeric(5,2)"),
        ("CK", "nivel_certeza", "varchar(10)"),
        ("CK", "estado", "varchar(20)"),
    ],
}

# Extremos en notación pata de gallo (nombres de flecha de Graphviz)
UNO, CERO_O_UNO, CERO_O_MUCHOS, UNO_O_MUCHOS = "teetee", "teeodot", "crowodot", "crowtee"

# (desde, hacia, extremo en "desde", extremo en "hacia", verbo)
RELACIONES = [
    ("USUARIO", "CONSULTA", UNO, CERO_O_MUCHOS, "atiende"),
    ("CONSULTA", "TRADUCCION_REGISTRADA", UNO, CERO_O_MUCHOS, "contiene"),
    ("CATEGORIA", "SENA", UNO, CERO_O_MUCHOS, "agrupa"),
    ("SENA", "REFERENCIA_GEOMETRICA", UNO, UNO_O_MUCHOS, "se describe por"),
    ("SENA", "TRADUCCION_REGISTRADA", CERO_O_UNO, CERO_O_MUCHOS, "se reconoce como"),
    ("USUARIO", "SENA", UNO, CERO_O_MUCHOS, "registra"),
    ("USUARIO", "REFERENCIA_GEOMETRICA", UNO, CERO_O_MUCHOS, "captura"),
]


def _tabla(nombre: str, columnas: list[tuple[str, str, str]]) -> str:
    filas = []
    for clave, col, tipo in columnas:
        if clave == "PK":
            texto = f"<B><U>{col}</U></B>"
        elif clave == "FK":
            texto = f"<I>{col}</I>"
        else:
            texto = col
        etiqueta = f'<FONT POINT-SIZE="9" COLOR="{GRIS}">{clave}</FONT>' if clave else " "
        filas.append(
            f'<TR><TD ALIGN="LEFT" WIDTH="30">{etiqueta}</TD>'
            f'<TD ALIGN="LEFT">{texto}</TD>'
            f'<TD ALIGN="LEFT"><FONT COLOR="{GRIS}">{tipo}</FONT></TD></TR>'
        )
    return (
        '<<TABLE BORDER="1" CELLBORDER="0" CELLSPACING="0" CELLPADDING="4" COLOR="#8a96a3">'
        f'<TR><TD COLSPAN="3" BGCOLOR="{AZUL}"><FONT COLOR="white"><B>{nombre}</B></FONT></TD></TR>'
        + "".join(filas)
        + "</TABLE>>"
    )


def modelo_er() -> str:
    lineas = [
        "digraph ModeloER {",
        '  graph [rankdir=LR, nodesep=0.6, ranksep=1.2, fontname="Helvetica", bgcolor="white", pad=0.3];',
        '  node [shape=plain, fontname="Helvetica", fontsize=11];',
        '  edge [dir=both, color="#33414e", fontname="Helvetica", fontsize=10, fontcolor="#33414e", penwidth=1.2];',
    ]
    lineas += [f"  {n} [label={_tabla(n, c)}];" for n, c in ENTIDADES.items()]
    lineas += [
        f'  {a} -> {b} [arrowtail={ta}, arrowhead={hb}, label="  {verbo}  "];'
        for a, b, ta, hb, verbo in RELACIONES
    ]
    lineas += ["  { rank=same; USUARIO; CATEGORIA; }", "  { rank=same; CONSULTA; SENA; }", "}"]
    return "\n".join(lineas) + "\n"


# ------------------------------------------------------------ Casos de uso
ACTORES = {
    "paciente": "Paciente sordo",
    "medico": "Personal médico",
    "admin": "Administrador",
}

CASOS = {
    "CU01": "CU-01\\nIniciar sesión",
    "CU02": "CU-02\\nIniciar consulta\\nde traducción",
    "CU03": "CU-03\\nPausar / reanudar\\nconsulta",
    "CU04": "CU-04\\nFinalizar consulta",
    "CU05": "CU-05\\nTraducir seña",
    "CU06": "CU-06\\nSolicitar repetición\\nde seña",
    "CU07": "CU-07\\nConfirmar o descartar\\npalabra",
    "CU08": "CU-08\\nEscuchar traducción\\npor voz",
    "CU09": "CU-09\\nAvisar mano\\nno detectada",
    "CU10": "CU-10\\nConsultar / limpiar\\nhistorial",
    "CU11": "CU-11\\nExportar resumen",
    "CU12": "CU-12\\nConsultar vocabulario",
    "CU13": "CU-13\\nRegistrar referencia\\ngeométrica",
}

# Solo asociaciones con casos base: un caso incluido o de extensión no necesita
# asociación directa, el actor participa a través del caso base (UML 2.5, §18.1.3).
ASOCIACIONES = [
    ("paciente", "CU05"), ("paciente", "CU12"),
    ("medico", "CU02"), ("medico", "CU03"), ("medico", "CU04"), ("medico", "CU05"),
    ("medico", "CU10"), ("medico", "CU12"),
    ("admin", "CU13"),
]

# (caso base, caso relacionado, tipo, condición)  la flecha va del relacionado al base en extend
RELACIONES_CU = [
    ("CU05", "CU06", "extend", "certeza roja"),
    ("CU05", "CU07", "extend", "certeza amarilla"),
    ("CU05", "CU08", "extend", "voz activada"),
    ("CU05", "CU09", "extend", "> 3 s sin manos"),
    ("CU04", "CU11", "extend", "el médico lo pide"),
    ("CU13", "CU01", "include", ""),
    ("CU02", "CU01", "include", ""),
]


def _icono_actor(ruta: Path) -> None:
    """Monigote UML dibujado con Pillow (Graphviz no trae uno)."""
    w, h, s = 60, 90, 4  # se dibuja a 4x y se reduce para suavizar
    img = Image.new("RGBA", (w * s, h * s), (255, 255, 255, 0))
    d = ImageDraw.Draw(img)
    c, lw = (31, 78, 121, 255), 3 * s
    cx = w * s // 2
    d.ellipse([cx - 11 * s, 4 * s, cx + 11 * s, 26 * s], outline=c, width=lw)
    d.line([cx, 26 * s, cx, 58 * s], fill=c, width=lw)
    d.line([cx - 22 * s, 38 * s, cx + 22 * s, 38 * s], fill=c, width=lw)
    d.line([cx, 58 * s, cx - 18 * s, 86 * s], fill=c, width=lw)
    d.line([cx, 58 * s, cx + 18 * s, 86 * s], fill=c, width=lw)
    img.resize((w, h), Image.LANCZOS).save(ruta)


def casos_de_uso(icono: str) -> str:
    lineas = [
        "digraph CasosDeUso {",
        '  graph [rankdir=LR, nodesep=0.25, ranksep=0.9, fontname="Helvetica", bgcolor="white", pad=0.3];',
        '  node [fontname="Helvetica", fontsize=11];',
        '  edge [color="#33414e", arrowhead=none, fontname="Helvetica", fontsize=9, fontcolor="#33414e"];',
    ]
    for clave, nombre in ACTORES.items():
        lineas.append(
            f'  {clave} [shape=none, image="{icono}", label="{nombre}", labelloc=b, '
            'height=1.25, width=1.5, imagepos=tc, fixedsize=true];'
        )
    lineas.append(
        '  subgraph cluster_sistema { label="SignLink"; labeljust=l; fontsize=14; fontcolor="#1F4E79"; '
        'style=rounded; color="#1F4E79"; penwidth=1.5;'
    )
    for clave, nombre in CASOS.items():
        lineas.append(
            f'    {clave} [shape=ellipse, style=filled, fillcolor="#e6ecf5", color="#1F4E79", label="{nombre}"];'
        )
    lineas.append("  }")
    lineas += [f"  {a} -> {c};" for a, c in ASOCIACIONES]
    for base, rel, tipo, cond in RELACIONES_CU:
        etiqueta = f"«{tipo}»" + (f"\\n[{cond}]" if cond else "")
        if tipo == "extend":
            # Se dibuja base -> extensión (para que la extensión quede en la columna siguiente)
            # pero con la punta de flecha en el caso base, como indica UML.
            lineas.append(f'  {base} -> {rel} [style=dashed, dir=back, arrowtail=vee, label="{etiqueta}"];')
        else:
            lineas.append(f'  {base} -> {rel} [style=dashed, arrowhead=vee, label="{etiqueta}"];')
    lineas.append("}")
    return "\n".join(lineas) + "\n"


def _render(nombre: str, fuente: str) -> None:
    dot = DOCS / f"{nombre}.dot"
    dot.write_text(fuente, encoding="utf-8")
    for fmt, extra in (("png", ["-Gdpi=170"]), ("svg", [])):
        subprocess.run(["dot", f"-T{fmt}", *extra, str(dot), "-o", str(DOCS / f"{nombre}.{fmt}")],
                       check=True, cwd=DOCS)


if __name__ == "__main__":
    _icono_actor(DOCS / "actor.png")
    _render("modelo-er", modelo_er())
    _render("casos-de-uso", casos_de_uso("actor.png"))
    print("Diagramas generados en", DOCS)
