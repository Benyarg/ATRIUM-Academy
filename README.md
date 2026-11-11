# 🏛️ ATRIUM Academy | Plataforma de Formación

<p align="center">
  <img src="https://img.shields.io/badge/.NET-9.0-512BD4?style=for-the-badge&logo=dotnet&logoColor=white" alt=".NET 9" />
  <img src="https://img.shields.io/badge/ASP.NET_Core-MVC-512BD4?style=for-the-badge&logo=dotnet&logoColor=white" alt="ASP.NET Core MVC" />
  <img src="https://img.shields.io/badge/Entity_Framework_Core-9.0.4-68217A?style=for-the-badge&logo=nuget&logoColor=white" alt="Entity Framework Core" />
  <img src="https://img.shields.io/badge/PostgreSQL-Neon-4169E1?style=for-the-badge&logo=postgresql&logoColor=white" alt="PostgreSQL Neon" />
  <img src="https://img.shields.io/badge/Docker-Container-2496ED?style=for-the-badge&logo=docker&logoColor=white" alt="Docker" />
  <img src="https://img.shields.io/badge/Deployment-Vercel-000000?style=for-the-badge&logo=vercel&logoColor=white" alt="Vercel" />
</p>

> **"Arquitectura · Diseño · Formación"**
>
> Plataforma web de formación especializada en arquitectura, representación y herramientas digitales, diseñada para gestionar experiencias de aprendizaje mediante cursos, contenidos educativos y seguimiento académico.

<p align="center">
  <a href="https://atrium-academy-rouge.vercel.app"><strong>🌐 Ver plataforma</strong></a>
  ·
  <a href="https://github.com/Benyarg/ATRIUM-Academy"><strong>💻 Ver repositorio</strong></a>
</p>

---

## 📖 Descripción general

**ATRIUM Academy** es una plataforma web de formación orientada al aprendizaje de arquitectura, representación y herramientas digitales.

El proyecto nace de la necesidad de ofrecer un entorno digital que permita administrar cursos, contenidos educativos, recursos, pedidos y seguimiento académico desde una misma solución.

La aplicación está desarrollada con **ASP.NET Core MVC sobre .NET 9**, siguiendo una arquitectura organizada en capas que separa las responsabilidades del dominio, la infraestructura y la presentación.

El sistema incorpora autenticación mediante ASP.NET Core Identity, persistencia con PostgreSQL, almacenamiento de imágenes en Neon Object Storage e integración de correo electrónico mediante Gmail SMTP.

El resultado es una plataforma educativa que centraliza la gestión académica y administrativa, facilitando tanto la experiencia de aprendizaje de los estudiantes como el control operativo de la institución.

---

## ✨ Filosofía de diseño (UI/UX)

La interfaz se construyó siguiendo principios de diseño minimalista, arquitectónico y editorial, buscando transmitir profesionalismo, claridad y una identidad visual coherente con el sector de la arquitectura y el diseño.

### 🎨 Identidad visual

La propuesta visual se caracteriza por una combinación de elementos que priorizan la sobriedad y la jerarquía del contenido:

- **Estética Dark:** utilización de tonos oscuros para construir una interfaz elegante y contemporánea.
- **Diseño minimalista:** eliminación de elementos visuales innecesarios para mantener el enfoque en el contenido.
- **Identidad arquitectónica:** composición visual inspirada en principios de proporción, equilibrio y organización espacial.

### 🔤 Tipografía editorial

Se utiliza una combinación tipográfica que equilibra personalidad y legibilidad:

- **Space Grotesk:** aporta carácter visual y personalidad a los títulos.
- **Inter:** utilizada en textos y elementos de interfaz para proporcionar claridad y legibilidad.

### 🧩 Componentes y composición

La interfaz integra componentes organizados por responsabilidades, con especial atención a la presentación de cursos, contenidos, formularios y paneles administrativos.

### 📐 Diseño responsive

La plataforma se adapta a diferentes tamaños de pantalla, incluyendo escritorio, tablets y dispositivos móviles, reorganizando los elementos para conservar una experiencia de navegación consistente.

