// Temporary stub for AgenticFlow.Persistence.DependencyInjection.
// This will be removed once the Persistence module (Module 2) is implemented.

using Microsoft.Extensions.DependencyInjection;

namespace AgenticFlow.Persistence;

public static class PersistenceServiceCollectionExtensions
{
    public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        return services;
    }
}
