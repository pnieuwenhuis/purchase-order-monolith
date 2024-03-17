namespace Nice.Core.Definitions;

public interface IResourceDefinitionSpec;

public record ApiServiceResourceDefinition
{
    public string ApiVersion { get; set; } = string.Empty;
    public string Kind { get; set; } = string.Empty;
    public ResourceDefinitionMetadata Metadata { get; set; } = new ResourceDefinitionMetadata();
    public ApiServiceV1Beta1Definition Spec { get; set; } = new();
}

public record ResourceDefinitionMetadata
{
    public string Name { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Dictionary<string, string> Labels { get; set; } = new Dictionary<string, string>();
    public Dictionary<string, string> Annotations { get; set; } = new Dictionary<string, string>();
}

public record ApiServiceV1Beta1Definition
{
    public string BuildType { get; set; } = string.Empty;
    public string DeploymentType { get; set; } = string.Empty;
    public Dictionary<string, int> Ports { get; set; } = new Dictionary<string, int>();
}
