using Mapster;

namespace FootballManager.Tests;

public class MappingTests
{
    // Every mapping is a Mapster config: compiling them all catches a renamed or retyped field before a tool call does.
    [Fact]
    public void Api_and_infrastructure_mappings_compile()
    {
        var config = new TypeAdapterConfig();
        new Infrastructure.Mappings.MappingConfig().Register(config);
        new Api.Mappings.MappingConfig().Register(config);

        config.Compile();
    }
}