---

## 🚀 Características principales

La plataforma organiza sus funcionalidades según los diferentes perfiles de usuario.

### 1. Experiencia del visitante

- **Página de inicio:** presentación institucional y contenido destacado.
- **Información institucional:** información sobre ATRIUM Academy y su propuesta formativa.
- **Catálogo de cursos:** consulta de la oferta académica disponible.
- **Detalle de cursos:** visualización de información y características de cada curso.
- **Preguntas frecuentes:** sección informativa para resolver consultas habituales.
- **Formulario de contacto:** registro de consultas mediante un formulario.
- **Autenticación:** registro e inicio de sesión de usuarios.
- **Validación de certificados:** consulta pública de la autenticidad de certificados.

### 2. Funcionalidades del estudiante

- **Cursos adquiridos:** acceso a los cursos disponibles para el estudiante.
- **Carrito de compras:** administración de los cursos seleccionados.
- **Gestión de pedidos:** consulta y seguimiento de pedidos.
- **Recursos educativos:** acceso a materiales complementarios de aprendizaje.
- **Seguimiento académico:** registro del progreso en los contenidos de los cursos.
- **Certificados:** gestión y consulta de certificados obtenidos.
- **Perfil de usuario:** administración de información personal.
- **Historial de pedidos:** consulta de operaciones realizadas.

### 3. Administración de la plataforma

- **Dashboard administrativo:** panel centralizado de gestión.
- **Usuarios:** administración de cuentas y roles.
- **Categorías:** organización de la oferta académica.
- **Cursos:** creación, edición y administración de cursos.
- **Módulos y contenidos:** estructuración del material educativo.
- **Recursos:** administración de archivos y materiales complementarios.
- **Pedidos:** seguimiento y administración de pedidos.
- **Certificados:** gestión de certificados académicos.
- **Contenido de portada:** administración del carousel y elementos destacados.

### 4. Integración de correo electrónico

- **Gmail SMTP:** envío de notificaciones de nuevas consultas.
- **Persistencia de consultas:** almacenamiento de la información en PostgreSQL antes de realizar el envío.
- **Gestión de errores:** si el envío de correo falla temporalmente, la consulta permanece registrada en la base de datos.

### 5. Almacenamiento de imágenes

La plataforma utiliza **Neon Object Storage** mediante su API compatible con Amazon S3 para almacenar imágenes de cursos y elementos del carousel.

- Formatos permitidos: JPG, JPEG, PNG y WEBP.
- Tamaño máximo: 5 MB por archivo.
- Validación de extensión, MIME y firma binaria.
- Identificadores únicos mediante GUID.
- Separación lógica entre `cursos/` y `carousel/`.
- Eliminación asíncrona de archivos cuando corresponde.
- Implementación local mediante `LocalImageStorageService` para desarrollo y pruebas.

---

## 🛠️ Tecnologías utilizadas

### Backend, persistencia e integraciones

| Tecnología | Versión | Aplicación |
|---|---:|---|
| .NET | 9.0 | Plataforma de ejecución y desarrollo. |
| ASP.NET Core MVC | 9.0 | Framework para aplicaciones web MVC. |
| C# | — | Lenguaje de programación backend. |
| Entity Framework Core | 9.0.4 | ORM y acceso a datos. |
| ASP.NET Core Identity | 9.0.4 | Autenticación y gestión de usuarios y roles. |
| Npgsql EF Core | 9.0.4 | Proveedor de PostgreSQL para Entity Framework Core. |
| PostgreSQL / Neon | — | Base de datos relacional. |
| AWS SDK for .NET (S3) | 4.0.104.1 | Integración con almacenamiento compatible con S3. |
| Neon Object Storage | — | Almacenamiento persistente de imágenes. |
| MailKit | 4.18.1 | Gestión de conexiones y envío de correo SMTP. |
| Gmail SMTP | — | Servicio de correo electrónico. |

### Frontend y presentación

