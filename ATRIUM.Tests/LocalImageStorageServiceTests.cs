using ATRIUM.Tests.TestSupport;
using ATRIUM.Web.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ATRIUM.Tests;

[TestClass]
public class LocalImageStorageServiceTests
{
    private string _root = null!;
    private LocalImageStorageService _service = null!;

    [TestInitialize]
    public void Setup()
    {
        _root = Path.Combine(
            Path.GetTempPath(),
            $"atrium-images-{Guid.NewGuid():N}");

        Directory.CreateDirectory(_root);

        _service = new LocalImageStorageService(
            new TestWebHostEnvironment
            {
                WebRootPath = _root
            });
    }

    [TestCleanup]
    public void Cleanup()
    {
        if (Directory.Exists(_root))
        {
            Directory.Delete(
                _root,
                recursive: true);
        }
    }

    // QA: Qué probamos: imagen requerida ausente.
    // QA: Esperado: se devuelve mensaje de validación.
    [TestMethod]
    public async Task ValidateAsync_ArchivoRequeridoNulo_DevuelveError()
    {
        var error = await _service.ValidateAsync(
            null,
            required: true);

        Assert.IsFalse(
            string.IsNullOrWhiteSpace(error));
    }

    // QA: Qué probamos: imagen opcional ausente.
    // QA: Esperado: no genera error.
    [TestMethod]
    public async Task ValidateAsync_ArchivoOpcionalNulo_NoDevuelveError()
    {
        var error = await _service.ValidateAsync(
            null,
            required: false);

        Assert.IsNull(error);
    }

    // QA: Qué probamos: archivo vacío cuando la imagen es requerida.
    // QA: Esperado: se rechaza.
    [TestMethod]
    public async Task ValidateAsync_ArchivoVacioRequerido_DevuelveError()
    {
        var file = TestHelpers.CreateFormFile(
            [],
            "foto.jpg",
            "image/jpeg");

        var error = await _service.ValidateAsync(
            file,
            required: true);

        Assert.IsFalse(
            string.IsNullOrWhiteSpace(error));
    }

    // QA: Qué probamos: archivo que supera 5 MB por un byte.
    // QA: Esperado: se rechaza por tamaño.
    [TestMethod]
    public async Task ValidateAsync_ArchivoSobreCincoMegabytes_DevuelveError()
    {
        var file = TestHelpers.CreateFormFile(
            new byte[5_000_001],
            "foto.jpg",
            "image/jpeg");

        var error = await _service.ValidateAsync(
            file,
            required: true);

        StringAssert.Contains(
            error!,
            "5 MB");
    }

    // QA: Qué probamos: extensión no permitida.
    // QA: Esperado: se rechaza aunque el MIME parezca una imagen.
    [TestMethod]
    public async Task ValidateAsync_ExtensionNoPermitida_DevuelveError()
    {
        var file = TestHelpers.CreateFormFile(
            TestHelpers.MinimalJpeg(),
            "foto.gif",
            "image/jpeg");

        var error = await _service.ValidateAsync(
            file,
            required: true);

        StringAssert.Contains(
            error!,
            "Formato no permitido");
    }

    // QA: Qué probamos: extensión JPG con MIME PNG.
    // QA: Esperado: se rechaza por inconsistencia entre extensión y Content-Type.
    [TestMethod]
    public async Task ValidateAsync_MimeNoCoincideConExtension_DevuelveError()
    {
        var file = TestHelpers.CreateFormFile(
            TestHelpers.MinimalJpeg(),
            "foto.jpg",
            "image/png");

        var error = await _service.ValidateAsync(
            file,
            required: true);

        StringAssert.Contains(
            error!,
            "no coincide");
    }

    // QA: Qué probamos: archivo con extensión/MIME válidos pero firma binaria falsa.
    // QA: Esperado: se rechaza para evitar archivos disfrazados de imagen.
    [TestMethod]
    public async Task ValidateAsync_FirmaBinariaInvalida_DevuelveError()
    {
        var file = TestHelpers.CreateFormFile(
            [1, 2, 3, 4, 5, 6, 7, 8],
            "foto.png",
            "image/png");

        var error = await _service.ValidateAsync(
            file,
            required: true);

        StringAssert.Contains(
            error!,
            "imagen");
    }

