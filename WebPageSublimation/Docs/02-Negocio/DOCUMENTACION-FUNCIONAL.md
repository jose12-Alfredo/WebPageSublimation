---
name: simons-publicidad-system
description: >
  Skill maestra para desarrollar WebPageSublimation, sistema empresarial de
  Simons Publicidad.

  Define la fuente de verdad del negocio, reglas de negocio, alcance funcional,
  roles, permisos, integridad histórica, arquitectura, seguridad y UX/UI
  responsive.

  Stack: C#, .NET, ASP.NET Core, Blazor Web App, PostgreSQL,
  Entity Framework Core, Npgsql y ASP.NET Core Identity.

  Esta Skill debe consultarse antes de modificar WebPageSublimation.

  NO contiene la estrategia completa de pruebas E2E.
---

# 1. OBJETIVO

Construir para Simons Publicidad un sistema web empresarial que permita:

- administrar catálogo;
- administrar productos y precios;
- administrar promotores;
- crear usuarios para promotores;
- registrar clientes;
- generar proformas;
- registrar pedidos;
- identificar quién realizó cada venta;
- consultar ventas por promotor;
- facilitar cálculo manual de comisiones;
- incorporar finanzas según las reglas que Simons confirme.

El sistema debe resolver problemas reales del negocio.

NO desarrollar funcionalidades solamente para aumentar complejidad técnica.

---

# 2. FUENTES DE INFORMACIÓN

Las decisiones del sistema deben distinguir claramente cuatro niveles de
información.

## FUENTE A — REQUERIMIENTOS CONFIRMADOS

Información comunicada directamente por el cliente.

Tiene máxima prioridad.

## FUENTE B — INFORMACIÓN PROPORCIONADA POR EL PROPIETARIO DEL PROYECTO

Información obtenida durante reuniones, conversaciones, capturas, documentos
o material proporcionado durante el desarrollo.

## FUENTE C — INFORMACIÓN PÚBLICA VERIFICABLE

Puede utilizarse para comprender:

- identidad visual;
- actividad comercial;
- comunicación;
- contexto empresarial.

Fuente pública conocida:

Facebook:
https://www.facebook.com/p/simonsbo-61566951686964/

No convertir automáticamente contenido de redes sociales en reglas del negocio.

## FUENTE D — INFERENCIAS

Una inferencia puede ayudar a formular preguntas.

NO puede convertirse automáticamente en una regla de negocio.

---

# 3. PRIORIDAD DE FUENTES

Cuando exista contradicción:

1. regla confirmada directamente por Simons;
2. requerimiento confirmado del proyecto;
3. información proporcionada por el propietario del proyecto;
4. información pública verificable;
5. inferencia.

Nunca permitir que una inferencia de Codex sustituya una decisión empresarial.

---

# 4. CONTEXTO CONOCIDO DEL NEGOCIO

Empresa:

SIMONS PUBLICIDAD.

Actividad conocida:

- publicidad;
- sublimación;
- imprenta;
- productos personalizados.

La empresa comercializa productos y trabajos que pueden variar en:

- producto;
- cantidad;
- características;
- personalización;
- precio.

NO interpretar esta descripción como catálogo completo.

El catálogo real debe administrarse desde base de datos.

---

# 5. IDENTIDAD VISUAL

La identidad proporcionada/observada de Simons utiliza principalmente:

- negro;
- blanco;
- verde lima;
- marca SIMONS.

La aplicación debe sentirse perteneciente a Simons.

NO modificar el logo.

NO inventar otra identidad.

NO utilizar una plantilla administrativa azul genérica sin adaptación.

El verde debe utilizarse principalmente como acento.

---

# 6. REQUERIMIENTO ORIGINAL CONFIRMADO

El cliente solicitó:

1. catálogo de todos sus productos;
2. sección administrativa;
3. posibilidad de cargar/administrar catálogo;
4. apartado de finanzas;
5. creación de usuarios para promotores;
6. usuario y contraseña para cada promotor;
7. acceso limitado para promotores;
8. promotor puede consultar catálogo;
9. promotor puede registrar pedidos;
10. promotor puede realizar proformas;
11. administrador recibe/visualiza pedidos;
12. administrador identifica qué promotor realizó el pedido;
13. esa información sirve para calcular comisión;
14. inicialmente la comisión puede calcularse manualmente.

ESTO constituye la fuente principal del alcance.

---

# 7. CLASIFICACIÓN DEL ALCANCE

## NIVEL A — CONFIRMADO

- autenticación;
- administrador;
- promotores;
- catálogo;
- productos;
- precios comerciales;
- proformas;
- pedidos;
- identificación del promotor;
- finanzas.

## NIVEL B — NECESARIO PARA SOPORTAR NIVEL A

- categorías;
- clientes;
- detalle de proforma;
- detalle de pedido;
- búsqueda;
- filtros básicos;
- imágenes;
- variantes cuando existan;
- historial comercial;
- estados básicos;
- conversión proforma → pedido;
- ventas agrupadas por promotor;
- dashboard básico.

## REQUISITO TRANSVERSAL

RESPONSIVE DESIGN.

Responsive es obligatorio desde V1.

## NIVEL C — BACKLOG / NO CONFIRMADO

No implementar automáticamente:

- PDF profesional;
- WhatsApp;
- archivos de diseño;
- aprobación de diseños;
- duplicar pedido;
- inventario;
- stock;
- proveedores;
- compras;
- producción avanzada;
- control de calidad;
- ecommerce;
- portal cliente;
- pagos online;
- facturación electrónica;
- contabilidad completa;
- cuentas por cobrar;
- pagos parciales;
- comisiones automáticas;
- aplicación móvil nativa;
- inteligencia artificial;
- reportes avanzados.

---

# 8. REGLA DE ALCANCE

Ante una nueva funcionalidad:

¿SIMONS LA CONFIRMÓ?

SI
→ puede entrar al alcance.

NO
→ preguntar:

¿ES NECESARIA PARA HACER FUNCIONAR CORRECTAMENTE ALGO CONFIRMADO?

SI
→ implementar solamente lo mínimo.

NO
→ BACKLOG.

No convertir el sistema en un ERP completo.

---

# 9. RN-AUTH — USUARIOS

## RN-AUTH-001

Solamente ADMINISTRADOR puede crear cuentas de promotores.

## RN-AUTH-002

No existe registro público de promotores.

## RN-AUTH-003

Cada promotor debe utilizar una cuenta individual.

No utilizar cuentas compartidas.

## RN-AUTH-004

Un promotor NO puede:

- crear usuarios;
- crear otros promotores;
- modificar su rol;
- obtener permisos administrativos.

## RN-AUTH-005

Administrador puede desactivar un promotor.

Desactivar NO elimina:

- pedidos;
- proformas;
- ventas;
- historial.

---

# 10. RN-PRO — PROMOTORES

## RN-PRO-001

El promotor puede:

- iniciar sesión;
- consultar catálogo;
- buscar productos;
- consultar precios comerciales autorizados;
- trabajar con clientes permitidos;
- crear proformas;
- registrar pedidos;
- consultar sus proformas;
- consultar sus pedidos.

## RN-PRO-002

El promotor NO puede:

- administrar catálogo;
- modificar precios base;
- crear usuarios;
- acceder a administración;
- consultar costos internos;
- consultar utilidad;
- consultar gastos;
- consultar finanzas generales;
- consultar operaciones privadas de otros promotores.

## RN-PRO-003

Toda operación comercial creada por un promotor debe identificar quién la creó.

## RN-PRO-004

PromoterId debe obtenerse de la identidad autenticada.

NO debe recibirse del navegador como fuente confiable.

---

# 11. RN-CAT — CATÁLOGO

## RN-CAT-001

El catálogo es administrable.

No hardcodear catálogo.

## RN-CAT-002

Administrador puede:

- crear producto;
- editar producto;
- cargar imagen;
- asignar categoría;
- administrar precio;
- administrar variantes;
- activar/desactivar.

