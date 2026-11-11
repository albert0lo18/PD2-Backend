from pathlib import Path
from docx import Document
from docx.shared import Inches, Pt, RGBColor
from docx.enum.text import WD_ALIGN_PARAGRAPH
from docx.enum.table import WD_TABLE_ALIGNMENT, WD_CELL_VERTICAL_ALIGNMENT
from docx.oxml import OxmlElement
from docx.oxml.ns import qn

ROOT = Path(__file__).resolve().parents[1]
OUT = ROOT / 'docs' / 'Proyecto_Final_GeoExpande.docx'
EVIDENCE = Path(r'C:\Users\alberto\AppData\Local\Temp\codex-clipboard-ad1fe416-7aea-442a-82b9-c5d47e1e1855.png')
doc = Document()
sec = doc.sections[0]
sec.top_margin = Inches(.68); sec.bottom_margin = Inches(.68)
sec.left_margin = Inches(.78); sec.right_margin = Inches(.78)

styles = doc.styles
styles['Normal'].font.name = 'Aptos'; styles['Normal'].font.size = Pt(10)
styles['Normal'].font.color.rgb = RGBColor(42, 52, 65)
for sname, size, color in [('Title', 28, '12304A'), ('Heading 1', 19, '12304A'), ('Heading 2', 13, '087E8B')]:
    s = styles[sname]; s.font.name = 'Aptos Display'; s.font.size = Pt(size); s.font.bold = True; s.font.color.rgb = RGBColor.from_string(color)
    s.paragraph_format.space_before = Pt(12); s.paragraph_format.space_after = Pt(6)

def shade(cell, fill):
    pr = cell._tc.get_or_add_tcPr(); shd = OxmlElement('w:shd'); shd.set(qn('w:fill'), fill); pr.append(shd)

def set_cell_text(cell, value, bold=False, color=None, size=8):
    cell.text = ''
    p = cell.paragraphs[0]; p.paragraph_format.space_after = Pt(0)
    r = p.add_run(str(value)); r.bold = bold; r.font.name = 'Aptos'; r.font.size = Pt(size)
    if color: r.font.color.rgb = RGBColor.from_string(color)
    cell.vertical_alignment = WD_CELL_VERTICAL_ALIGNMENT.CENTER

def table(headers, rows, widths=None):
    t = doc.add_table(rows=1, cols=len(headers)); t.alignment = WD_TABLE_ALIGNMENT.CENTER; t.style = 'Table Grid'
    for i,h in enumerate(headers):
        set_cell_text(t.rows[0].cells[i], h, True, 'FFFFFF', 8); shade(t.rows[0].cells[i], '12304A')
    for idx,row in enumerate(rows):
        cells = t.add_row().cells
        for i,v in enumerate(row):
            set_cell_text(cells[i], v, size=7.5)
            if idx % 2 == 1: shade(cells[i], 'F1F6F8')
    if widths:
        for row in t.rows:
            for i,w in enumerate(widths): row.cells[i].width = Inches(w)
    doc.add_paragraph().paragraph_format.space_after = Pt(0)
    return t

def bullet(txt):
    doc.add_paragraph(txt, style='List Bullet')

# Cover
p=doc.add_paragraph(); p.alignment=WD_ALIGN_PARAGRAPH.CENTER; p.paragraph_format.space_before=Pt(82)
r=p.add_run('PROYECTO FINAL'); r.bold=True; r.font.size=Pt(13); r.font.color.rgb=RGBColor.from_string('087E8B')
p=doc.add_paragraph(style='Title'); p.alignment=WD_ALIGN_PARAGRAPH.CENTER; p.add_run('Pruebas unitarias de GeoExpande')
p=doc.add_paragraph(); p.alignment=WD_ALIGN_PARAGRAPH.CENTER
r=p.add_run('Tercer parcial  |  Calidad y pruebas de software'); r.font.size=Pt(13); r.font.color.rgb=RGBColor.from_string('516579')
doc.add_paragraph('\n')
table(['Datos del alumno','Información'], [
    ('Nombre','____________________________________________'),
    ('Matrícula','____________________________________________'),
    ('Grupo','____________________________________________'),
    ('Materia / docente','____________________________________________'),
    ('Fecha de entrega','____________________________________________')], [2.0,4.45])
p=doc.add_paragraph(); p.alignment=WD_ALIGN_PARAGRAPH.CENTER; p.paragraph_format.space_before=Pt(50)
p.add_run('Sistema evaluado: API GeoExpande  |  Lenguaje: C#  |  Framework: MSTest')
doc.add_page_break()

