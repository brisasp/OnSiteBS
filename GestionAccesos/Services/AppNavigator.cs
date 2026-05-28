using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Routing;
using System.Security.Claims;

namespace GestionAccesos.Services;

public class AppNavigator
{
    private readonly NavigationManager _navigationManager;
    private readonly ILogger<AppNavigator> _logger;
    private readonly AuthenticationStateProvider _authStateProvider;

    public AppNavigator(NavigationManager navigationManager, ILogger<AppNavigator> logger, AuthenticationStateProvider authStateProvider)
    {
        _navigationManager = navigationManager;
        _logger = logger;
        _authStateProvider = authStateProvider;
    }

    public void NavigateTo(string uri, bool forceLoad = false)
    {
        _logger.LogInformation("Navigating to {Uri}", uri);
        _navigationManager.NavigateTo(uri, forceLoad);
    }

    public void NavigateBack()
    {
        _navigationManager.NavigateTo(_navigationManager.Uri, forceLoad: true);
    }

    public string CurrentUri => _navigationManager.Uri;
    public string BaseUri => _navigationManager.BaseUri;

    public string CurrentRelativePath => _navigationManager.ToBaseRelativePath(_navigationManager.Uri);
    public string ToBaseRelativePath(string uri) => _navigationManager.ToBaseRelativePath(uri);

    public event EventHandler<LocationChangedEventArgs>? LocationChanged
    {
        add => _navigationManager.LocationChanged += value;
        remove => _navigationManager.LocationChanged -= value;
    }

    public async Task NavigateToHomeAsync()
    {
        var authState = await _authStateProvider.GetAuthenticationStateAsync();
        var user = authState.User;

        if (user?.Identity?.IsAuthenticated == true)
        {
            var rol = user.FindFirst(ClaimTypes.Role)?.Value;

            switch (rol)
            {
                case "RRHH":
                    NavigateTo("/DashboardTrabajadores");
                    break;
                case "VISITAS":
                    NavigateTo("/DashboardVisitas");
                    break;
                case "ADMIN":
                    NavigateTo("/GestionGeneralEtt");
                    break;
                default:
                    NavigateTo("/Login");
                    break;
            }
        }
        else
        {
            NavigateTo("/Login");
        }
    }

    public async Task ProtegerPaginaAsync(params string[] rolesPermitidos)
    {
        var authState = await _authStateProvider.GetAuthenticationStateAsync();
        var user = authState.User;

        if (user == null || !user.Identity?.IsAuthenticated == true)
        {
            _navigationManager.NavigateTo("/Login", true);
            return;
        }

        var rolUsuario = user.FindFirst(ClaimTypes.Role)?.Value;

        if (rolUsuario == null || !rolesPermitidos.Contains(rolUsuario))
        {
            _navigationManager.NavigateTo("/Login", true);
        }
    }
}