namespace GestionAccesos.Services;

public interface IToastService
{
    event Action<ToastMessage>? OnShow;
    event Action? OnClear;

    void MostrarError(string mensaje);
    void MostrarOk(string mensaje);
    void MostrarInfo(string mensaje);
    void Clear();
}