# PRUEBAS DE PUNTA A PUNTA — WEBPAGESUBLIMATION / SIMONS PUBLICIDAD

Este documento define la estrategia obligatoria de pruebas del sistema.

Las pruebas deben verificar las REGLAS DE NEGOCIO utilizando la aplicación
realmente levantada y una base PostgreSQL destinada a desarrollo/pruebas.

NO considerar suficiente:

- que compile;
- que una página abra;
- que existan pruebas unitarias;
- que visualmente parezca funcionar.

Cuando sea técnicamente aplicable, los flujos críticos deben ejercerse como lo
haría un usuario real, incluyendo:

- peticiones HTTP reales;
- formularios;
- autenticación;
- cookies;
- autorización;
- antiforgery/CSRF;
- PostgreSQL;
- redirecciones;
- códigos HTTP;
- persistencia;
- roles;
- responsive.

---

# 1. OBJETIVO

Verificar que WebPageSublimation funciona como un sistema completo para Simons
Publicidad.

Los flujos principales a proteger son:

ADMINISTRADOR
→ ADMINISTRA CATÁLOGO
→ CREA PROMOTOR

PROMOTOR
→ INICIA SESIÓN
→ CONSULTA CATÁLOGO
→ CREA PROFORMA
→ CREA/CONVIERTE PEDIDO

ADMINISTRADOR
→ RECIBE PEDIDO
→ IDENTIFICA PROMOTOR
→ CONSULTA INFORMACIÓN COMERCIAL

Las pruebas deben comprobar tanto el camino correcto como intentos inválidos o no
autorizados.

---

# 2. REGLAS GENERALES DE PRUEBA

## PR-001 — Base de datos

Las pruebas de integración/E2E que necesiten persistencia deben utilizar
PostgreSQL.

NO utilizar automáticamente una base diferente que pueda ocultar diferencias de
comportamiento respecto a PostgreSQL.

Nunca ejecutar pruebas destructivas contra producción.

---

## PR-002 — Datos de prueba

Utilizar datos identificables como pruebas.

Ejemplos conceptuales:

Administrador de prueba
Promotor A
Promotor B
Cliente Prueba A
Cliente Prueba B
Producto Prueba A

NO utilizar datos reales sensibles innecesariamente.

---

## PR-003 — Limpieza

Las pruebas que escriban información deben:

- ejecutarse en entorno de desarrollo/pruebas;
- dejar identificables los registros generados;
- permitir su limpieza.

Antes de una demostración al cliente, comprobar que no queden datos basura.

---

# 3. AUTENTICACIÓN

## PR-AUTH-001 — Administrador correcto

DADO un administrador activo

CUANDO introduce credenciales correctas

ENTONCES:

- inicia sesión;
- recibe acceso administrativo;
- puede acceder a rutas autorizadas.

---

## PR-AUTH-002 — Contraseña incorrecta

DADO un usuario existente

CUANDO introduce contraseña incorrecta

ENTONCES:

- NO inicia sesión;
- NO obtiene cookie/sesión autenticada;
- recibe mensaje apropiado;
- NO accede a información protegida.

---

## PR-AUTH-003 — Promotor correcto

DADO un promotor activo

CUANDO inicia sesión correctamente

ENTONCES:

- entra al área correspondiente;
- puede consultar catálogo;
- puede acceder a sus funciones comerciales;
- NO obtiene permisos administrativos.

---

## PR-AUTH-004 — Promotor desactivado

DADO un promotor desactivado

CUANDO intenta autenticarse o utilizar una sesión según el comportamiento
implementado

ENTONCES:

- no debe obtener acceso no autorizado;
- sus pedidos históricos permanecen;
- sus proformas históricas permanecen.

---

# 4. AUTORIZACIÓN POR ROL

Probar como mínimo:

