# Plan y avance de WebPageSublimation

Este documento mantiene el plan de ejecución y el estado real del sistema. Debe actualizarse
al comenzar y terminar cada bloque de trabajo para poder retomar el proyecto sin depender del
historial de una conversación.

### Regla de continuidad

Cada bloque de trabajo debe cerrar con cuatro elementos:

1. qué se implementó;
2. qué se verificó y con qué resultado;
3. qué quedó pendiente o bloqueado;
4. el plan concreto del siguiente bloque.

No se reportará solamente que una tarea terminó. El siguiente trabajo quedará planeado en
este archivo antes de cerrar cada entrega.

## 1. Fuentes de trabajo

El desarrollo se guía, en este orden, por:

1. `02-Negocio/DOCUMENTACION-FUNCIONAL.md`, porque contiene las reglas específicas de Simons
   Publicidad y se declara como la skill maestra del sistema.
2. `02-Negocio/PRUEBAS-PUNTA-A-PUNTA.md`, para los criterios de verificación funcional,
   seguridad y responsive.
3. `01-Estandar-Tecnico/ESTANDAR-TECNICO-MONOLITO.md`, para las prácticas técnicas que no
   contradigan la skill funcional.
4. `DESPLIEGUE.md`, después de corregir las referencias heredadas que no corresponden todavía
   a WebPageSublimation.

Cuando una regla de negocio sea confirmada directamente por Simons, esa confirmación tiene
prioridad y debe incorporarse a la documentación funcional.

## 2. Decisión de arquitectura

Se utilizará un **monolito modular en un solo proyecto** mientras el tamaño del sistema lo
permita.

Motivos:

- es la arquitectura indicada específicamente en la sección 39 de la skill funcional;
- el proyecto actual ya tiene esa forma;
- evita introducir separación y abstracciones antes de que exista lógica que las justifique;
- permite mantener límites claros mediante carpetas, servicios e interfaces por capacidad.

El requisito genérico de cuatro proyectos del estándar técnico queda registrado como un
**desvío deliberado**. Se reconsiderará únicamente si el proyecto crece hasta dificultar las
pruebas, los límites entre módulos o el mantenimiento.

Estructura inicial prevista:

```text
WebPageSublimation/
├── Components/
│   ├── Account/
│   ├── Admin/
│   ├── Layout/
│   ├── Pages/
│   └── Shared/
├── Data/
│   ├── Configurations/
│   ├── Migrations/
│   └── Seed/
├── Features/
│   ├── Auth/
│   ├── Promotores/
│   ├── Catalogo/
│   ├── Clientes/
│   ├── Proformas/
│   └── Pedidos/
├── Security/
├── wwwroot/
└── Program.cs
```

Las carpetas se crearán cuando cada módulo se implemente; no se crearán estructuras vacías.

## 3. Estado inicial verificado

Fecha de inspección: **2026-09-18**.

- Proyecto Blazor Web App sobre `.NET 10`.
- Un solo proyecto: `WebPageSublimation.csproj`.
- Render interactivo de servidor disponible, todavía sin páginas de negocio.
- Interfaz original de la plantilla de Blazor.
- No existe PostgreSQL ni Entity Framework Core.
- No existe `DbContext`, esquema ni migraciones.
- No existe ASP.NET Core Identity.
- No existen roles, login ni autorización por áreas.
- No existen pruebas automatizadas.
- No existe configuración de despliegue implementada en el código actual.
- El build inicial termina con **0 errores y 0 advertencias**.

## 4. Fase 1 — Fundamentos, identidad y acceso

### Objetivo

Entregar una base ejecutable y segura sobre la que puedan construirse promotores, catálogo,
clientes, proformas y pedidos.

Al terminar la fase debe ser posible:

1. levantar el sistema contra PostgreSQL;
2. aplicar migraciones;
3. crear los roles `Administrador` y `Promotor` mediante seed idempotente;
4. iniciar y cerrar sesión;
5. proteger rutas por autenticación y rol;
6. entrar a un panel inicial según el rol;
7. usar la interfaz desde móvil y escritorio.

### Bloque 1.1 — Persistencia

- [x] Agregar EF Core 10, Npgsql y la convención `snake_case`.
- [x] Definir `ApplicationUser` dentro de la infraestructura del proyecto.
- [x] Crear `AppDbContext` integrado con Identity.
- [x] Configurar `IDbContextFactory<AppDbContext>` para las operaciones de Blazor.
- [x] Usar `ConnectionStrings:DefaultConnection` como nombre único de configuración.
- [ ] Guardar la conexión local mediante User Secrets o variable de entorno.
- [x] Crear la primera migración.
- Verificar la migración contra PostgreSQL real de desarrollo.

Resultado verificable: la aplicación arranca, conecta a PostgreSQL y crea/actualiza el esquema
sin contener secretos en el repositorio.

### Bloque 1.2 — Identity y roles

- Configurar ASP.NET Core Identity con cookies y roles.
- Crear las constantes de roles `Administrador` y `Promotor`.
- Implementar un seed idempotente para los dos roles.
- Permitir la creación inicial del administrador mediante configuración segura.
- No incluir contraseñas reales o de demostración en Git.
- Preparar el bloqueo de acceso para usuarios desactivados sin eliminar su historial futuro.

Resultado verificable: ejecutar el seed varias veces no duplica roles ni usuarios.

### Bloque 1.3 — Autenticación

- Crear página de login responsive.
- Implementar el POST con antiforgery y mensajes de error controlados.
- Crear logout con validación antiforgery explícita.
- Configurar las rutas de acceso denegado y retorno seguro.
- Validar cualquier `returnUrl` para impedir redirecciones externas.
- Mantener deshabilitado el registro público.

Resultado verificable: credenciales correctas crean sesión; credenciales incorrectas no la
crean; un POST sin token válido se rechaza; logout termina la sesión.

### Bloque 1.4 — Autorización y navegación

- Proteger `/admin/*` para `Administrador`.
- Proteger `/promotor/*` para `Promotor`.
- Redirigir visitantes anónimos al login.
- Mostrar navegación acorde con el rol sin usarla como único control de seguridad.
- Crear paneles iniciales sencillos para administrador y promotor.
- Asegurar que un promotor no pueda entrar directamente al área administrativa.

Resultado verificable: la autorización funciona mediante peticiones directas, además de la
interfaz.

### Bloque 1.5 — Base visual responsive

- Sustituir la apariencia de plantilla por la identidad de Simons: negro, blanco y verde lima.
- No alterar ni inventar un logo; usar solamente un recurso confirmado cuando esté disponible.
- Crear navegación adaptable para móvil, tablet y escritorio.
- Evitar sidebar permanente en pantallas pequeñas.
- Comprobar login, menú y paneles en 360, 768, 1366 y 1440 píxeles.
- Comprobar que no exista overflow horizontal accidental.

Resultado verificable: autenticación y navegación son utilizables desde teléfono sin zoom ni
controles inaccesibles.

### Bloque 1.6 — Seguridad y operación básica

- Ordenar el pipeline: proxy cuando corresponda, errores, HTTPS local, autenticación,
  autorización, antiforgery y rate limiting.
- Aplicar rate limiting al login.
- Configurar cultura `es-BO`.
- Evitar stack traces y datos sensibles fuera de desarrollo.
- Preparar persistencia de Data Protection para el despliegue, sin activar configuraciones de
  producción inexistentes en desarrollo.
