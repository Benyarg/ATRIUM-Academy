using ATRIUM.Domain.Validation;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ATRIUM.Tests;

[TestClass]
public class HttpUrlValidationTests
{
    // QA: Qué probamos: URLs HTTP y HTTPS absolutas.
    // QA: Esperado: ambas son aceptadas.
    [TestMethod]
    public void HttpUrlRules_HttpYHttps_SonSeguras()
    {
        Assert.IsTrue(HttpUrlRules.IsSafe("http://example.com/recurso"));
        Assert.IsTrue(HttpUrlRules.IsSafe("https://example.com/recurso?id=1"));
    }

    // QA: Qué probamos: espacios alrededor de una URL válida.
    // QA: Esperado: se normalizan con Trim y la URL sigue siendo válida.
    [TestMethod]
    public void HttpUrlRules_UrlConEspacios_SeAcepta()
    {
        Assert.IsTrue(HttpUrlRules.IsSafe("  https://example.com  "));
    }

    // QA: Qué probamos: esquema javascript usado habitualmente en intentos XSS.
    // QA: Esperado: se rechaza.
    [TestMethod]
    public void HttpUrlRules_Javascript_SeRechaza()
    {
        Assert.IsFalse(HttpUrlRules.IsSafe("javascript:alert(1)"));
    }

    // QA: Qué probamos: esquemas FTP y file.
    // QA: Esperado: se rechazan porque la plataforma solo admite HTTP/HTTPS.
    [TestMethod]
    public void HttpUrlRules_EsquemasNoPermitidos_SeRechazan()
    {
        Assert.IsFalse(HttpUrlRules.IsSafe("ftp://example.com/file.pdf"));
        Assert.IsFalse(HttpUrlRules.IsSafe("file:///C:/secreto.txt"));
    }

    // QA: Qué probamos: URL relativa.
    // QA: Esperado: se rechaza porque la regla exige URL absoluta.
    [TestMethod]
    public void HttpUrlRules_UrlRelativa_SeRechaza()
    {
        Assert.IsFalse(HttpUrlRules.IsSafe("/recursos/documento.pdf"));
    }

    // QA: Qué probamos: valores nulos, vacíos o solo espacios en la regla de seguridad.
    // QA: Esperado: la regla devuelve false.
    [TestMethod]
    public void HttpUrlRules_ValorVacio_SeRechaza()
    {
        Assert.IsFalse(HttpUrlRules.IsSafe(null));
        Assert.IsFalse(HttpUrlRules.IsSafe(string.Empty));
        Assert.IsFalse(HttpUrlRules.IsSafe("   "));
    }

    // QA: Qué probamos: comportamiento del atributo cuando la URL opcional no se informa.
    // QA: Esperado: null y blanco son válidos; [Required] debe usarse aparte cuando corresponda.
    [TestMethod]
    public void HttpUrlAttribute_ValorOpcionalVacio_EsValido()
    {
        var attribute = new HttpUrlAttribute();
        Assert.IsTrue(attribute.IsValid(null));
        Assert.IsTrue(attribute.IsValid(string.Empty));
        Assert.IsTrue(attribute.IsValid("   "));
    }
}