| Escenario | Resultado esperado |
|---|---|
| Admin → área administrativa | Permitido |
| Promotor → área de promotor | Permitido |
| Anónimo → área administrativa | Denegado/Login |
| Anónimo → área privada promotor | Denegado/Login |
| Promotor → administración de productos | Denegado |
| Promotor → administración de usuarios | Denegado |
| Promotor → finanzas | Denegado |
| Promotor → pedido propio | Permitido |
| Promotor → pedido ajeno | Denegado |
| Promotor → proforma propia | Permitido |
| Promotor → proforma ajena | Denegado |

No verificar autorización solamente mirando si un botón está oculto.

Realizar la petición directamente contra el recurso cuando corresponda.

---

# 5. IDOR — PRUEBA CRÍTICA

IDOR = Insecure Direct Object Reference
(referencia directa insegura a objetos).

Esta prueba es OBLIGATORIA.

Crear:

Promotor A
Promotor B

Promotor A crea:

PED-TEST-A

Promotor B crea:

PED-TEST-B

Autenticarse como Promotor A.

Intentar acceder directamente al identificador de PED-TEST-B.

RESULTADO ESPERADO:

DENEGADO.

Realizar la misma prueba para:

- pedidos;
- proformas;
- clientes cuando estén restringidos;
- archivos asociados;
- cualquier recurso privado identificado mediante ID.

No confiar únicamente en filtros visuales.

---

# 6. CATÁLOGO

## PR-CAT-001 — Producto activo

DADO un producto activo

ENTONCES:

- aparece en catálogo;
- puede encontrarse mediante los mecanismos implementados;
- muestra información comercial autorizada.

---

## PR-CAT-002 — Producto desactivado

DADO un producto utilizado históricamente

CUANDO administrador lo desactiva

ENTONCES:

- deja de estar disponible para nuevas operaciones cuando corresponda;
- continúa existiendo en pedidos históricos;
- continúa existiendo en proformas históricas.

---

## PR-CAT-003 — Edición

DADO un producto

CUANDO administrador modifica información editable

ENTONCES:

- los nuevos datos aparecen donde corresponde;
- no se destruye información histórica.

---

# 7. PRECIOS

## PR-PRE-001 — Precio actual

DADO un producto con:

Precio = Bs 25

CUANDO se agrega a una nueva operación

ENTONCES:

UnitPrice = Bs 25

según las reglas comerciales vigentes.

---

## PR-PRE-002 — Congelamiento histórico

DADO:

Producto = Bs 25

Y existe:

PEDIDO A
10 × Bs 25
Total = Bs 250

CUANDO administrador cambia:

Producto = Bs 30

ENTONCES:

CATÁLOGO NUEVO:
Bs 30

PEDIDO A:
Bs 25 por unidad
Bs 250 total

El pedido histórico NO cambia.

---

## PR-PRE-003 — Precio manipulado desde navegador

Intentar modificar manualmente desde una petición:

UnitPrice
Subtotal
Total

RESULTADO:

El servidor debe validar/recalcular según las reglas correspondientes.

No confiar ciegamente en valores enviados por el navegador.

---

# 8. CLIENTES

## PR-CLI-001 — Crear cliente

DADO un usuario autorizado

CUANDO registra un cliente válido

ENTONCES:

- se guarda correctamente;
- queda disponible según permisos.

---

## PR-CLI-002 — Privacidad

Si la política de Simons asigna clientes por promotor:

Promotor A NO debe poder consultar clientes privados de Promotor B.

Probar mediante petición directa, no solamente desde UI.

---

# 9. PROFORMAS

## PR-COT-001 — Crear proforma

DADO un promotor autenticado

CUANDO:

- selecciona cliente;
- selecciona productos;
- introduce cantidades válidas;
- confirma;

ENTONCES se crea una proforma que conserva:

- número;
- fecha;
- cliente;
- PromoterId;
- productos;
- cantidades;
- precios;
- subtotales;
- total.

---

## PR-COT-002 — Promotor automático

