using Amazon.Runtime;
using Amazon.S3;
using ATRIUM.Domain.Models;
using ATRIUM.Infrastructure.Context;
using ATRIUM.Infrastructure.SeedData;
using ATRIUM.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.DataProtection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews(options =>
{
    var authenticatedPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();

    options.Filters.Add(new AuthorizeFilter(authenticatedPolicy));

    options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
});

var connectionString =
    builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException(
        "No se encontró ConnectionStrings:DefaultConnection.");

builder.Services.AddDbContext<AtriumDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services
    .AddDataProtection()
    .PersistKeysToDbContext<AtriumDbContext>()
    .SetApplicationName("ATRIUM-Academy");

//
// OBJECT STORAGE - NEON
//

builder.Services.Configure<ObjectStorageOptions>(
    builder.Configuration.GetSection("ObjectStorage"));

var objectStorageOptions = builder.Configuration
    .GetSection("ObjectStorage")
    .Get<ObjectStorageOptions>()
    ?? throw new InvalidOperationException(
        "No se encontró la configuración ObjectStorage.");

if (string.IsNullOrWhiteSpace(objectStorageOptions.ServiceUrl))
{
    throw new InvalidOperationException(
        "No se configuró ObjectStorage:ServiceUrl.");
}

if (string.IsNullOrWhiteSpace(objectStorageOptions.AccessKey))
{
    throw new InvalidOperationException(
        "No se configuró ObjectStorage:AccessKey.");
}

if (string.IsNullOrWhiteSpace(objectStorageOptions.SecretKey))
{
    throw new InvalidOperationException(
        "No se configuró ObjectStorage:SecretKey.");
}

if (string.IsNullOrWhiteSpace(objectStorageOptions.Region))
{
    throw new InvalidOperationException(
        "No se configuró ObjectStorage:Region.");
}

if (string.IsNullOrWhiteSpace(objectStorageOptions.BucketName))
{
    throw new InvalidOperationException(
        "No se configuró ObjectStorage:BucketName.");
}

var objectStorageCredentials =
    new BasicAWSCredentials(
        objectStorageOptions.AccessKey,
        objectStorageOptions.SecretKey);

var s3Config = new AmazonS3Config
{
    ServiceURL = objectStorageOptions.ServiceUrl,
    AuthenticationRegion = objectStorageOptions.Region,
    ForcePathStyle = true
};

builder.Services.AddSingleton<IAmazonS3>(
    new AmazonS3Client(
        objectStorageCredentials,
        s3Config));

builder.Services.AddScoped<
    IImageStorageService,
    NeonImageStorageService>();

//
// SERVICIOS DE ATRIUM
//

builder.Services.AddScoped<
    ICourseAccessService,
    CourseAccessService>();

builder.Services.Configure<SmtpEmailOptions>(
    builder.Configuration.GetSection("Email"));

builder.Services.AddScoped<
    IContactEmailService,
    GmailContactEmailService>();

builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
});

//
// IDENTITY
//

builder.Services
    .AddIdentity<Usuario, IdentityRole>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;

        options.User.RequireUniqueEmail = true;

        options.Password.RequiredLength = 8;
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireNonAlphanumeric = false;

        options.Lockout.MaxFailedAccessAttempts = 5;

        options.Lockout.DefaultLockoutTimeSpan =
            TimeSpan.FromMinutes(10);
    })
    .AddEntityFrameworkStores<AtriumDbContext>()
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";

    options.AccessDeniedPath = "/Account/AccessDenied";

    options.Cookie.Name = "ATRIUM.Auth";

    options.Cookie.HttpOnly = true;

    options.Cookie.SameSite = SameSiteMode.Lax;

    options.ExpireTimeSpan =
        TimeSpan.FromHours(8);

    options.SlidingExpiration = true;
});

var app = builder.Build();

//
// PIPELINE HTTP
//

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");

    app.UseHsts();

    app.UseHttpsRedirection();
}

app.UseResponseCompression();

app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

//
// DATOS INICIALES
//

using (var scope = app.Services.CreateScope())
{
    try
    {
        await SeedData.InitializeAsync(
            scope.ServiceProvider);
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider
            .GetRequiredService<ILoggerFactory>()
            .CreateLogger("SeedData");

        logger.LogError(
            ex,
            "No se pudieron inicializar los datos base de ATRIUM Academy.");
    }
}

app.Run();

public partial class Program { }