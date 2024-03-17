using System.Text.Json.Serialization;

namespace Nice.Core.Web;

public record SuccessDetails(object Data, IDictionary<string, string> Properties)
{
    public static bool Success => true;
    public static SuccessDetails From(object data) => From(data, new Dictionary<string, string>());
    public static SuccessDetails From(object data, IDictionary<string, string> properties) => new(data, properties);
}

public record ErrorDetails(string Code, string Message, IDictionary<string, string> Properties)
{
    public static bool Success => false;
    public static ErrorDetails From(string code, string message) => From(code, message, new Dictionary<string, string>());
    public static ErrorDetails From(string code, string message, IDictionary<string, string> properties) => new(code, message, properties);
}

[JsonSerializable(typeof(SuccessDetails))]
[JsonSerializable(typeof(ErrorDetails))]
public partial class ResponseModelsSerializerContext : JsonSerializerContext;