Intentar manipular el PromoterId enviado desde navegador.

RESULTADO ESPERADO:

La operación debe pertenecer al usuario autenticado.

El navegador NO decide quién recibe la atribución comercial.

---

## PR-COT-003 — Proforma ajena

Promotor A intenta consultar proforma de Promotor B.

RESULTADO:

DENEGADO.

---

## PR-COT-004 — Histórico

Modificar posteriormente:

- producto;
- precio;
- variante;

NO debe alterar incorrectamente una proforma histórica.

---

# 10. PROFORMA → PEDIDO

## PR-CONV-001 — Conversión correcta

DADO una proforma válida

CUANDO se convierte en pedido

ENTONCES:

- se crea nuevo pedido;
- conserva cliente;
- conserva promotor;
- conserva productos;
- conserva cantidades;
- conserva precios;
- conserva referencia a la proforma origen.

---

## PR-CONV-002 — No destruir origen

Después de convertir:

la proforma original continúa existiendo.

---

## PR-CONV-003 — Autorización

Promotor A NO puede convertir en pedido una proforma perteneciente a Promotor B.

Intentarlo modificando directamente el ID.

RESULTADO:

DENEGADO.

---

## PR-CONV-004 — Doble conversión

Si la regla final del negocio establece que una proforma solamente puede originar
un pedido:

intentar convertirla dos veces.

RESULTADO:

no crear accidentalmente pedidos duplicados.

Aplicar esta prueba solamente cuando dicha regla quede confirmada.

---

# 11. PEDIDO DIRECTO

## PR-PED-001 — Pedido sin proforma

DADO un promotor autenticado

CUANDO registra un pedido directamente

ENTONCES:

- se crea correctamente;
- QuoteId puede permanecer vacío;
- PromoterId corresponde al usuario autenticado;
- aparece para el administrador.

---

# 12. PEDIDOS Y PROMOTOR

## PR-PED-002 — Atribución

DADO Promotor A autenticado

CUANDO crea PEDIDO A

ENTONCES:

PEDIDO A.PromoterId = Promotor A.

---

## PR-PED-003 — Manipulación

Modificar manualmente una petición intentando:

PromoterId = Promotor B.

RESULTADO:

el sistema NO debe atribuir el pedido a Promotor B.

---

## PR-PED-004 — Administrador

Después de crear el pedido:

el administrador debe poder consultar:

- número;
- fecha;
- cliente;
- promotor;
- total;
- estado.

---

## PR-PED-005 — Cambio posterior de cliente/promotor

DADO:

PEDIDO A
Promotor = Carlos
Cliente = Empresa ABC

Posteriormente cambia la asignación actual del cliente.

RESULTADO:

PEDIDO A continúa mostrando a Carlos como promotor histórico.

---

# 13. ESTADOS DEL PEDIDO

Probar los estados que finalmente estén implementados.

Para V1, si se confirman:

Pending
Confirmed
InProduction
Ready
Delivered
Cancelled

Verificar:

- estado inicial correcto;
- transiciones permitidas;
- rechazo de estados inválidos;
- permisos para cambiar estado;
- persistencia.

No inventar transiciones que Simons todavía no haya definido.

---

# 14. CANCELACIÓN

DADO un pedido existente

CUANDO se cancela mediante un usuario autorizado

ENTONCES:

- permanece en base de datos;
- conserva número;
- conserva cliente;
- conserva promotor;
- conserva información histórica;
- Status = Cancelled.

Cancelar NO significa DELETE.

---

# 15. COMISIONES / VENTAS POR PROMOTOR

Como la comisión inicialmente será manual:

NO probar una fórmula inexistente.

Sí verificar:

DADO:

Promotor A → pedidos válidos
Promotor B → pedidos válidos

ENTONCES administrador puede distinguir correctamente:

Promotor A
→ sus pedidos
→ importe correspondiente

Promotor B
→ sus pedidos
→ importe correspondiente