- Registrar eventos relevantes de autenticación sin contraseñas ni secretos.

### Bloque 1.7 — Pruebas y cierre

- Agregar pruebas relevantes de configuración y reglas de acceso.
- Probar login correcto, contraseña incorrecta y usuario sin autorización.
- Probar administrador en área administrativa.
- Probar promotor en área de promotor.
- Probar promotor intentando acceder directamente a administración.
- Probar antiforgery en login/logout.
- Probar redirecciones locales y entradas maliciosas.
- Ejecutar build sin advertencias.
- Ejercer la aplicación levantada mediante HTTP real.
- Registrar resultados y pendientes en este documento.

## 5. Criterios de aceptación de la Fase 1

La fase se considerará terminada solamente cuando:

- PostgreSQL sea la base usada en la verificación de integración;
- las migraciones puedan aplicarse desde una base vacía;
- los roles se creen de manera idempotente;
- no exista registro público;
- login y logout funcionen con antiforgery;
- administrador y promotor tengan accesos separados;
- una petición directa del promotor a `/admin` sea rechazada;
- los secretos permanezcan fuera del repositorio;
- la interfaz funcione en móvil, tablet y escritorio;
- el build termine sin errores ni advertencias;
- se registren las pruebas ejecutadas y sus resultados.

## 6. Decisiones necesarias antes o durante la fase

Estas decisiones no bloquean la preparación técnica inicial, pero deben resolverse antes de
cerrar la fase:

1. Dirección de correo o nombre de usuario del primer administrador.
2. Método para entregar su contraseña inicial sin guardarla en Git.
3. Recurso oficial del logo de Simons, si debe mostrarse desde la Fase 1.
4. PostgreSQL local existente o autorización para usar un contenedor de desarrollo.

Como valor técnico provisional se usará correo electrónico como identificador de inicio de
sesión. No se crearán credenciales definitivas hasta recibir los datos correspondientes.

## 7. Almacenamiento de imágenes

Las imágenes de productos se almacenarán como archivos; PostgreSQL guardará solamente sus
metadatos y una ruta relativa estable. No se guardarán imágenes binarias dentro de la base.

- En desarrollo: directorio local configurable fuera del código fuente publicado.
- En producción: volumen persistente de Coolify montado, inicialmente previsto en
  `/app-data/uploads`.
- En base de datos: ruta relativa, nombre original, tipo MIME, tamaño y dimensiones cuando
  corresponda.
- La aplicación generará nombres internos únicos y validará tipo, tamaño y contenido antes de
  aceptar un archivo.
- La presentación accederá a las imágenes mediante una ruta controlada por la aplicación.

Esta solución evita que un redespliegue borre las imágenes y permite migrar más adelante a un
servicio de almacenamiento de objetos sin cambiar las entidades comerciales. La carga y
gestión de imágenes se implementará en la Fase 3 junto con Catálogo.

## 8. Riesgos controlados

- No se implementarán todavía catálogo, clientes, proformas, pedidos ni finanzas.
- No se inventarán productos, precios, comisiones o reglas financieras.
- No se tomará el texto actual de `DESPLIEGUE.md` como evidencia de funcionalidades ya
  implementadas.
- El acceso visual por rol siempre tendrá una comprobación equivalente en el servidor.
- La desactivación futura de promotores deberá conservar el historial comercial.

## 9. Bitácora

### 2026-09-18 — Inspección y planificación

**Realizado:**

- leídas la skill funcional, el estándar técnico, la estrategia E2E y la guía de despliegue;
- inspeccionados `.csproj`, `Program.cs`, `App.razor`, rutas, imports, layout, páginas,
  configuración y recursos estáticos;
- ejecutado el build inicial con 0 errores y 0 advertencias;
- detectada y resuelta para el plan la contradicción entre cuatro proyectos y un proyecto;
- definido el alcance y los criterios de aceptación de la Fase 1.

**Estado:** planificación de Fase 1 terminada; implementación todavía no iniciada.

**Siguiente paso:** ejecutar el Bloque 1.1, comenzando por paquetes, modelo de Identity,
`AppDbContext`, configuración segura de PostgreSQL y primera migración.

### 2026-09-18 — Inicio del Bloque 1.1

**En curso:**

- incorporación de los paquetes de EF Core, Npgsql, Identity y convención `snake_case`;
- creación de `ApplicationUser` y `AppDbContext`;
- registro de `DbContext` y `IDbContextFactory`;
- unificación de la configuración bajo `ConnectionStrings:DefaultConnection`;
- habilitación de User Secrets para desarrollo;
- definición de la estrategia de almacenamiento persistente de imágenes.

**Cadena de conexión:** todavía no recibida ni necesaria para compilar. Será necesaria para
aplicar y verificar la migración contra PostgreSQL real.

### 2026-09-18 — Persistencia e Identity preparados

**Realizado:**

- agregados EF Core 10, Npgsql, Identity EF y la convención `snake_case`;
- creado `ApplicationUser` con estado activo y fecha de creación;
- creado `AppDbContext` y registrados `DbContext` y `IDbContextFactory`;
- configuradas las tablas de Identity con nombres en `snake_case`;
- generada la migración inicial `InitialIdentity`;
- configuradas cookies de Identity, política de contraseña y bloqueo por intentos fallidos;
- definidos los roles `Administrador` y `Promotor`;
- creado el inicializador idempotente de migraciones, roles y administrador inicial;
- verificado el build con 0 errores y 0 advertencias.

**Pendiente para cerrar el Bloque 1.1:** guardar una cadena de desarrollo y aplicar la
migración contra PostgreSQL real.

La cadena debe guardarse desde la raíz de la solución con:

```powershell
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=SERVIDOR;Port=5432;Database=BASE;Username=USUARIO;Password=CONTRASEÑA" --project .\WebPageSublimation\WebPageSublimation.csproj
```

No copiar el valor resultante a `appsettings.json` ni incorporarlo al control de versiones.

## 10. Punto actual y secuencia de continuación

### Estado resumido

| Trabajo | Estado | Dependencia |
|---|---|---|
| Inspección inicial | Terminado | Ninguna |
| Decisión de arquitectura | Terminada | Monolito modular de un proyecto |
| Paquetes de persistencia | Terminado | Ninguna |
| `ApplicationUser` y `AppDbContext` | Terminado | Ninguna |
| Migración `InitialIdentity` | Generada | Falta aplicarla |
| Roles y seed idempotente | Implementado, no ejecutado | PostgreSQL configurado |
| Cadena de conexión | Pendiente | Configuración del propietario |
| Verificación contra PostgreSQL | Pendiente | Cadena de conexión |
| Login y logout | Implementados, no probados por HTTP | PostgreSQL operativo |
| Autorización por rol | Implementada, no probada por HTTP | PostgreSQL y usuarios de prueba |
| Base visual responsive | Implementada, pendiente de inspección visual | Aplicación levantada |
| Pruebas HTTP y de integración | Pendiente | Aplicación y PostgreSQL operativos |

### Próximos pasos exactos

