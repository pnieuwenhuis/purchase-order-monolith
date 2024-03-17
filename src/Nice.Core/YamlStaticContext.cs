using Nice.Core.Definitions;
using YamlDotNet.Serialization;

namespace Nice.Core;

[YamlStaticContext]
[YamlSerializable(typeof(ApiServiceResourceDefinition))]
[YamlSerializable(typeof(ApiServiceV1Beta1Definition))]
[YamlSerializable(typeof(ResourceDefinitionMetadata))]
public partial class YamlStaticContext : StaticContext
{
    // Used by YamlDotNet for AOT deserialization
}
