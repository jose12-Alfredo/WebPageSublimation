# Plan de mejora visual y comercial de la web

Estado: **documentado, pendiente de implementación por fases**  
Fecha de análisis: **19 de septiembre de 2026**

## Avance de implementación

Actualizado el **19 de septiembre de 2026**:

- **Logotipo oficial implementado y verificado visualmente:** aparece completo en la cabecera pública de la instancia activa. También quedó incorporado en acceso, navegación interna, pie público, documentos y favicon para la próxima carga completa de componentes.
- **Base de tokens iniciada:** colores principales, foco, selección de texto, superficie y sombra compartida están disponibles como variables CSS globales.
- **Foco de plantilla corregido:** el azul heredado de Blazor se sustituyó por un foco verde propio de Simons.
- **Catálogo público mejorado en código:** filtros desde categorías activas de PostgreSQL, estado vacío contextual, tarjetas con dimensiones de imagen y acción específica por producto.
- **Cotización conectada al producto:** la acción del catálogo lleva el nombre del producto al formulario y prepara el detalle para que el visitante agregue cantidad y características.
- **SEO básico iniciado:** favicon oficial, descripción y metadatos sociales en catálogo; descripción en cotización.
- **Galería de producto implementada:** hasta ocho imágenes por producto, validación de firma real, portada seleccionable y eliminación administrativa.
- **Catálogo inicial publicado:** cuatro productos y seis fotografías almacenadas en PostgreSQL. Los precios se muestran como referenciales con la etiqueta “Desde”. Estos productos quedan fuera de pedidos y proformas hasta que un administrador confirme el precio y desmarque el estado referencial.
- **Portada fotográfica implementada:** la composición adopta de Colorisa el fondo oscuro, la jerarquía editorial, la fotografía protagonista, el catálogo temprano y los llamados comerciales visibles, manteniendo el logotipo, el verde lima y el contenido real de Simons.
- **Movimiento accesible implementado:** entrada escalonada del hero, revelado al desplazarse, ampliación moderada de fotografías y respuesta de botones. Todos los efectos respetan `prefers-reduced-motion` y disponen de un estado visible de respaldo.
- **Responsive verificado:** portada, catálogo, cotización y acceso fueron medidos en Chromium a 320, 390, 768, 1024, 1440 y 1920 px. Las 24 combinaciones terminaron sin desbordamiento horizontal, imágenes rotas, objetivos táctiles menores de 44 px ni fallos del menú móvil. También se comprobó visualmente una orientación horizontal de 844 × 390 px.
- **Hero premium refinado:** la estructura comercial se conserva y gana profundidad mediante una base `#050807`, iluminación radial lima, una placa geométrica inclinada, marco desplazado, sombra ambiental, textura tonal y un acento breve sobre el titular. El efecto se adapta en móvil y mantiene la reducción de movimiento.
- **Verificación técnica:** compilación Release sin errores ni advertencias y detector Impeccable sin hallazgos en los componentes modificados.

La instancia local fue reiniciada después de aplicar las migraciones de PostgreSQL. El logotipo, los filtros, las tarjetas, las galerías y los precios referenciales ya están disponibles en la aplicación activa.

## Objetivo

Hacer que la web pública de Simons muestre el trabajo real desde el primer desplazamiento, facilite explorar el catálogo y conecte cada producto con la solicitud de cotización. La evolución debe conservar la identidad negro/blanco/verde lima y todas las reglas comerciales, de seguridad e integridad histórica.

## Referencia y criterio de adaptación

Colorisa consigue impacto mediante fotografías grandes y consistentes, catálogo temprano, filtros por familias, tarjetas informativas, espacio abundante y una navegación comercial clara.

Simons ya posee una identidad más fuerte y diferenciada: contraste negro/blanco/lima, titulares con personalidad, portada reconocible, formulario público sólido, login cuidado y separación clara entre web pública y sistema interno. Además, el catálogo, la galería y la información comercial provienen de PostgreSQL.

Se adoptarán de la referencia el protagonismo fotográfico, la jerarquía del catálogo y la facilidad para explorar. No se copiarán su paleta rosa, tipografía, contenido, productos, categorías, testimonios, tiempos, precios ni afirmaciones.