## RN-CAT-003

Promotor puede consultar catálogo pero NO administrarlo.

## RN-CAT-004

Un producto inactivo deja de utilizarse para nuevas operaciones cuando
corresponda.

NO desaparece de operaciones históricas.

## RN-CAT-005

No inventar productos basándose en ejemplos o información pública incompleta.

El catálogo real debe provenir de Simons.

---

# 12. RN-CAT — CATEGORÍAS

Las categorías deben ser datos administrables.

No hardcodear categorías como:

Tazas
Poleras
Gorras

sin confirmación.

Pueden utilizarse solamente como ejemplos durante diseño/desarrollo.

---

# 13. RN-CAT — VARIANTES

Un producto puede tener:

0..N variantes.

No obligar a todos los productos a tener variantes.

Una variante puede representar características comerciales cuando corresponda.

No inventar estructura específica hasta conocer catálogo real.

---

# 14. RN-PRE — PRECIOS

## RN-PRE-001

PRICE = precio comercial.

COST = costo interno.

PRICE ≠ COST.

## RN-PRE-002

Promotor puede consultar precio comercial autorizado.

NO puede consultar costo interno.

## RN-PRE-003

Si existen precios por cantidad:

deben ser configurables.

NO hardcodear escalas.

## RN-PRE-004

No inventar descuentos.

Hasta que exista política confirmada:

el promotor NO puede aplicar descuentos arbitrarios.

## RN-PRE-005

El precio utilizado en una operación debe quedar congelado históricamente.

---

# 15. RN-HIS — PRECIO HISTÓRICO

Ejemplo:

HOY:

Producto = Bs 25.

Se crea:

100 × Bs 25 = Bs 2.500.

MAÑANA:

Producto = Bs 30.

El pedido anterior continúa:

UnitPrice = Bs 25.
Total = Bs 2.500.

Nunca recalcular operaciones antiguas usando precios actuales.

---

# 16. RN-CLI — CLIENTES

El sistema necesita identificar al cliente de una proforma/pedido.

Customer puede evolucionar hacia:

- Name / BusinessName;
- CI/NIT;
- Phone;
- WhatsApp;
- Email;
- Address;
- ContactPerson;
- Notes;
- PromoterId;
- IsActive.

NO hacer obligatorio cada campo automáticamente.

---

# 17. RN-CLI — PRIVACIDAD

Administrador puede consultar todos los clientes.

La visibilidad exacta de clientes entre promotores debe respetar la política que
Simons confirme.

Hasta conocerla:

NO asumir automáticamente que todos los promotores comparten toda la cartera.

NO asumir automáticamente exclusividad absoluta.

Marcar la política como pendiente.

---

# 18. RN-COT — PROFORMAS

## RN-COT-001

Promotor autorizado puede crear proformas.

## RN-COT-002

Una proforma debe identificar:

- número;
- fecha;
- cliente;
- promotor;
- productos;
- cantidades;
- precios utilizados;
- subtotales;
- total;
- observaciones cuando correspondan.

## RN-COT-003

PromoterId proviene del usuario autenticado.

## RN-COT-004

Una proforma debe conservar snapshots históricos.

Como mínimo cuando corresponda:

- ProductName;
- VariantName;
- Quantity;
- UnitPrice;
- Subtotal.

## RN-COT-005

Modificar posteriormente el catálogo NO modifica la proforma histórica.

---

# 19. RN-COT — CONVERSIÓN A PEDIDO

Una proforma puede convertirse en pedido.

Flujo:

PROFORMA
→ PEDIDO.

No volver a ingresar:

- cliente;
- promotor;
- productos;
- cantidades;
- precios.

Conservar:

QuoteId.

No destruir/modificar incorrectamente la proforma original.

---

# 20. RN-PED — PEDIDO DIRECTO

La proforma NO es obligatoria.

Flujos válidos:

CATÁLOGO
→ PROFORMA
→ PEDIDO.

y:

CATÁLOGO
→ PEDIDO.

---