    // QA: Qué probamos: firmas mínimas válidas para JPG, PNG y WEBP.
    // QA: Esperado: las tres pasan la validación.
    [TestMethod]
    public async Task ValidateAsync_FormatosPermitidosConFirmaValida_NoDevuelvenError()
    {
        var jpegFile = TestHelpers.CreateFormFile(
            TestHelpers.MinimalJpeg(),
            "a.JPG",
            "image/jpeg");

        var pngFile = TestHelpers.CreateFormFile(
            TestHelpers.MinimalPng(),
            "b.png",
            "image/png");

        var webpFile = TestHelpers.CreateFormFile(
            TestHelpers.MinimalWebp(),
            "c.webp",
            "image/webp");

        Assert.IsNull(
            await _service.ValidateAsync(
                jpegFile,
                required: true));

        Assert.IsNull(
            await _service.ValidateAsync(
                pngFile,
                required: true));

        Assert.IsNull(
            await _service.ValidateAsync(
                webpFile,
                required: true));
    }

    // QA: Qué probamos: guardado físico de una imagen de curso.
    // QA: Esperado: devuelve ruta /img/cursos/... y el archivo existe en wwwroot.
    [TestMethod]
    public async Task SaveAsync_ImagenCurso_GuardaArchivoEnRutaEsperada()
    {
        var file = TestHelpers.CreateFormFile(
            TestHelpers.MinimalJpeg(),
            "foto.jpg",
            "image/jpeg");

        var relativePath = await _service.SaveAsync(
            file,
            ImageStorageArea.Courses);

        var physicalPath = Path.Combine(
            _root,
            relativePath
                .TrimStart('/')
                .Replace(
                    '/',
                    Path.DirectorySeparatorChar));

        Assert.IsTrue(
            relativePath.StartsWith(
                "/img/cursos/",
                StringComparison.Ordinal));

        Assert.IsTrue(
            File.Exists(physicalPath));
    }

    // QA: Qué probamos: dos guardados con el mismo nombre original.
    // QA: Esperado: se generan nombres únicos y no se sobrescribe el primer archivo.
    [TestMethod]
    public async Task SaveAsync_MismoNombreOriginal_GeneraNombresUnicos()
    {
        var firstFile = TestHelpers.CreateFormFile(
            TestHelpers.MinimalPng(),
            "imagen.png",
            "image/png");

        var secondFile = TestHelpers.CreateFormFile(
            TestHelpers.MinimalPng(),
            "imagen.png",
            "image/png");

        var first = await _service.SaveAsync(
            firstFile,
            ImageStorageArea.Carousel);

        var second = await _service.SaveAsync(
            secondFile,
            ImageStorageArea.Carousel);

        Assert.AreNotEqual(
            first,
            second);
    }

    // QA: Qué probamos: eliminación de un archivo dentro del área correcta.
    // QA: Esperado: el archivo es eliminado.
    [TestMethod]
    public async Task DeleteAsync_RutaValida_EliminaArchivo()
    {
        var file = TestHelpers.CreateFormFile(
            TestHelpers.MinimalJpeg(),
            "foto.jpg",
            "image/jpeg");

        var path = await _service.SaveAsync(
            file,
            ImageStorageArea.Courses);

        var physicalPath = Path.Combine(
            _root,
            path
                .TrimStart('/')
                .Replace(
                    '/',
                    Path.DirectorySeparatorChar));

        await _service.DeleteAsync(
            path,
            ImageStorageArea.Courses);

        Assert.IsFalse(
            File.Exists(physicalPath));
    }

    // QA: Qué probamos: intento de borrar una ruta ajena al área autorizada.
    // QA: Esperado: se ignora la solicitud y el archivo externo permanece intacto.
    [TestMethod]
    public async Task DeleteAsync_RutaFueraDelArea_NoEliminaArchivo()
    {
        var outsideFile = Path.Combine(
            _root,
            "no-borrar.txt");

        File.WriteAllText(
            outsideFile,
            "seguro");

        await _service.DeleteAsync(
            "/no-borrar.txt",
            ImageStorageArea.Courses);

        Assert.IsTrue(
            File.Exists(outsideFile));
    }

    // QA: Qué probamos: valor de enum no definido para área de almacenamiento.
    // QA: Esperado: SaveAsync lanza ArgumentOutOfRangeException.
    [TestMethod]
    public async Task SaveAsync_AreaDesconocida_LanzaArgumentOutOfRangeException()
    {
        var file = TestHelpers.CreateFormFile(
            TestHelpers.MinimalJpeg(),
            "foto.jpg",
            "image/jpeg");

        await Assert.ThrowsExceptionAsync<ArgumentOutOfRangeException>(
            () => _service.SaveAsync(
                file,
                (ImageStorageArea)999));
    }
}