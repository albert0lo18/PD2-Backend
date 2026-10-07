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
