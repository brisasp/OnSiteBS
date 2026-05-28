using System.Text.Json.Serialization;

namespace GestionAccesos.DTO.ResponseModels;

public class SingleResult<T> : PrimitiveResult
{
    [JsonPropertyName("data")] public T? Data { get; set; }
}