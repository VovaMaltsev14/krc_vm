using ShopDomain.Model;

namespace ShopInfrastructure.Services;

public interface IImportService<TEntity>
    where TEntity : Entity
{
    Task ImportFromStreamAsync(Stream stream, CancellationToken cancellationToken);
}