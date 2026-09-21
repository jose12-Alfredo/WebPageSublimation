# Estándar técnico JsonCorp — Monolito web

Contrato de construcción para aplicaciones web monolíticas. Define **cómo** se construye, no
**qué** hace: el dominio va en un documento aparte.

**Cómo se usa.** Se entrega a un agente de IA junto con la especificación de negocio. Las
reglas están en imperativo y son de cumplimiento obligatorio; debajo de cada una está el
motivo, para que ante un caso que el documento no previó se pueda decidir por analogía en vez
de copiar a ciegas. Cuando una regla se rompa a propósito, se documenta el desvío y su razón
(ver *Desvíos deliberados*).

Este estándar aplica a la rama **monolítica**. Los proyectos de microservicios tienen el suyo.

---

## 1. Stack

Fijo. El agente no elige nada de esta tabla.

| Pieza | Versión | Notas |
|---|---|---|
| Runtime | .NET 10 (`net10.0`) | `Nullable` e `ImplicitUsings` habilitados en los cuatro proyectos |
| Web | ASP.NET Core Blazor Web App | Render por página, ver §4 |
| Base de datos | PostgreSQL 17 | |
| ORM | EF Core 10 + `Npgsql.EntityFrameworkCore.PostgreSQL` | `EnableRetryOnFailure(3)` |
| Convención de nombres | `EFCore.NamingConventions` → `UseSnakeCaseNamingConvention()` | La base queda en `snake_case`, el C# en `PascalCase` |
| Identidad | ASP.NET Core Identity (`AddIdentityCore` + roles) | |
| Caché | `Microsoft.Extensions.Caching.Memory` | |
| PDF | QuestPDF (licencia Community) | Requiere `libfontconfig1` en Linux, ver §9 |
| Pruebas | xUnit + SQLite en memoria | |
| Solución | formato `.slnx` | |

Nada de AutoMapper, MediatR ni librerías de CQRS. Un monolito de este tamaño no las amortiza:
agregan indirección y vuelven ilegible el salto entre la petición y la consulta.

---

## 2. Arquitectura

Cuatro proyectos. Las dependencias apuntan **hacia adentro** y no hay excepciones.

```
<Proyecto>.slnx
├── src/
│   ├── <P>.Domain/           Entidades y enums. Cero dependencias de framework.
│   ├── <P>.Application/      Interfaces, DTOs y reglas puras. Depende solo de Domain.
│   ├── <P>.Infrastructure/   EF Core, DbContext, migraciones, servicios, seed, Identity.
│   └── <P>.Web/              Presentación: componentes, endpoints, generación de archivos.
└── tests/
    └── <P>.Tests/            xUnit sobre SQLite en memoria.
```

**Reglas:**

1. **`Domain` no referencia a nadie.** Solo entidades y enums. Si una entidad necesita un
   atributo de EF o de Identity para existir, está mal modelada.

2. **`Application` define las interfaces; `Infrastructure` las implementa.** Toda capacidad
   del sistema entra por una interfaz declarada en `Application`.
   *Por qué:* permite sustituir la implementación en pruebas y deja el contrato legible en un
   archivo corto, sin tener que leer la consulta SQL para entender qué hace el servicio.

3. **El usuario de Identity vive en `Infrastructure`, nunca en `Domain`.** Las entidades del
   dominio referencian al usuario por su id (`string`).
   *Por qué:* si `IdentityUser` entra al dominio, el dominio arrastra ASP.NET entero y deja de
   poder probarse en aislamiento.

4. **La capa web consume interfaces, jamás EF para lógica de negocio.** Un componente puede
   leer datos de catálogo por su servicio; no puede calcular ni decidir nada contra el
   `DbContext`.

5. **Acceso a datos por `IDbContextFactory<T>`.** Se registran las dos formas:

   ```csharp
   services.AddDbContext<AppDbContext>(Configurar);
   services.AddDbContextFactory<AppDbContext>(Configurar, ServiceLifetime.Scoped);
   ```

   *Por qué:* Blazor renderiza en paralelo y un `DbContext` compartido no es seguro entre
   hilos; la factory da uno por operación. El scoped queda registrado solo porque Identity lo
   exige como servicio.

6. **Un único registro de dependencias por capa**, en `Infrastructure/DependencyInjection.cs`,
   expuesto como `services.AddInfraestructura(configuration)`. `Program.cs` no registra
   servicios de datos uno por uno.