1. Configurar `ConnectionStrings:DefaultConnection` mediante User Secrets.
2. Ejecutar `dotnet ef database update` contra la base de desarrollo.
3. Arrancar la aplicación y comprobar que el seed crea los dos roles sin duplicarlos.
4. Configurar de forma segura el correo y la contraseña del primer administrador.
5. Ejecutar nuevamente el inicializador y verificar la cuenta administrativa.
6. Implementar la pantalla y el endpoint de login.
7. Implementar logout con antiforgery.
8. Crear acceso denegado y validación segura de `returnUrl`.
9. Crear las áreas iniciales `/admin` y `/promotor`, protegidas por rol.
10. Sustituir la navegación de plantilla por la navegación responsive de Simons.
11. Probar autenticación, autorización, cookies, antiforgery y acceso directo a rutas.
12. Ejecutar pruebas responsive y cerrar la Fase 1 con un reporte de resultados.

### Punto de reanudación

Al retomar el proyecto, leer primero esta sección. Si la cadena ya está configurada, continuar
en el paso 2. Si todavía no está configurada, avanzar con los pasos 6 a 10 y dejar los pasos
que requieren PostgreSQL pendientes de verificación.

### Próximo cambio de código previsto

El siguiente bloque de implementación será autenticación:

- endpoint POST tradicional para iniciar sesión;
- formulario responsive con antiforgery;
- rechazo de usuarios inactivos;
- redirección local validada;
- logout protegido;
- página de acceso denegado.

Después se implementarán los paneles mínimos de administrador y promotor con autorización
real del lado servidor.

### 2026-09-18 — Autenticación y base visual implementadas

**Realizado:**

- agregado formulario de login mediante POST tradicional y antiforgery;
- agregado rate limiting de cinco intentos por minuto e IP;
- rechazados usuarios inactivos antes de crear la sesión;
- habilitado bloqueo temporal después de cinco credenciales incorrectas;
- implementado logout protegido por autenticación y antiforgery;
- creado helper de redirección local que rechaza destinos externos, doble slash, barra
  invertida y caracteres de control;
- creadas las rutas `/admin` y `/promotor` con autorización por rol;
- creadas las páginas de login y acceso denegado;
- sustituidos el layout y la navegación originales de Blazor;
- aplicada una base visual responsive en negro, blanco y verde lima;
- creada navegación móvil sin dependencia de JavaScript;
- reemplazada la portada de plantilla por la entrada al sistema;
- build verificado con 0 errores y 0 advertencias.

La skill `frontend-design` orientó la identidad visual: se mantuvo la paleta confirmada de
Simons y se concentró el gesto distintivo en una franja diagonal verde. No se incorporó un
logo inventado; la letra `S` actual es un marcador tipográfico temporal hasta recibir el
recurso oficial.

**No verificado todavía:**

- arranque completo, porque falta `ConnectionStrings:DefaultConnection`;
- aplicación de la migración sobre PostgreSQL;
- creación real de roles y administrador;
- login, logout, cookies, antiforgery y autorización mediante HTTP real;
- inspección visual en los tamaños responsive definidos.

**Punto exacto de continuación:** configurar la cadena mediante User Secrets, ejecutar la
migración, configurar las credenciales iniciales y levantar la aplicación para las pruebas.

### 2026-09-18 — Verificación del entorno PostgreSQL

**Comprobado:**

- `DependencyInjection.cs` utiliza correctamente `ConnectionStrings:DefaultConnection`;
- el inicializador aplica migraciones antes de sembrar roles y administrador;
- no existe todavía el archivo de User Secrets del proyecto;
- no se detectó un servicio local de PostgreSQL;
- `psql` y Docker no están disponibles en el `PATH` del equipo.

**Bloqueo actual:** no es posible aplicar `InitialIdentity` hasta contar con una instancia de
PostgreSQL y sus datos de conexión. El código continúa compilando; no se ha escrito ninguna
credencial en el repositorio.

**Opciones para continuar:**

1. utilizar una base PostgreSQL existente y guardar su cadena mediante User Secrets; o
2. instalar/preparar PostgreSQL local y luego configurar la cadena.

Después de resolverlo, ejecutar la migración, verificar tablas, crear roles y administrador,
y comenzar las pruebas HTTP reales.

### 2026-09-18 — Cadena configurada y conectividad comprobada

**Realizado:**

- recibida una cadena PostgreSQL en formato URI;
- convertida al formato de palabras clave requerido por Npgsql;
- guardada como `ConnectionStrings:DefaultConnection` mediante User Secrets;
- confirmado que la cadena no quedó escrita en el repositorio ni en esta documentación;
- ejecutada una consulta no destructiva del estado de migraciones;
- comprobado el puerto PostgreSQL mediante conexión TCP directa.

**Resultado:** el servidor remoto no acepta conexiones TCP desde este equipo en el puerto
configurado. EF Core no alcanzó la etapa de autenticación y `InitialIdentity` continúa
pendiente; no se realizaron cambios en la base.

**Para continuar:** comprobar que PostgreSQL esté escuchando en el puerto indicado y que el
firewall, proveedor o lista de IP permitidas autorice conexiones desde el equipo de
desarrollo. Después se repetirá la lectura de migraciones antes de ejecutar
`dotnet ef database update`.

## 11. Plan del siguiente bloque

### Último punto de reanudación — Sesiones y navegación (2026-09-18)

Implementado según RN-AUTH-005: cada petición autenticada por cookie conserva primero la
validación de Identity y luego comprueba que el usuario exista y esté activo. Si está
desactivado o eliminado, rechaza la identidad y cierra la cookie. Esto agrega una consulta por
petición autenticada. La validación de circuitos InteractiveServer privados queda pendiente
si se incorporan páginas privadas interactivas; las áreas actuales usan SSR.

Corregido el selector CSS aislado del login: ahora el panel pasa a una columna en móvil.
El control del menú móvil es alcanzable por teclado, tiene etiqueta accesible y foco visible.

Verificación: 14 pruebas aprobadas, ninguna fallida, mediante `dotnet test
WebPageSublimation.slnx --no-restore`. Incluyen usuario activo, inactivo e inexistente,
cierre de cookie y ejecución del validador de Identity. Son pruebas con servicios sustituidos,
no pruebas de persistencia ni HTTP. La inspección visual sigue pendiente.

Siguiente bloque: cuando PostgreSQL acepte conexiones, inspeccionar esquema antes de migrar,
aplicar InitialIdentity, verificar seed repetible, configurar administrador y ejecutar pruebas
HTTP de login/logout, roles y usuario desactivado; después verificar móvil/tablet/desktop.
No se considera terminada la Fase 1 hasta completar esa evidencia.

## 12. Fase 2 — Promotores

### Resultado — 2026-09-18

Implementado:

- Perfil `Promotor` separado de la cuenta Identity, con relación única a `ApplicationUser`.
- Alta administrativa que crea usuario individual, asigna solamente el rol `Promotor` y crea
  el perfil comercial.
- Desactivación lógica de perfil y cuenta; actualiza el sello de seguridad para cortar sesiones.
- Endpoints protegidos y formularios POST con antiforgery.
- Vista `/admin/promotores` responsive, vacía por defecto y sin cuentas o datos inventados.
- Migración `AddPromotores` aplicada en Neon; incluye clave foránea restrictiva e índice único.
- Compilación y 14 pruebas automatizadas aprobadas.

Pendiente de verificación funcional: crear un administrador inicial, iniciar sesión y ejercer
la alta/desactivación con HTTP real. Esto requiere las credenciales iniciales del administrador.

## 13. Plan de Fase 3 — Categorías, productos y catálogo

