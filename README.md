# ATRIUM Academy

**Arquitectura · Diseño · Formación**

ATRIUM Academy es una plataforma web de formación orientada al aprendizaje de arquitectura, representación y herramientas digitales.

El sistema permite gestionar cursos, contenidos, pedidos, progreso académico, recursos, certificados y comunicación con usuarios desde una solución ASP.NET Core organizada en capas. Incluye autenticación con ASP.NET Core Identity, panel administrativo, persistencia en PostgreSQL y almacenamiento de imágenes en Neon Object Storage.

---

## Estado del proyecto

- ✅ Compilación sin errores.
- ✅ 83/83 pruebas automatizadas correctas.
- ✅ ASP.NET Core MVC sobre .NET 9.
- ✅ PostgreSQL en Neon + Entity Framework Core.
- ✅ ASP.NET Core Identity con roles de estudiante y administrador.
- ✅ Panel administrativo.
- ✅ Seguimiento de progreso y certificados.
- ✅ Módulo de contacto con persistencia en PostgreSQL + Gmail SMTP.
- ✅ Almacenamiento persistente de imágenes con Neon Object Storage.
- ✅ Despliegue en Vercel mediante contenedor Docker.
- ✅ Diseño responsive para escritorio, tablet y móvil.

---

## Funcionalidades

### Visitante

- Página de inicio.
- Información institucional.
- Catálogo público de cursos.
- Detalle de cursos.
- Preguntas frecuentes.
- Formulario de contacto.
- Registro e inicio de sesión.
- Validación pública de certificados.

### Estudiante

- Acceso a cursos adquiridos.
- Carrito de compras.
- Gestión de pedidos.
- Recursos educativos.
- Seguimiento del progreso.
- Certificados.
- Perfil de usuario.
- Historial de pedidos.

### Administrador

- Dashboard administrativo.
- Gestión de usuarios.
- Gestión de categorías.
- Gestión de cursos.
- Gestión de módulos.
- Gestión de contenidos.
- Gestión de recursos.
- Gestión de pedidos.
- Gestión de certificados.
- Administración del carousel y contenido de portada.

---

## Arquitectura de despliegue

```text
GitHub
   ↓
Vercel
   ↓
ASP.NET Core .NET 9
   ↓
├── Neon PostgreSQL
│     └── Datos de la aplicación
│
├── Neon Object Storage
│     ├── cursos/
│     └── carousel/
│
└── Gmail SMTP
      └── Notificaciones de contacto
```

La aplicación se publica en Vercel utilizando `Dockerfile.vercel` y `vercel.json`. PostgreSQL y Object Storage se alojan en Neon, mientras que las credenciales se proporcionan mediante variables de entorno y nunca se almacenan en el repositorio.

---

## Módulo de contacto

```text
Formulario
    ↓
Validación server-side
    ↓
PostgreSQL / Neon
    ↓
Consulta almacenada
    ↓
Gmail SMTP
    ↓
Notificación al correo de ATRIUM
```

La consulta se persiste primero en PostgreSQL. Si el envío de correo falla temporalmente, la información continúa registrada en la base de datos.

---

## Almacenamiento de imágenes

Las imágenes nuevas de cursos y carousel se almacenan de forma persistente en Neon Object Storage mediante su API compatible con S3.

```text
Administrador
    ↓
Validación de archivo
    ↓
NeonImageStorageService
    ↓
Neon Object Storage
    ↓
URL persistente almacenada en PostgreSQL
```

Características:

- JPG, JPEG, PNG y WEBP.
- Tamaño máximo de 5 MB.
- Validación de extensión, MIME y firma binaria.
- Nombres de archivo únicos mediante GUID.
- Separación lógica entre `cursos/` y `carousel/`.
- Eliminación asíncrona del archivo cuando corresponde.
- `LocalImageStorageService` se conserva como implementación local y para pruebas.

---

## Tecnologías