### El patrón del dueño único

**Cada dato sensible o derivado tiene exactamente un servicio dueño, y nadie más lo consulta.**

El caso típico es el de valores que dependen de quién pregunta: tarifas por cliente, cupos,
descuentos, permisos calculados, saldos. La regla:

- Un solo servicio lee esa tabla. Se declara explícitamente en su documentación XML.
- Ningún componente de presentación la toca, ni siquiera para "mostrar".
- El servidor entrega **solo el valor que ese usuario tiene derecho a ver**. Nunca se manda al
  navegador una lista de valores para que elija cuál mostrar.
- Un único componente de presentación pinta ese valor, y lo recibe ya resuelto.

*Por qué:* si dos lugares calculan lo mismo, un día divergen y el bug es indetectable en
revisión de código. Y si el navegador recibe las opciones, la regla de negocio se volvió
decorativa: basta abrir el inspector para ver lo que no corresponde.

### Congelar lo que se emitió

**Todo documento emitido —cotización, pedido, factura, comprobante— copia los valores, no los
referencia.** Un cambio posterior en la tabla de origen no puede mover un documento ya emitido.
Se verifica con una prueba dedicada.

---

## 3. Organización interna

`Application` se organiza **por capacidad**, no por tipo técnico:

```
Application/
├── Catalogo/         ICatalogoService.cs  + DTOs
├── Pricing/          IPricingService.cs   + DTOs
├── Seguridad/        IUsuarioActual.cs
└── …
```

No hay carpetas `Interfaces/`, `DTOs/` ni `Services/`. Una capacidad se lee en un solo
directorio.

`Domain/Entidades` agrupa **por área**, no un archivo por clase: entidades que se leen juntas
viven en el mismo archivo (`Catalogo.cs`, `Clientes.cs`, `Precios.cs`). Un archivo de 15
líneas por entidad multiplica los saltos sin aportar nada.

`Infrastructure/Persistencia/Configuraciones/` tiene las configuraciones de EF agrupadas por
área (`IEntityTypeConfiguration<T>`), nunca atributos sobre las entidades: eso metería EF
dentro de `Domain`.

---

## 4. Render: SSR estático por defecto

| Modo | Dónde | Por qué |
|---|---|---|
| **SSR estático** | Todo lo público e indexable, y los formularios de cara al cliente | Tiene que indexarse y funcionar sin JavaScript |
| **InteractiveServer** | Backoffice (`/admin/*`) | Requiere estado; la autorización se resuelve en servidor |

**`@rendermode InteractiveServer` se declara por página, nunca en `App.razor` ni en el
layout.** Declararlo global mata el SSR estático de todo el sitio público.

**Los formularios de cara al cliente son POST tradicionales, no interactivos.**
*Por qué:* un formulario interactivo depende de un circuito WebSocket vivo. Con conexión
inestable —celular en zona rural, oficina con wifi saturado— el circuito se corta y el usuario
pierde lo cargado. Con POST funciona sin JavaScript y sobrevive a cada recarga. El backoffice
sí es interactivo, que es donde la interacción rinde y la conexión es estable.

**Si el layout usa un framework CSS con componentes JS (desplegables, menú hamburguesa), el
bundle JS va cargado en `App.razor`.** Cargar solo el CSS deja menús que no abren: el HTML
está bien y no hay error en consola, así que el fallo pasa inadvertido hasta que alguien
intenta usar el menú en un celular.

---

## 5. Endpoints y formularios

Minimal API, agrupados por área en `Web/Endpoints/`.

### Binding — el error más caro

**Todo parámetro que venga de un formulario lleva `[FromForm]` explícito.**

```csharp
grupo.MapPost("/agregar", async (HttpContext ctx, IServicio svc,
    [FromForm] int itemId, [FromForm] int cantidad, [FromForm] string? volverA) => { … });
```

*Por qué:* en Minimal APIs un parámetro escalar sin atributo se bindea **desde la query
string**, no del cuerpo. Un formulario `method="post"` manda todo en el body, así que el
endpoint falla con `BadHttpRequestException: Required parameter "…" was not provided from
query string` antes de entrar al handler. El síntoma no señala la causa y cuesta horas.

**Un campo opcional que puede llegar vacío se recibe como `string?` y se convierte a mano.**
Un `<select>` con opción vacía envía el campo con cadena vacía, no lo omite; el binder intenta
parsearla contra el `int?` y devuelve 400. Un helper compartido resuelve la conversión, donde
vacío significa "no contestó" y no "petición inválida".

