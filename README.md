# ATRIUM Academy

**Arquitectura · Diseño · Formación**

**Baseline actual después del refactor técnico senior, la fase QA y la optimización de rendimiento. Esta versión se considera Release Candidate hasta validar `build`, `test` y smoke tests en Visual Studio.**

## Solución

```text
ATRIUM-Academy/
├── ATRIUM.Academy.sln
├── ATRIUM.Domain/          # Entidades, constantes y validaciones de dominio
├── ATRIUM.Infrastructure/  # EF Core, Identity store, migraciones y SeedData
├── ATRIUM.Web/             # MVC, ViewModels, servicios, Razor, CSS/JS
├── ATRIUM.Tests/           # Suite QA con MSTest
├── database/               # Migración del nombre histórico de la base
└── docs/                   # Documentación visual y de roles
```

## Documentación obligatoria antes de trabajar sobre esta versión

- [`README_AUDITORIA_DEBUGGING.md`](README_AUDITORIA_DEBUGGING.md): problemas encontrados, causa, riesgo y corrección.
- [`README_CAMBIOS_REFACTOR.md`](README_CAMBIOS_REFACTOR.md): qué cambió, por qué y qué mejoró.
- [`database/README_DATABASE.md`](database/README_DATABASE.md): transición `VIMOD_Web` → `ATRIUM_Academy` y migración EF correctiva.
- [`docs/VALIDACION_ESTATICA.md`](docs/VALIDACION_ESTATICA.md): verificaciones estructurales ejecutadas antes de empaquetar.
- [`README_PRUEBAS_QA_MSTEST.md`](README_PRUEBAS_QA_MSTEST.md): suite QA y casos cubiertos.
- [`README_RENDIMIENTO.md`](README_RENDIMIENTO.md): auditoría de rendimiento por impacto y código optimizado.
- [`README_RESUMEN_SISTEMA.md`](README_RESUMEN_SISTEMA.md): visión técnica general del sistema y de su evolución.

## Requisitos

- .NET SDK 9.x
- Visual Studio 2022 actualizado con ASP.NET and web development
- SQL Server local
- Entity Framework CLI 9.x si se usarán comandos `dotnet ef`

## Primer arranque

1. Abre `ATRIUM.Academy.sln`.
2. Marca `ATRIUM.Web` como proyecto de inicio.
3. Restaura paquetes NuGet.
4. Compila la solución.
5. Sigue `database/README_DATABASE.md` antes de aplicar cambios a una base existente.
6. Configura el administrador mediante User Secrets.

### User Secrets

```powershell
dotnet user-secrets set "AdminSeed:Email" "admin@atrium.local" --project .\ATRIUM.Web\ATRIUM.Web.csproj
dotnet user-secrets set "AdminSeed:Password" "TU_CLAVE_SEGURA" --project .\ATRIUM.Web\ATRIUM.Web.csproj
```

La contraseña no debe escribirse en `appsettings.json`.

## Conexión local predeterminada

```text
Server=localhost;Database=ATRIUM_Academy;Integrated Security=True;TrustServerCertificate=True;
```

## Roles

- `Administrador`
- `Estudiante`

La aplicación usa autorización global por defecto: una acción es privada salvo que se marque explícitamente con `[AllowAnonymous]`.

## Estado de la baseline

La solución ya incorpora **naming técnico, debugging, seguridad, mantenibilidad, QA con MSTest y optimizaciones de rendimiento de bajo riesgo**. Los cambios futuros deberían ser incrementales y respaldados por nuevas necesidades o mediciones reales.

## Fase QA — MSTest

La baseline de QA añade una suite MSTest orientada a comportamiento esperado, entradas inválidas, valores vacíos, límites, errores esperados, seguridad y casos poco comunes.

Documentación principal:

- `README_PRUEBAS_QA_MSTEST.md`: catálogo completo de pruebas con objetivo, resultado esperado y código.
- `README_CAMBIOS_QA.md`: cambios y defectos corregidos durante la fase QA.
- `docs/VALIDACION_QA_ESTATICA.md`: comprobaciones estáticas realizadas antes del empaquetado.

Ejecutar:

```powershell
dotnet test .\ATRIUM.Tests\ATRIUM.Tests.csproj
```


## Fase de rendimiento — v6

Documentación:

- `README_RENDIMIENTO.md`: problemas por impacto, optimizaciones aplicadas y código modificado.
- `README_RESUMEN_SISTEMA.md`: explicación técnica general de la arquitectura y de toda la evolución del sistema.
- `docs/VALIDACION_RENDIMIENTO_ESTATICA.md`: comprobaciones estáticas específicas de esta fase.

Validación local recomendada:

```powershell
dotnet restore
dotnet build
dotnet test .\ATRIUM.Tests\ATRIUM.Tests.csproj
```
