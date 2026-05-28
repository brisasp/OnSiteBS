using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using GestionAccesos.Entities;
using GestionAccesos.DTO;

namespace GestionAccesos.Controllers;

public class MvcAuthController : Controller
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;

    public MvcAuthController(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager)
    {
        _signInManager = signInManager;
        _userManager = userManager;
    }

    //[HttpGet("/Login")]
    //public IActionResult Login()
    //{
    //    return View(); // Renderiza /Views/MvcAuth/Login.cshtml
    //}

    [HttpPost("/auth/login")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginRequestDTO model)
    {
        if (string.IsNullOrWhiteSpace(model.NombreUsuario))
        {
            // Opcional: puedes pasar algún mensaje de error para mostrar en la vista
            return Redirect("/login?error=usuario");
        }

        var user = await _userManager.FindByNameAsync(model.NombreUsuario);
        if (user == null)
            return Redirect("/login?error=usuario");

        var result = await _signInManager.PasswordSignInAsync(user, model.Contraseña, model.Recordarme, false);
        if (!result.Succeeded)
            return Redirect("/login?error=credenciales");

        var roles = await _userManager.GetRolesAsync(user);
        var rol = roles.FirstOrDefault();
        Console.WriteLine($"Rol detectado: {rol}");
        return rol switch
        {
            "RRHH" => Redirect("/GestionGeneralEtt"),
            "Administrador" => Redirect("/GestionGeneralEtt"),
            "Visitas" => Redirect("/GestionGeneralEtt"),
            _ => Redirect("/login?error=rol")
        };
    }

    [HttpGet("/AccessDenied")]
    public IActionResult AccessDenied()
    {
        return Redirect("/login?error=acceso");
    }

    [HttpGet("/authentication/logout")]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return Redirect("/login?logout=1");
    }
}