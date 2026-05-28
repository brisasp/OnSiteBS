namespace GestionAccesos.DTO;

public class LoginRequestDTO
{
    public string NombreUsuario { get; set; } = string.Empty;
    public string Contraseña { get; set; } = string.Empty;
    public bool Recordarme { get; set; } = false;
}