1. Crear entidades administrables de categoría, producto y variante.
2. Guardar precio comercial como `decimal` y no modelar costos, descuentos ni comisiones.
3. Permitir productos sin variantes y desactivar categorías o productos sin borrar historial.
4. Construir endpoints administrativos para alta, edición y activación.
5. Construir catálogo de lectura para administrador y promotor, con búsqueda básica y sin
   exponer campos administrativos.
6. Mantener imágenes como un punto pendiente hasta configurar volumen persistente de producción.
7. Generar migración, aplicar en Neon y probar que no haya productos de ejemplo.
8. Documentar el resultado y planear Clientes.

### Resultado de Fase 3 — 2026-09-18

Implementado y aplicado en Neon:

- Categorías, productos y variantes como entidades administrables.
- Precio comercial `decimal(18,2)`; no existen costos, descuentos ni comisiones.
- Catálogo vacío por defecto, sin productos de ejemplo.
- Formulario administrativo para categorías y productos, protegido por rol y antiforgery.
- Catálogo de lectura para usuarios autenticados con búsqueda y únicamente precio comercial.
- Migración `AddCatalogo` aplicada: categorías, productos, variantes, índice único de categoría
  y clave foránea restrictiva para conservar historial futuro.
- Compilación sin advertencias y 14 pruebas de seguridad aprobadas.

Pendiente: gestión de variantes, edición, activación/desactivación e imágenes. Las imágenes no
se implementaron porque el volumen persistente de producción sigue sin configurarse; no se
usará el sistema de archivos temporal del contenedor para datos comerciales.

## 14. Plan de Fase 4 — Clientes

1. Crear entidad de cliente con datos opcionales: nombre o razón social, CI/NIT, teléfono,
   correo, dirección, contacto y observaciones.
2. Crear administración de clientes para el rol Administrador.
3. Preparar la relación opcional de cliente con promotor sin activar visibilidad compartida ni
   exclusiva hasta que Simons defina su política de cartera.
4. Usar los clientes solo como referencia de futuras proformas y pedidos, sin destruirlos al
   desactivar.
5. Crear migración, aplicar en Neon y documentar explícitamente la política pendiente.
6. Después planear Proformas con snapshots históricos y precios resueltos en servidor.

## 15. Estado vigente

La conexión Neon está activa y las migraciones aplicadas son `InitialIdentity`,
`AddPromotores` y `AddCatalogo`. Las notas antiguas sobre el endpoint anterior solo quedan
como historial del diagnóstico. Faltan credenciales para el administrador inicial, por lo que
las pantallas protegidas no pueden ejercerse aún con una sesión real. Las fases implementadas
no contienen productos, clientes ni cuentas de demostración.

### Diagnóstico actualizado de conexión — 2026-09-18

Se repitieron pruebas sin modificar la base:

- La IP indicada responde y se resuelve correctamente.
- El puerto entregado en la cadena (`5433`) no acepta conexiones TCP desde este equipo.
- El puerto estándar (`5432`) sí acepta conexiones TCP.
- Al probar temporalmente las mismas credenciales contra `5432`, PostgreSQL respondió
  `28P01: password authentication failed for user postgres`.

Conclusión: `DependencyInjection.cs` no bloquea la conexión. En `5433` no se alcanza ningún
servidor PostgreSQL; en `5432` existe uno, pero no acepta las credenciales de la cadena. La
migración continúa pendiente y no se cambió el secreto guardado.

Siguiente paso planificado: confirmar en el proveedor o servidor cuál es el endpoint público
vigente (host, puerto y credenciales), o habilitar el puerto `5433`. Cuando esté corregido,
repetir primero `migrations list`; solo después inspeccionar y aplicar `InitialIdentity`.

### Neon — Migración y pruebas HTTP (2026-09-18)

**Plan del bloque:** usar la conexión Neon con SSL estricto, consultar el estado de migraciones,
aplicar `InitialIdentity` solo si estaba pendiente, levantar la aplicación para ejecutar el
seed y comprobar los flujos públicos de autenticación antes de crear una cuenta real.

**Implementado y verificado:**

- La nueva cadena Neon fue guardada en User Secrets, sin incluirla en archivos del repositorio.
- `InitialIdentity` fue aplicada sobre `neondb`.
- Se crearon las tablas `users`, `roles`, tablas de relación, índices y
  `__EFMigrationsHistory`.
- La aplicación inició contra Neon y el seed creó `Administrador` y `Promotor`.
- Una lectura posterior de migraciones muestra `20260918152537_InitialIdentity` aplicada.
- `GET /` y `GET /cuenta/ingresar` devuelven `200`.
- `GET /admin` anónimo devuelve `302` al login con `returnUrl=/admin`.
- El formulario de login entrega token antiforgery.
- Un POST sin token devuelve `400`.
- Un POST con token y credenciales inválidas devuelve `302` con mensaje controlado.

**Pendiente:** no se creó el administrador inicial porque aún faltan su correo y contraseña.
No se verificaron login correcto, logout autenticado, acceso autorizado a `/admin` ni el flujo
de promotor.

**Siguiente bloque planificado:**

1. Configurar `InitialAdmin:Email` y `InitialAdmin:Password` únicamente en User Secrets.
2. Ejecutar el seed dos veces y verificar que no duplica usuario ni roles.
3. Iniciar sesión como administrador y comprobar `/admin`, logout y rechazo de `/promotor`.
4. Crear un promotor de prueba desde el futuro módulo administrativo, o temporalmente con un
   seed de desarrollo que se elimine antes de producción.
5. Ejecutar pruebas de acceso cruzado y revisar la interfaz en móvil, tablet y escritorio.
6. Actualizar esta bitácora y pasar al módulo Promotores solo después de cerrar esas pruebas.

## 12. Fase 2 — Promotores

### Plan antes de implementar

1. Crear `Promotor` como perfil comercial separado de `ApplicationUser`, con un usuario
   individual obligatorio y único.
2. Mantener `IsActive` en ambos registros para que desactivar no elimine historial.
3. Crear endpoints administrativos protegidos para alta y desactivación.
4. Obtener toda autorización del servidor; ningún formulario recibe un rol o identificador de
   usuario confiable desde el navegador.
5. Usar formularios POST tradicionales con antiforgery y errores controlados.
6. Construir una vista administrativa responsive y sin datos de ejemplo.
7. Generar migración, aplicarla contra Neon y verificar la relación única.
8. Agregar pruebas de las reglas de seguridad que no requieren datos comerciales.

### Diseño de interfaz

El administrador necesita distinguir rápidamente a quién se le entregó acceso comercial y si
esa cuenta puede seguir operando. La pantalla usará una lista de personas con su estado y un
formulario de alta de una sola columna en móvil. El verde lima marcará únicamente estados
activos y acciones de confirmación; el negro y blanco conservarán la jerarquía de Simons.

### Bloque en ejecución — Regresión de seguridad

Plan previo a implementar: preservar la validación de sesiones de Identity al personalizar
cookies, probar redirecciones locales y rechazos de destinos externos, y comprobar mediante
pruebas que la validación de cookies continúa registrada. Crear un proyecto xUnit separado
solo para pruebas; la aplicación conserva un único proyecto. Repetir la conectividad remota
y registrar por separado los resultados unitarios y la integración pendiente.

### Resultado del bloque — Regresión de seguridad (2026-09-18)

Implementado:

- Proyecto xUnit en `tests/WebPageSublimation.Tests`, agregado a la solución.
- Corrección en `DependencyInjection.cs`: personalizar redirecciones sin reemplazar los
  eventos de cookies de Identity. El reemplazo anterior eliminaba `OnValidatePrincipal`
  y con ello la validación del sello de seguridad.
- Pruebas de destinos externos, barras dobles, barra invertida, controles y rutas locales.
- Regresión que verifica que Identity conserva su validador de sesiones.

Verificado: `dotnet test WebPageSublimation.slnx --no-restore`, 11 casos aprobados,
0 fallidos. Compilación de aplicación y pruebas completada. Estas pruebas no usan base de
datos ni sustituyen las pruebas HTTP o PostgreSQL.

Conectividad repetida: `TcpTestSucceeded=False`. No se aplicaron migraciones ni se
modificó la base remota. La Fase 1 continúa abierta.

Siguiente bloque planificado:

1. Revisar desactivación de usuarios con sesión existente: invalidar su sello de seguridad
   y comprobar el rechazo posterior, además del rechazo actual al iniciar sesión.
2. Corregir navegación móvil por teclado y revisar el selector responsive del login.
3. Ejecutar regresiones después de esas correcciones.
4. Cuando PostgreSQL sea accesible, inspeccionar el esquema, aplicar la migración y probar
   seed, login, logout y roles mediante HTTP real.
5. Registrar evidencia visual responsive antes de marcar la interfaz como terminada.

El plan del módulo Promotores permanece posterior al cierre de estas verificaciones.

### Bloque inmediato — Cerrar persistencia y autenticación

Este bloque comienza cuando el servidor PostgreSQL acepte conexiones.

1. Repetir la prueba TCP al puerto configurado.
2. Consultar las migraciones aplicadas sin modificar la base.
3. Confirmar que la base indicada puede usarse para WebPageSublimation.
4. Aplicar `InitialIdentity`.
5. Verificar las tablas, índices y restricciones creadas.
6. Ejecutar el seed dos veces para demostrar que es idempotente.
7. Configurar el primer administrador mediante User Secrets.
8. Levantar la aplicación y probar login, logout y `/admin` mediante HTTP real.
9. Probar credenciales incorrectas, antiforgery, rate limit y redirecciones externas.
10. Inspeccionar login y navegación en móvil, tablet y escritorio.
11. Registrar resultados y cerrar formalmente los bloques 1.1 a 1.5 que hayan quedado
    verificados.

### Trabajo posible mientras PostgreSQL siga bloqueado

1. Crear el proyecto de pruebas automatizadas.
2. Agregar pruebas de regresión para `SafeRedirect`.
3. Agregar pruebas de las constantes y políticas independientes de la base.
4. Revisar accesibilidad básica del HTML generado por los componentes.
5. Preparar el diseño del módulo de administración de promotores sin inventar reglas nuevas.

### Bloque posterior — Administración de promotores

Una vez cerrada la autenticación:

1. modelar el perfil de promotor separado de `ApplicationUser`;
2. crear la migración correspondiente;
3. implementar listado, creación, edición y desactivación para administradores;
4. impedir registro público y creación por promotores;
5. conservar el usuario y sus relaciones históricas al desactivarlo;
6. agregar pruebas de rol, usuario inactivo y acceso directo;
7. verificar formularios y listado en móvil, tablet y escritorio;
8. documentar resultados y planear el módulo de categorías y catálogo.

## 16. Resultado de Fase 4 — Clientes (2026-09-18)

### Plan ejecutado

1. Modelar clientes sin volver obligatorios los datos que el negocio definió como opcionales.
2. Conservar historial mediante desactivación lógica.
3. Proteger las operaciones de alta, asignación, retiro de acceso y desactivación con el rol
   `Administrador` y antiforgery.
4. Resolver la visibilidad del promotor en el servidor a partir de su usuario autenticado.
5. Representar la cartera con una relación explícita cliente–promotor que admita cero, uno o
   varios promotores, sin imponer todavía exclusividad ni visibilidad global.
6. Crear y aplicar la migración en Neon, ejecutar regresiones y comprobar las rutas HTTP.

### Implementado

- Entidades `Cliente` y `ClientePromotor`, con nombre o razón social, CI/NIT, teléfono,
  WhatsApp, correo, dirección, persona de contacto y observaciones.
- Solo el nombre o razón social es obligatorio. Los demás datos se validan cuando se envían.
- Administración responsive en `/admin/clientes`: alta, listado, asignación, retiro de acceso
  y desactivación lógica.
- Vista responsive `/clientes` para promotores, limitada a clientes activos expresamente
  asignados a su perfil activo.
- El promotor no envía su propio identificador para consultar la cartera; se obtiene del
  `NameIdentifier` de la sesión y se filtra en la consulta de base de datos.
- Las claves foráneas usan eliminación restrictiva y la asignación usa una clave compuesta que
  evita duplicados.
- Navegación de Administrador y Promotor actualizada.
- Migración `20260918191843_AddClientes` aplicada y registrada en Neon. La base contiene
  `clientes` y `clientes_promotores` con sus índices y restricciones.

### Verificación

- `dotnet build WebPageSublimation.slnx --no-restore`: aprobado, 0 advertencias y 0 errores.
- `dotnet test WebPageSublimation.slnx --no-build --no-restore`: 14 pruebas aprobadas,
  0 fallidas.
- La aplicación inició contra Neon y confirmó que la base estaba actualizada.
- Los accesos anónimos a `/admin/clientes`, `/clientes` y al POST de creación respondieron
  `302` hacia el inicio de sesión.
- `dotnet ef migrations list` confirmó cuatro migraciones aplicadas: `InitialIdentity`,
  `AddPromotores`, `AddCatalogo` y `AddClientes`.

### Decisión pendiente de negocio

Simons todavía debe confirmar si cada cliente será exclusivo de un promotor, compartido entre
varios o visible para todo el equipo. La implementación actual usa permisos explícitos y
admite cero, uno o varios promotores. Ningún promotor ve clientes sin asignación. Esta base
permite aplicar después cualquiera de esas políticas sin migrar los datos de clientes.

### Verificación pendiente

Faltan `InitialAdmin:Email` y `InitialAdmin:Password` en User Secrets. Por esa razón aún no se
puede ejecutar con una sesión real el flujo administrativo completo ni crear un promotor para
probar el acceso cruzado entre dos carteras. No se crearon usuarios ni clientes de ejemplo.

## 17. Plan de Fase 5 — Proformas

1. Confirmar con Simons numeración, vigencia, impuestos, moneda, descuentos y política de
   precios antes de codificar cálculos.
2. Modelar cabecera y detalle de proforma con cliente, promotor, estado y fechas.
3. Guardar snapshots del nombre del cliente, producto, variante, precio unitario y descripción
   para que el documento histórico no cambie cuando se edite el catálogo.
4. Resolver precios y totales exclusivamente en el servidor usando `decimal`; no confiar en
   importes enviados por el navegador.
5. Permitir al promotor seleccionar únicamente clientes visibles para su sesión y productos
   activos del catálogo.
6. Definir la matriz de estados y transiciones con el negocio; no inventar aprobaciones ni
   permisos todavía.
7. Crear interfaces responsive para elaborar, revisar y consultar proformas, con autorización
   contra acceso directo por identificador.
8. Crear la migración, aplicar en Neon y añadir pruebas de totales, snapshots, roles, cartera e
   IDOR.