**Un campo oculto cuyo valor es nulo no se renderiza.** Emitir `value=""` produce el mismo 400.

### Validación y errores esperables

**Los rechazos previsibles devuelven un resultado tipado y redirigen con mensaje; no lanzan
excepciones.** Aceptar algo en estado inválido, pedir un recurso ajeno o mandar un formulario
incompleto son respuestas normales del sistema, no fallas del servidor. Una excepción ahí
produce un 500 con traza en pantalla.

Los parámetros que el propio handler valida se declaran nullables, para que la validación
propia produzca el mensaje amable en vez de un 400 crudo del binder.

### Redirección posterior al POST

Un `volverA` que termine en la cabecera `Location` **solo acepta rutas propias**:

```csharp
public static string Destino(string? volverA, string porDefecto)
{
    if (string.IsNullOrWhiteSpace(volverA)) return porDefecto;
    var ruta = volverA.Trim();
    if (ruta.Length < 1 || ruta[0] != '/') return porDefecto;
    if (ruta.Length > 1 && (ruta[1] == '/' || ruta[1] == '\\')) return porDefecto;
    if (ruta.Any(char.IsControl)) return porDefecto;
    return Uri.TryCreate(ruta, UriKind.Relative, out _) ? ruta : porDefecto;
}
```

*Por qué cada guarda:* rechazar solo `//` no alcanza —los navegadores normalizan la barra
invertida antes de resolver la autoridad, así que `/\destino.com` sale del sitio igual—, y un
salto de línea sin filtrar permite inyectar cabeceras.

---

## 6. Seguridad

### Orden del pipeline

```csharp
if (detrasDeProxy) app.UseForwardedHeaders();   // primero de todo
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
if (!detrasDeProxy) app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();                            // después de la autenticación
app.UseRateLimiter();
```

**`UseAntiforgery()` va después de `UseAuthentication()`.** El token que emite
`<AntiforgeryToken />` queda atado al usuario que lo pidió; validándolo antes, `HttpContext.User`
todavía está anónimo y el token de un usuario con sesión se rechaza por pertenecer "a otro
usuario".

### Autorización en tres lugares

**Cada operación protegida se valida en la página, en el endpoint y en el servicio.** No basta
con `[Authorize]` en la página: el endpoint es alcanzable directamente.

**El id de un recurso que llega de un formulario se verifica contra el dueño, dentro de la
consulta:**

```csharp
var solicitud = await db.Solicitudes
    .Where(s => s.Id == solicitudId
                && s.ClienteId != null
                && s.Cliente!.Usuarios.Any(u => u.UsuarioId == usuarioId))
    .FirstOrDefaultAsync(ct);

if (solicitud is null) return new Resultado(Estado.NoDisponible);
```

*Por qué en la consulta y no en un `if` posterior:* un recurso ajeno queda indistinguible de
uno inexistente, así que probar ids no revela cuáles existen. Recibir un `ClaimsPrincipal` en
el servicio y no usarlo para filtrar es el bug más fácil de pasar por alto en revisión: el
código *parece* consciente del usuario.

### Antiforgery en endpoints sin formulario

**Un endpoint POST sin parámetros de formulario valida el token a mano:**

```csharp
if (!await antiforgery.IsRequestValidAsync(ctx)) return Results.Redirect("/");
```

*Por qué:* `UseAntiforgery()` solo valida automáticamente los endpoints que bindean formulario.
Un logout sin campos nunca se valida, aunque el formulario mande el token religiosamente:
cualquier sitio puede cerrarle la sesión al visitante con un POST a ciegas.

### Rate limiting

Limitador de ventana fija sobre los envíos públicos. **El rechazo escribe su propio cuerpo:**

```csharp
opt.OnRejected = async (contexto, ct) =>
{
    contexto.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
    contexto.HttpContext.Response.ContentType = "text/plain; charset=utf-8";
    await contexto.HttpContext.Response.WriteAsync("Demasiados envíos. Probá en unos minutos.", ct);
};
```

*Por qué:* `UseStatusCodePagesWithReExecute` reescribe respuestas de error vacías. Al
reejecutar el POST rechazado contra la página de error, el usuario recibía un **400 de
antiforgery** en lugar del 429 —un mensaje que no tiene nada que ver con lo que pasó—.