| Tecnología | Aplicación |
|---|---|
| Razor Views | Generación de vistas dinámicas. |
| HTML5 | Estructura semántica. |
| CSS3 | Estilos y diseño responsive. |
| JavaScript | Interacciones y comportamiento de la interfaz. |

### Pruebas y calidad de software

| Tecnología | Versión | Aplicación |
|---|---:|---|
| MSTest | 3.6.1 | Pruebas unitarias y automatizadas. |
| Microsoft.NET.Test.Sdk | 17.11.1 | Infraestructura de ejecución de pruebas. |
| EF Core InMemory | 9.0.4 | Persistencia en memoria para pruebas. |

### Herramientas y despliegue

| Tecnología | Aplicación |
|---|---|
| Docker | Contenerización de la aplicación. |
| Vercel | Plataforma de despliegue mediante contenedor. |
| Visual Studio 2022 | Entorno de desarrollo. |
| Git | Control de versiones. |
| GitHub | Alojamiento del repositorio. |

---

## 🏗️ Arquitectura del proyecto

ATRIUM Academy sigue una arquitectura en capas, organizada para separar las responsabilidades del dominio, la infraestructura y la aplicación web.

```text
ATRIUM-Academy/
│
├── ATRIUM.Domain/
│   ├── Models/                  # Entidades del dominio
│   ├── Constants/               # Constantes de aplicación
│   └── Validation/              # Validaciones del dominio
│
├── ATRIUM.Infrastructure/
│   ├── Context/                  # Contexto de Entity Framework
│   ├── Migrations/               # Migraciones de base de datos
│   └── SeedData/                 # Datos iniciales
│
├── ATRIUM.Web/
│   ├── Controllers/              # Controladores MVC
│   ├── Services/                 # Servicios e integraciones
│   ├── ViewModels/               # Modelos para las vistas
│   ├── Views/                    # Vistas Razor
│   └── wwwroot/                  # Recursos estáticos
│
├── ATRIUM.Tests/                 # Suite de pruebas automatizadas
├── database/                     # Recursos relacionados con la BD
├── docs/                         # Documentación del proyecto
├── Dockerfile.vercel             # Configuración del contenedor
├── vercel.json                   # Configuración de despliegue
├── ATRIUM.Academy.sln            # Solución de Visual Studio
└── README.md                     # Documentación principal
```

### ATRIUM.Domain

Contiene las entidades, constantes y reglas de validación propias del dominio de negocio.

Su responsabilidad es mantener la lógica fundamental de la plataforma independiente de las tecnologías de infraestructura y presentación.

### ATRIUM.Infrastructure

Gestiona los mecanismos de persistencia y acceso a datos mediante Entity Framework Core y PostgreSQL.

Incluye:

- Contexto de base de datos.
- Configuración de Identity.
- Migraciones.
- Datos iniciales mediante SeedData.
- Configuración de relaciones, índices y restricciones.

### ATRIUM.Web

Contiene la aplicación ASP.NET Core MVC y sus diferentes componentes de presentación y lógica de aplicación.

Incluye:

- Controladores MVC.
- Servicios de aplicación.
- ViewModels.
- Vistas Razor.
- Recursos estáticos.
- Integraciones con servicios externos.

Entre sus principales servicios se encuentran:

| Servicio | Responsabilidad |
|---|---|
| `CourseAccessService` | Gestión del acceso a cursos. |
| `GmailContactEmailService` | Envío de notificaciones mediante Gmail SMTP. |
| `LocalImageStorageService` | Almacenamiento local de imágenes para desarrollo y pruebas. |
| `NeonImageStorageService` | Administración de imágenes mediante Neon Object Storage. |

### ATRIUM.Tests

Contiene la suite de pruebas automatizadas basada en MSTest, orientada a validar componentes, servicios, controladores, entidades y reglas de negocio.

---

## 🧠 Decisiones de ingeniería

Durante el desarrollo se priorizaron criterios de mantenibilidad, separación de responsabilidades, seguridad y eficiencia en el acceso a datos.

