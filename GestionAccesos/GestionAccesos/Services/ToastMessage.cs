namespace GestionAccesos.Services;

public class ToastMessage
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Message { get; set; } = string.Empty;
    public string CssClass { get; set; } = string.Empty;
    public string IconClass { get; set; } = string.Empty;
    public int Duration { get; set; } = 5000;
    public bool IsClosing { get; set; }
}