No mezclar operaciones entre promotores.

---

# 16. FINANZAS

Hasta definir el alcance exacto del módulo:

probar únicamente reglas financieras confirmadas.

Regla mínima obligatoria:

PEDIDO CREADO ≠ PAGO RECIBIDO.

Crear un pedido NO debe generar automáticamente un ingreso financiero salvo que
posteriormente Simons defina explícitamente esa regla.

---

# 17. VALIDACIÓN DE FORMULARIOS

Ejercer formularios reales.

Probar:

- campos válidos;
- campos requeridos vacíos;
- IDs inexistentes;
- cantidades inválidas;
- valores negativos;
- campos opcionales vacíos;
- caracteres especiales;
- doble envío;
- datos manipulados.

No limitarse a llamar directamente Services desde tests.

La aplicación completa debe procesar correctamente los formularios.

---

# 18. ANTIFORGERY / CSRF

Toda operación sensible mediante formulario debe verificarse según la protección
antiforgery utilizada por ASP.NET Core/Blazor.

Probar cuando corresponda:

POST válido + token válido
→ permitido.

POST sin token requerido
→ rechazado.

POST con token inválido
→ rechazado.

Especial atención a:

- crear producto;
- editar producto;
- crear usuario;
- desactivar usuario;
- crear proforma;
- crear pedido;
- cambiar estado;
- cancelar;
- logout.

---

# 19. REDIRECCIONES

Si la aplicación utiliza parámetros como:

returnUrl
volverA
redirect

probar que solamente permitan destinos seguros.

Intentar:

URL externa;
doble slash;
backslash;
valores codificados;
entradas malformadas.

Nunca permitir Open Redirect (redirección abierta).

---

# 20. CÓDIGOS HTTP

Verificar comportamiento coherente.

Ejemplos:

Recurso existente:
200 cuando corresponda.

Recurso inexistente:
404 cuando corresponda.

No autenticado:
redirección/login o 401 según arquitectura.

Autenticado sin autorización:
403/404/redirección según diseño de seguridad.

Rate limit:
429 si posteriormente existe.

Error de validación:
NO debe convertirse innecesariamente en 500.

No devolver 200 para recursos inexistentes solamente porque Blazor renderizó una
página.

---

# 21. MANEJO DE ERRORES

Provocar entradas inválidas controladas.

Verificar:

- no mostrar stack trace;
- no mostrar connection string;
- no mostrar secretos;
- no devolver detalles internos innecesarios;
- mensaje entendible;
- logging técnico cuando corresponda.

---

# 22. RESPONSIVE — PRUEBAS OBLIGATORIAS

Responsive forma parte de la Definition of Done.

Toda página visual nueva o modificada debe comprobarse al menos en tamaños
representativos.

Referencias:

320-375 px → móvil pequeño
390-430 px → móvil
768 px → tablet
1024-1366 px → laptop
1440 px o superior → desktop

Son tamaños de prueba, NO breakpoints obligatorios.

---

# 23. RESPONSIVE — LOGIN

Verificar en móvil:

- logo visible;
- formulario completo;
- inputs utilizables;
- botón accesible;
- mensajes de error visibles;
- teclado móvil no vuelve imposible completar el formulario;
- no existe overflow horizontal.

---

# 24. RESPONSIVE — NAVEGACIÓN

Desktop:
navegación completa.

Móvil:
menú colapsable/drawer/hamburguesa según implementación.

Probar REALMENTE:

- abrir;
- cerrar;
- navegar;
- volver;
- cerrar sesión.

No considerar suficiente que el icono hamburguesa aparezca.

La interacción debe funcionar.

---

# 25. RESPONSIVE — CATÁLOGO

Comprobar:

- cards adaptables;
- imágenes proporcionadas;
- nombres legibles;
- precios visibles;
- búsqueda utilizable;
- filtros utilizables;
- botones accesibles;
- sin overflow.

---

