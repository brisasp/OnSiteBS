using System.Text.Json;

namespace GestionAccesos.Services;

public class Festivo
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateOnly Fecha { get; set; }
    public string Descripcion { get; set; } = string.Empty;
    public string Ambito { get; set; } = "Nacional";
}

public class FestivosService
{
    private readonly string _filePath;
    private readonly JsonSerializerOptions _jsonOptions = new() { WriteIndented = true };

    public FestivosService(IWebHostEnvironment env)
    {
        _filePath = Path.Combine(env.ContentRootPath, "Data", "festivos.json");
        Directory.CreateDirectory(Path.GetDirectoryName(_filePath)!);
        if (!File.Exists(_filePath))
            File.WriteAllText(_filePath, "[]");
    }

    public async Task<List<Festivo>> GetAllAsync()
    {
        var json = await File.ReadAllTextAsync(_filePath);
        return JsonSerializer.Deserialize<List<Festivo>>(json, _jsonOptions) ?? [];
    }

    public async Task AddAsync(Festivo festivo)
    {
        var festivos = await GetAllAsync();
        festivo.Id = Guid.NewGuid();
        festivos.Add(festivo);
        festivos = [.. festivos.OrderBy(f => f.Fecha)];
        await File.WriteAllTextAsync(_filePath, JsonSerializer.Serialize(festivos, _jsonOptions));
    }

    public async Task DeleteAsync(Guid id)
    {
        var festivos = await GetAllAsync();
        festivos.RemoveAll(f => f.Id == id);
        await File.WriteAllTextAsync(_filePath, JsonSerializer.Serialize(festivos, _jsonOptions));
    }

    public async Task<bool> EsFestivoAsync(DateOnly fecha)
    {
        var festivos = await GetAllAsync();
        return festivos.Any(f => f.Fecha == fecha);
    }
}
