# ATRIUM Academy

**Arquitectura · Diseño · Formación**

ATRIUM Academy es una plataforma web de formación orientada al aprendizaje de arquitectura, representación y herramientas digitales.

El sistema permite gestionar cursos, contenidos, pedidos, progreso académico, recursos y certificados desde una arquitectura organizada en capas, con autenticación mediante ASP.NET Core Identity y un panel administrativo independiente.

---

## Estado del proyecto

- ✅ Compilación sin errores.
- ✅ 83/83 pruebas automatizadas correctas.
- ✅ SQL Server + Entity Framework Core.
- ✅ ASP.NET Core Identity.
- ✅ Roles de estudiante y administrador.
- ✅ Panel administrativo.
- ✅ Seguimiento de progreso.
- ✅ Certificados.
- ✅ Módulo de contacto con SQL Server + Gmail SMTP.
- ✅ Diseño responsive.
- ⏳ Despliegue en Microsoft Azure.

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
- Administración de contenido de portada.

---

## Módulo de contacto

ATRIUM incluye un módulo de soporte funcional.

```text
Formulario
    ↓
Validación server-side
    ↓
SQL Server
    ↓
Consulta almacenada
    ↓
Gmail SMTP
    ↓
Notificación al correo de ATRIUM
```

La consulta se almacena primero en SQL Server.

Si Gmail falla temporalmente, la información permanece registrada en la base de datos y no se pierde.

---

## Tecnologías

```text
ASP.NET Core MVC
.NET 9
C#
Entity Framework Core 9
SQL Server
ASP.NET Core Identity
MailKit
Gmail SMTP
MSTest
Razor Views
HTML5
CSS3
JavaScript
```

---

## Arquitectura

La solución está organizada en cuatro proyectos principales:

```text
ATRIUM-Academy/
│
├── ATRIUM.Domain/
│   ├── Models/
│   └── Constants/
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
│
├── database/
├── docs/
├── ATRIUM.Academy.sln
├── .gitignore
└── README.md
```

### ATRIUM.Domain

Contiene las entidades, constantes y reglas pertenecientes al dominio.

### ATRIUM.Infrastructure

Gestiona la persistencia mediante Entity Framework Core, SQL Server, Identity, migraciones y SeedData.

### ATRIUM.Web

Contiene la aplicación ASP.NET Core MVC, controladores, servicios, ViewModels, Razor Views, CSS y JavaScript.

### ATRIUM.Tests

Contiene la suite automatizada de pruebas mediante MSTest.

---

## Seguridad

El proyecto incorpora:

- ASP.NET Core Identity.
- Autorización basada en roles.
- Validación de ownership sobre recursos privados.
- Protección CSRF mediante AntiForgeryToken.
- Validaciones server-side.
- Protección contra overposting.
- Validación de imágenes.
- Validación de URLs HTTP/HTTPS.
- Restricciones e índices a nivel de base de datos.
- Invalidación de sesiones cuando corresponde.
- Protección del último administrador.
- User Secrets para información sensible.

Las credenciales privadas no se almacenan dentro del repositorio.

---

## Configuración local

### Requisitos

```text
.NET SDK 9
Visual Studio
SQL Server
Entity Framework Core 9
```

Clonar:

```bash
git clone <URL_DEL_REPOSITORIO>
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

Las credenciales deben mantenerse fuera del repositorio.

Ejemplo de estructura:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "TU_CONEXION_SQL_SERVER"
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
  }
}
```

Nunca publicar valores reales dentro de:

```text
appsettings.json
appsettings.Development.json
README.md
```

---

## Base de datos

ATRIUM utiliza SQL Server mediante Entity Framework Core.

Aplicar migraciones desde Visual Studio:

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

Migraciones principales:

```text
20260915045435_InitialCreate
20260916064111_AtriumTechnicalRefactor
20261001185352_AddContactSupportModule
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
- Entity Framework Core.
- Servicios.
- Controladores.
- Seguridad declarativa.
- Acceso a cursos.
- Almacenamiento de imágenes.

---

## Rendimiento

Durante el desarrollo se realizaron mejoras orientadas a:

- Eliminación de consultas N+1.
- Uso de `AsNoTracking`.
- Proyecciones específicas.
- Uso de `Any()` / `EXISTS`.
- Agregaciones SQL.
- Reducción de `Include`.
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

Estilo:

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

## Próximos pasos

- Despliegue en Microsoft Azure.
- Panel administrativo de consultas.
- Reintento de correos fallidos.
- Observabilidad.
- CI/CD.
- Ampliación de pruebas.
- Mejoras continuas de accesibilidad y UX.

---

## Autor

**Benjamin Rumay**

Software Developer  
Ingeniería de Sistemas Computacionales

---

## ATRIUM Academy

**Arquitectura · Diseño · Formación**