# 26. RESPONSIVE — PROFORMA

Desde viewport móvil debe ser posible completar:

LOGIN
→ CLIENTE
→ PRODUCTO
→ CANTIDAD
→ AGREGAR
→ REVISAR
→ GUARDAR PROFORMA

sin:

- zoom;
- scroll horizontal accidental;
- controles inaccesibles.

---

# 27. RESPONSIVE — PEDIDO

Desde móvil debe ser posible:

LOGIN
→ NUEVO PEDIDO
→ CLIENTE
→ PRODUCTOS
→ CANTIDADES
→ INFORMACIÓN DEL TRABAJO
→ GUARDAR

y posteriormente:

MIS PEDIDOS
→ ABRIR PEDIDO
→ CONSULTAR ESTADO.

---

# 28. RESPONSIVE — TABLAS

Para cada tabla administrativa importante verificar:

Desktop:
información completa y utilizable.

Móvil:
información esencial accesible.

Si utiliza cards/listas en móvil, verificar que no se pierdan acciones críticas.

Si utiliza scroll horizontal justificadamente, comprobar que sigue siendo usable.

---

# 29. RESPONSIVE — MODALES

Probar modales en viewport pequeño.

Verificar:

- no exceden pantalla de forma inutilizable;
- encabezado visible;
- contenido desplazable cuando corresponda;
- botones accesibles;
- pueden cerrarse.

---

# 30. RESPONSIVE — INTERACCIÓN TÁCTIL

Comprobar que:

- botones principales son fáciles de pulsar;
- acciones destructivas no están pegadas a acciones normales;
- no existen funciones esenciales dependientes de hover;
- selects e inputs son utilizables desde teléfono.

---

# 31. PERSISTENCIA

Reiniciar la aplicación cuando sea pertinente.

Verificar que:

- productos persisten;
- clientes persisten;
- proformas persisten;
- pedidos persisten;
- atribución de promotor persiste;
- estados persisten.

Si el despliegue utiliza contenedores posteriormente, repetir pruebas relevantes
después de recrear el contenedor.

---

# 32. SESIONES Y DATA PROTECTION

Cuando el despliegue definitivo esté definido, verificar persistencia adecuada de
Data Protection Keys.

Una actualización/reinicio normal del sistema no debería cerrar sesiones
inesperadamente si la arquitectura de despliegue requiere conservarlas.

No asumir la configuración hasta conocer hosting definitivo.

---

# 33. CONCURRENCIA

Probar operaciones simultáneas relevantes.

Especialmente:

- creación simultánea de proformas;
- creación simultánea de pedidos;
- numeración comercial.

Dos solicitudes concurrentes NO deben producir el mismo:

PRO-XXXX

o

PED-XXXX.

---

# 34. DOBLE ENVÍO

Simular doble click / doble POST cuando corresponda.

Verificar especialmente:

- crear pedido;
- convertir proforma;
- operaciones financieras futuras.

Evitar duplicados accidentales cuando la operación deba ser única.

---

# 35. POSTGRESQL

Verificar contra PostgreSQL real del entorno de prueba:

- migrations;
- restricciones;
- claves foráneas;
- decimal/numeric;
- índices relevantes;
- concurrencia;
- consultas;
- persistencia.

No asumir que comportamiento observado con un proveedor de BD diferente será
idéntico.

---

# 36. CULTURA BOLIVIANA

Verificar formato definido para el sistema.

Cuando corresponda:

- moneda Bs;
- fechas;
- decimales;
- cultura es-BO.

No confundir representación visual con valor almacenado.

Los cálculos continúan utilizando decimal.

---

# 37. BUILD Y TESTS AUTOMATIZADOS

Antes de cerrar una tarea relevante:

BUILD:
debe completar correctamente.

TESTS:
ejecutar pruebas relacionadas existentes.

E2E/INTEGRACIÓN:
ejecutar flujos afectados cuando la modificación pueda romperlos.

