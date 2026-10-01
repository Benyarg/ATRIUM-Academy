using ATRIUM.Domain.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ATRIUM.Infrastructure.Context;

public class AtriumDbContext : IdentityDbContext<Usuario, IdentityRole, string>
{
    public AtriumDbContext(DbContextOptions<AtriumDbContext> options)
        : base(options)
    {
    }

    public DbSet<Categoria> Categorias => Set<Categoria>();
    public DbSet<Curso> Cursos => Set<Curso>();
    public DbSet<ModuloCurso> ModulosCurso => Set<ModuloCurso>();
    public DbSet<ContenidoModulo> ContenidosModulo => Set<ContenidoModulo>();
    public DbSet<ProgresoEstudiante> ProgresosEstudiante => Set<ProgresoEstudiante>();
    public DbSet<Pedido> Pedidos => Set<Pedido>();
    public DbSet<PedidoDetalle> PedidoDetalles => Set<PedidoDetalle>();
    public DbSet<CarroCompras> CarritoCompras => Set<CarroCompras>();
    public DbSet<Carousel> Carouseles => Set<Carousel>();
    public DbSet<Certificado> Certificados => Set<Certificado>();
    public DbSet<ContenidoEducativo> ContenidosEducativos => Set<ContenidoEducativo>();
    public DbSet<ConsultaContacto> ConsultasContacto => Set<ConsultaContacto>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        ConfigurePrecision(builder);
        ConfigureIndexes(builder);
        ConfigureRelationships(builder);
        ConfigureIdentityDeleteBehavior(builder);
    }

    private static void ConfigurePrecision(ModelBuilder builder)
    {
        builder.Entity<Curso>().Property(course => course.Precio).HasPrecision(18, 2);
        builder.Entity<Curso>().Property(course => course.PrecioDescuento).HasPrecision(18, 2);
        builder.Entity<Pedido>().Property(order => order.TotalPedido).HasPrecision(18, 2);
        builder.Entity<PedidoDetalle>().Property(detail => detail.PrecioIndividual).HasPrecision(18, 2);
    }

    private static void ConfigureIndexes(ModelBuilder builder)
    {
        builder.Entity<Categoria>()
            .HasIndex(category => category.Nombre)
            .IsUnique();

        builder.Entity<CarroCompras>()
            .HasIndex(item => new { item.UsuarioId, item.IdCurso })
            .IsUnique();

        builder.Entity<ProgresoEstudiante>()
            .HasIndex(progress => new { progress.IdUsuario, progress.IdContenido })
            .IsUnique();

        builder.Entity<Certificado>()
            .HasIndex(certificate => certificate.CodigoUnico)
            .IsUnique();

        builder.Entity<Certificado>()
            .HasIndex(certificate => new { certificate.IdUsuario, certificate.IdCurso })
            .IsUnique();

        builder.Entity<ConsultaContacto>()
            .HasIndex(contact => new {contact.Estado,contact.FechaCreacion});
    }

    private static void ConfigureRelationships(ModelBuilder builder)
    {
        builder.Entity<Curso>()
            .HasOne(course => course.Categoria)
            .WithMany(category => category.Cursos)
            .HasForeignKey(course => course.IdCategoria)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<ModuloCurso>()
            .HasOne(module => module.Curso)
            .WithMany(course => course.Modulos)
            .HasForeignKey(module => module.IdCurso)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<ContenidoModulo>()
            .HasOne(content => content.ModuloCurso)
            .WithMany(module => module.Contenidos)
            .HasForeignKey(content => content.IdModulo)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<ProgresoEstudiante>()
            .HasOne(progress => progress.Usuario)
            .WithMany(user => user.Progresos)
            .HasForeignKey(progress => progress.IdUsuario)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<ProgresoEstudiante>()
            .HasOne(progress => progress.ContenidoModulo)
            .WithMany(content => content.Progresos)
            .HasForeignKey(progress => progress.IdContenido)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Pedido>()
            .HasOne(order => order.Usuario)
            .WithMany(user => user.Pedidos)
            .HasForeignKey(order => order.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<PedidoDetalle>()
            .HasOne(detail => detail.Pedido)
            .WithMany(order => order.Detalles)
            .HasForeignKey(detail => detail.IdPedido)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<PedidoDetalle>()
            .HasOne(detail => detail.Curso)
            .WithMany(course => course.PedidoDetalles)
            .HasForeignKey(detail => detail.IdCurso)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<CarroCompras>()
            .HasOne(item => item.Usuario)
            .WithMany(user => user.CarritoCompras)
            .HasForeignKey(item => item.UsuarioId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<CarroCompras>()
            .HasOne(item => item.Curso)
            .WithMany(course => course.CarritoCompras)
            .HasForeignKey(item => item.IdCurso)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<ContenidoEducativo>()
            .HasOne(resource => resource.Curso)
            .WithMany(course => course.ContenidosEducativos)
            .HasForeignKey(resource => resource.CursoId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Certificado>()
            .HasOne(certificate => certificate.Usuario)
            .WithMany(user => user.Certificados)
            .HasForeignKey(certificate => certificate.IdUsuario)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Certificado>()
            .HasOne(certificate => certificate.Curso)
            .WithMany(course => course.Certificados)
            .HasForeignKey(certificate => certificate.IdCurso)
            .OnDelete(DeleteBehavior.Restrict);
        builder.Entity<ConsultaContacto>()
    .HasOne(contact => contact.Usuario)
    .WithMany(user => user.ConsultasContacto)
    .HasForeignKey(contact => contact.UsuarioId)
    .OnDelete(DeleteBehavior.SetNull);
    }

    private static void ConfigureIdentityDeleteBehavior(ModelBuilder builder)
    {
        builder.Entity<IdentityRoleClaim<string>>()
            .HasOne<IdentityRole>()
            .WithMany()
            .HasForeignKey(claim => claim.RoleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<IdentityUserClaim<string>>()
            .HasOne<Usuario>()
            .WithMany()
            .HasForeignKey(claim => claim.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<IdentityUserLogin<string>>()
            .HasOne<Usuario>()
            .WithMany()
            .HasForeignKey(login => login.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<IdentityUserRole<string>>()
            .HasOne<IdentityRole>()
            .WithMany()
            .HasForeignKey(userRole => userRole.RoleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<IdentityUserRole<string>>()
            .HasOne<Usuario>()
            .WithMany()
            .HasForeignKey(userRole => userRole.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<IdentityUserToken<string>>()
            .HasOne<Usuario>()
            .WithMany()
            .HasForeignKey(token => token.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