| Área | Tecnología |
| --- | --- |
| Backend | ASP.NET Core MVC, .NET 9, C# |
| Frontend | Razor Views, HTML5, CSS3, JavaScript |
| Persistencia | Entity Framework Core 9 |
| Base de datos | PostgreSQL / Neon |
| Autenticación | ASP.NET Core Identity |
| Object Storage | Neon Object Storage, AWS SDK for .NET (S3) |
| Correo | MailKit + Gmail SMTP |
| Pruebas | MSTest |
| Contenedores | Docker |
| Despliegue | Vercel |
| Control de versiones | Git + GitHub |

---

## Arquitectura de la solución

```text
ATRIUM-Academy/
│
├── ATRIUM.Domain/
│   ├── Models/
│   ├── Constants/
│   └── Validation/
│
├── ATRIUM.Infrastructure/
│   ├── Context/
│   ├── Migrations/
│   └── SeedData/
│
├── ATRIUM.Web/
│   ├── Controllers/
│   ├── Services/
│   ├── ViewModels/
│   ├── Views/
│   └── wwwroot/
│
├── ATRIUM.Tests/
├── database/
├── docs/
├── Dockerfile.vercel
├── vercel.json
├── ATRIUM.Academy.sln
├── .gitignore
└── README.md
```

### ATRIUM.Domain

Contiene entidades, constantes y validaciones propias del dominio.

### ATRIUM.Infrastructure

Gestiona la persistencia con Entity Framework Core, PostgreSQL, Identity, migraciones y SeedData.

### ATRIUM.Web

Contiene la aplicación MVC, controladores, servicios, ViewModels, Razor Views, recursos estáticos e integraciones externas.

Entre los servicios principales se encuentran:

- `CourseAccessService`.
- `GmailContactEmailService`.
- `LocalImageStorageService`.
- `NeonImageStorageService`.

### ATRIUM.Tests

Contiene la suite automatizada basada en MSTest.

---

## Seguridad

El proyecto incorpora:

- ASP.NET Core Identity.
- Autorización basada en roles.
- Política global de autenticación con excepciones explícitas mediante `AllowAnonymous`.
- Validación de ownership sobre recursos privados.
- Protección CSRF mediante `AntiForgeryToken`.
- Validaciones server-side.
- Protección contra overposting.
- Validación de imágenes por extensión, MIME, tamaño y firma binaria.
- Validación de URLs HTTP/HTTPS.
- Restricciones e índices a nivel de base de datos.
- Lockout por intentos fallidos de autenticación.
- Protección del último administrador.
- User Secrets para desarrollo local.
- Variables de entorno para producción.
- Secret scanning y protecciones del repositorio en GitHub.

Las credenciales privadas no se almacenan dentro del repositorio.

---

## Configuración local

### Requisitos

```text
.NET SDK 9
Visual Studio 2022 o compatible
PostgreSQL / Neon
Cuenta Neon con Object Storage
```

Clonar el repositorio:

```bash
git clone https://github.com/Benyarg/ATRIUM-Academy.git
cd ATRIUM-Academy
```

Restaurar dependencias:

```bash
dotnet restore
```

Compilar:

```bash
dotnet build
```

---

## User Secrets

Las credenciales de desarrollo deben mantenerse fuera del repositorio.

Ejemplo de estructura:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "TU_CONEXION_POSTGRESQL"
  },
  "AdminSeed": {
    "Email": "TU_ADMIN",
    "Password": "TU_PASSWORD",
    "Nombre": "Administrador",
    "Apellido": "ATRIUM"
  },
  "Email": {
    "Host": "smtp.gmail.com",
    "Port": 587,
    "SenderEmail": "TU_CORREO@gmail.com",
    "SenderName": "ATRIUM Academy",
    "Username": "TU_CORREO@gmail.com",
    "AppPassword": "TU_APP_PASSWORD",
    "RecipientEmail": "TU_CORREO@gmail.com"
  },
  "ObjectStorage": {
    "ServiceUrl": "TU_ENDPOINT_S3",
    "AccessKey": "TU_ACCESS_KEY",
    "SecretKey": "TU_SECRET_KEY",
    "Region": "TU_REGION",
    "BucketName": "atrium-assets"
  }
}
```

Nunca publicar valores reales dentro de:

```text
appsettings.json
appsettings.Development.json
README.md
.env.local
```

---

## Variables de entorno en Vercel

ASP.NET Core traduce `__` a `:` en las variables de entorno.

```text
ConnectionStrings__DefaultConnection

