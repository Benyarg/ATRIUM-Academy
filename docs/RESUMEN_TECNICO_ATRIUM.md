# ATRIUM Academy — Resumen técnico

## 1. Descripción

ATRIUM Academy es una plataforma web de formación enfocada en arquitectura, diseño y herramientas digitales.

Está desarrollada con ASP.NET Core MVC sobre .NET 9 y utiliza SQL Server mediante Entity Framework Core.

El sistema contempla tres contextos principales:

```text
Visitante
Estudiante
Administrador
```

---

# 2. Arquitectura

La solución se divide en:

```text
ATRIUM.Domain
ATRIUM.Infrastructure
ATRIUM.Web
ATRIUM.Tests
```

---

## ATRIUM.Domain

Contiene:

```text
Entidades
Constantes
Reglas de dominio
Validaciones
```

Entidades principales:

```text
Usuario
Curso
Categoria
Pedido
PedidoDetalle
Certificado
ProgresoEstudiante
ConsultaContacto
Carousel
ContenidoEducativo
ContenidoModulo
ModuloCurso
CarroCompras
```

Constantes principales:

```text
AppRoles
OrderStatuses
PaymentMethods
ModuleContentTypes
CertificateDefaults
ContactStatuses
```

---

## ATRIUM.Infrastructure

Responsable de la persistencia.

Contiene:

```text
AtriumDbContext
Entity Framework Core
Migraciones
Configuración de relaciones
Índices
ASP.NET Identity Store
SeedData
```

Base de datos:

```text
ATRIUM_Academy
```

Motor:

```text
SQL Server
```

---

## ATRIUM.Web

Aplicación ASP.NET Core MVC.

Contiene:

```text
Controllers
Services
ViewModels
Razor Views
CSS
JavaScript
Autenticación
Autorización
```

---

## ATRIUM.Tests

Framework:

```text
MSTest
```

Estado:

```text
83 pruebas
83 correctas
0 fallidas
```

---

# 3. Roles

Roles Identity:

```text
Administrador
Estudiante
```

El visitante no representa un rol almacenado.

---

## Visitante

Puede:

```text
ver Home
ver Nosotros
explorar cursos
ver detalles
consultar FAQ
usar Contacto
registrarse
iniciar sesión
validar certificados
```

---

## Estudiante

Puede:

```text
comprar cursos
gestionar carrito
consultar pedidos
acceder a cursos aprobados
consultar recursos
registrar progreso
obtener certificados
administrar su perfil
```

---

## Administrador

Puede gestionar:

```text
usuarios
categorías
cursos
módulos
contenidos
recursos
pedidos
certificados
portada
```

---

# 4. Autenticación

ATRIUM utiliza:

```text
ASP.NET Core Identity
```

Identity gestiona:

```text
usuarios
password hashes
cookies
roles
security stamps
lockout
sesiones
```

La aplicación utiliza autenticación global.

Las rutas públicas deben declararse mediante:

```csharp
[AllowAnonymous]
```

---

# 5. Seguridad

Se implementaron:

```text
autorización basada en roles
ownership de recursos
AntiForgeryToken
validación server-side
protección contra overposting
validación de imágenes
validación HTTP/HTTPS
transacciones
índices únicos
restricciones FK
protección del último administrador
security stamps
User Secrets
```

Los secretos no se almacenan dentro del repositorio.

---

# 6. User Secrets

Se utilizan para:

```text
ConnectionStrings
AdminSeed
Email SMTP
```

Ejemplo conceptual:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "..."
  },

  "AdminSeed": {
    "Email": "...",
    "Password": "..."
  },

  "Email": {
    "Host": "smtp.gmail.com",
    "Port": 587,
    "SenderEmail": "...",
    "Username": "...",
    "AppPassword": "...",
    "RecipientEmail": "..."
  }
}
```

---

# 7. Cursos

Un curso pertenece a una categoría.

Puede relacionarse con:

```text
módulos
contenidos
recursos
pedidos
certificados
```

El catálogo es público.

---

# 8. Pedidos

Flujo general:

```text
Estudiante
    ↓
Curso
    ↓
Carrito
    ↓
Pedido
    ↓
Pendiente
    ↓
Aprobación administrativa
    ↓