# 21. RN-PED — PEDIDOS

Todo pedido debe conservar conceptualmente:

- Id;
- Number;
- CustomerId;
- PromoterId;
- QuoteId opcional;
- CreatedAt;
- RequiredDate opcional;
- Status;
- Subtotal;
- Discount cuando corresponda;
- Total;
- Notes.

Agregar campos solamente cuando exista necesidad.

---

# 22. RN-PED — PROMOTOR RESPONSABLE

Todo pedido originado por un promotor guarda:

PromoterId.

PromoterId pertenece históricamente al pedido.

Ejemplo:

Carlos crea PED-001 para Empresa ABC.

Posteriormente Empresa ABC pasa a María.

PED-001 continúa perteneciendo comercialmente a Carlos.

NO reconstruir responsabilidad histórica desde Customer.PromoterId.

---

# 23. RN-PED — ADMINISTRADOR

Cuando un promotor registra un pedido:

el administrador debe verlo.

Como mínimo debe poder identificar:

- número;
- fecha;
- cliente;
- promotor;
- total;
- estado.

Esta es una regla central del requerimiento original.

---

# 24. RN-PED — PROMOTOR

Promotor puede consultar:

MIS PEDIDOS.

No puede consultar pedidos privados de otro promotor.

La restricción debe aplicarse en servidor.

Ocultar botones NO es seguridad.

---

# 25. RN-PED — INFORMACIÓN DEL TRABAJO

El pedido debe conservar suficiente información para entender qué solicitó el
cliente.

Cuando corresponda:

- producto;
- variante;
- cantidad;
- personalización;
- especificaciones;
- observaciones;
- fecha requerida.

No reducir un trabajo personalizado a solamente:

PRODUCTO + CANTIDAD

si eso hace perder información importante.

---

# 26. RN-PED — ESTADOS

Estados iniciales sugeridos:

Pending
Confirmed
InProduction
Ready
Delivered
Cancelled.

Son una propuesta técnica inicial.

Las transiciones exactas deben confirmarse con Simons.

NO inventar un workflow complejo.

---

# 27. RN-PED — CANCELACIÓN

Cancelar NO significa eliminar.

Un pedido cancelado conserva:

- número;
- cliente;
- promotor;
- productos;
- importes;
- historia.

No inventar políticas de devolución/reembolso.

---

# 28. RN-COM — COMISIONES

## RN-COM-001

Inicialmente la comisión puede calcularse manualmente.
Pero el sistema tiene que poder calcular la comicion de los pedidos segun las metricas que ponga el administrados qeu hayga una apartado de porcentage de comicion para que asi el administrador calcule la comision al poner el porcentage que el decida
## RN-COM-002

El sistema debe proporcionar:

PROMOTOR
→ PEDIDOS
→ IMPORTE.

## RN-COM-003

NO implementar automáticamente fórmula de comisión.

No asumir:

- porcentaje;
- monto fijo;
- comisión por producto;
- comisión por margen;
- comisión por pago.

## RN-COM-004

PEDIDO CREADO ≠ COMISIÓN PAGADERA.

El momento exacto de devengo debe confirmarse.

---

# 29. RN-FIN — FINANZAS

Finanzas fue solicitado por el cliente.

Por tanto pertenece al alcance.

Pero su significado exacto todavía debe levantarse.

NO interpretar automáticamente Finanzas como:

- contabilidad;
- libro diario;
- impuestos;
- facturación;
- bancos;
- conciliación;
- estados financieros.

---

# 30. RN-FIN — PRINCIPIO FINANCIERO

PEDIDO CREADO ≠ DINERO COBRADO.

Venta y pago son conceptos diferentes.

No generar automáticamente ingreso financiero al crear pedido salvo regla
confirmada.

El diseño debe permitir posteriormente, si Simons lo necesita:

- anticipos;
- pagos;
- saldos;
- cuentas por cobrar.

NO implementarlos anticipadamente.

---

# 31. RN-FIN — PRIVACIDAD

Finanzas pertenece al área administrativa.

Promotor NO consulta:

- costos;
- gastos;
- utilidad;
- márgenes;
- movimientos financieros generales.

---

# 32. NUMERACIÓN COMERCIAL

Utilizar:

PK técnica
+
número comercial.

Ejemplo:

PRO-2026-00001
PED-2026-00001.

Los ejemplos NO definen obligatoriamente el formato final.

La numeración debe ser segura ante concurrencia.

NO utilizar:

last + 1

sin protección.

---

# 33. INTEGRIDAD HISTÓRICA

Una operación comercial representa lo que ocurrió cuando fue creada.

Cambios posteriores de:

- producto;
- nombre;
- variante;
- precio;
- cliente;
- promotor;

NO deben modificar incorrectamente documentos históricos.

Utilizar snapshots cuando corresponda.

---

# 34. SEGURIDAD

Autorización siempre del lado servidor.

Validar:

USUARIO
+
ROL
+
PROPIEDAD DEL RECURSO.

Prevenir IDOR.

Promotor A no obtiene acceso a Pedido B simplemente modificando:

/pedido/100
→
/pedido/101.

---

# 35. DINERO

Utilizar:

decimal.

Nunca:

float
double.

PostgreSQL:

numeric(18,2)

cuando corresponda.

---

# 36. CÁLCULOS

No confiar en:

UnitPrice
Subtotal
Total
PromoterId

enviados desde navegador.

Validar/recalcular en servidor.

---

# 37. AUTENTICACIÓN TÉCNICA

Utilizar:

ASP.NET Core Identity.

NO:

- passwords manuales;
- hashes propios;
- autenticación casera;
- texto plano.

---

# 38. STACK

Mantener:

C#
.NET
ASP.NET Core
Blazor Web App
PostgreSQL
EF Core
Npgsql
ASP.NET Core Identity.

No cambiar sin autorización.

---

# 39. ARQUITECTURA

MONOLITO MODULAR.

Un proyecto mientras sea suficiente.

NO introducir automáticamente:

- microservicios;
- Clean Architecture multiproyecto;
- Onion;
- Hexagonal;
- CQRS;
- MediatR;
- Event Bus;
- Repository genérico;
- UnitOfWork;
- Redis;
- Kafka;
- RabbitMQ;
- AutoMapper.

---

# 40. EF CORE

DbContext es suficiente para persistencia normal.

Preferir:

- async/await;
- AsNoTracking en lectura;
- proyecciones;
- paginación;
- consultas eficientes.

Evitar N+1.

---

# 41. CREDENCIALES

No guardar secretos reales en:

- código;
- Skill;
- Git;
- documentación;
- logs.

Configuración:

ConnectionStrings:DefaultConnection.

Desarrollo:

User Secrets.

Producción:

variables de entorno o sistema seguro equivalente.

---

# 42. RESPONSIVE — OBLIGATORIO

Toda WebPageSublimation debe ser responsive.

Debe funcionar en:

- móvil;
- tablet;
- laptop;
- desktop.

NO considerar terminada una interfaz que solamente funcione en desktop.

---

# 43. MOBILE FIRST — PROMOTOR

Promotor debe poder completar desde teléfono:

LOGIN
→ CATÁLOGO
→ BUSCAR
→ CLIENTE
→ PROFORMA/PEDIDO
→ GUARDAR
→ CONSULTAR.

Prioridad especial:

- catálogo;
- clientes;
- proformas;
- pedidos.

---

# 44. ADMIN RESPONSIVE

Administrador puede aprovechar desktop.

Pero funciones principales deben continuar siendo utilizables desde tablet y
móvil.

---

# 45. NAVEGACIÓN RESPONSIVE

Desktop:

sidebar/topbar cuando corresponda.

Móvil:

sidebar colapsable, drawer o menú hamburguesa.

No mantener sidebar grande permanentemente visible.

---

# 46. TABLAS RESPONSIVE

No resolver todo con scroll horizontal.

En móvil evaluar:

- cards;
- ocultar información secundaria;
- reorganizar columnas;
- menú de acciones.