## Auditoría Impeccable

| Dimensión | Puntuación | Hallazgo principal |
|---|---:|---|
| Accesibilidad | 2/4 | Algunos textos verdes pequeños no alcanzan contraste AA. |
| Rendimiento | 2/4 | Se sirven imágenes originales sin miniaturas ni `srcset`. |
| Responsive | 3/4 | La estructura se adapta, pero faltan controles táctiles y pruebas completas. |
| Sistema visual | 2/4 | La paleta es coherente, pero todavía no existen tokens CSS compartidos. |
| Integridad | 3/4 | La identidad es específica de Simons con inconsistencias puntuales. |
| **Total** | **12/20** | **Aceptable; necesita una pasada importante.** |

El detector mecánico encontró bordes laterales gruesos en alertas de `Login.razor.css` y `Promotores.razor.css`. Son correcciones menores frente a los hallazgos visuales y funcionales.

## Hallazgos que guían el trabajo

1. **Contenido real pendiente.** Sin productos confirmados, el catálogo público no puede demostrar el alcance del negocio.
2. **Primera impresión poco fotográfica.** El cartel verde tiene carácter, pero no enseña inmediatamente qué produce Simons.
3. **Contraste insuficiente.** Verdes cercanos a `#628d00` sobre `#f6f7f2` no alcanzan 4.5:1 cuando se usan en texto pequeño.
4. **Tipografía no controlada.** Se declara Inter sin cargarla y queda un foco azul heredado de Blazor.
5. **Ausencia de tokens.** Los colores y medidas se repiten con pequeñas variaciones en numerosos archivos CSS.
6. **Imágenes sin derivados.** `loading="lazy"` ayuda, pero no evita transferir originales grandes.
7. **Exploración limitada.** El catálogo solo ofrece búsqueda textual aunque las categorías ya existen como datos.
8. **Cotización desconectada.** El formulario no conserva el producto desde el que comenzó la consulta.
9. **Estado vacío impreciso.** El mismo mensaje se usa con y sin búsqueda.
10. **Áreas táctiles pequeñas.** Algunos controles no alcanzan 44 × 44 px.
11. **SEO básico.** Faltan descripción, metadatos sociales y datos estructurados para páginas públicas.

## Dirección aprobada

La dirección se denomina **“El taller gráfico de alto contraste”**. La fotografía real demuestra el oficio; la interfaz negra, clara y lima funciona como marco. La web pública persuade y el sistema interno opera con mayor sobriedad.

El sistema visual actual queda documentado en `DESIGN.md`. La verdad de producto y sus restricciones quedan registradas en `PRODUCT.md`.

## Trabajo propuesto

### Fase 0 — Contenido y preparación del catálogo

- Confirmar nombre, categoría, descripción, precio comercial y variantes de cada producto.
- Asociar una imagen principal por producto mediante la administración existente.
- Corregir la clasificación dudosa entre “Toma todos” y “Vasos térmicos”.
- Confirmar si los precios impresos en una de las imágenes siguen vigentes y cómo deben modelarse los precios por cantidad.
- Evitar publicar fotografías con datos comerciales incrustados que puedan quedar desactualizados.

### Fase 1 — Base visual y accesibilidad

- Extraer variables CSS para color, tipografía, espaciado, radios, sombras y foco.
- Cargar una fuente web confirmada o ajustar la pila tipográfica para garantizar consistencia.
- Sustituir el foco azul heredado por un foco propio de Simons con contraste AA.
- Oscurecer los verdes usados en textos pequeños y reservar el lima brillante para fondos, detalles y acciones.
- Unificar botones, campos, estados, tarjetas y mensajes.
- Asegurar áreas táctiles mínimas de 44 × 44 px y respeto por `prefers-reduced-motion`.

### Fase 2 — Portada pública

- Reemplazar el cartel abstracto por una composición con productos reales aprobados.
- Mantener el titular actual y ajustar su escala para no forzar espaciado menor a `-0.04em`.
- Mostrar un avance fotográfico del catálogo en el primer recorrido.
- Mantener visible la acción de solicitar cotización.
- Añadir una explicación breve del proceso solo con pasos confirmados por Simons.

### Fase 3 — Catálogo público