Acceso al curso
```

Los precios se recuperan desde el servidor para evitar confiar en valores manipulados desde el navegador.

---

# 9. Acceso a cursos

Servicios:

```text
ICourseAccessService
CourseAccessService
```

Responsabilidades:

```text
comprobar acceso aprobado
comprobar pedidos pendientes
obtener cursos no disponibles para nueva compra
```

---

# 10. Recursos educativos

Los recursos se relacionan con cursos.

Pueden contener:

```text
título
descripción
URL
estado
curso
```

---

# 11. Progreso

El sistema registra el avance de cada estudiante sobre los contenidos.

Permite:

```text
marcar contenido como completado
consultar progreso
utilizar progreso para certificados
```

---

# 12. Certificados

Un certificado se relaciona con:

```text
Usuario
Curso
Código único
Fecha de emisión
Ruta de archivo
```

El código único permite realizar validaciones públicas.

---

# 13. Módulo de contacto

Entidad:

```text
ConsultaContacto
```

Campos:

```text
IdConsulta
UsuarioId
Nombre
Email
TipoConsulta
NumeroPedido
Asunto
Mensaje
Estado
FechaCreacion
CorreoNotificacionEnviado
FechaCorreoEnviado
```

Estados:

```text
Pendiente
En revisión
Resuelta
```

---

# 14. Flujo del contacto

```text
Visitante / estudiante
        ↓
Formulario
        ↓
ContactoViewModel
        ↓
HomeController
        ↓
Validación
        ↓
SQL Server
        ↓
ConsultaContacto
        ↓
Gmail SMTP
```

La consulta se almacena antes de intentar enviar el correo.

Si SMTP falla:

```text
Consulta guardada = sí
Correo enviado = no
```

Esto evita pérdida de información.

---

# 15. Gmail SMTP

Servicios:

```text
IContactEmailService
GmailContactEmailService
```

Biblioteca:

```text
MailKit
```

Configuración:

```text
smtp.gmail.com
Puerto 587
STARTTLS
```

Autenticación:

```text
Gmail
+
Contraseña de aplicación
```

La contraseña normal de Gmail no debe utilizarse.

---

# 16. Persistencia de contacto

Migración:

```text
20261001185352_AddContactSupportModule
```

Tabla:

```text
ConsultasContacto
```

Índices:

```text
UsuarioId
Estado + FechaCreacion
```

La relación con `AspNetUsers` utiliza:

```text
ON DELETE SET NULL
```

Así la consulta puede mantenerse aunque el usuario asociado deje de existir.

---

# 17. Migraciones

Principales:

```text
20260915045435_InitialCreate

20260916064111_AtriumTechnicalRefactor

20261001185352_AddContactSupportModule
```

No eliminar migraciones ya aplicadas.

---

# 18. Servicios

Principales:

```text
ICourseAccessService
CourseAccessService

IImageStorageService
LocalImageStorageService