### Auditoría

Interceptor de `SaveChangesAsync` (`SaveChangesInterceptor`) sobre una lista explícita de
entidades, guardando valores anteriores y nuevos en `jsonb`. **No se audita todo:** el registro
se llena de ruido y deja de consultarse.

### Secretos

Nunca en el repositorio. `dotnet user-secrets` en desarrollo, variable de entorno en
producción. El arranque **falla con mensaje explícito** si falta la cadena de conexión, en vez
de arrancar y romper en la primera consulta.

### Checklist de verificación

Los siete fallos de esta lista aparecieron todos en un proyecto real que compilaba sin
advertencias y con las pruebas en verde. Ninguno se detecta leyendo el código: hay que ejercer
la aplicación con peticiones HTTP reales.

| # | Verificar | Síntoma si falla |
|---|---|---|
| 1 | Cada POST de formulario entra al handler | 400 "not provided from query string" |
| 2 | Campos opcionales vacíos (`<select>` sin elegir) no rompen | 400 al enviar el formulario |
| 3 | Menús y desplegables abren | HTML correcto, clic sin efecto |
| 4 | `volverA=/\externo.com` no sale del sitio | Open redirect explotable |
| 5 | Cambiar el id de un recurso ajeno lo rechaza | IDOR: se opera sobre datos de otro |
| 6 | Rutas inexistentes devuelven 404, no 200 | Soft-404: Google indexa páginas de error |
| 7 | La sesión sobrevive a un redeploy | Todos deslogueados en cada actualización |

---

## 7. Pruebas

**xUnit sobre SQLite en memoria**, no el proveedor `InMemory`.
*Por qué:* SQLite es relacional de verdad, así que respeta índices únicos, claves foráneas y
restricciones `check`. El proveedor `InMemory` no, y deja pasar pruebas que fallan en
PostgreSQL.

Una clase base compartida abre la conexión, crea el esquema y siembra un juego de datos
mínimo pero completo: al menos **dos entidades del mismo tipo con dueños distintos**, para
poder probar el acceso cruzado.

```csharp
public sealed class BaseDePrueba : IDisposable, IDbContextFactory<AppDbContext>
{
    public BaseDePrueba()
    {
        _conexion = new SqliteConnection("DataSource=:memory:");
        _conexion.Open();
        _opciones = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_conexion).UseSnakeCaseNamingConvention().Options;
        using var db = CreateDbContext();
        db.Database.EnsureCreated();
        Sembrar(db);
    }
}
```

Implementa `IDbContextFactory<T>`, de modo que los servicios reales se instancian tal cual
contra la base de prueba, sin dobles.

**Qué se prueba, en orden de prioridad:**

1. **Las reglas de negocio con dinero o permisos de por medio**, una prueba por regla.
2. **Acceso cruzado entre usuarios** — que el usuario A no opere sobre lo de B.
3. **Inmutabilidad de lo emitido** — cambiar el origen no mueve el documento ya generado.
4. **Los helpers de seguridad**, con `[Theory]` cubriendo los bypass conocidos.
5. **Errores esperables** — que devuelvan resultado y no excepción.

**Los nombres de las pruebas son frases en español que describen la regla**, no
`Metodo_Caso_Resultado`:

```
El_carrito_valoriza_con_el_precio_que_corresponde_al_usuario
Un_cambio_de_tarifario_posterior_no_mueve_la_cotizacion_emitida
Otro_cliente_no_puede_aceptar_una_cotizacion_ajena
Una_cotizacion_inexistente_no_se_distingue_de_una_ajena
```

*Por qué:* la salida del runner se lee como una lista de reglas verificadas y sirve de
documentación viva.

**Las pruebas unitarias no cubren el pipeline HTTP.** Binding, antiforgery, redirecciones,
códigos de estado y autorización de endpoints solo se verifican ejerciendo la aplicación
levantada. Es obligatorio hacer esa pasada antes de dar por terminado un entregable.

---

## 8. SEO y rendimiento

- **URLs limpias por recurso:** `/productos/{slug}`, `/categoria/{slug}`. Nada de ids en la URL
  pública.
- **`<title>` y `<meta name="description">` por página**, editables desde el backoffice.
- **`sitemap.xml` generado desde la base**, no un archivo estático.
- **`robots.txt` bloquea** las áreas privadas (`/admin`, `/portal`, `/cuenta`, carrito).
- **Open Graph y Twitter Card** en las fichas: el enlace se comparte por WhatsApp y tiene que
  verse bien.
