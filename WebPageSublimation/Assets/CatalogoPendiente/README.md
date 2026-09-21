# Fotografías incorporadas al catálogo

Origen: `catalogo-simons.zip`, entregado por el propietario del proyecto el 19 de septiembre de 2026.

Las seis fotografías son material real de Simons. Se importaron a PostgreSQL, se organizaron en cuatro productos y se publicaron con precios referenciales autorizados por el propietario. La web identifica estos importes con la palabra **“Desde”** y aclara que la cotización final depende de la cantidad y la personalización.

Los archivos del ZIP tenían extensión `.png`, pero su firma real era JPEG. Las copias de esta carpeta se renombraron a `.jpg` sin recomprimirlas para que coincidan con `image/jpeg` y superen la validación del cargador.

## Productos publicados

| Producto | Precio referencial | Fotografías |
|---|---:|---:|
| Gorra personalizada | Desde Bs 45 | 1 |
| Jarra personalizada | Desde Bs 55 | 2 |
| Botella personalizada | Desde Bs 65 | 2 |
| Vaso térmico personalizado | Desde Bs 90 | 1 |

## Regla comercial aplicada

Los productos con `PrecioEsReferencial = true` pueden mostrarse en el catálogo público, pero quedan excluidos de la creación de pedidos y proformas. Cuando Simons confirme un precio operativo, el administrador debe editar el producto y desmarcar **“Mostrar como precio referencial ‘Desde’”**. Desde ese momento podrá utilizarse en operaciones comerciales.

## Mantenimiento

1. Revisar nombres, categorías, descripciones e imágenes desde `/admin/catalogo`.
2. Agregar variantes únicamente cuando hayan sido confirmadas.
3. Elegir otra imagen principal cuando sea necesario.
4. Confirmar los precios vigentes antes de habilitar cada producto para pedidos y proformas.
5. Revisar la ficha pública y la cotización en móvil y escritorio después de cada cambio.

## Observación pendiente

El archivo `toma-todos/aeb0cf61-3102-4813-97d3-d23f2b37a105.jpg` estaba dentro de “Toma todos”, pero la pieza gráfica lo identifica como “Vaso térmico”. También muestra precios por cantidad. Se clasificó como **Vasos térmicos**; la vigencia de las escalas impresas todavía debe confirmarse antes de convertirlas en reglas del sistema.
