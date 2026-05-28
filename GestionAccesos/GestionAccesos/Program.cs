using Blazored.LocalStorage;
using GestionAccesos;
using GestionAccesos.Components;
using GestionAccesos.Data.Auth;
using GestionAccesos.Data.GestionAccesos;
using GestionAccesos.Entities;
using GestionAccesos.Services;
using GestionAccesos.Services.ExcelExporter;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection"));
}, ServiceLifetime.Scoped);


builder.Services.AddDbContextFactory<AppDbContext>(
    (sp, options) =>
    {
        options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection"));
    },
    ServiceLifetime.Scoped);

builder.Services.AddDbContext<AuthDbContext>(options =>
{
    options.UseSqlite(builder.Configuration.GetConnectionString("AuthConnection"));
});

builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<AuthDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddAuthentication();
builder.Services.AddAuthorization();
builder.Services.AddHttpContextAccessor();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.Name = "auth";
    options.LoginPath = "/login";
    options.AccessDeniedPath = "/AccessDenied";
    //options.LogoutPath = "/logout";
    options.LogoutPath = "/authentication/logout";
    options.ExpireTimeSpan = TimeSpan.FromDays(7);
    options.SlidingExpiration = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    options.Cookie.SameSite = SameSiteMode.Lax;
    //options.Cookie.HttpOnly = true;
});

builder.Services.AddCascadingAuthenticationState();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddHubOptions(options =>
    {
        options.MaximumReceiveMessageSize = 10 * 1024 * 1024; // 10MB para permitir fotos
    });


builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

builder.Services.AddScoped<TranslationService>();
builder.Services.AddScoped<AppNavigator>();
builder.Services.AddBlazoredLocalStorage();
builder.Services.AddControllersWithViews();

builder.Services.AddScoped<ICryptoService, CryptoService>();
builder.Services.AddScoped<IGestionAccesosService, GestionAccesosService>();
builder.Services.AddScoped<IToastService, ToastService>();
builder.Services.AddScoped<EmailService>();
builder.Services.AddScoped<PdfExporter>();

builder.Services.AddScoped<VisitasExcelExporter>();
builder.Services.AddScoped<ExcelExportCoordinator>();

builder.Services.AddSingleton<FestivosService>();
builder.Services.AddSingleton<FichajeAutoCloseHistorial>();
builder.Services.AddHostedService<FichajeAutoCloseService>();
builder.Services.AddHostedService<VisitaAutoCloseService>();
builder.Services.Configure<ConfigEmail>(builder.Configuration.GetSection("EmailConfiguration"));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var crypto = scope.ServiceProvider.GetRequiredService<ICryptoService>();

    if (!context.PersonasAvisitars.Any())
    {
        var persona = new PersonasAvisitar
        {
            NombreCompleto = crypto.Encrypt("Juan Pérez"),
            Correo = crypto.Encrypt("juan.perez@email.com"),
            Departamento = crypto.Encrypt("IT"),
            FechaRegistro = DateTime.Now,
            Borrado = false
        };

        context.PersonasAvisitars.Add(persona);
        context.SaveChanges();
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
// app.UseHttpsRedirection(); // Deshabilitado para acceso desde red local (iPad)

app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapControllers();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

    // Roles
    string[] roles = { "Administrador", "RRHH", "Visitas" };

    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }
    }

    // Usuario admin
    var adminEmail = "admin@admin.com";
    var adminUser = await userManager.FindByEmailAsync(adminEmail);

    if (adminUser == null)
    {
        var user = new ApplicationUser
        {
            UserName = "admin",
            Email = adminEmail,
            EmailConfirmed = true
        };
        var result = await userManager.CreateAsync(user, "Admin123!");
        if (result.Succeeded)
            await userManager.AddToRoleAsync(user, "Administrador");
    }

    // Usuario RRHH
    var rrhhEmail = "rrhh@onsite.com";
    var rrhhUser = await userManager.FindByEmailAsync(rrhhEmail);
    if (rrhhUser == null)
    {
        var user = new ApplicationUser
        {
            UserName = "rrhh",
            Email = rrhhEmail,
            EmailConfirmed = true
        };
        var result = await userManager.CreateAsync(user, "Rrhh123!");
        if (result.Succeeded)
            await userManager.AddToRoleAsync(user, "RRHH");
    }

    // Usuario Recepción/Visitas
    var recepcionEmail = "recepcion@onsite.com";
    var recepcionUser = await userManager.FindByEmailAsync(recepcionEmail);
    if (recepcionUser == null)
    {
        var user = new ApplicationUser
        {
            UserName = "recepcion",
            Email = recepcionEmail,
            EmailConfirmed = true
        };
        var result = await userManager.CreateAsync(user, "Recepcion123!");
        if (result.Succeeded)
            await userManager.AddToRoleAsync(user, "Visitas");
    }
}


app.Run();