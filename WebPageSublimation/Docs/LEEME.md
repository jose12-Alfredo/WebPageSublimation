# Documentación del proyecto

Dos carpetas, dos propósitos distintos. Se entregan por separado.

## `01-Estandar-Tecnico/`

**[ESTANDAR-TECNICO-MONOLITO.md](01-Estandar-Tecnico/ESTANDAR-TECNICO-MONOLITO.md)** — contrato
de construcción de JsonCorp para aplicaciones monolíticas. Describe **cómo** se construye, sin
nada del dominio: stack, arquitectura de capas, patrones, render, endpoints, seguridad,
pruebas, SEO, despliegue y convenciones de código.

Es reusable. Se le pasa a un agente de IA junto con la especificación de negocio de *otro*
proyecto para que salga con la misma estructura técnica que este.

## `02-Negocio/`

**[DOCUMENTACION-FUNCIONAL.md](02-Negocio/DOCUMENTACION-FUNCIONAL.md)** — qué hace el sistema:
los dos flujos de venta, las trece reglas de negocio con su ubicación en el código, el modelo
de precios, los roles, la superficie de pantallas y endpoints, el estado de los entregables y
los puntos abiertos.

**[PRUEBAS-PUNTA-A-PUNTA.md](02-Negocio/PRUEBAS-PUNTA-A-PUNTA.md)** — resultados medidos de
ejercer la aplicación levantada: reglas verificadas con sus valores reales, matriz de
autorización, los ocho fallos encontrados y corregidos, verificación en contenedor Linux y
cobertura automatizada.

## `03-Experiencia/`

**[PLAN-MEJORA-WEB.md](03-Experiencia/PLAN-MEJORA-WEB.md)** — auditoría Impeccable, comparación
con la referencia visual, dirección de diseño, fases de implementación, criterios de
verificación e inventario de las fotografías entregadas para el catálogo.

## Suelto

**[DESPLIEGUE.md](DESPLIEGUE.md)** — puesta en marcha en Coolify: variables de entorno, volumen
de claves, proxy inverso y verificaciones previas. Es operativo de *este* proyecto, no encaja
en ninguna de las dos categorías anteriores.

**[PLAN-Y-AVANCE.md](PLAN-Y-AVANCE.md)** — plan de ejecución, decisiones arquitectónicas,
criterios de aceptación por fase y bitácora del estado real del desarrollo. Debe actualizarse
durante el trabajo para poder retomarlo sin depender del historial de conversación.

---

El `README.md` de la raíz del repositorio queda donde está: es lo que muestra el servidor Git
al abrir el proyecto.

`PRODUCT.md` y `DESIGN.md`, ubicados en la raíz, son las fuentes durables de contexto de
producto y sistema visual utilizadas por Impeccable.
