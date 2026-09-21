---
name: Simons Publicidad
description: Sistema editorial de alto contraste para mostrar el oficio de Simons y operar su ciclo comercial.
colors:
  primary-lime: "#b6ff27"
  primary-lime-soft: "#d9ff57"
  ink: "#10120f"
  paper: "#f4f5f1"
  public-paper: "#f6f7f2"
  surface: "#ffffff"
  text-muted: "#62675e"
  border: "#e4e6e0"
  danger: "#801d1d"
  hero-ink: "#050807"
  hero-ink-lit: "#071009"
  hero-lime: "#b8ff1a"
  hero-green: "#8bcf00"
typography:
  display:
    fontFamily: "Inter, Segoe UI, Arial, sans-serif"
    fontSize: "clamp(4rem, 10vw, 9rem)"
    fontWeight: 950
    lineHeight: 0.85
    letterSpacing: "-0.04em"
  headline:
    fontFamily: "Inter, Segoe UI, Arial, sans-serif"
    fontSize: "clamp(2.5rem, 6vw, 5rem)"
    fontWeight: 900
    lineHeight: 0.95
    letterSpacing: "-0.04em"
  body:
    fontFamily: "Inter, Segoe UI, Arial, sans-serif"
    fontSize: "1rem"
    fontWeight: 400
    lineHeight: 1.6
  label:
    fontFamily: "Inter, Segoe UI, Arial, sans-serif"
    fontSize: "0.75rem"
    fontWeight: 850
    lineHeight: 1.2
    letterSpacing: "0.12em"
rounded:
  none: "0"
  sm: "0.5rem"
  md: "0.8rem"
  lg: "1.1rem"
spacing:
  xs: "0.5rem"
  sm: "1rem"
  md: "1.5rem"
  lg: "2rem"
components:
  button-primary:
    backgroundColor: "{colors.ink}"
    textColor: "{colors.surface}"
    rounded: "{rounded.none}"
    padding: "0.9rem 1.1rem"
    height: "3.2rem"
  button-accent:
    backgroundColor: "{colors.primary-lime}"
    textColor: "{colors.ink}"
    rounded: "{rounded.none}"
    padding: "0.9rem 1.1rem"
    height: "3.2rem"
  input:
    backgroundColor: "{colors.surface}"
    textColor: "{colors.ink}"
    rounded: "{rounded.none}"
    padding: "0.8rem"
    height: "3.2rem"
---

# Design System: Simons Publicidad

## Overview

**Creative North Star: "El taller gráfico de alto contraste"**

La interfaz se comporta como una extensión digital del taller de Simons: directa, expresiva y enfocada en el resultado producido. Los titulares grandes y el contraste entre tinta negra, papel claro y verde lima crean reconocimiento inmediato. La fotografía real debe convertirse en la prueba principal del oficio, con el diseño actuando como marco.

La web pública tiene un modo persuasivo y editorial; el sistema interno conserva la misma identidad con una composición más sobria, densa y operativa. Colorisa aporta una referencia útil de protagonismo fotográfico y exploración del catálogo, pero no define la identidad de Simons.

**Key Characteristics:**

- Alto contraste con verde lima usado como acento escaso y reconocible.
- Tipografía pesada en titulares y texto funcional, claro y compacto en operación.
- Fotografías reales con recortes consistentes, sin marcos decorativos innecesarios.
- Superficies mayormente planas; la separación nace de color, espacio, bordes y sombras ambientales suaves.
- Navegación comercial visible y acciones de cotización fáciles de encontrar.
- Movimiento breve y funcional: el hero entra de forma escalonada, las secciones se revelan al alcanzar el viewport y las fotografías responden con una ampliación contenida.

## Colors

La paleta combina energía industrial y claridad editorial. El verde lima identifica acciones y momentos de marca; nunca sustituye el texto oscuro cuando el contraste sería insuficiente.

### Primary

