using ATRIUM.Domain.Constants;
using ATRIUM.Tests.TestSupport;
using ATRIUM.Web.ViewModels;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ATRIUM.Tests;

[TestClass]
public class ViewModelValidationTests
{
    // QA: Qué probamos: un registro con todos los datos válidos.
    // QA: Esperado: no se genera ningún error de validación.
    [TestMethod]
    public void RegisterViewModel_DatosCorrectos_EsValido()
    {
        var model = CreateValidRegisterModel();
        Assert.AreEqual(0, TestHelpers.Validate(model).Count);
    }

    // QA: Qué probamos: los campos obligatorios vacíos en el registro.
    // QA: Esperado: Nombre, Apellido, Email, Dirección, Fecha, Password y ConfirmPassword son rechazados.
    [TestMethod]
    public void RegisterViewModel_CamposObligatoriosVacios_NoEsValido()
    {
        var model = new RegisterViewModel();
        var results = TestHelpers.Validate(model);

        Assert.IsTrue(TestHelpers.HasErrorFor(results, nameof(RegisterViewModel.Nombre)));
        Assert.IsTrue(TestHelpers.HasErrorFor(results, nameof(RegisterViewModel.Apellido)));
        Assert.IsTrue(TestHelpers.HasErrorFor(results, nameof(RegisterViewModel.Email)));
        Assert.IsTrue(TestHelpers.HasErrorFor(results, nameof(RegisterViewModel.Direccion)));
        Assert.IsTrue(TestHelpers.HasErrorFor(results, nameof(RegisterViewModel.FechaDeNacimiento)));
        Assert.IsTrue(TestHelpers.HasErrorFor(results, nameof(RegisterViewModel.Password)));
        Assert.IsTrue(TestHelpers.HasErrorFor(results, nameof(RegisterViewModel.ConfirmPassword)));
    }

    // QA: Qué probamos: una confirmación de contraseña distinta.
    // QA: Esperado: ConfirmPassword produce un error.
    [TestMethod]
    public void RegisterViewModel_ConfirmacionDistinta_NoEsValido()
    {
        var model = CreateValidRegisterModel();
        model.ConfirmPassword = "OtraClave1";

        Assert.IsTrue(TestHelpers.HasErrorFor(TestHelpers.Validate(model), nameof(RegisterViewModel.ConfirmPassword)));
    }

    // QA: Qué probamos: contraseña justo por debajo del límite mínimo de 8 caracteres.
    // QA: Esperado: Password es rechazado.
    [TestMethod]
    public void RegisterViewModel_PasswordDeSieteCaracteres_NoEsValido()
    {
        var model = CreateValidRegisterModel();
        model.Password = "Clave12";
        model.ConfirmPassword = "Clave12";

        Assert.IsTrue(TestHelpers.HasErrorFor(TestHelpers.Validate(model), nameof(RegisterViewModel.Password)));
    }

    // QA: Qué probamos: longitud máxima permitida para nombre, apellido y dirección.
    // QA: Esperado: los valores exactamente en el límite son válidos.
    [TestMethod]
    public void RegisterViewModel_LongitudesExactamenteEnLimite_EsValido()
    {
        var model = CreateValidRegisterModel();
        model.Nombre = new string('N', 100);
        model.Apellido = new string('A', 100);
        model.Direccion = new string('D', 220);

        Assert.AreEqual(0, TestHelpers.Validate(model).Count);
    }

    // QA: Qué probamos: valores un carácter por encima de los límites del registro.
    // QA: Esperado: se rechazan las propiedades excedidas.
    [TestMethod]
    public void RegisterViewModel_LongitudesSobreLimite_NoEsValido()
    {
        var model = CreateValidRegisterModel();
        model.Nombre = new string('N', 101);
        model.Apellido = new string('A', 101);
        model.Direccion = new string('D', 221);

        var results = TestHelpers.Validate(model);
        Assert.IsTrue(TestHelpers.HasErrorFor(results, nameof(RegisterViewModel.Nombre)));
        Assert.IsTrue(TestHelpers.HasErrorFor(results, nameof(RegisterViewModel.Apellido)));
        Assert.IsTrue(TestHelpers.HasErrorFor(results, nameof(RegisterViewModel.Direccion)));
    }

