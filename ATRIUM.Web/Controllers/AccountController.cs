using ATRIUM.Domain.Constants;
using ATRIUM.Domain.Models;
using ATRIUM.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ATRIUM.Web.Controllers;

public class AccountController : Controller
{
    private readonly SignInManager<Usuario> _signInManager;
    private readonly UserManager<Usuario> _userManager;

    public AccountController(
        SignInManager<Usuario> signInManager,
        UserManager<Usuario> userManager)
    {
        _signInManager = signInManager;
        _userManager = userManager;
    }

    [HttpGet, AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectAuthenticatedUser();

        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost, AllowAnonymous, ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;

        if (!ModelState.IsValid)
            return View(model);

        var email = model.Email.Trim();
        var user = await _userManager.FindByEmailAsync(email);
        if (user is null)
        {
            AddInvalidCredentialsError();
            return View(model);
        }

        var result = await _signInManager.PasswordSignInAsync(
            user,
            model.Password,
            model.RememberMe,
            lockoutOnFailure: true);

        if (!result.Succeeded)
        {
            ModelState.AddModelError(
                string.Empty,
                result.IsLockedOut
                    ? "Cuenta bloqueada temporalmente por varios intentos fallidos."
                    : "Correo o contraseña incorrectos.");

            return View(model);
        }

        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            return LocalRedirect(returnUrl);

        return await _userManager.IsInRoleAsync(user, AppRoles.Administrator)
            ? RedirectToAction("Index", "Admin")!
            : RedirectToAction("MisCursos", "Curso")!;
    }

    [HttpGet, AllowAnonymous]
    public IActionResult Register()
    {
        return User.Identity?.IsAuthenticated == true
            ? RedirectAuthenticatedUser()
            : View();
    }

    [HttpPost, AllowAnonymous, ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var email = model.Email.Trim();
        if (await _userManager.FindByEmailAsync(email) is not null)
        {
            ModelState.AddModelError(nameof(model.Email), "Ya existe una cuenta con este correo.");
            return View(model);
        }

        var user = new Usuario
        {
            UserName = email,
            Email = email,
            Nombre = model.Nombre.Trim(),
            Apellido = model.Apellido.Trim(),
            Direccion = model.Direccion.Trim(),
            Telefono = model.Telefono.Trim(),
            FechaDeNacimiento = model.FechaDeNacimiento.Date
        };

        var createResult = await _userManager.CreateAsync(user, model.Password);
        if (!createResult.Succeeded)
        {
            AddIdentityErrors(createResult);
            return View(model);
        }

        var roleResult = await _userManager.AddToRoleAsync(user, AppRoles.Student);
        if (!roleResult.Succeeded)
        {
            var cleanupResult = await _userManager.DeleteAsync(user);
            AddIdentityErrors(roleResult);

            if (!cleanupResult.Succeeded)
                AddIdentityErrors(cleanupResult);

            return View(model);
        }

        await _signInManager.SignInAsync(user, isPersistent: false);
        TempData["SuccessMessage"] = "Tu cuenta ATRIUM fue creada correctamente.";

        return RedirectToAction("MisCursos", "Curso");
    }

    [Authorize, HttpGet]
    public async Task<IActionResult> Perfil()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null)
            return Challenge();

        return View(ToPerfilViewModel(user));
    }

    [Authorize, HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Perfil(PerfilViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var user = await _userManager.GetUserAsync(User);
        if (user is null)
            return Challenge();

        var email = model.Email.Trim();
        var emailOwner = await _userManager.FindByEmailAsync(email);
        if (emailOwner is not null && emailOwner.Id != user.Id)
        {
            ModelState.AddModelError(nameof(model.Email), "Ese correo ya está registrado.");
            return View(model);
        }

        user.Nombre = model.Nombre.Trim();
        user.Apellido = model.Apellido.Trim();
        user.Direccion = model.Direccion.Trim();
        user.Telefono = model.Telefono.Trim();
        user.FechaDeNacimiento = model.FechaDeNacimiento.Date;
        user.Email = email;
        user.UserName = email;

        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            AddIdentityErrors(updateResult);
            return View(model);
        }

        await _signInManager.RefreshSignInAsync(user);
        TempData["SuccessMessage"] = "Perfil actualizado.";

        return RedirectToAction(nameof(Perfil));
    }

    [Authorize, HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }

    [AllowAnonymous]
    public IActionResult AccessDenied() => View();

    private IActionResult RedirectAuthenticatedUser() =>
        User.IsInRole(AppRoles.Administrator)
            ? RedirectToAction("Index", "Admin")!
            : RedirectToAction("MisCursos", "Curso")!;

    private static PerfilViewModel ToPerfilViewModel(Usuario user) => new()
    {
        Nombre = user.Nombre,
        Apellido = user.Apellido,
        Email = user.Email ?? string.Empty,
        Direccion = user.Direccion,
        Telefono = user.Telefono,
        FechaDeNacimiento = user.FechaDeNacimiento
    };

    private void AddInvalidCredentialsError() =>
        ModelState.AddModelError(string.Empty, "Correo o contraseña incorrectos.");

    private void AddIdentityErrors(IdentityResult result)
    {
        foreach (var error in result.Errors)
            ModelState.AddModelError(string.Empty, error.Description);
    }
}