9. Probar el flujo autenticado con Administrador y Promotor cuando se configuren las
   credenciales iniciales, y registrar aquí la evidencia.

## 18. Resultado de Fase 5 — Proformas (2026-09-18)

### Plan ejecutado

1. Modelar cabecera y detalle con número, fecha, cliente, promotor, productos, cantidades,
   precios, subtotales, total y observaciones.
2. Obtener el promotor desde la identidad autenticada y validar en base de datos que el cliente
   esté activo y asignado a ese promotor.
3. Resolver precios desde el catálogo activo en el servidor, sin aceptar precios, descuentos ni
   totales enviados por el navegador.
4. Guardar snapshots históricos del cliente, promotor, producto, variante cuando corresponda,
   cantidad, precio y subtotal.
5. Permitir varios renglones de producto sin fijar un máximo comercial arbitrario.
6. Restringir consulta por propietario para promotores y permitir consulta global al
   administrador.
7. Crear migración, aplicar en Neon, ejecutar pruebas y comprobar el arranque y las rutas HTTP.

### Implementado

- Entidades `Proforma` y `DetalleProforma` con relaciones restrictivas para conservar historia.
- Número técnico secuencial generado por PostgreSQL y protegido por índice único.
- Cantidades decimales de hasta tres posiciones. Se eligió esta representación porque el
  catálogo real todavía podría incluir unidades, metros o superficies; no se impuso que toda
  cantidad sea entera.
- Precio unitario con precisión de dos decimales y subtotales/totales con precisión suficiente
  para multiplicar cantidades de tres decimales sin perder el valor almacenado.
- Servicio que calcula todos los importes con `decimal`, verifica límites de persistencia y
  rechaza productos inactivos, clientes ajenos y perfiles de promotor inactivos.
- Endpoint `POST /proformas/crear` protegido por rol Promotor y validación antiforgery manual.
- Formulario responsive `/proformas/nueva` con renglones agregables y removibles.
- Listado `/proformas` limitado al promotor autenticado.
- Listado `/admin/proformas` con cliente, promotor, fecha y total para el administrador.
- Detalle `/proformas/{id}` con filtro de propietario dentro de la consulta para evitar IDOR;
  un promotor no puede recuperar una proforma ajena por conocer su identificador.
- Navegación y paneles de Administrador y Promotor actualizados.
- Migración `20260918193250_AddProformas` aplicada en Neon. Se crearon `proformas` y
  `detalles_proforma` con índices, precisiones y claves foráneas restrictivas.

### Corrección encontrada durante la verificación

El primer arranque detectó que Minimal APIs no podía generar el binder para arreglos
`string[]` anotados con `FromForm`. Se sustituyó ese enlace por lectura explícita de
`IFormCollection` y validación manual de antiforgery. Después de la corrección, la aplicación
inició correctamente y las comprobaciones se repitieron.

### Verificación

- Compilación final: 0 errores y 0 advertencias.
- Pruebas automatizadas: 19 aprobadas y 0 fallidas. Incluyen cálculos decimales y rechazo de
  cantidades o precios inválidos, además de las regresiones de seguridad existentes.
- Entity Framework no reporta cambios de modelo posteriores a la migración.
- La aplicación inició contra Neon y confirmó que la base estaba actualizada.
- `/admin/proformas`, `/proformas`, `/proformas/nueva`, un detalle por GUID y el POST de
  creación respondieron `302` al login para usuarios anónimos.
- El recurso JavaScript para renglones dinámicos respondió `200`.
- Neon confirmó cinco migraciones aplicadas, incluida `AddProformas`.

### Decisiones pendientes y alcance no inventado

- No se agregaron descuentos, impuestos, vigencia ni estados de proforma porque Simons no ha
  definido esas reglas.
- `VarianteNombre` está preparado como snapshot, pero queda vacío mientras no exista selección
  administrativa de variantes y precios asociados.
- No se crearon clientes, productos, promotores ni proformas de demostración.
- Faltan las credenciales del administrador inicial. Por ello siguen pendientes la prueba
  autenticada completa, la creación con datos reales y la prueba cruzada Promotor A contra
  proforma de Promotor B.

## 19. Plan de Fase 6 — Pedidos

1. Confirmar los estados reales del pedido y quién puede realizar cada transición. `Status` es
   obligatorio conceptualmente, pero sus valores no se inventarán.
2. Confirmar si Simons usa fecha requerida y en qué casos, manteniéndola opcional mientras no
   exista regla.
3. Modelar pedido y detalle con número, cliente, promotor, `QuoteId` opcional, fechas, estado,
   importes, observaciones y snapshots históricos.
4. Implementar conversión Proforma → Pedido copiando cliente, promotor, productos, cantidades
   y precios, conservando `QuoteId` y sin alterar la proforma original.
5. Impedir una segunda conversión accidental de la misma proforma mediante restricción única e
   idempotencia en servidor.
6. Implementar pedido directo sin exigir proforma, reutilizando las validaciones de cartera,
   catálogo activo y cálculo de precios del servidor.
7. Mostrar al promotor solo sus pedidos y al administrador todos, con número, fecha, cliente,
   promotor, total y estado.
8. Proteger detalle, conversión y cambios de estado contra IDOR y manipulación de
   `PromotorId`, precios o totales.
9. Crear migración, aplicar en Neon y probar conversión, snapshots, totales, idempotencia,
   permisos y conservación de la proforma.
10. Actualizar esta bitácora y planear la Fase 7 — Dashboard y ventas por promotor.

## 20. Cuentas demo y cierre de pruebas autenticadas (2026-09-18)

### Plan ejecutado

1. Mantener las credenciales fuera del repositorio mediante User Secrets.
2. Crear un administrador inicial y un perfil Promotor demo con seed idempotente.
3. Mantener la política fuerte por defecto y habilitar la contraseña simple solicitada solo
   cuando `DemoUsers:Enabled` y `DemoUsers:AllowSimplePassword` estén activadas en la
   configuración externa.
4. Ejecutar el inicializador dos veces y comprobar que no duplica usuarios, roles ni perfil.
5. Probar inicio de sesión y autorización cruzada mediante HTTP real.

### Implementado y verificado

- Cuenta `ADMINISTRADOR@GMAIL.COM` creada con rol `Administrador`.
- Cuenta demo de proveedor creada con rol `Promotor` y perfil comercial `Promotor Demo`.
- La contraseña entregada se guardó solamente en User Secrets y no se escribió en esta
  bitácora ni en archivos del repositorio.
- El seed demo está apagado por defecto, exige bandera explícita y no pisa cuentas existentes.
- Segunda ejecución del inicializador: solo realizó consultas; no emitió inserciones.
- El listado administrativo mostró exactamente un perfil `Promotor Demo`.
- Administrador: login `302 → /admin`, `/admin` respondió `200` y `/promotor` redirigió a
  acceso denegado.
- Promotor: login `302 → /promotor`, `/promotor` respondió `200` y `/admin` redirigió a acceso
  denegado.
- El administrador autenticado accedió a `/admin/proformas` con respuesta `200`.
- Compilación: 0 errores y 0 advertencias.
- Pruebas automatizadas: 21 aprobadas y 0 fallidas. Dos pruebas nuevas demuestran que la
  política normal sigue siendo fuerte y que la excepción simple requiere ambas banderas demo.

### Estado vigente

