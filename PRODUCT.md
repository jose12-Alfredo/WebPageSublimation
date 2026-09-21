# Product

<!-- impeccable:product-schema 1 -->

## Platform

web

## Users

- **Visitante público:** explora trabajos y productos de Simons, entiende qué puede personalizar y solicita una cotización.
- **Promotor:** trabaja principalmente desde teléfono o computadora para consultar el catálogo y los precios comerciales autorizados, registrar clientes, preparar proformas y crear pedidos propios.
- **Administrador:** mantiene usuarios, promotores, catálogo, clientes, solicitudes, proformas, pedidos y resúmenes comerciales de toda la empresa.

## Product Purpose

Simons Publicidad necesita presentar su oferta comercial y operar el ciclo que va desde el catálogo hasta la proforma y el pedido. El sistema une una web pública orientada a captar solicitudes con un espacio interno que conserva responsables, precios y datos históricos.

El producto tiene éxito cuando un visitante entiende rápidamente qué produce Simons y puede iniciar una cotización, y cuando el equipo registra operaciones sin perder la identidad del promotor ni modificar el pasado al cambiar el catálogo.

## Positioning

El sistema combina el catálogo real de Simons con su operación comercial. Los productos, imágenes, categorías, variantes y precios se administran desde PostgreSQL; las proformas y pedidos congelan los datos necesarios para preservar la historia. No es una tienda electrónica ni un ERP general.

## Operating Context

- El negocio ofrece publicidad, sublimación, imprenta y productos personalizados.
- El trabajo comercial puede variar por producto, cantidad, características, personalización y precio.
- Un visitante consulta la web pública y envía una solicitud; el equipo continúa el contacto y la cotización.
- Un promotor autenticado consulta el catálogo, trabaja con clientes permitidos y registra proformas o pedidos.
- Un administrador controla el catálogo y puede identificar qué promotor realizó cada operación.
- El uso móvil es prioritario para catálogo, clientes, proformas y pedidos.

## Capabilities and Constraints

- Stack confirmado: C#, .NET 10, ASP.NET Core, Blazor Web App, PostgreSQL, Entity Framework Core, Npgsql y ASP.NET Core Identity.
- Arquitectura confirmada: monolito modular de un proyecto mientras sea suficiente.
- Solo el administrador crea cuentas de promotores y administra el catálogo.
- El promotor autenticado nunca se acepta desde el navegador; se obtiene de la identidad del servidor.
- Los precios y totales se validan o calculan en el servidor con `decimal`.
- Los cambios de producto, variante, precio, cliente o promotor no alteran proformas y pedidos históricos.
- Las categorías, productos, precios y variantes deben provenir de Simons; no se hardcodean ni se deducen de ejemplos públicos.
- Finanzas está dentro del alcance, pero su definición detallada continúa pendiente de confirmación.
- El sistema no incluye por defecto carrito, pagos, stock, facturación electrónica, contabilidad completa, portal de cliente ni comercio electrónico.

## Brand Commitments

- Nombre: **Simons Publicidad**.
- Identidad principal: negro, blanco y verde lima; el verde funciona como acento.
- El logotipo oficial no debe modificarse ni sustituirse por una identidad inventada.
- La web puede adoptar mayor protagonismo fotográfico y mejor exploración comercial, manteniendo el lenguaje propio de Simons.
- Colorisa es una referencia de jerarquía fotográfica y facilidad de exploración. Sus colores, tipografía, productos, afirmaciones y contenido no forman parte de la identidad de Simons.

## Evidence on Hand

- Reglas funcionales y de negocio: `WebPageSublimation/Docs/02-Negocio/DOCUMENTACION-FUNCIONAL.md`.
- Estándar técnico: `WebPageSublimation/Docs/01-Estandar-Tecnico/ESTANDAR-TECNICO-MONOLITO.md`.
- Logotipo oficial: `WebPageSublimation/wwwroot/images/simons-logo.png`.
- Seis fotografías entregadas para preparar el catálogo: `WebPageSublimation/Assets/CatalogoPendiente/`.
- Las fotografías permiten confirmar familias visuales generales, pero no confirman por sí solas nombres comerciales, descripciones, variantes ni todos los precios.
- No hay testimonios, tiempos de entrega, garantías ni afirmaciones de calidad confirmadas que puedan publicarse como prueba comercial.

## Product Principles

1. **La información real manda.** Publicar solo productos, precios y condiciones confirmados por Simons.
2. **La fotografía explica el oficio.** Mostrar trabajos reales con claridad antes de recurrir a decoración abstracta.
3. **Cotizar debe ser directo.** Mantener visible la acción comercial y conservar el producto que originó la consulta.
4. **La historia no cambia.** Proformas y pedidos conservan responsable, nombres e importes del momento en que se crearon.
5. **Móvil es una condición de operación.** Los promotores deben completar los flujos principales cómodamente desde teléfono.

## Accessibility & Inclusion

- Objetivo mínimo: WCAG 2.2 nivel AA para contraste, foco visible, nombres accesibles y operación con teclado.
- Los controles táctiles principales deben alcanzar al menos 44 × 44 px.
- La interfaz debe respetar reducción de movimiento y evitar depender únicamente del color para comunicar estado.
- El contenido y los formularios deben funcionar sin desbordamiento horizontal desde 320 px.