doc.add_heading('Resumen del proyecto', 1)
doc.add_paragraph('Este documento presenta pruebas unitarias para GeoExpande, una API REST desarrollada en C# con ASP.NET Core. El sistema apoya el análisis de oportunidades de expansión comercial por zonas geográficas y genera reportes ejecutivos en PDF. La suite contiene 15 pruebas en tres módulos y su ejecución registrada reportó 15 pruebas correctas, 0 errores y 0 omitidas.')
doc.add_heading('Descripción del sistema y módulos', 1)
doc.add_paragraph('GeoExpande recibe y procesa información de análisis de zonas, protege el acceso mediante tokens JWT y permite generar documentos PDF con resultados y recomendaciones. Para este trabajo se aislaron funciones deterministas que pueden probarse sin iniciar PostgreSQL ni depender de servicios externos.')
table(['Módulo','Responsabilidad cubierta','Componente probado'], [
    ('Autenticación','Validar configuración JWT, claims de identidad/rol y hash del identificador de revocación.','JwtSettings, TokenService, TokenRevocationService'),
    ('Análisis geográfico','Validar límites opcionales de latitud/longitud usados en consultas de zonas.','GeographicBoundsValidator'),
    ('Reportes PDF','Construir PDF con resumen, zonas, métricas y opción de mapa/proveedores.','ReportePdfBuilder')], [1.25,3.5,1.7])

doc.add_heading('Diseño de las pruebas', 1)
doc.add_paragraph('Las pruebas aplican el patrón Arrange, Act, Assert: preparan datos de entrada, ejecutan la operación bajo prueba y comparan el resultado con lo esperado. Se incluyen entradas válidas, inválidas, valores frontera y reglas de negocio. Al ser pruebas unitarias, no requieren credenciales ni conexión a la base de datos.')

doc.add_heading('Casos por módulo', 1)
doc.add_heading('Autenticación', 2)
auth=[
('JwtSettings_AceptaConfiguracionValida','Normal','La expiración de 60 minutos se conserva.'),
('JwtSettings_RechazaClaveMenorA32Bytes','Error','Lanza InvalidOperationException si la clave tiene menos de 32 bytes.'),
('JwtSettings_RechazaExpiracionFueraDeRango','Frontera','Rechaza una expiración de 4 minutos; el mínimo permitido es 5.'),
('TokenService_EmiteTokenConIdentidadYRol','Negocio','El token incluye sujeto, rol, empresa y vencimiento futuro.'),
('HashTokenId_EsDeterministaYNoExponeElIdOriginal','Negocio','El mismo ID produce hash SHA-256 estable de 64 caracteres, distinto al original.')]
table(['Prueba','Tipo','Resultado esperado'],auth,[2.5,1.0,2.95])

doc.add_heading('Análisis geográfico', 2)
analysis=[
('Limites_OpcionalesVaciosSonValidos','Normal','Acepta una consulta sin filtro geográfico.'),
('Limites_CompletosConCoordenadasValidasSonAceptados','Normal','Acepta una caja geográfica válida de Sonora.'),
('Limites_RechazaParametrosParciales','Error','Rechaza límites incompletos y solicita los cuatro parámetros.'),
('Limites_RechazaCoordenadaFueraDeRango','Error','Rechaza latitud menor que -90.'),
('Limites_RechazaRangoDegradadoEnFrontera','Frontera','Rechaza latitud o longitud con mínimo igual al máximo.')]
table(['Prueba','Tipo','Resultado esperado'],analysis,[2.5,1.0,2.95])

doc.add_heading('Reportes PDF', 2)
reports=[
('Build_GeneraDocumentoConFirmaPdf','Normal','El documento comienza con la firma %PDF-1.4.'),
('Build_IncluyeDatosPrincipalesDelAnalisis','Normal','Incluye nombre de análisis, giro y cantidad de zonas.'),
('Build_IndicaCuandoNoHayZonas','Límite','Muestra un mensaje cuando el análisis no contiene zonas recomendadas.'),
('Build_IncluyeZonaYMetricas','Negocio','Incluye municipio, puntuación y recomendación de la zona.'),
('Build_AnexaPaginaDeMapaCuandoSeSolicita','Opcional','Incluye página de mapa y sección de proveedores cuando se solicitan.')]
table(['Prueba','Tipo','Resultado esperado'],reports,[2.5,1.0,2.95])

