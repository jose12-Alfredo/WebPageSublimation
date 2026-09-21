# Publicación de Simons Publicidad

El proyecto está preparado para desplegarse como contenedor Docker y utilizar la base PostgreSQL existente en Neon. La configuración recomendada para una primera publicación es Render mediante `render.yaml`.

## Variables secretas obligatorias

Configurar en el panel del proveedor, sin incluir sus valores en Git:

- `ConnectionStrings__DefaultConnection`: conexión PostgreSQL de Neon con SSL.
- `InitialAdmin__Email`: correo del administrador inicial.
- `InitialAdmin__Password`: contraseña fuerte de al menos 12 caracteres, con mayúscula, minúscula, número y símbolo.

## Datos de demostración para una base nueva

Para una demostración sobre una base PostgreSQL vacía, el arranque aplica primero las migraciones y luego puede cargar datos de ejemplo de forma idempotente. Configure estas variables solo en el entorno demo:

- `DemoUsers__Enabled=true`
- `DemoData__Enabled=true`
- `DemoAccess__ShowCredentials=true` para mostrar en el login las cuentas demo y facilitar la presentación.
- `DemoUsers__PromoterEmail`, `DemoUsers__PromoterPassword` y `DemoUsers__PromoterName`
- `DemoUsers__ClientEmail` y `DemoUsers__ClientName`

La carga incluye las cuatro referencias de catálogo y sus seis fotografías aprobadas que ya están en el repositorio, un promotor, un cliente, una proforma y dos pedidos sintéticos. No copia usuarios, contraseñas, contactos, pedidos ni proformas reales de otra base. Con `DemoAccess__ShowCredentials=true`, la pantalla de acceso muestra las contraseñas de las cuentas demo configuradas. En producción mantenga las tres opciones en `false`.

La configuración de Render deshabilita la creación automática de usuarios demo. Las cuentas que ya existan en Neon permanecen en la base, por lo que sus contraseñas deben rotarse antes de hacer pública la dirección.

## Publicación con Render

1. Crear un repositorio privado en GitHub y subir este proyecto.
2. En Render, elegir **New → Blueprint** y conectar el repositorio.
3. Render detectará `render.yaml` y solicitará las tres variables secretas.
4. Confirmar la creación del servicio.
5. Esperar que `/health` responda correctamente y abrir la URL `onrender.com` asignada.
6. Probar inicio de sesión, catálogo, proforma, pedido y comisión.

El Blueprint usa el plan gratuito para realizar la primera publicación. Ese plan se suspende tras períodos sin tráfico y puede tardar cerca de un minuto en volver a iniciar. Para operación diaria conviene cambiar el servicio a un plan de pago y agregar un disco persistente montado en `/keys`, evitando que las sesiones se cierren después de cada reinicio.

## Dominio propio

Después de validar la dirección temporal:

1. Agregar el dominio en **Settings → Custom Domains**.
2. Copiar en el proveedor del dominio los registros DNS indicados por Render.
3. Esperar la validación del certificado TLS administrado.
4. Verificar redirección HTTPS e inicio de sesión.

## Comprobaciones antes de publicar

- Compilación Release sin errores.
- Pruebas automatizadas aprobadas.
- Contraseñas administrativas y de promotores rotadas.
- Productos y precios comerciales confirmados.
- Copia de seguridad disponible en Neon.
- `DemoUsers__Enabled=false`.
- Ningún secreto presente en el repositorio.
