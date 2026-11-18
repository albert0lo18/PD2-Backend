# Formato de respuestas JSON para el frontend

Este documento describe las respuestas que entrega la API para autenticación, selectores de sector y giro, y perfil de empresa. ASP.NET Core serializa las propiedades en `camelCase`; por ejemplo, `IdUsuario` se recibe como `idUsuario`.

## Registro e inicio de sesión

`POST /api/auth/register` y `POST /api/auth/login` devuelven el mismo objeto cuando la operación es exitosa:

```json
{
  "token": "eyJ...",
  "expiresAt": "2026-10-06T20:30:00Z",
  "idUsuario": 12,
  "nombre": "Ana",
  "correo": "ana@empresa.com",
  "rol": "Usuario"
}
```

| Campo | Tipo | Uso en el frontend |
| --- | --- | --- |
| `token` | string | Guardarlo para enviar `Authorization: Bearer <token>` en endpoints protegidos. |
| `expiresAt` | string ISO 8601 | Fecha y hora de expiración del token. |
| `idUsuario` | number | Identificador del usuario. |
| `nombre` | string | Nombre del usuario. |
| `correo` | string | Correo de inicio de sesión. |
| `rol` | string | Nombre del rol asignado. |

El registro valida formato/longitud de los campos, requiere una contraseña de al menos 12 caracteres (máximo 72 bytes UTF-8) y verifica que el giro pertenezca al sector activo. El correo se normaliza a minúsculas. El inicio de sesión valida el formato del correo y mantiene el mismo mensaje para credenciales inválidas o cuentas desactivadas.

`POST /api/auth/logout` requiere el JWT actual y lo revoca; los siguientes usos de ese token responden `401`. La validación de cada JWT protegido también consulta que el usuario y, cuando aplica, la empresa sigan activos.

Para persistir revocaciones, aplicar una sola vez el script `docs/sql/crear_tokens_revocados.sql` a la base PostgreSQL antes de usar rutas protegidas con JWT.

La configuración JWT requiere una clave de al menos 32 bytes, emisor y audiencia no vacíos, y `ExpiresMinutes` entre 5 y 1440. Valores ausentes, mal formados o fuera de rango detienen el arranque con un mensaje de configuración claro.

## Selectores de sector y giro

`GET /api/sectores` devuelve un arreglo, incluso cuando no hay resultados:

```json
[
  {
    "idSector": 1,
    "codigo": "31-33",
    "nombre": "Industrias manufactureras",
    "descripcion": "Descripción del sector",
    "activo": true
  }
]
```

`GET /api/sectores/{idSector}/giros` devuelve un arreglo con la misma estructura básica para los giros:

```json
[
  {
    "idGiro": 4,
    "idSector": 1,
    "codigo": "311",
    "nombre": "Industria alimentaria",
    "descripcion": "Descripción del giro",
    "activo": true
  }
]
```

Usa `idSector` como valor del selector de sectores y `idGiro` como valor del selector de giros. Al cambiar el sector, consulta de nuevo el endpoint de giros pasando el `idSector` seleccionado.

## Perfil de empresa

`GET /api/empresa/perfil` responde con un solo objeto que contiene únicamente estos ocho campos:

```json
{
  "correo": "contacto@empresa.com",
  "razonSocial": "Empresa Ejemplo S.A. de C.V.",
  "nombreComercial": "Empresa Ejemplo",
  "ciudadBase": "Hermosillo",
  "idSector": 1,
  "idGiro": 4,
  "tamano": "Mediana",
  "descripcionNegocio": "Descripción del negocio"
}
```

`PUT /api/empresa/actualizar` acepta esos mismos campos. Cuando la actualización es exitosa responde `204 No Content`, por lo que no hay cuerpo JSON que procesar. El campo `correo` aquí es el correo de contacto de la empresa; no es el correo de inicio de sesión del usuario.

Ambos endpoints de perfil requieren el encabezado `Authorization: Bearer <token>`.

## Preferencias de expansión

`GET /api/empresa/preferencias` devuelve las preferencias más recientes de la empresa autenticada. Si todavía no hay una fila guardada, devuelve los campos nulos y `regionesInteres` vacío.

`PUT /api/empresa/preferencias` crea o actualiza la fila más reciente de `preferencias_expansion` y devuelve el objeto guardado:

```json
{
  "presupuestoMinimoMXN": null,
  "presupuestoMaximoMXN": 4500000,
  "radioBusquedaKm": 50,
  "numeroUnidadesDeseadas": null,
  "regionesInteres": ["Noroeste", "Bajío"],
  "prioridadCompetencia": null,
  "prioridadPoblacion": null,
  "prioridadProveedores": null,
  "prioridadAccesibilidad": null
}
```

El endpoint valida rangos numéricos, presupuesto mínimo frente al máximo y un máximo de 500 caracteres para las regiones concatenadas. Ambos endpoints requieren JWT.

## Dashboard

`GET /api/dashboard/resumen` requiere JWT y devuelve métricas para la empresa del usuario autenticado, calculadas desde sus análisis y las filas DENUE del giro configurado. `puntuacion` es la puntuación máxima guardada para ese municipio; el endpoint devuelve hasta cinco municipios.