- **Lima Simons:** acento de marca, foco, detalles de llamada a la acción y estados positivos destacados.
- **Lima de soporte:** fondo puntual para métricas o superficies que necesitan una versión menos intensa del acento.
- **Ambiente del hero:** `#050807` y `#071009` construyen la base oscura; `#b8ff1a` y `#8bcf00` se reservan para iluminación, placas geométricas y acentos de alta presencia.

### Neutral

- **Tinta:** texto principal, navegación oscura, botones y superficies de máximo contraste.
- **Papel de taller:** fondo general del sistema interno.
- **Papel público:** fondo ligeramente más claro de la web comercial.
- **Superficie:** formularios, tarjetas y documentos.
- **Texto secundario:** descripciones y metadatos sobre fondos claros; debe sustituirse por una variante más oscura cuando el tamaño no alcance contraste AA.
- **Borde:** divisores y contornos discretos.
- **Peligro:** errores y acciones destructivas, acompañado siempre por texto explícito.

**The Accent Discipline Rule.** El verde lima enfatiza identidad y acción; no se usa como color de texto pequeño sobre fondos claros.

## Typography

**Display Font:** Inter con Segoe UI y Arial como respaldo actual.
**Body Font:** Inter con Segoe UI y Arial como respaldo actual.

**Character:** La jerarquía combina titulares muy pesados y compactos con texto de lectura neutro. La implementación declara Inter, pero todavía debe cargarse como fuente web o sustituirse por una familia alojada y confirmada para asegurar consistencia entre equipos.

### Hierarchy

- **Display** (950, fluido hasta 9rem, línea 0.85): portada y nombres de secciones públicas de mayor impacto.
- **Headline** (900, fluido hasta 5rem, línea 0.95): encabezados de página y llamadas comerciales.
- **Title** (700–900, 1.2–2rem): títulos de tarjetas, formularios y registros.
- **Body** (400–650, 1rem, línea 1.6): descripciones, instrucciones y contenido operativo; mantener aproximadamente 65–75 caracteres por línea.
- **Label** (850, 0.68–0.75rem, espaciado amplio, mayúsculas): categorías y metadatos breves. No usar para frases largas.

**The Controlled Compression Rule.** Los titulares pueden ser compactos, pero el espaciado entre letras no debe superar `-0.04em` en la implementación final.

## Layout

La web pública usa secciones amplias con relleno horizontal fluido entre 1rem y 5rem. La portada combina texto y fotografía en escritorio y se apila en pantallas estrechas. El catálogo emplea una cuadrícula adaptable con tarjetas de al menos 18rem cuando el espacio lo permite.

La portada utiliza una cabecera oscura persistente y una composición editorial de dos columnas. El texto ocupa el lado izquierdo y una fotografía real del catálogo, acompañada por dos vistas secundarias, ocupa el derecho. En móvil, texto, acciones y fotografía se apilan sin ocultar el producto.

## Motion

- **Entrada principal:** `opacity`, desplazamiento, escala y desenfoque durante 0.8–0.9 segundos con salida exponencial.
- **Revelado por desplazamiento:** `IntersectionObserver` activa cada bloque una sola vez al entrar en el viewport.
- **Respuesta de producto:** las fotografías escalan como máximo a 1.045 y permanecen recortadas dentro de su marco.
- **Reducción de movimiento:** `prefers-reduced-motion` elimina animaciones y transiciones, conservando todo el contenido visible.

El sistema interno utiliza navegación lateral en escritorio y navegación colapsable en móvil. Formularios y operaciones se organizan en una columna cuando el ancho es insuficiente. Los puntos de quiebre observados se concentran en 520, 700, 760, 800, 850 y 900 px; la consolidación futura debe reducirlos a una escala compartida.

El ritmo parte de pasos de 0.5rem, 1rem, 1.5rem y 2rem. Las secciones públicas pueden ampliar ese ritmo mediante `clamp()` para conservar aire en pantallas grandes.