Ya no está pendiente la creación del administrador ni del primer promotor. Continúan vacíos el
catálogo y la cartera, por lo que para probar una proforma completa se debe registrar al menos
una categoría, un producto y un cliente, y asignar el cliente al Promotor Demo. La siguiente
fase planificada continúa siendo Fase 6 — Pedidos, precedida por la confirmación de los estados
reales que utiliza Simons.

## 21. Normalización de correos demo (2026-09-18)

Se corrigió el seed para que los correos configurados se recorten y almacenen en minúsculas.
El cambio también actualiza de forma idempotente cuentas existentes cuyo `Email` o `UserName`
estuviera escrito con mayúsculas. Identity conserva internamente sus campos normalizados para
que el inicio de sesión continúe siendo independiente de mayúsculas y minúsculas.

Estado verificado en Neon:

- `administrador@gmail.com` almacenado y autenticando correctamente;
- el correo del proveedor se almacena y muestra en minúsculas en el listado administrativo;
- ninguna aparición en mayúsculas del correo del promotor en la pantalla;
- compilación con 0 errores y 0 advertencias;
- 21 pruebas aprobadas y 0 fallidas.

## 22. Corrección definitiva de datos demo (2026-09-18)

Se corrigieron los datos para usar nombres completos y bien escritos:

- `administrador@gmail.com`: usuario con rol Administrador;
- `proveedor@gmail.com`: misma cuenta comercial existente, renombrada desde el correo escrito
  incorrectamente, con rol Promotor y sin crear un duplicado;
- `cliente@gmail.com`: registro comercial `Cliente Demo`, asignado al Promotor Demo.

Cliente no es un rol de inicio de sesión en el alcance funcional vigente. Los accesos siguen
siendo Administrador y Promotor; el cliente se utiliza en proformas y pedidos. Se verificó el
login de Administrador y Proveedor, la aparición de los tres correos correctos en sus pantallas,
la ausencia del correo mal escrito y la idempotencia del seed en una segunda ejecución.

## 23. Página pública y solicitudes (2026-09-19)

Implementado: portada comercial responsive, servicios confirmados, catálogo público sin login,
galería alimentada por imágenes reales del catálogo, contacto configurable, formulario público de
cotización con antiforgery y límite por IP, y bandeja administrativa. La migración
`AddSolicitudesPublicas` está aplicada en Neon. No se inventó teléfono de WhatsApp.

Verificación: compilación limpia y 24 pruebas aprobadas después del bloque. Quedan pendientes el
número oficial, dirección, datos legales y logotipo de Simons.

## 24. Catálogo visual completo (2026-09-19)

Implementado: edición y activación lógica de productos; variantes, tallas o colores; imagen
principal JPG, PNG o WebP de hasta 5 MB; visualización pública y administrativa. Las imágenes se
guardan en PostgreSQL para que no dependan del disco efímero. La migración
`ExtendCatalogoImages` está aplicada en Neon.

No se cargaron categorías, productos ni precios supuestamente reales porque Simons aún no entregó
el tarifario. Próximo paso de contenido: recibir ese listado y cargarlo desde Administración.

## 25. Fase 6 — Pedidos (2026-09-19)

Implementado: pedido directo, conversión desde proforma, fecha requerida opcional, especificación
por renglón, snapshots de cliente/promotor/producto/precio, listados por rol, detalle protegido por
propietario y estados Recibido, En producción, Terminado, Entregado y Cancelado. Un índice único
sobre `ProformaId` y el servicio idempotente impiden convertir una proforma dos veces. La migración
`AddPedidos` está aplicada en Neon.

## 26. Fase 7 — Paneles y ventas (2026-09-19)

Implementado: total de ventas entregadas, pedidos pendientes, pedidos del periodo, proformas
recientes y ventas agrupadas por promotor. Administración puede filtrar por fecha, cliente y
promotor. La base para comisión se muestra sin aplicar porcentajes inventados. Se definió “venta”
como pedido Entregado para que trabajos todavía abiertos no inflen el total.

## 27. Fase 8 — Definición financiera (2026-09-19)

Se creó `/admin/finanzas` con las decisiones necesarias sobre pagos, anticipos, saldos, gastos,
cuentas pendientes y comisiones. No se crearon movimientos financieros porque faltan reglas sobre
imputación, deuda, anulaciones, categorías, aprobaciones y base de comisión. Este bloqueo es de
negocio; implementar tablas antes de definirlo produciría saldos ambiguos.

## 28. Documentos y preparación de producción (2026-09-19)

Implementado: proforma y pedido imprimibles, opción del navegador para guardar PDF, encabezado con
datos y logotipo configurables, Dockerfile multi-stage, ejecución sin privilegios, endpoint de salud
con conexión real a PostgreSQL, soporte de proxy HTTPS y persistencia de claves de sesión. La guía
`DESPLIEGUE.md` se reescribió para Simons y documenta secretos, imágenes, backup y verificación.

Pendiente externo: dominio, certificado, política de copias del proveedor y restauración de prueba.
Pendiente de contenido: datos legales, logotipo, WhatsApp, catálogo y precios reales.

### Siguiente bloque planificado

1. Ejecutar todas las pruebas, comprobar que el modelo no tiene cambios sin migración y arrancar la
   aplicación contra Neon.
2. Verificar rutas públicas, autorización de rutas internas y `/health` mediante HTTP.
3. Revisar visualmente las vistas públicas y administrativas en anchos móvil, tablet y escritorio.
4. Corregir cualquier regresión encontrada.
5. Con datos de Simons, cargar catálogo, contacto y empresa; luego definir finanzas antes de crear
   pagos, gastos o comisiones persistentes.

## 29. Verificación integral del bloque (2026-09-19)

- `dotnet build WebPageSublimation.slnx --no-restore`: 0 advertencias, 0 errores.
- `dotnet test WebPageSublimation.slnx --no-build --no-restore`: 26 pruebas aprobadas, 0 fallidas.
- Las pruebas nuevas comprueban el vocabulario de estados y el índice único que evita duplicar la
  conversión de proforma a pedido.
- `dotnet ef migrations has-pending-model-changes`: el modelo coincide con la última migración.
- Arranque real contra Neon: base actualizada y aplicación escuchando correctamente.
- HTTP anónimo: `/`, `/productos`, `/solicitar-cotizacion` y `/health` respondieron 200;
  las rutas de Administrador y Promotor comprobadas respondieron 302 al login.
- No se pudo ejecutar el build del contenedor porque Docker no está instalado en este equipo.
- No se obtuvieron capturas responsive porque la herramienta Computer Use no expuso un navegador
  disponible. El CSS incluye puntos de quiebre para las vistas nuevas, pero la revisión visual en
  móvil, tablet y escritorio continúa pendiente y debe hacerse antes de publicar.

### Siguiente bloque planificado

1. Recibir y configurar WhatsApp, datos legales, dirección, teléfono y logotipo oficial.
2. Recibir categorías, productos, variantes, imágenes y precios reales y cargarlos desde el panel.
3. Confirmar las reglas financieras enumeradas en `/admin/finanzas`; después modelar pagos, gastos,
   saldos y comisiones con migración y pruebas.
4. Definir proveedor, dominio y política de backup; construir el contenedor, desplegar, emitir HTTPS
   y ejecutar una restauración de prueba.
5. Realizar la revisión visual responsive con un navegador disponible y corregir cualquier hallazgo.