Mantener siempre información esencial.

---

# 47. FORMULARIOS RESPONSIVE

En móvil:

- labels legibles;
- inputs cómodos;
- una columna cuando sea necesario;
- validaciones visibles;
- botones accesibles;
- sin zoom manual.

---

# 48. SIN OVERFLOW

No permitir overflow horizontal accidental.

Revisar:

- layout;
- sidebar;
- tablas;
- cards;
- imágenes;
- formularios;
- modales;
- botones;
- textos largos.

---

# 49. DISEÑO SIMONS

Antes de crear UI:

1. revisar identidad Simons;
2. revisar Layout existente;
3. reutilizar componentes;
4. mantener consistencia;
5. diseñar responsive desde el principio.

No diseñar exclusivamente desktop y luego achicar.

---

# 50. DASHBOARD ADMIN

Debe responder rápidamente:

¿qué se vendió?
¿quién lo vendió?
¿a quién?
¿cuánto?
¿cuándo?
¿en qué estado está?

Priorizar información útil sobre gráficos decorativos.

---

# 51. DASHBOARD PROMOTOR

Mantener simple:

- accesos rápidos;
- catálogo;
- mis proformas;
- mis pedidos.

No mostrar información administrativa.

---

# 52. REGLA CODEX — INSPECCIONAR ANTES

Antes de modificar:

1. inspeccionar .csproj;
2. versión .NET;
3. render modes;
4. Program.cs;
5. App.razor;
6. Routes.razor;
7. _Imports.razor;
8. Layout;
9. Pages;
10. wwwroot;
11. appsettings;
12. paquetes;
13. implementaciones relacionadas.

NO asumir.

NO duplicar.

---

# 53. CAMBIO MÍNIMO

INSPECCIONAR
→ ENTENDER
→ PLANIFICAR
→ REUTILIZAR
→ MODIFICAR LO MÍNIMO
→ COMPILAR
→ VERIFICAR.

No realizar refactorizaciones generales durante tareas pequeñas.

---

# 54. NO INVENTAR

Si falta una regla sobre:

- precios;
- descuentos;
- comisión;
- pagos;
- anticipos;
- saldos;
- cancelaciones;
- devoluciones;
- impuestos;
- producción;
- permisos sensibles;
- finanzas;

NO INVENTARLA.

Marcarla como decisión pendiente.

---

# 55. ORDEN DE DESARROLLO

FASE 0
Inspección.

FASE 1
PostgreSQL + EF Core + Identity + Login + Roles.

FASE 2
Promotores.

FASE 3
Categorías + Productos + Catálogo.

FASE 4
Clientes.

FASE 5
Proformas.

FASE 6
Pedidos.

FASE 7
Dashboard + Ventas por promotor.

FASE 8
Finanzas, una vez definidas sus reglas.

RESPONSIVE:

se aplica transversalmente desde FASE 1.

No existe una fase final de "hacer responsive".

---

# 56. V1 — HISTORIA PRINCIPAL

ADMINISTRADOR CREA PROMOTOR
↓
PROMOTOR INICIA SESIÓN
↓
CONSULTA CATÁLOGO
↓
ATIENDE CLIENTE
↓
CREA PROFORMA O PEDIDO
↓
PEDIDO QUEDA REGISTRADO
↓
ADMINISTRADOR LO VE
↓
IDENTIFICA AL PROMOTOR
↓
PUEDE CONSULTAR SUS VENTAS.

Ese es el núcleo de V1.

---

# 57. DEFINITION OF DONE

Una tarea está terminada cuando:

- cumple requerimiento;
- respeta reglas de negocio;
- respeta permisos;
- protege datos históricos;
- autorización está en servidor;
- compila;
- no expone secretos;
- no rompe funcionalidad existente;
- UI mantiene identidad Simons;
- si afecta UI, funciona en móvil/tablet/laptop/desktop;
- no presenta overflow accidental;
- no introduce arquitectura innecesaria.

---

# 58. INVARIANTES