```json
{
  "analisisRealizados": 3,
  "zonasEvaluadas": 12,
  "zonasAltoPotencial": 4,
  "establecimientosGiro": 125,
  "ultimaActualizacionDenue": "2026-08-01",
  "metricas": [
    { "etiqueta": "Análisis realizados", "valor": 3, "delta": "En tu cuenta" }
  ],
  "comparativaCiudades": [
    { "ciudad": "Hermosillo", "puntuacion": 82, "establecimientosGiro": 125 }
  ]
}
```

## Reportes ejecutivos

Las tres rutas requieren JWT. Para persistir tanto el historial como el archivo PDF, ejecutar una vez `docs/sql/crear_reportes.sql`. La tabla conserva el PDF en `bytea`; debe consultarse mediante la API con una conexión de backend que tenga permisos para pasar RLS.

`POST /api/reportes/ejecutivo` recibe el análisis y las opciones del reporte. Valida que el análisis pertenezca al usuario autenticado. La respuesta es el PDF como `application/pdf`; los encabezados `X-Reporte-Id` y `X-Generado-En` identifican el registro creado.

```json
{
  "idAnalisis": 123,
  "incluirMapas": true,
  "incluirProveedores": false
}
```

`GET /api/reportes/historial?page=1&pageSize=10` devuelve un objeto paginado con `page`, `pageSize`, `totalItems`, `totalPages` e `items`. Cada item incluye `idReporte`, `idAnalisis`, `nombreAnalisis`, `fechaGeneracion`, `incluirMapas`, `incluirProveedores` y `nombreArchivo`.

`GET /api/reportes/{idReporte}/archivo` devuelve `application/pdf` y solo permite descargar reportes del usuario autenticado. Si el análisis o reporte no pertenece al usuario o no existe, responde `404`.

La generación de recomendaciones acepta `GET` y `POST /api/empresa/recomendar-zonas` (JWT requerido). `GET` recibe `idGiro` y `radioKm` opcional en la query string para compatibilidad con el cliente web actual; `POST` recibe esos campos en un cuerpo JSON. Para integraciones nuevas se recomienda `POST`. Devuelve `201 Created`, `Location` a `GET /api/analisis/{idAnalisis}/zonas` y un cuerpo con `idAnalisis`, `idEscenario`, `zonasGuardadas` y `recomendaciones`. Cada recomendación contiene latitud y longitud validadas dentro de sus rangos geográficos; si no hay zonas, no se guarda el análisis.

`GET /api/analisis/{analisisId}/zonas?nivel=municipal|estatal|nacional` devuelve zonas guardadas con coordenadas válidas. `municipal` es el nivel predeterminado. `estatal` agrupa las zonas municipales guardadas por estado y `nacional` las resume en una sola zona; los resúmenes usan únicamente las recomendaciones persistidas para ese análisis (hasta cinco). Para ambos resúmenes devuelve `nivel`, `idEstado`/`estado` e `idMunicipio`/`municipio` como nulos cuando no aplican, además de `zonasAgrupadas`, puntaje y centro geográfico.

Las respuestas de error usan `ProblemDetails` (`type`, `title`, `status`, `detail`, `instance`); errores de validación incluyen `errors`. Las excepciones no controladas devuelven un error genérico y no exponen detalles internos.

`GET /api/analisis/historial` devuelve una lista directa para el cliente web. Si se envía `page` o `pageSize`, devuelve el historial paginado con `page`, `pageSize`, `totalItems`, `totalPages` e `items`. `page` inicia en 1 y `pageSize` acepta de 1 a 100.

Las rutas `GET /api/analisis/{analisisId}/zonas` y `GET /api/analisis/{analisisId}/puntos-calor` aceptan opcionalmente el cuadro visible completo mediante `minLat`, `maxLat`, `minLng` y `maxLng`; si se envía uno, deben enviarse los cuatro. Los límites geográficos válidos son latitud de -90 a 90 y longitud de -180 a 180, con mínimo menor que máximo. `puntos-calor` también acepta `zoom` de 0 a 22 (por defecto 10), agrupa los puntos según ese nivel y limita la consulta a 5,000 establecimientos base. Devuelve una lista directa de puntos agrupados con centro (`latitud`, `longitud`), `intensidad` e `cantidad`.

La opción `incluirMapas` agrega una vista esquemática vectorial a partir de las coordenadas guardadas; no incluye un mapa base. `incluirProveedores` incorpora los indicadores de proveedores cercanos guardados en las recomendaciones; no consulta un directorio de proveedores.

## Respuestas de error

Los errores de validación automática de ASP.NET Core (por ejemplo, cuando falta un campo requerido) usan el formato `ValidationProblemDetails`. En cambio, varios errores definidos por los controladores actualmente devuelven texto plano, no un objeto JSON. Ejemplos de códigos que puede manejar el frontend:

| Código | Caso |
| --- | --- |
| `400` | Contraseña inválida o giro que no corresponde al sector. |
| `401` | Falta un JWT válido o las credenciales de inicio de sesión son incorrectas. |
| `404` | El usuario autenticado no tiene una empresa asociada. |
| `409` | Ya existe un usuario con ese correo durante el registro. |
| `204` | Perfil actualizado correctamente; respuesta sin contenido. |

Para errores, conviene usar el código HTTP para decidir el flujo y leer el cuerpo como texto u objeto según el `Content-Type` recibido.