Un build correcto NO sustituye las pruebas funcionales.

---

# 38. REGRESIÓN

Cada bug real descubierto debe evaluarse para convertirse en prueba de regresión.

Proceso:

BUG DESCUBIERTO
→ REPRODUCIR
→ CORREGIR
→ CREAR PRUEBA CUANDO SEA RAZONABLE
→ VERIFICAR QUE NO REGRESE.

Especial prioridad:

- autorización;
- precios;
- histórico;
- PromoterId;
- numeración;
- proforma → pedido;
- finanzas;
- seguridad.

---

# 39. REPORTE DE PRUEBAS

Después de una ronda completa, generar reporte utilizando esta estructura:

# Pruebas de punta a punta — resultados

**Fecha:**
**Versión/commit:**
**Entorno:**
**Base de datos:**
**Método:**

## 1. Reglas de negocio verificadas

Por cada regla:

### RN-XXX — Nombre

Qué se hizo.

Datos utilizados.

Resultado esperado.

Resultado observado.

ESTADO:
PASS / FAIL

## 2. Autorización

| Escenario | Esperado | Obtenido | Estado |
|---|---|---|---|

## 3. Flujos principales

### Administrador
Resultado.

### Promotor
Resultado.

### Proforma
Resultado.

### Pedido
Resultado.

### Proforma → Pedido
Resultado.

## 4. Responsive

| Pantalla | Móvil | Tablet | Desktop |
|---|---|---|---|
| Login | PASS/FAIL | PASS/FAIL | PASS/FAIL |
| Catálogo | PASS/FAIL | PASS/FAIL | PASS/FAIL |
| Clientes | PASS/FAIL | PASS/FAIL | PASS/FAIL |
| Proformas | PASS/FAIL | PASS/FAIL | PASS/FAIL |
| Pedidos | PASS/FAIL | PASS/FAIL | PASS/FAIL |
| Administración | PASS/FAIL | PASS/FAIL | PASS/FAIL |

## 5. Seguridad

- autenticación;
- autorización;
- IDOR;
- CSRF;
- manipulación de datos;
- redirecciones;
- errores.

## 6. Fallos encontrados y corregidos

Por cada fallo:

### X.X — Nombre

PROBLEMA:
qué fallaba.

IMPACTO:
qué podía provocar.

CAUSA:
por qué ocurría.

CORRECCIÓN:
qué se modificó.

REGRESIÓN:
qué prueba evita que vuelva.

## 7. Persistencia / PostgreSQL

Resultados.

## 8. Cobertura automatizada

Número de pruebas.

Pruebas nuevas.

Pruebas fallidas.

## 9. Datos generados

Indicar:

- usuarios;
- clientes;
- proformas;
- pedidos;
- otros registros de prueba.

Indicar si requieren limpieza.

## 10. Resultado final

No utilizar solamente:

"todo funciona".

Informar:

PASS:
qué fue realmente comprobado.

FAIL:
qué sigue fallando.

NO PROBADO:
qué no pudo verificarse.

PENDIENTE:
qué depende de una regla del negocio todavía no definida.

---

# 40. REGLA FINAL DE PRUEBAS

Una funcionalidad crítica NO se considera terminada solamente porque:

COMPILA.

Debe comprobarse según corresponda:

CÓDIGO
+
BASE DE DATOS
+
AUTENTICACIÓN
+
AUTORIZACIÓN
+
REGLAS DE NEGOCIO
+
HTTP/FORMULARIOS
+
RESPONSIVE
+
SEGURIDAD
+
PERSISTENCIA.

Especialmente para:

LOGIN
PROMOTORES
CATÁLOGO
CLIENTES
PROFORMAS
PEDIDOS
VENTAS POR PROMOTOR
FINANZAS.

El objetivo no es conseguir pruebas verdes.

El objetivo es demostrar que WebPageSublimation respeta las reglas reales de
Simons Publicidad.