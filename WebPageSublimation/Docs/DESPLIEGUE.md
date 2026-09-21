# Despliegue de Simons Publicidad

La aplicación se publica como contenedor en el puerto `8080`. El proxy del proveedor debe
terminar HTTPS y reenviar `X-Forwarded-For` y `X-Forwarded-Proto`.

## Variables obligatorias

| Variable | Uso |
|---|---|
| `ConnectionStrings__DefaultConnection` | Cadena Npgsql de producción con SSL. Debe marcarse como secreta. |
| `Deployment__BehindProxy=true` | Lee correctamente esquema HTTPS e IP detrás del proxy. |
| `Deployment__DataProtectionKeysPath=/keys` | Conserva sesiones y antiforgery entre despliegues. |
| `ASPNETCORE_ENVIRONMENT=Production` | Activa manejo de errores y HSTS. |

Las cuentas demo deben apagarse con `DemoUsers__Enabled=false`. Las contraseñas y la cadena de
conexión nunca se guardan en `appsettings.json` ni en el repositorio.

## Base de demostración vacía

Una base nueva recibe automáticamente las migraciones al arrancar. Para cargar también la demostración, configure en el proveedor:

| Variable | Valor para demo |
|---|---|
| `DemoUsers__Enabled` | `true` |
| `DemoData__Enabled` | `true` |
| `DemoAccess__ShowCredentials` | `true` para mostrar las cuentas demo configuradas en el login |
| `DemoUsers__PromoterEmail` | correo ficticio del promotor demo |
| `DemoUsers__PromoterPassword` | contraseña fuerte del promotor demo |
| `DemoUsers__PromoterName` | nombre visible del promotor demo |
| `DemoUsers__ClientEmail` | correo ficticio del cliente demo |
| `DemoUsers__ClientName` | nombre visible del cliente demo |

La carga es idempotente: reiniciar el servicio no duplica el catálogo, las imágenes ni las operaciones de ejemplo. Usa las seis fotografías reales ya aprobadas dentro del repositorio y datos comerciales sintéticos; no replica cuentas, contraseñas, contactos ni documentos reales de otra base. Cuando se habilita la visualización de credenciales, el login muestra las cuentas y contraseñas demo configuradas.

## Datos comerciales configurables

Configurar cuando Simons entregue los datos definitivos:

- `PublicContact__WhatsApp`: número internacional, solo dígitos o con prefijo `+`;
- `Company__Name`, `Company__LegalName`, `Company__TaxId`, `Company__Address` y `Company__Phone`;
- `Company__LogoUrl`: recurso HTTPS o ruta estática del logotipo aprobado.

Los campos vacíos se ocultan. No se publican datos inventados.

## Persistencia

- Categorías, productos, imágenes, clientes, proformas y pedidos viven en PostgreSQL Neon.
- Las imágenes se almacenan como `bytea`; sobreviven al reemplazo del contenedor.
- Montar un volumen privado y persistente en `/keys` para Data Protection.
- Neon debe tener copias de seguridad o point-in-time restore habilitado según el plan contratado.
  Antes de producción se debe ejecutar una restauración de prueba en otra base; una copia no está
  verificada hasta demostrar que restaura.

## Despliegue

1. Crear la aplicación desde el `Dockerfile` de la raíz.
2. Exponer solo el puerto `8080` detrás del proxy.
3. cargar las variables anteriores como secretos;
4. montar el volumen `/keys`;
5. asignar el dominio y emitir el certificado TLS;
6. desplegar y comprobar `GET /health`;
7. revisar los registros del contenedor y confirmar que todas las migraciones se aplicaron;
8. probar inicio de sesión, catálogo público, cotización, proforma y pedido.

El contenedor incluye un `HEALTHCHECK` contra `/health`; este comprueba acceso real a PostgreSQL.

## Verificación previa

```bash
docker build -t simons-publicidad .
docker run --rm -p 8080:8080 \
  -e "ConnectionStrings__DefaultConnection=Host=...;Database=...;Username=...;Password=...;SSL Mode=VerifyFull" \
  -e "Deployment__DataProtectionKeysPath=/keys" \
  -v simons-keys:/keys \
  simons-publicidad
```

Comprobar en móvil, tablet y escritorio: navegación, formulario público, imágenes, paneles,
formularios largos, impresión de proforma y pedido. El dominio, certificado y restauración de
backup solo pueden cerrarse cuando exista el proveedor de producción y el dominio definitivo.