- **JSON-LD** de la entidad principal. El bloque `<script>` se emite como `MarkupString`:
  Blazor descarta los `<script>` que escribe un componente.
- **Las páginas públicas entregan el contenido en el HTML.** Si hace falta JavaScript para
  leerlas, están mal.

**Un recurso inexistente devuelve 404 real, no 200 con mensaje.** Se marca con un componente
que fija el estado:

```csharp
[CascadingParameter] private HttpContext? HttpContext { get; set; }

protected override void OnInitialized()
{
    if (HttpContext is not null && !HttpContext.Response.HasStarted)
        HttpContext.Response.StatusCode = Codigo;
}
```

**Cuidado con el primer render.** El componente se renderiza una vez con los datos todavía en
`null`, antes de que `OnParametersSetAsync` complete. Marcar el 404 en esa pasada deja en 404
**también las páginas que sí existen**. Hay que distinguir "aún no cargué" de "no existe" con
una bandera `_cargado`, y anidar la marca para no perder el análisis de nulabilidad del
compilador:

```razor
@if (_recurso is null)
{
    @if (_cargado) { <EstadoHttp /> }
    …mensaje de no encontrado…
}
else { …contenido… }
```

**Un filtro de ruta que no corresponde a nada también devuelve 404.** Si `/categoria/lo-que-sea`
responde 200 con el listado completo, son infinitas URLs sirviendo contenido duplicado.

**Las imágenes llevan `width`, `height` y `loading="lazy"`,** y se sirven en un tamaño acorde
al de presentación. Un PNG de 300 KB mostrado a 96 px, repetido en el layout, es el archivo
más pesado del sitio.

---

## 9. Despliegue

Contenedor Docker sobre Coolify. Build en dos etapas: el SDK compila, la imagen final lleva
solo el runtime.

**Los `.csproj` se copian antes que el código fuente**, para que `dotnet restore` quede
cacheado y un cambio en una vista no vuelva a resolver los paquetes.

```dockerfile
COPY src/<P>.Domain/<P>.Domain.csproj                 src/<P>.Domain/
COPY src/<P>.Application/<P>.Application.csproj       src/<P>.Application/
COPY src/<P>.Infrastructure/<P>.Infrastructure.csproj src/<P>.Infrastructure/
COPY src/<P>.Web/<P>.Web.csproj                       src/<P>.Web/
RUN dotnet restore src/<P>.Web/<P>.Web.csproj
COPY src/ src/
RUN dotnet publish src/<P>.Web/<P>.Web.csproj -c Release --no-restore -o /app/publish
```

**Dependencias nativas del runtime.** Si se genera PDF con QuestPDF hay que instalar
`libfontconfig1` y una familia de fuentes: SkiaSharp resuelve tipografías por fontconfig, y sin
eso la app arranca bien y recién revienta al generar el primer documento.

**El proceso corre como usuario sin privilegios** (`USER app`). El directorio de claves se crea
en la imagen con su dueño —`mkdir -p /keys && chown app:app /keys`— porque un volumen nombrado
vacío hereda esos permisos al montarse.

**Claves de Data Protection persistidas fuera del contenedor.** Cifran las cookies de sesión y
los tokens antiforgery; el sistema de archivos del contenedor se descarta en cada despliegue,
así que sin volumen **cada actualización cierra la sesión de todos**:

```csharp
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(rutaClaves))
    .SetApplicationName("<Proyecto>");   // fijo, para que el anillo sobreviva a las versiones
```

**Detrás del proxy inverso** (`Biofarma:DetrasDeProxy` o equivalente):

- `UseForwardedHeaders()` primero de todo, con `KnownIPNetworks` y `KnownProxies` vacíos,
  porque el proxy llega con una IP de la red de Docker. Es seguro **solo mientras el contenedor
  no sea alcanzable directamente**; si se publica el puerto a internet, hay que revisarlo.
- `UseHttpsRedirection()` **desactivado**: el redirect ya ocurrió en el borde y repetirlo genera
  un rebote contra el proxy, además de devolverle un 307 al chequeo de salud.

**`HEALTHCHECK` que pegue contra una página que consulte la base**, para que el contenedor se
marque enfermo si la base deja de responder en vez de seguir sirviendo errores.

