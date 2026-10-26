using System.ComponentModel.DataAnnotations;
using FashionFlow.DAL;

namespace FashionFlow.BLL;

public sealed class CatalogService<TEntity>(ICatalogRepository<TEntity> repository) where TEntity : class
{
    public Task<List<TEntity>> GetAllAsync(CancellationToken cancellationToken = default) => repository.GetAllAsync(cancellationToken);
    public Task<List<ForeignKeyEditorField>> GetForeignKeyFieldsAsync(CancellationToken cancellationToken = default) => repository.GetForeignKeyFieldsAsync(cancellationToken);

    public async Task SaveAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        var errors = new List<ValidationResult>();
        if (!Validator.TryValidateObject(entity, new ValidationContext(entity), errors, validateAllProperties: true))
            throw new ValidationException(string.Join(Environment.NewLine, errors.Select(x => x.ErrorMessage)));
        await repository.SaveAsync(entity, cancellationToken);
    }

    public Task DeleteAsync(TEntity entity, CancellationToken cancellationToken = default) => repository.DeleteAsync(entity, cancellationToken);
}
