namespace GestionAccesos;

public class ConfigEmail
{
    public string EmailFrom { get; set; } = string.Empty;
    public string SmtpServer { get; set; } = string.Empty;
    public int SmtpPort { get; set; }
    public string SmtpUsername { get; set; } = string.Empty;
    public string SmtpPassword { get; set; } = string.Empty;
    public string AdminEmail { get; set; } = string.Empty;
    public string PopServer { get; set; } = string.Empty;
    public int PopPort { get; set; }
    public string PopUsername { get; set; } = string.Empty;
    public string PopPassword { get; set; } = string.Empty;
}