**Las migraciones se aplican al arrancar**, y el seed maestro (roles, catálogos fijos, usuario
administrador) corre siempre y es **idempotente**. El seed de datos de ejemplo va detrás de una
bandera de configuración, apagada por defecto, y no pisa datos existentes.

**Configuración por variables de entorno** con doble guión bajo (`ConnectionStrings__Default`).
El `.dockerignore` excluye `bin/`, `obj/`, `tests/`, `appsettings.Development.json` y todo
archivo de secretos.

---

## 10. Convenciones de código

**Nombres de dominio en español**, igual que el negocio los nombra: `Solicitud`, `Presentacion`,
`ZonaRoutingService`. Los términos técnicos quedan en inglés (`Service`, `Endpoints`, `Dto`).
*Por qué:* traducir el vocabulario del cliente obliga a un diccionario mental en cada
conversación y es donde se cuelan los malentendidos.

**Los comentarios explican por qué, nunca qué.** Un comentario que parafrasea la línea de abajo
es ruido que además envejece mal. Se comenta:

- la razón de una decisión no evidente;
- el escenario real que motivó una guarda ("los promotores trabajan desde el celular en zona
  rural");
- las trampas del framework que harían que alguien "simplifique" y rompa algo.

Documentación XML (`/// <summary>`) en interfaces y servicios, explicando el contrato y las
invariantes. Cuando un método tiene una sutileza que no se ve en la firma, va en `<remarks>`.

**Los identificadores de las reglas de negocio se citan en el código** (`// RN-07: la unidad
bonificada se factura`), y existe una tabla en el README que mapea cada regla a su
implementación. *Por qué:* permite auditar contra la especificación sin leer el sistema entero.

### Desvíos deliberados

Cuando se decide apartarse de la especificación o de este estándar, **se documenta en el README
con su motivo**, en prosa. Ejemplo del formato:

> **Desvío consciente de la especificación (§1.1):** el carrito quedó en SSR con formularios
> POST en vez de interactivo. El negocio despacha a provincias con conexión pobre y los
> promotores trabajan desde el celular en la calle; un carrito que depende de un circuito
> WebSocket se cae ahí.

*Por qué:* un desvío sin explicar se lee como un error y alguien lo "arregla" seis meses
después, reintroduciendo el problema original.

### README del proyecto

Obligatorio, con: arquitectura y decisiones, puesta en marcha reproducible, tabla de reglas de
negocio mapeadas a su implementación, estado de los entregables, y **puntos abiertos que siguen
abiertos**. Esta última sección es la que evita que una cosa provisoria se vuelva permanente
por olvido.

---

## 11. Escala de referencia

Números de un proyecto terminado bajo este estándar, como orden de magnitud esperable:

| Métrica | Valor |
|---|---|
| Archivos `.cs` + `.razor` | ~97 |
| Líneas totales | ~16.000 |
| Domain | 9 archivos |
| Application | 9 archivos (9 interfaces) |
| Infrastructure | 18 archivos (6 servicios, 3 configuraciones EF, interceptor, seed) |
| Web | 54 archivos (42 componentes, 6 grupos de endpoints) |
| Pruebas | 6 archivos, 42 casos |
| Tablas | 27 `DbSet` |
| Páginas interactivas | 11, todas de backoffice |
| Imagen Docker | ~520 MB |

La distribución importa más que los totales: **`Domain` y `Application` juntos pesan menos que
`Web`**. Si `Application` empieza a tener más archivos que `Infrastructure`, probablemente se
estén escribiendo abstracciones que nadie necesita.

---

## 12. Definición de terminado

Un entregable no está listo hasta que:

- [ ] Compila sin advertencias.
- [ ] Todas las pruebas pasan.
- [ ] Se ejerció la aplicación **levantada**, con peticiones HTTP reales, cubriendo los siete
      puntos del checklist de §6.
- [ ] Cada regla de negocio tiene su prueba y su entrada en la tabla del README.
- [ ] Los secretos están fuera del repositorio y el arranque falla claro si falta alguno.
- [ ] Las rutas inexistentes devuelven 404 y las privadas redirigen al login.
- [ ] La imagen Docker construye y arranca contra la base real.
- [ ] Los desvíos deliberados están documentados con su motivo.

**Al reportar el estado se dice lo que falta.** Un entregable parcial informado es útil; uno
dado por completo que no lo está cuesta el doble.