Email__Host
Email__Port
Email__SenderEmail
Email__SenderName
Email__Username
Email__AppPassword
Email__RecipientEmail

AdminSeed__Email
AdminSeed__Password
AdminSeed__Nombre
AdminSeed__Apellido

ObjectStorage__ServiceUrl
ObjectStorage__AccessKey
ObjectStorage__SecretKey
ObjectStorage__Region
ObjectStorage__BucketName
```

Los valores sensibles deben configurarse únicamente desde el entorno de despliegue.

---

## Base de datos

ATRIUM utiliza PostgreSQL alojado en Neon mediante `Npgsql.EntityFrameworkCore.PostgreSQL`.

Aplicar migraciones desde Visual Studio Package Manager Console:

```powershell
Update-Database -Project ATRIUM.Infrastructure -StartupProject ATRIUM.Web -Context AtriumDbContext
```

Mediante CLI:

```bash
dotnet ef database update \
  --project ATRIUM.Infrastructure \
  --startup-project ATRIUM.Web \
  --context AtriumDbContext
```

Migraciones PostgreSQL actuales:

```text
20261001233651_InitialPostgreSql
20261001234200_FixUsuarioFechaNacimientoPostgreSql
```

---

## Ejecutar la aplicación

```bash
dotnet run --project ATRIUM.Web/ATRIUM.Web.csproj
```

---

## Pruebas

Ejecutar:

```bash
dotnet test ./ATRIUM.Tests/ATRIUM.Tests.csproj
```

Estado actual:

```text
Total:     83
Correctas: 83
Errores:    0
Omitidas:   0
```

Las pruebas cubren:

- ViewModels.
- Validaciones.
- Entidades.
- Modelo de Entity Framework Core.
- Servicios.
- Controladores.
- Seguridad declarativa.
- Acceso a cursos.
- Validación y almacenamiento local de imágenes.

---

## Despliegue

El despliegue de producción utiliza Vercel con un contenedor ASP.NET Core.

Archivos principales:

```text
Dockerfile.vercel
vercel.json
```

El contenedor publica `ATRIUM.Web` en modo `Release` y expone la aplicación utilizando el puerto proporcionado por Vercel.

Flujo:

```text
git push origin main
        ↓
GitHub
        ↓
Vercel deployment
        ↓
ASP.NET Core container
        ↓
Neon PostgreSQL + Neon Object Storage
```

---

## Rendimiento

Durante el desarrollo se aplicaron mejoras orientadas a:

- Eliminación de consultas N+1.
- Uso de `AsNoTracking`.
- Proyecciones específicas.
- Uso de `Any()` / `EXISTS`.
- Agregaciones SQL.
- Reducción de `Include` innecesarios.
- Menos viajes a base de datos.
- Response Compression.
- Optimización de imágenes WebP.

---

## Diseño

Identidad:

```text
ATRIUM Academy
Arquitectura · Diseño · Formación
```

Estilo visual:

```text
Minimalista
Arquitectónico
Dark
Editorial
```

Tipografías:

```text
Space Grotesk
Inter
```

La interfaz está adaptada para escritorio, tablet y dispositivos móviles.

---

## Mejoras futuras

- Panel administrativo de consultas de contacto.
- Reintento automático de correos fallidos.
- Persistencia externa de Data Protection Keys para escenarios multi-instancia.
- Observabilidad y métricas.
- CI/CD con validaciones automatizadas adicionales.
- Ampliación de cobertura de pruebas para Neon Object Storage.
- Mejoras continuas de accesibilidad y UX.

---

## Autor

**Benjamin Rumay**

Software Developer  
Ingeniería de Sistemas Computacionales

---

## ATRIUM Academy

**Arquitectura · Diseño · Formación**