- **Arquitectura por capas:** separación entre dominio, infraestructura y presentación para facilitar el mantenimiento y evolución de la aplicación.
- **Persistencia mediante ORM:** utilización de Entity Framework Core para administrar entidades, relaciones, consultas y migraciones.
- **Autenticación centralizada:** implementación de ASP.NET Core Identity con roles de estudiante y administrador.
- **Servicios desacoplados:** separación de las integraciones de correo y almacenamiento de imágenes mediante servicios independientes.
- **Almacenamiento externo:** utilización de Neon Object Storage para evitar depender del sistema de archivos local en producción.
- **Persistencia de consultas:** almacenamiento de las solicitudes de contacto antes de intentar enviar notificaciones por correo.
- **Gestión de datos:** utilización de proyecciones específicas, `AsNoTracking`, `Any()` y agregaciones SQL para reducir operaciones innecesarias.
- **Optimización de consultas:** reducción de problemas N+1, `Include` innecesarios y viajes adicionales a la base de datos.
- **Compresión de respuestas:** implementación de Response Compression para optimizar la transferencia de contenido.
- **Optimización visual:** utilización de imágenes WebP para reducir el peso de los recursos gráficos.

---

## 🔒 Seguridad

La plataforma incorpora diferentes mecanismos de protección en las operaciones de autenticación, autorización, persistencia y administración de archivos.

### Autenticación y autorización

- ASP.NET Core Identity para la gestión de usuarios y contraseñas.
- Autorización basada en roles de estudiante y administrador.
- Política global de autenticación con excepciones explícitas mediante `AllowAnonymous`.
- Validación de propiedad (*ownership*) sobre recursos privados.
- Bloqueo temporal después de intentos fallidos de autenticación.
- Protección de la cuenta del último administrador.

### Protección de operaciones

- Protección CSRF mediante `AntiForgeryToken`.
- Validaciones en el servidor (*server-side validation*).
- Protección contra asignación masiva de propiedades (*overposting*).
- Validación de URL mediante protocolos HTTP y HTTPS.
- Restricciones e índices de integridad en la base de datos.

### Protección de archivos y credenciales

- Validación de imágenes por extensión, MIME, tamaño y firma binaria.
- Identificadores únicos para los archivos almacenados.
- User Secrets para configuración de desarrollo.
- Variables de entorno para configuración de producción.
- Secret scanning y protecciones del repositorio en GitHub.

Las credenciales privadas y los datos sensibles de configuración no deben almacenarse en el repositorio.

---

## 🖥️ Arquitectura de despliegue

La aplicación utiliza Vercel como plataforma de despliegue mediante un contenedor Docker que ejecuta ASP.NET Core.

La arquitectura integra tres servicios principales:

```text
                  GitHub
                     │
                     ▼
                  Vercel
                     │
                     ▼
            Docker Container
                     │
                     ▼
              ASP.NET Core 9
                     │
          ┌──────────┼──────────┐
          │          │          │
          ▼          ▼          ▼
      PostgreSQL   Object      Gmail
        Neon       Storage      SMTP
          │          │          │
          ▼          ▼          ▼
       Datos      Imágenes   Notificaciones
```

### Módulo de contacto

El proceso de registro y notificación de consultas sigue este flujo:

```text
Formulario de contacto
         │
         ▼
Validación server-side
         │
         ▼
Persistencia en PostgreSQL
         │
         ▼
Consulta registrada
         │
         ▼
Envío mediante Gmail SMTP
         │
         ▼
Notificación al correo de ATRIUM
```

La persistencia se realiza antes del envío de correo. Esto permite conservar la consulta aunque se produzca un fallo temporal en el servicio SMTP.

### Almacenamiento de imágenes

```text
Administrador
      │
      ▼
Validación del archivo
      │
      ▼
NeonImageStorageService
      │
      ▼
Neon Object Storage
      │
      ▼
Generación de URL persistente
      │
      ▼
Almacenamiento de URL en PostgreSQL
```

