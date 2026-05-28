namespace GestionAccesos.Helpers;

public static class ImagenHelper
{
    public static string ObtenerFotoBase64(byte[]? foto, string rutaPorDefecto = "/Images/usuario-generico.jpg")
    {
        if (foto == null || foto.Length == 0)
            return rutaPorDefecto;

        return $"data:image/png;base64,{Convert.ToBase64String(foto)}";
    }
}