    // QA: Qué probamos: un correo con formato inválido durante login.
    // QA: Esperado: Email produce un error.
    [TestMethod]
    public void LoginViewModel_CorreoInvalido_NoEsValido()
    {
        var model = new LoginViewModel { Email = "correo-invalido", Password = "ClaveSegura1" };
        Assert.IsTrue(TestHelpers.HasErrorFor(TestHelpers.Validate(model), nameof(LoginViewModel.Email)));
    }

    // QA: Qué probamos: login con correo y contraseña vacíos.
    // QA: Esperado: ambos campos son rechazados.
    [TestMethod]
    public void LoginViewModel_CredencialesVacias_NoEsValido()
    {
        var results = TestHelpers.Validate(new LoginViewModel());
        Assert.IsTrue(TestHelpers.HasErrorFor(results, nameof(LoginViewModel.Email)));
        Assert.IsTrue(TestHelpers.HasErrorFor(results, nameof(LoginViewModel.Password)));
    }

    // QA: Qué probamos: curso gratis sin descuento.
    // QA: Esperado: el ViewModel es válido.
    [TestMethod]
    public void CursoViewModel_CursoGratis_EsValido()
    {
        var model = CreateValidCourseModel();
        model.Precio = 0;
        model.PrecioDescuento = 0;

        Assert.AreEqual(0, TestHelpers.Validate(model).Count);
    }

    // QA: Qué probamos: descuento igual al precio regular.
    // QA: Esperado: PrecioDescuento es rechazado porque no representa un descuento real.
    [TestMethod]
    public void CursoViewModel_DescuentoIgualAlPrecio_NoEsValido()
    {
        var model = CreateValidCourseModel();
        model.Precio = 100;
        model.PrecioDescuento = 100;

        Assert.IsTrue(TestHelpers.HasErrorFor(TestHelpers.Validate(model), nameof(CursoViewModel.PrecioDescuento)));
    }

    // QA: Qué probamos: descuento mayor que el precio regular.
    // QA: Esperado: PrecioDescuento es rechazado.
    [TestMethod]
    public void CursoViewModel_DescuentoMayorAlPrecio_NoEsValido()
    {
        var model = CreateValidCourseModel();
        model.Precio = 100;
        model.PrecioDescuento = 120;

        Assert.IsTrue(TestHelpers.HasErrorFor(TestHelpers.Validate(model), nameof(CursoViewModel.PrecioDescuento)));
    }

    // QA: Qué probamos: precio regular en el máximo aceptado.
    // QA: Esperado: 999999 es válido y 1000000 queda fuera del rango.
    [TestMethod]
    public void CursoViewModel_PrecioMaximo_RespetaLimite()
    {
        var valid = CreateValidCourseModel();
        valid.Precio = 999999m;
        Assert.IsFalse(TestHelpers.HasErrorFor(TestHelpers.Validate(valid), nameof(CursoViewModel.Precio)));

        var invalid = CreateValidCourseModel();
        invalid.Precio = 1000000m;
        Assert.IsTrue(TestHelpers.HasErrorFor(TestHelpers.Validate(invalid), nameof(CursoViewModel.Precio)));
    }

    // QA: Qué probamos: IdCategoria en cero.
    // QA: Esperado: se rechaza porque ninguna categoría válida tiene Id 0.
    [TestMethod]
    public void CursoViewModel_CategoriaCero_NoEsValido()
    {
        var model = CreateValidCourseModel();
        model.IdCategoria = 0;

        Assert.IsTrue(TestHelpers.HasErrorFor(TestHelpers.Validate(model), nameof(CursoViewModel.IdCategoria)));
    }