- Añadir filtros dinámicos construidos desde categorías activas de PostgreSQL.
- Rediseñar las tarjetas alrededor de fotografía, categoría, nombre, descripción, variantes, precio y consulta.
- Conservar búsqueda textual y combinarla con el filtro sin hardcodear categorías.
- Enviar el identificador o nombre del producto al formulario de cotización.
- Diferenciar “catálogo todavía sin publicar” de “sin resultados para esta búsqueda”.
- Mantener una acción de cotización útil en ambos estados vacíos.

### Fase 4 — Sistema interno

- Mantener una densidad más sobria que la web pública.
- Unificar formularios, validaciones, estados, métricas y tarjetas.
- Mejorar los flujos móviles de clientes, proformas y pedidos.
- Añadir resúmenes persistentes antes de guardar operaciones largas.
- Conservar autorización de servidor, promotor autenticado, cálculos de precio en servidor e integridad histórica.

### Fase 5 — Imágenes, rendimiento y SEO

- Generar miniaturas y derivados WebP/AVIF conservando el original.
- Entregar tamaños adecuados con `width`, `height`, `srcset` y caché.
- Definir recortes consistentes por contexto sin deformar el producto.
- Añadir descripción de página, Open Graph y datos estructurados basados únicamente en datos reales.
- Medir el resultado con catálogo real cargado, no con tarjetas vacías.

### Fase 6 — Verificación

- Revisar en 320, 390, 768 y 1440 px en una sola ronda acotada.
- Probar teclado, foco, menú móvil, búsqueda, filtros, selección de producto y cotización.
- Comprobar contraste AA y ausencia de desbordamiento horizontal.
- Medir transferencia de imágenes y estabilidad visual.
- Ejecutar compilación, detector Impeccable y pruebas funcionales relacionadas.

## Inventario recibido para el catálogo

El ZIP entregado contiene seis fotografías válidas, todas por debajo del límite actual de 5 MB. Los archivos llegaron con extensión `.png`, aunque su contenido real era JPEG; las copias de trabajo se normalizaron a `.jpg` sin recomprimirlas para que el cargador pueda validar correctamente el tipo:

| Carpeta de origen | Cantidad | Resoluciones | Observación |
|---|---:|---|---|
| Gorras | 1 | 896 × 1193 | Gorra personalizada; falta nombre comercial, precio y descripción. |
| Tazas | 2 | 960 × 1280 | Jarras/tazas personalizadas; falta confirmar si son un producto con ejemplos o productos separados. |
| Toma todos | 2 | 1200 × 1600 y 900 × 1600 | Botellas personalizadas; faltan nombre, material, capacidad y precio. |
| Toma todos | 1 | 1200 × 1200 | La propia pieza dice “Vaso térmico”, material acero inoxidable y 450 ml; la categoría y los precios requieren confirmación. |
| Vasos térmicos | 0 | — | La carpeta del ZIP está vacía. |

Los originales se conservaron en `WebPageSublimation/Assets/CatalogoPendiente/`. El manifiesto `catalogo-importacion.csv` mantiene cada archivo trazable sin convertir observaciones visuales en datos comerciales.

## Datos que faltan antes de confirmar los precios operativos

Por cada producto se necesita:

- nombre comercial;
- categoría definitiva;
- descripción aprobada;
- precio comercial definitivo;
- variantes, si existen;
- confirmación de la imagen principal;
- confirmación para retirar el estado referencial y habilitar el producto en pedidos y proformas;
- política de precios por cantidad, si corresponde.

## Límites de alcance

No se añadirán carrito, pagos, stock, comercio electrónico, testimonios, garantías, tiempos de entrega ni reglas de descuentos sin confirmación expresa. Las categorías seguirán siendo administrables, los precios se calcularán en servidor y las operaciones históricas conservarán sus datos originales.

## Secuencia de implementación

1. Contexto de producto (`PRODUCT.md`).
2. Documentación visual (`DESIGN.md` y `.impeccable/design.json`).
3. Inventario y confirmación de contenido del catálogo.
4. Extracción de tokens y correcciones de accesibilidad.
5. Rediseño de portada y catálogo.
6. Adaptación responsive.
7. Optimización de imágenes y SEO.
8. Pulido y verificación final.