doc.add_heading('Evidencia de ejecución', 1)
doc.add_paragraph('La captura corresponde a la ejecución de MSTest desde PowerShell. El resumen muestra 15 pruebas totales, 15 correctas y 0 errores. La compilación terminó correctamente; reportó advertencias de versiones de Entity Framework Core que no impidieron ejecutar la suite.')
if EVIDENCE.exists():
    p=doc.add_paragraph(); p.alignment=WD_ALIGN_PARAGRAPH.CENTER
    p.add_run().add_picture(str(EVIDENCE), width=Inches(6.5))
    cp=doc.add_paragraph('Figura 1. Resultado de ejecución MSTest proporcionado para el proyecto.'); cp.alignment=WD_ALIGN_PARAGRAPH.CENTER
else:
    doc.add_paragraph('[Insertar captura de la ejecución de MSTest: 15 correctas, 0 errores.]')
doc.add_paragraph('Comando de ejecución desde la carpeta raíz del repositorio:')
p=doc.add_paragraph(); r=p.add_run('dotnet test .\\tests\\GeoExpande.Tests\\GeoExpande.Tests.csproj --logger "console;verbosity=normal"'); r.font.name='Consolas'; r.font.size=Pt(9)

doc.add_heading('Código fuente de las pruebas', 1)
doc.add_paragraph('Los archivos fuente completos se incluyen en el apéndice para que cada caso pueda revisarse y reproducirse.')
for title, rel in [
    ('Pruebas del módulo de autenticación','tests/GeoExpande.Tests/AutenticacionTests.cs'),
    ('Pruebas del módulo de análisis geográfico','tests/GeoExpande.Tests/AnalisisTests.cs'),
    ('Pruebas del módulo de reportes PDF','tests/GeoExpande.Tests/ReportesTests.cs')]:
    doc.add_page_break(); doc.add_heading(title, 1)
    p=doc.add_paragraph()
    run=p.add_run((ROOT / rel).read_text(encoding='utf-8-sig')); run.font.name='Consolas'; run.font.size=Pt(7)
    run._element.rPr.rFonts.set(qn('w:eastAsia'),'Consolas')

doc.add_heading('Conclusiones', 1)
doc.add_paragraph('Las pruebas unitarias permiten comprobar reglas de validación y resultados esperados antes de integrar los módulos o publicar cambios. En GeoExpande, validar límites geográficos evita consultas con parámetros incompletos o coordenadas fuera del rango permitido; probar la creación de tokens verifica claims esenciales de identidad; y probar el generador PDF confirma que el reporte conserve su estructura y datos principales.')
doc.add_paragraph('Un reto técnico fue separar la validación geográfica del controlador para probarla de forma aislada. También fue necesario construir datos de prueba controlados para emitir tokens y reportes sin acceder a una base de datos. La suite ejecutada obtuvo 15 resultados correctos, lo que aporta confianza sobre estos comportamientos cubiertos; no sustituye pruebas de integración del API, del proveedor PostgreSQL ni una revisión visual completa de los PDF generados.')
doc.add_paragraph('Reflexión personal del alumno: agregar una experiencia propia sobre el reto principal, el aprendizaje más útil y qué pruebas adicionales realizaría.')

doc.add_heading('Guion breve para la presentación', 1)
for line in [
    '1. Presentar GeoExpande y el problema que resuelve (1 minuto).',
    '2. Explicar autenticación, análisis geográfico y reportes PDF (2 minutos).',
    '3. Mostrar ejemplos normal, de error y de frontera de las pruebas (2 minutos).',
    '4. Ejecutar dotnet test y explicar el resultado 15/15 (2 minutos).',
    '5. Comentar límites de cobertura y aprendizaje personal (1 minuto).']:
    bullet(line)

# Page number field in footer
for section in doc.sections:
    fp=section.footer.paragraphs[0]; fp.alignment=WD_ALIGN_PARAGRAPH.CENTER
    rr=fp.add_run('GeoExpande  |  Proyecto final  •  '); rr.font.size=Pt(8); rr.font.color.rgb=RGBColor.from_string('647789')
    fld=OxmlElement('w:fldSimple'); fld.set(qn('w:instr'),'PAGE'); fp._p.append(fld)

doc.core_properties.title='Proyecto Final Pruebas Unitarias de GeoExpande'
doc.core_properties.subject='Pruebas unitarias en C# con MSTest'
doc.save(OUT)
print(OUT)