    // QA: Qué probamos: rol administrativo fuera del catálogo permitido.
    // QA: Esperado: Rol produce un error de validación.
    [TestMethod]
    public void UsuarioAdminViewModel_RolDesconocido_NoEsValido()
    {
        var model = CreateValidAdminUserModel();
        model.Rol = "SuperUsuario";

        Assert.IsTrue(TestHelpers.HasErrorFor(TestHelpers.Validate(model), nameof(UsuarioAdminViewModel.Rol)));
    }

    // QA: Qué probamos: rol con diferente capitalización.
    // QA: Esperado: se rechaza porque los roles se comparan de forma ordinal y exacta.
    [TestMethod]
    public void UsuarioAdminViewModel_RolConCapitalizacionDistinta_NoEsValido()
    {
        var model = CreateValidAdminUserModel();
        model.Rol = "administrador";

        Assert.IsTrue(TestHelpers.HasErrorFor(TestHelpers.Validate(model), nameof(UsuarioAdminViewModel.Rol)));
    }

    // QA: Qué probamos: IdCurso igual a cero en el checkout.
    // QA: Esperado: se rechaza antes de llegar a la lógica de compra.
    [TestMethod]
    public void PedidoViewModel_IdCursoCero_NoEsValido()
    {
        var model = CreateValidOrderModel();
        model.IdCurso = 0;

        Assert.IsTrue(TestHelpers.HasErrorFor(TestHelpers.Validate(model), nameof(PedidoViewModel.IdCurso)));
    }

    // QA: Qué probamos: datos de entrega obligatorios vacíos.
    // QA: Esperado: dirección, provincia, localidad y teléfono se rechazan.
    [TestMethod]
    public void PedidoViewModel_DatosEntregaVacios_NoEsValido()
    {
        var model = CreateValidOrderModel();
        model.Direccion = string.Empty;
        model.Provincia = string.Empty;
        model.Localidad = string.Empty;
        model.Telefono = string.Empty;

        var results = TestHelpers.Validate(model);
        Assert.IsTrue(TestHelpers.HasErrorFor(results, nameof(PedidoViewModel.Direccion)));
        Assert.IsTrue(TestHelpers.HasErrorFor(results, nameof(PedidoViewModel.Provincia)));
        Assert.IsTrue(TestHelpers.HasErrorFor(results, nameof(PedidoViewModel.Localidad)));
        Assert.IsTrue(TestHelpers.HasErrorFor(results, nameof(PedidoViewModel.Telefono)));
    }

    private static RegisterViewModel CreateValidRegisterModel() => new()
    {
        Nombre = "Usuario",
        Apellido = "ATRIUM",
        Email = "usuario@atrium.local",
        Direccion = "Cajamarca",
        Telefono = "999999999",
        FechaDeNacimiento = new DateTime(2000, 1, 1),
        Password = "ClaveSegura1",
        ConfirmPassword = "ClaveSegura1"
    };

    private static CursoViewModel CreateValidCourseModel() => new()
    {
        Nombre = "Diseño arquitectónico",
        Descripcion = "Curso válido",
        Precio = 100m,
        PrecioDescuento = 80m,
        DocenteAsignado = "Docente",
        IdCategoria = 1
    };

    private static UsuarioAdminViewModel CreateValidAdminUserModel() => new()
    {
        Id = "user-1",
        Nombre = "Ana",
        Apellido = "Arquitecta",
        Email = "ana@atrium.local",
        Direccion = "Cajamarca",
        Telefono = "999999999",
        FechaDeNacimiento = new DateTime(1998, 5, 1),
        Rol = AppRoles.Student
    };

    private static PedidoViewModel CreateValidOrderModel() => new()
    {
        IdCurso = 1,
        Direccion = "Jr. Arquitectura 123",
        CodigoPostal = "06001",
        Provincia = "Cajamarca",
        Localidad = "Cajamarca",
        Telefono = "999999999",
        FormaPago = PaymentMethods.Yape
    };
}
