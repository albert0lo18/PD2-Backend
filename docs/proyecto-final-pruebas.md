# Proyecto Final: pruebas unitarias de GeoExpande

## Proyecto y módulos

GeoExpande es una API REST desarrollada en C# con ASP.NET Core y Entity Framework Core para analizar oportunidades de expansión comercial por zonas geográficas.

| Módulo probado | Alcance | Archivo |
|---|---|---|
| Autenticación | Configuración JWT, emisión de claims y hash del identificador de revocación | `AutenticacionTests.cs` |
| Análisis geográfico | Parámetros opcionales, rangos de latitud/longitud y orden de límites | `AnalisisTests.cs` |
| Reportes | Firma PDF, datos, ausencia de zonas, métricas y opción de mapa/proveedores | `ReportesTests.cs` |

## Casos de prueba

### Autenticación

| Prueba | Tipo | Resultado esperado |
|---|---|---|
| `JwtSettings_AceptaConfiguracionValida` | Normal | Conserva la expiración configurada |
| `JwtSettings_RechazaClaveMenorA32Bytes` | Error | Lanza `InvalidOperationException` |
| `JwtSettings_RechazaExpiracionFueraDeRango` | Frontera | Rechaza una expiración inferior a 5 minutos |
| `TokenService_EmiteTokenConIdentidadYRol` | Lógica de negocio | JWT contiene identidad, rol, empresa y vencimiento futuro |
| `HashTokenId_EsDeterministaYNoExponeElIdOriginal` | Lógica de negocio | Hash SHA-256 estable de 64 caracteres hexadecimales |

### Análisis geográfico

| Prueba | Tipo | Resultado esperado |
|---|---|---|
| `Limites_OpcionalesVaciosSonValidos` | Normal | Acepta consulta sin filtro geográfico |
| `Limites_CompletosConCoordenadasValidasSonAceptados` | Normal | Acepta caja válida de Sonora |
| `Limites_RechazaParametrosParciales` | Error | Pide proporcionar los cuatro límites juntos |
| `Limites_RechazaCoordenadaFueraDeRango` | Error | Rechaza latitud menor que -90 |
| `Limites_RechazaRangoDegradadoEnFrontera` | Frontera | Rechaza mínimo igual a máximo |

### Reportes PDF

| Prueba | Tipo | Resultado esperado |
|---|---|---|
| `Build_GeneraDocumentoConFirmaPdf` | Normal | Comienza con la firma `%PDF-1.4` |
| `Build_IncluyeDatosPrincipalesDelAnalisis` | Normal | Incluye análisis, giro y conteo de zonas |
| `Build_IndicaCuandoNoHayZonas` | Límite | Explica que no hay zonas recomendadas |
| `Build_IncluyeZonaYMetricas` | Lógica de negocio | Incluye municipio, puntuación y recomendación |
| `Build_AnexaPaginaDeMapaCuandoSeSolicita` | Opcional/límite | Incluye mapa y datos de proveedores configurados |

## Ejecución

Desde la raíz del repositorio:

```powershell
dotnet test tests/GeoExpande.Tests/GeoExpande.Tests.csproj --logger "console;verbosity=normal"
```

Para guardar el resultado en un archivo de texto:

```powershell
dotnet test tests/GeoExpande.Tests/GeoExpande.Tests.csproj --logger "console;verbosity=normal" *> docs/evidencia-ejecucion.txt
```

## Evidencias para entregar

Guardar capturas reales de la consola con el resumen de pruebas y del Explorador de pruebas de Visual Studio, si se usa, en `docs/evidencias/`. Las capturas deben tomarse después de ejecutar la suite y mostrar el resultado PASSED/FAILED; no deben sustituirse por resultados escritos manualmente.

## Portada y conclusiones del alumno

Completar en la versión final del documento: nombre, matrícula, grupo y fecha. Añadir una reflexión propia sobre por qué las pruebas ayudaron a detectar errores, los retos al aislar lógica de base de datos y el nivel de calidad alcanzado. La presentación de 5–10 minutos puede seguir este orden: propósito de GeoExpande, tres módulos, ejemplos de casos y ejecución en vivo.