IContactEmailService
GmailContactEmailService
```

Se utilizan servicios donde existe una responsabilidad reutilizable concreta.

---

# 19. Imágenes

El almacenamiento valida:

```text
extensión
MIME
firma del archivo
tamaño
directorio autorizado
```

No se confía únicamente en la extensión enviada por el usuario.

---

# 20. Entity Framework Core

Se utilizan patrones como:

```text
AsNoTracking
Select
Any
Count
Sum
Include solo cuando es necesario
```

---

# 21. Rendimiento

Se aplicaron mejoras como:

```text
eliminación de N+1
reducción de Includes
proyecciones
agregaciones SQL
Any en lugar de materialización innecesaria
Response Compression
optimización WebP
reducción de viajes a base de datos
```

---

# 22. Testing

Proyecto:

```text
ATRIUM.Tests
```

Tecnologías:

```text
MSTest
EF Core InMemory
```

Áreas cubiertas:

```text
ViewModels
validaciones
dominio
Entity Framework Core
servicios
controladores
seguridad
almacenamiento de imágenes
acceso a cursos
```

Resultado actual:

```text
Total:      83
Correctas:  83
Errores:     0
Omitidas:    0
```

---

# 23. Home

Utiliza:

```text
HomeIndexViewModel
```

Muestra:

```text
cursos destacados
banners
categorías
total de cursos
total de recursos
```

Las consultas de solo lectura utilizan `AsNoTracking`.

---

# 24. Sistema visual

Marca:

```text
ATRIUM Academy
Arquitectura · Diseño · Formación
```

Estilo:

```text
minimalista
arquitectónico
dark
editorial
```

Tipografías:

```text
Space Grotesk
Inter
```

Paleta aproximada:

```text
#080808
#0D0D0E
#121315
#18191C
#F5F5F2
#A2A3A7
#27282A
```

---

# 25. Frontend

Tecnologías:

```text
Razor Views
HTML
CSS
JavaScript
```

La identidad visual se mantiene entre:

```text
sitio público
zona de estudiante
panel administrativo
```

---

# 26. Responsive

Se consideran:

```text
desktop
tablet
móvil
```

La navegación cambia a menú colapsable en resoluciones pequeñas.

---

# 27. Estructura final

```text
ATRIUM-Academy/
│
├── ATRIUM.Domain/
├── ATRIUM.Infrastructure/
├── ATRIUM.Web/
├── ATRIUM.Tests/
│
├── database/
├── docs/
│   └── RESUMEN_TECNICO_ATRIUM.md
│
├── ATRIUM.Academy.sln
├── .gitignore
└── README.md
```

---

# 28. Compilar

```powershell
dotnet restore
dotnet build
```

Resultado esperado:

```text
0 errores
```

---

# 29. Ejecutar pruebas

```powershell
dotnet test .\ATRIUM.Tests\ATRIUM.Tests.csproj
```

Resultado actual:

```text
83/83
```

---

# 30. Ejecutar ATRIUM

```powershell
dotnet run --project .\ATRIUM.Web\ATRIUM.Web.csproj
```

---

# 31. Crear migraciones

Visual Studio Package Manager Console:

```powershell
Add-Migration NombreMigracion -Project ATRIUM.Infrastructure -StartupProject ATRIUM.Web -Context AtriumDbContext
```

Aplicar:

```powershell
Update-Database -Project ATRIUM.Infrastructure -StartupProject ATRIUM.Web -Context AtriumDbContext
```

---

# 32. Base de datos

Nombre actual:

```text
ATRIUM_Academy
```

El servidor depende del entorno.

La cadena debe mantenerse en User Secrets o variables de entorno.

---

# 33. Git

Archivos que deben permanecer ignorados:

```text
.vs/
bin/
obj/
*.user
*.suo
```

Los User Secrets se almacenan fuera del repositorio.

---

# 34. Despliegue

Objetivo:

```text
Microsoft Azure
Azure for Students
```

En producción será necesario configurar:

```text
ConnectionStrings
AdminSeed
Email SMTP
```

mediante configuración segura del entorno.

---

# 35. Estado actual

```text
Arquitectura             ✅
Refactor técnico         ✅
Rebranding               ✅
Seguridad                ✅
Testing                  ✅
83/83                    ✅
Rendimiento              ✅
Responsive               ✅
SQL Server               ✅
Identity                 ✅
Roles                    ✅
Contacto SQL             ✅
Gmail SMTP               ✅
Git                      ✅
GitHub                   pendiente
Azure                    pendiente
Portafolio               pendiente
```

---

# 36. Decisiones técnicas

## Entity Framework Core directo

No se utiliza un Repository Pattern genérico adicional.

`AtriumDbContext` representa la unidad de trabajo y los `DbSet<T>` gestionan las entidades.

Los servicios se utilizan únicamente donde existe comportamiento reutilizable real.

---

## ViewModels separados del dominio

Entidades persistentes:

```text
ATRIUM.Domain/Models
```

Modelos de interfaz:

```text
ATRIUM.Web/ViewModels
```

---

## Secretos fuera del código

En desarrollo se utiliza User Secrets.

En producción deben utilizarse variables o secretos del proveedor de despliegue.

---

## SQL antes que Gmail

Las consultas se guardan antes de enviar la notificación.

Esto evita perder una solicitud si el servicio SMTP falla.

---

# 37. Próximas mejoras

```text
panel administrativo de consultas
reintento automático de correo
observabilidad
logging centralizado
más pruebas de integración
CI/CD
deploy automatizado
mejoras de accesibilidad
```

---

# ATRIUM Academy

**Arquitectura · Diseño · Formación**

Documento técnico resumido de referencia.
