using ATRIUM.Domain.Constants;
using ATRIUM.Web.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Reflection;

namespace ATRIUM.Tests;

[TestClass]
public class SecurityAttributeTests
{
    // QA: Qué probamos: administración de categorías protegida a nivel de controlador.
    // QA: Esperado: solo rol Administrador está autorizado.
    [TestMethod]
    public void CategoriaController_RequiereRolAdministrador()
    {
        var attribute = typeof(CategoriaController).GetCustomAttribute<AuthorizeAttribute>();
        Assert.IsNotNull(attribute);
        Assert.AreEqual(AppRoles.Administrator, attribute.Roles);
    }

    // QA: Qué probamos: carrito protegido por rol.
    // QA: Esperado: solo Estudiante o Administrador pueden acceder.
    [TestMethod]
    public void CarroComprasController_RequiereEstudianteOAdministrador()
    {
        var attribute = typeof(CarroComprasController).GetCustomAttribute<AuthorizeAttribute>();
        Assert.IsNotNull(attribute);
        Assert.AreEqual(AppRoles.StudentOrAdministrator, attribute.Roles);
    }

    // QA: Qué probamos: detalle público de curso.
    // QA: Esperado: tiene AllowAnonymous para que un visitante pueda ver el catálogo.
    [TestMethod]
    public void CursoDetails_PermiteAnonimos()
    {
        var method = typeof(CursoController).GetMethod(nameof(CursoController.Details), [typeof(int?)])!;
        Assert.IsNotNull(method.GetCustomAttribute<AllowAnonymousAttribute>());
    }

    // QA: Qué probamos: validación pública de certificados.
    // QA: Esperado: permite acceso anónimo.
    [TestMethod]
    public void CertificadoValidar_PermiteAnonimos()
    {
        var method = typeof(CertificadoController).GetMethod(nameof(CertificadoController.Validar), [typeof(string)])!;
        Assert.IsNotNull(method.GetCustomAttribute<AllowAnonymousAttribute>());
    }

    // QA: Qué probamos: POST de cambio de estado de pedido.
    // QA: Esperado: exige rol Administrador, HttpPost y antiforgery.
    [TestMethod]
    public void PedidoCambiarEstado_EstaProtegido()
    {
        var method = typeof(PedidoController).GetMethod(nameof(PedidoController.CambiarEstado), [typeof(int), typeof(string)])!;
        var authorize = method.GetCustomAttribute<AuthorizeAttribute>();

        Assert.IsNotNull(authorize);
        Assert.AreEqual(AppRoles.Administrator, authorize.Roles);
        Assert.IsNotNull(method.GetCustomAttribute<HttpPostAttribute>());
        Assert.IsNotNull(method.GetCustomAttribute<ValidateAntiForgeryTokenAttribute>());
    }

    // QA: Qué probamos: POST de creación de categorías.
    // QA: Esperado: incluye antiforgery para mitigar CSRF.
    [TestMethod]
    public void CategoriaCreatePost_UsaValidateAntiForgeryToken()
    {
        var method = typeof(CategoriaController)
            .GetMethods()
            .Single(m => m.Name == nameof(CategoriaController.Create)
                         && m.GetCustomAttribute<HttpPostAttribute>() is not null);

        Assert.IsNotNull(method.GetCustomAttribute<ValidateAntiForgeryTokenAttribute>());
    }

    // QA: Qué probamos: Login y Register GET permanecen públicos.
    // QA: Esperado: ambos tienen AllowAnonymous.
    [TestMethod]
    public void Account_LoginYRegisterGet_PermitenAnonimos()
    {
        var login = typeof(AccountController)
            .GetMethods()
            .Single(m => m.Name == nameof(AccountController.Login)
                         && m.GetCustomAttribute<HttpPostAttribute>() is null);
        var register = typeof(AccountController)
            .GetMethods()
            .Single(m => m.Name == nameof(AccountController.Register)
                         && m.GetCustomAttribute<HttpPostAttribute>() is null);

        Assert.IsNotNull(login.GetCustomAttribute<AllowAnonymousAttribute>());
        Assert.IsNotNull(register.GetCustomAttribute<AllowAnonymousAttribute>());
    }

    // QA: Qué probamos: Logout no puede ejecutarse por GET ni sin token CSRF.
    // QA: Esperado: exige Authorize, HttpPost y ValidateAntiForgeryToken.
    [TestMethod]
    public void Account_Logout_EstaProtegidoContraCsrf()
    {
        var method = typeof(AccountController).GetMethod(nameof(AccountController.Logout))!;
        Assert.IsNotNull(method.GetCustomAttribute<AuthorizeAttribute>());
        Assert.IsNotNull(method.GetCustomAttribute<HttpPostAttribute>());
        Assert.IsNotNull(method.GetCustomAttribute<ValidateAntiForgeryTokenAttribute>());
    }
}
