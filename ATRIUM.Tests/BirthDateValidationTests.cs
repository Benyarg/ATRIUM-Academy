using ATRIUM.Web.ViewModels.Validation;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ATRIUM.Tests;

[TestClass]
public class BirthDateValidationTests
{
    private readonly ValidBirthDateAttribute _attribute = new();

    // QA: Qué probamos: la fecha mínima permitida, 01/01/1900.
    // QA: Esperado: el límite inferior exacto es válido.
    [TestMethod]
    public void FechaNacimiento_FechaMinima_EsValida()
    {
        Assert.IsTrue(_attribute.IsValid(new DateTime(1900, 1, 1)));
    }

    // QA: Qué probamos: una fecha un día anterior al mínimo.
    // QA: Esperado: se rechaza.
    [TestMethod]
    public void FechaNacimiento_AntesDelMinimo_NoEsValida()
    {
        Assert.IsFalse(_attribute.IsValid(new DateTime(1899, 12, 31)));
    }

    // QA: Qué probamos: la fecha de hoy.
    // QA: Esperado: es válida como límite superior actual.
    [TestMethod]
    public void FechaNacimiento_Hoy_EsValida()
    {
        Assert.IsTrue(_attribute.IsValid(DateTime.Today));
    }

    // QA: Qué probamos: una fecha futura.
    // QA: Esperado: se rechaza.
    [TestMethod]
    public void FechaNacimiento_Manana_NoEsValida()
    {
        Assert.IsFalse(_attribute.IsValid(DateTime.Today.AddDays(1)));
    }

    // QA: Qué probamos: valor nulo y tipo incorrecto entregados directamente al atributo.
    // QA: Esperado: ambos casos son rechazados sin lanzar excepciones.
    [TestMethod]
    public void FechaNacimiento_NuloOTipoIncorrecto_NoEsValida()
    {
        Assert.IsFalse(_attribute.IsValid(null));
        Assert.IsFalse(_attribute.IsValid("2000-01-01"));
    }
}