## Elevation & Depth

El sistema es plano por defecto. Los fondos oscuros, blancos y gris verdoso crean la mayor parte de la profundidad. Las tarjetas operativas importantes pueden usar una sombra ambiental suave (`0 14px 38px rgba(20, 24, 17, .06)`); el login utiliza una elevación más marcada (`0 28px 70px rgba(15, 18, 13, .18)`).

**The Functional Depth Rule.** Una superficie se separa mediante borde o sombra según su función; no se superponen ambos recursos sin una razón visible.

## Shapes

La web pública favorece bordes rectos en acciones principales y una geometría editorial clara. Las imágenes y tarjetas operativas aceptan curvas moderadas entre 0.5rem y 1.1rem. Las píldoras se reservan para estados y variantes compactas. Los controles nunca sacrifican un área táctil mínima de 44 × 44 px.

## Components

### Buttons

- **Shape:** rectangular y firme; las acciones públicas principales no usan radio.
- **Primary:** tinta con texto blanco y detalle lima; altura mínima de 3.2rem.
- **Accent:** lima con texto tinta para acciones destacadas sobre fondos oscuros.
- **Hover / Focus:** cambio tonal discreto y anillo de foco lima con contraste suficiente.
- **Secondary:** superficie clara con borde neutro y texto tinta.

### Chips

- **Style:** píldoras compactas para variantes o estados; tinta, lima suave o gris según el significado.
- **State:** el texto acompaña al color y evita depender solo de la tonalidad.

### Cards / Containers

- **Corner Style:** recto en composiciones editoriales; 0.8–1.1rem en módulos operativos.
- **Background:** blanco sobre papel claro o tinta sobre fondos oscuros.
- **Shadow Strategy:** sombra ambiental únicamente cuando mejora la jerarquía.
- **Internal Padding:** entre 1.2rem y 2rem según densidad.

### Inputs / Fields

- **Style:** fondo blanco o papel muy claro, borde neutro de 1px y altura mínima de 3rem.
- **Focus:** borde o anillo lima oscuro que pertenezca a Simons; eliminar el azul heredado de la plantilla Blazor.
- **Error / Disabled:** mensaje explícito, contraste AA y estado perceptible sin depender solo de color.

### Navigation

La navegación pública es horizontal y comercial en escritorio, con acceso claro a Inicio, Productos y Solicitar cotización. En móvil se colapsa en un control con nombre accesible, foco visible y estado comunicado. La navegación interna usa fondo tinta, texto blanco/gris y lima para el elemento activo.

### Product Card

La fotografía ocupa el primer plano con relación visual consistente. Debajo aparecen categoría, nombre, descripción breve, variantes, precio confirmado y una acción que conserva el producto al abrir la cotización. La tarjeta no inventa tiempos de entrega ni características ausentes.

## Do's and Don'ts

### Do:

- **Do** usar el logotipo oficial sin deformarlo y dejar espacio suficiente a su alrededor.
- **Do** priorizar fotografías reales aprobadas por Simons en portada, catálogo y galería.
- **Do** mantener negro, blanco y lima como identidad y hacer que el lima siga siendo un acento.
- **Do** probar la interfaz a 320, 390, 768 y 1440 px.
- **Do** obtener categorías, productos, variantes y precios de la base de datos.

### Don't:

- **Don't** copiar la paleta rosa, la tipografía, el contenido ni las afirmaciones comerciales de Colorisa.
- **Don't** usar verde lima para texto pequeño sobre papel claro cuando no alcance contraste AA.
- **Don't** inventar productos, precios, testimonios, tiempos o garantías para llenar la interfaz.
- **Don't** añadir carrito, pagos, stock o comportamiento de comercio electrónico fuera del alcance confirmado.
- **Don't** convertir el sistema interno en una portada promocional; la operación debe seguir siendo rápida y legible.