## 30. Auditoría contra las skills — pendientes reales (2026-09-19)

Se revisó el código contra `DOCUMENTACION-FUNCIONAL.md` y
`PRUEBAS-PUNTA-A-PUNTA.md`. El proyecto continúa compilando con 0 advertencias y 0 errores,
mantiene 26 pruebas aprobadas y no tiene cambios de modelo pendientes de migración.

### Prioridad 1 — correcciones necesarias en código

1. Integrar la selección de variante activa en proformas y pedidos. Actualmente se administran y
   muestran variantes, pero los formularios comerciales no envían `VarianteId`; por ello el
   snapshot `VarianteNombre` queda vacío en operaciones nuevas.
2. Igualar la validación del pedido directo con la proforma: limitar cantidad, subtotal y total,
   controlar `OverflowException` y devolver mensajes de negocio en lugar de posibles errores 500.
3. Proteger la creación de pedido directo contra doble envío. La conversión desde proforma sí tiene
   índice único e idempotencia, pero dos POST directos iguales todavía pueden crear dos pedidos.
4. Validar la firma real de archivos JPG, PNG y WebP. La carga actual limita tamaño y `ContentType`,
   pero el encabezado MIME enviado por el navegador no basta para comprobar el contenido.
5. Completar el catálogo del promotor con imagen y variantes; la vista pública ya los muestra, la
   vista interna `/catalogo` todavía no.
6. Completar administración de categorías con edición y activación/desactivación, conservando
   productos históricos.
7. Eliminar las páginas de plantilla `/counter` y `/weather` antes de publicar.

### Prioridad 2 — reglas que Simons debe confirmar

1. Estados definitivos del pedido, estado inicial, quién cambia cada estado y transiciones válidas.
   La implementación actual permite al administrador saltar entre cualquier estado.
2. Qué significa una venta realizada y cuándo nace el derecho a comisión. El dashboard usa
   `Entregado` como criterio provisional; debe llamarse “pedidos entregados” hasta confirmación y no
   presentarse como regla financiera.
3. Política de clientes: exclusivos, compartidos o visibles para todos los promotores.
4. Numeración comercial final de proformas y pedidos; hoy se muestra el consecutivo técnico.
5. Alcance de Finanzas: pagos, anticipos, saldos, gastos, anulaciones y reglas de comisión.
6. Precios por cantidad, descuentos, vigencia, impuestos y datos obligatorios de cliente/documento.

### Prioridad 3 — contenido real pendiente

- WhatsApp, nombre legal, NIT, dirección, teléfono y logotipo oficial.
- Categorías, productos, variantes, precios e imágenes reales.
- Contenido real de galería y contacto.

### Prioridad 4 — pruebas que exige la skill

- Flujos HTTP autenticados completos de proforma, pedido directo y conversión.
- IDOR entre dos promotores para clientes, proformas y pedidos.
- Antiforgery válido, ausente e inválido en operaciones sensibles.
- Congelamiento histórico después de editar producto, precio, cliente y promotor.
- Concurrencia de numeración y doble envío.
- Persistencia después de reiniciar.
- Revisión real a 320–375, 390–430, 768, 1024–1366 y 1440 px, incluyendo menú móvil,
  formularios completos, acciones táctiles y ausencia de overflow.

### Prioridad 5 — producción

- Construir y probar la imagen Docker en un equipo que tenga Docker.
- Definir hosting y dominio, emitir HTTPS y configurar secretos de producción.
- Configurar backup/PITR de Neon y demostrar una restauración en otra base.
- Configurar retención o envío de registros de errores; hoy existen logs de consola, pero no un
  destino persistente de producción.
- Decidir si “Guardar como PDF” mediante impresión del navegador satisface el documento requerido.
  Si Simons necesita descarga directa e idéntica en todos los equipos, falta un generador PDF.
- Agregar paginación a listados antes de cargar un volumen alto de productos, clientes y operaciones.

### Próximo bloque recomendado

Corregir primero selección de variantes, límites de pedido, doble envío, validación de imágenes y
catálogo interno. Después crear las pruebas HTTP/E2E de los flujos comerciales. Las reglas de
estados, ventas, comisión y Finanzas deben esperar respuestas de Simons.

## 31. Correcciones de auditoría y arranque local (2026-09-19)

Se completaron las correcciones de código identificadas como prioridad 1:

- proformas y pedidos directos permiten seleccionar una variante activa y conservan su nombre histórico;
- los pedidos directos validan cantidades, subtotales y totales, y utilizan una clave idempotente con índice único para impedir duplicados por doble envío;
- las imágenes validan la firma binaria real de JPG, PNG y WebP además del tipo declarado;
- el catálogo interno muestra imágenes y variantes;
- las categorías se pueden editar, activar y desactivar;
- se retiraron las páginas de ejemplo `/counter` y `/weather`;
- los paneles hablan de pedidos e importes entregados sin presentarlos como una regla de comisión;
- se corrigió la consulta agrupada de los paneles para que PostgreSQL pueda ejecutarla;
- la redirección HTTPS se aplica en producción y el perfil local permanece disponible por HTTP.

La migración `AddPedidoIdempotency` se aplicó correctamente en Neon. El modelo no tiene cambios pendientes de migración. La compilación terminó con 0 errores y 0 advertencias, y las 31 pruebas automatizadas terminaron aprobadas.

La verificación HTTP real confirmó acceso 200 en la página pública, salud, catálogos, pedidos, proformas, finanzas y ambos paneles. También confirmó la separación de roles: el administrador no puede crear pedidos de promotor y el promotor no puede entrar al panel administrativo. La aplicación queda escuchando en `http://localhost:5015/`.

### Siguiente bloque planificado

1. Cargar desde el panel el catálogo, precios, variantes e imágenes reales entregados por Simons.
2. Configurar WhatsApp, datos legales, dirección, teléfono y logotipo oficiales.
3. Confirmar estados y transiciones de pedidos, política de clientes, numeración, impuestos, descuentos y vigencia de proformas.
4. Definir pagos, anticipos, saldos, gastos, anulaciones y comisiones antes de implementar Finanzas.
5. Completar pruebas E2E de mutaciones, IDOR, antiforgery, concurrencia, snapshots históricos y revisión visual responsive en un navegador disponible.
6. Preparar el despliegue cuando se definan hosting y dominio: Docker, HTTPS, secretos, registros y una restauración de backup comprobada.
## 32. Revisión ortográfica y de codificación (2026-09-19)

Se revisaron los textos visibles de las rutas públicas, autenticación, administración y área comercial. Se corrigieron caracteres dañados que aparecían en palabras como “sublimación”, “catálogo”, “área”, “producción”, “comisión”, “especificación” y “personalización”, además de los signos de apertura, separadores, flechas y guiones.

También se tradujeron al español la página de error, la página 404 y los mensajes de reconexión del servidor. Se normalizó el uso de “período” en los paneles. La revisión HTTP del HTML renderizado no encontró secuencias dañadas ni textos residuales de las plantillas en inglés en las rutas revisadas.

Verificación: compilación con 0 errores y 0 advertencias, 31 pruebas aprobadas y respuestas 200 en las rutas públicas y protegidas comprobadas con los perfiles de administrador y promotor.

### Siguiente bloque planificado

Continuar con la carga del contenido real de Simons y, cuando haya un navegador automatizable disponible, completar la revisión visual responsive en todos los anchos definidos por la guía de pruebas.