Las imágenes se organizan en dos áreas lógicas:

- `cursos/`: imágenes asociadas a cursos.
- `carousel/`: imágenes utilizadas en el contenido destacado de portada.

---

## ⚙️ Instalación y ejecución

### Requisitos previos

- .NET SDK 9.0.
- Visual Studio 2022 o un entorno compatible.
- PostgreSQL o una instancia de Neon.
- Cuenta de Neon con acceso a Object Storage.
- Cuenta de Gmail con contraseña de aplicación para SMTP.
- Git.

### 1. Clonar el repositorio

```bash
git clone https://github.com/Benyarg/ATRIUM-Academy.git
```

### 2. Acceder al proyecto

```bash
cd ATRIUM-Academy
```

### 3. Restaurar dependencias

```bash
dotnet restore
```

### 4. Compilar la solución

```bash
dotnet build
```

---

## 🔐 Configuración de User Secrets

Las credenciales de desarrollo deben mantenerse fuera del repositorio mediante el sistema de configuración segura de .NET.

El proyecto utiliza el identificador de User Secrets:

`ATRIUM-Academy-Development`

Para registrar una cadena de conexión:

```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "TU_CONEXION_POSTGRESQL" --project ATRIUM.Web/ATRIUM.Web.csproj
```

### Estructura de configuración

La aplicación utiliza las siguientes secciones de configuración:

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

Esta estructura es una referencia de los parámetros requeridos; los valores reales deben configurarse mediante User Secrets y nunca publicarse.

---

## 🗄️ Configuración de base de datos

ATRIUM utiliza PostgreSQL alojado en Neon mediante el proveedor `Npgsql.EntityFrameworkCore.PostgreSQL`.

Para aplicar las migraciones desde Visual Studio Package Manager Console:

```powershell
Update-Database -Project ATRIUM.Infrastructure -StartupProject ATRIUM.Web -Context AtriumDbContext
```

También puedes utilizar la CLI de Entity Framework:

```bash
dotnet ef database update \
  --project ATRIUM.Infrastructure \
  --startup-project ATRIUM.Web \
  --context AtriumDbContext
```

---

## ▶️ Ejecutar la aplicación

Para iniciar la aplicación en entorno de desarrollo:

```bash
dotnet run --project ATRIUM.Web/ATRIUM.Web.csproj
```

Una vez iniciada, abre en el navegador la dirección local indicada por ASP.NET Core.

---

## 🧪 Pruebas y calidad de software

El proyecto incorpora una suite de pruebas automatizadas basada en MSTest para validar las principales responsabilidades de la aplicación.

### Ejecución de pruebas

```bash
dotnet test ./ATRIUM.Tests/ATRIUM.Tests.csproj
```

### Estado de las pruebas

<badge color="success">83/83 pruebas correctas</badge>

| Resultado | Cantidad |
|---|---:|
| Pruebas totales | 83 |
| Correctas | 83 |
| Errores | 0 |
| Omitidas | 0 |

### Cobertura funcional documentada

Las pruebas contemplan:

- ViewModels.
- Validaciones.
- Entidades.
- Modelo de Entity Framework Core.
- Servicios.
- Controladores.
- Seguridad declarativa.
- Acceso a cursos.
- Validación y almacenamiento local de imágenes.

El estado indicado corresponde al último resultado documentado del proyecto.

---

## 🚀 Despliegue en producción

La plataforma está configurada para desplegarse en **Vercel mediante un contenedor Docker**, utilizando imágenes oficiales de .NET 9.

### Archivos de configuración

| Archivo | Responsabilidad |
|---|---|
| `Dockerfile.vercel` | Construcción y publicación de la aplicación ASP.NET Core. |
| `vercel.json` | Configuración del servicio de contenedor y las rutas de entrada. |

### Flujo de despliegue

```text
git push origin main
         │
         ▼
      GitHub
         │
         ▼
  Vercel Deployment
         │
         ▼
   Docker Build
         │
         ▼
 ASP.NET Core 9
         │
         ▼
 PostgreSQL + Object Storage
```

