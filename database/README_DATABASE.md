# Base de datos — ATRIUM Academy

## Nombre técnico actual

La aplicación usa `ATRIUM_Academy` mediante `ConnectionStrings:DefaultConnection`.

## Instalación que ya tenía la base anterior

1. Haz una copia de seguridad de la base local.
2. Detén ATRIUM Academy si está ejecutándose.
3. Ejecuta `01_RenameLegacyDatabase.sql` desde SQL Server Management Studio.
4. Desde la raíz de la solución genera **una sola vez** la migración que alinea el modelo actual con el snapshot histórico:

```powershell
dotnet ef migrations add AtriumTechnicalRefactor `
  --project .\ATRIUM.Infrastructure\ATRIUM.Infrastructure.csproj `
  --startup-project .\ATRIUM.Web\ATRIUM.Web.csproj `
  --context AtriumDbContext
```

5. Revisa la migración generada. Debe reflejar principalmente longitudes de columnas, relaciones de certificados e índices únicos del modelo actual.
6. Aplícala:

```powershell
dotnet ef database update `
  --project .\ATRIUM.Infrastructure\ATRIUM.Infrastructure.csproj `
  --startup-project .\ATRIUM.Web\ATRIUM.Web.csproj `
  --context AtriumDbContext
```

> No elimines `20260915045435_InitialCreate`: representa el historial que ya está registrado en `__EFMigrationsHistory`.

## Instalación nueva

Genera primero `AtriumTechnicalRefactor` como se indica arriba y después ejecuta `database update`. EF aplicará `InitialCreate` y luego la migración correctiva.

## Por qué no se incluyó una migración EF generada manualmente

El snapshot histórico pertenece al modelo anterior y la migración correctiva debe ser producida por las herramientas oficiales de EF Core sobre el modelo compilado actual. En el entorno donde se realizó esta auditoría no está disponible el SDK de .NET; escribir a mano el snapshot o fingir una migración generada habría sido más riesgoso que dejar un paso explícito y verificable para Visual Studio.