INV-001:
Todo pedido creado por promotor tiene PromoterId.

INV-002:
PromoterId proviene de identidad autenticada.

INV-003:
Promotor no accede a operaciones ajenas sin autorización.

INV-004:
Modificar precio actual no modifica precio histórico.

INV-005:
Desactivar producto no destruye historial.

INV-006:
Desactivar promotor no destruye historial.

INV-007:
Convertir proforma no destruye proforma original.

INV-008:
Cancelar pedido no elimina pedido.

INV-009:
PEDIDO CREADO ≠ PAGO RECIBIDO.

INV-010:
PEDIDO CREADO ≠ COMISIÓN AUTOMÁTICA.

INV-011:
Navegador no es fuente confiable de autorización.

INV-012:
No implementar reglas comerciales no confirmadas.

INV-013:
Toda funcionalidad del promotor debe ser operable desde teléfono.

INV-014:
Una interfaz no está terminada si solamente funciona en desktop.

INV-015:
No debe existir overflow horizontal accidental.

INV-016:
Responsive adapta interfaz; no elimina funcionalidad.

INV-017:
Permisos son iguales independientemente del dispositivo.

INV-018:
Cambios de catálogo no alteran documentos históricos.

---

# 59. DECISIONES DE NEGOCIO PENDIENTES

Estas preguntas deben hacerse a Simons progresivamente.

NO inventar respuestas:

1. ¿Cuáles son las categorías reales?
2. ¿Cuáles son los productos reales?
3. ¿Qué variantes maneja cada producto?
4. ¿Cómo define precios?
5. ¿Existen precios por cantidad?
6. ¿Quién puede cambiar precios?
7. ¿Promotores pueden ofrecer descuentos?
8. ¿Los clientes pertenecen a un promotor?
9. ¿Pueden compartir clientes?
10. ¿Qué información necesita realmente un pedido?
11. ¿Necesitan archivos de diseño?
12. ¿Existe aprobación de diseño?
13. ¿Qué estados reales utiliza un pedido?
14. ¿Quién cambia cada estado?
15. ¿Cómo manejan cancelaciones?
16. ¿Cómo manejan devoluciones?
17. ¿Qué significa exactamente "Finanzas" para Simons?
18. ¿Registran anticipos?
19. ¿Registran saldos?
20. ¿Registran pagos parciales?
21. ¿Cuándo consideran una venta realizada?
22. ¿Cuándo nace el derecho a comisión?
23. ¿Cómo calculan comisión?
24. ¿Qué sucede si se cancela una venta con comisión?
25. ¿Necesitan factura?
26. ¿Necesitan NIT/razón social obligatoriamente?
27. ¿Cómo es el proceso desde pedido hasta producción?
28. ¿Quién participa en producción?
29. ¿Cómo controlan fecha de entrega?
30. ¿Qué problemas actuales quieren eliminar primero?

Estas respuestas deben incorporarse posteriormente como RN-*.

---

# 60. REGLA PARA INVESTIGACIÓN EXTERNA

Información pública de Simons puede utilizarse para:

- branding;
- contexto;
- lenguaje;
- comprensión general del negocio.

NO utilizarla automáticamente para determinar:

- precios;
- productos activos;
- descuentos;
- procesos internos;
- comisiones;
- permisos;
- finanzas;
- políticas comerciales.

Esas decisiones requieren confirmación empresarial.

---

# 61. REGLA FINAL

El objetivo es construir una herramienta que Simons pueda utilizar diariamente.

Debe ayudar a:

VENDER
+
COTIZAR
+
REGISTRAR
+
ORGANIZAR
+
CONTROLAR
+
IDENTIFICAR QUIÉN VENDIÓ.

No desarrollar para impresionar técnicamente.

Desarrollar para resolver el negocio.

Para cada tarea:

INSPECCIONAR
→ ENTENDER REGLA
→ REUTILIZAR
→ IMPLEMENTAR LO MÍNIMO
→ PROTEGER DATOS
→ HACER RESPONSIVE
→ COMPILAR
→ VERIFICAR.