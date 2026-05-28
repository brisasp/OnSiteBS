namespace GestionAccesos.Services;

public class ToastService : IToastService
{
    public event Action<ToastMessage>? OnShow;
    public event Action? OnClear;

    public void MostrarError(string mensaje)
    {
        ShowToast(mensaje, "toast-error", "bi bi-exclamation-circle-fill");
    }

    public void MostrarOk(string mensaje)
    {
        ShowToast(mensaje, "toast-success", "bi bi-check-circle-fill");
    }

    public void MostrarInfo(string mensaje)
    {
        ShowToast(mensaje, "toast-info", "bi bi-info-circle-fill");
    }

    public void Clear()
    {
        OnClear?.Invoke();
    }

    private void ShowToast(string mensaje, string cssClass, string iconClass)
    {
        OnShow?.Invoke(new ToastMessage
        {
            Message = mensaje,
            CssClass = cssClass,
            IconClass = iconClass,
            Duration = 5000
        });
    }
}