El contenedor realiza la compilación en configuración `Release`, publica `ATRIUM.Web` y ejecuta la aplicación utilizando el puerto proporcionado por Vercel.

### Variables de entorno en Vercel

Las credenciales de producción se configuran desde el panel de Vercel.

ASP.NET Core permite utilizar `__` para representar la separación de secciones de configuración.

**Base de datos:**

```text
ConnectionStrings__DefaultConnection
```

**Correo electrónico:**

```text
Email__Host
Email__Port
Email__SenderEmail
Email__SenderName
Email__Username
Email__AppPassword
Email__RecipientEmail
```

**Administrador inicial:**

```text
AdminSeed__Email
AdminSeed__Password
AdminSeed__Nombre
AdminSeed__Apellido
```

**Almacenamiento de imágenes:**

```text
ObjectStorage__ServiceUrl
ObjectStorage__AccessKey
ObjectStorage__SecretKey
ObjectStorage__Region
ObjectStorage__BucketName
```

Los valores sensibles deben mantenerse exclusivamente en la configuración del entorno de despliegue.

---

## ⚡ Rendimiento y optimización

Durante el desarrollo se aplicaron mejoras para reducir operaciones innecesarias y optimizar la transferencia de información.

- **Consultas eficientes:** eliminación de consultas N+1.
- **Lecturas optimizadas:** utilización de `AsNoTracking` cuando no se requiere seguimiento de entidades.
- **Proyecciones específicas:** recuperación únicamente de los datos necesarios.
- **Existencia de registros:** utilización de `Any()` y operaciones equivalentes a `EXISTS`.
- **Agregaciones SQL:** delegación de operaciones de agregación a la base de datos.
- **Relaciones optimizadas:** reducción de instrucciones `Include` innecesarias.
- **Menos consultas:** disminución de viajes entre la aplicación y PostgreSQL.
- **Compresión HTTP:** implementación de Response Compression.
- **Optimización de imágenes:** utilización de formatos WebP para reducir el peso de los recursos visuales.

---

## 🎨 Identidad visual

ATRIUM Academy adopta una identidad gráfica inspirada en la arquitectura, el diseño editorial y las plataformas educativas contemporáneas.

**Nombre de marca:**

```text
ATRIUM Academy
Arquitectura · Diseño · Formación
```

**Estilo visual:**

- Minimalista.
- Arquitectónico.
- Dark.
- Editorial.

**Tipografías:**

- Space Grotesk.
- Inter.

La interfaz mantiene una composición visual coherente y adaptable a escritorio, tablets y dispositivos móviles.

---

## 🔮 Mejoras futuras

Entre las funcionalidades y mejoras contempladas para próximas versiones se encuentran:

- Panel administrativo para gestionar consultas de contacto.
- Reintento automático de correos electrónicos fallidos.
- Persistencia externa de Data Protection Keys para escenarios multi-instancia.
- Implementación de observabilidad y métricas.
- Integración de validaciones automatizadas adicionales en CI/CD.
- Ampliación de la cobertura de pruebas para Neon Object Storage.
- Mejoras continuas de accesibilidad y experiencia de usuario.

---

## 👨‍💻 Autor

**Benjamin Rumay** · Software Developer

Diseño y desarrollo web enfocado en la creación de experiencias digitales modernas, funcionales y orientadas a las necesidades de cada negocio.

[![GitHub](https://img.shields.io/badge/GitHub-Benyarg-181717?style=for-the-badge&logo=github)](https://github.com/Benyarg)

---

## 📄 Licencia

Este proyecto se distribuye bajo la licencia **MIT**.

La licencia se aplica al código fuente cuyos derechos corresponden al autor. Los logotipos, marcas comerciales, fotografías, contenidos educativos y demás recursos pertenecientes a ATRIUM Academy o a terceros quedan excluidos de esta concesión y conservan sus respectivos derechos de propiedad intelectual.

Consulta el archivo [LICENSE](./LICENSE) para conocer los términos completos.
