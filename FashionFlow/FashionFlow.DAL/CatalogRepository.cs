using FashionFlow.DAL.Entities;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace FashionFlow.DAL;

public interface ICatalogRepository<TEntity> where TEntity : class
{
    Task<List<TEntity>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<List<ForeignKeyEditorField>> GetForeignKeyFieldsAsync(CancellationToken cancellationToken = default);
    Task SaveAsync(TEntity entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(TEntity entity, CancellationToken cancellationToken = default);
}

public sealed record ForeignKeyEditorOption(string Text, object?[] Values);
public sealed record ForeignKeyEditorField(string Label, string[] PropertyNames, List<ForeignKeyEditorOption> Options);

public sealed class EfCatalogRepository<TEntity>(FashionFlowContextFactory factory, string connectionString)
    : ICatalogRepository<TEntity> where TEntity : class
{
    public async Task<List<TEntity>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        await using var db = factory.Create(connectionString);
        return await db.Set<TEntity>().AsNoTracking().ToListAsync(cancellationToken);
    }

    public async Task<List<ForeignKeyEditorField>> GetForeignKeyFieldsAsync(CancellationToken cancellationToken = default)
    {
        await using var db = factory.Create(connectionString);
        var entityType = db.Model.FindEntityType(typeof(TEntity)) ?? throw new InvalidOperationException("Không tìm thấy entity trong DbContext.");
        var fields = new List<ForeignKeyEditorField>();
        foreach (var foreignKey in entityType.GetForeignKeys())
        {
            var principalType = foreignKey.PrincipalEntityType;
            var setMethod = typeof(DbContext).GetMethod(nameof(DbContext.Set), Type.EmptyTypes)!.MakeGenericMethod(principalType.ClrType);
            var principalRows = (IQueryable)setMethod.Invoke(db, null)!;
            var rows = await principalRows.Cast<object>().ToListAsync(cancellationToken);
            var propertyNames = foreignKey.Properties.Select(x => x.Name).ToArray();
            var principalKeyNames = foreignKey.PrincipalKey.Properties.Select(x => x.Name).ToArray();
            var displayName = new[] { "TenKH", "TenSP", "TenNPL", "TenCD", "HoTen", "TenDonVi" }
                .FirstOrDefault(name => principalType.FindProperty(name) is not null);
            var options = rows.Select(row =>
            {
                var values = principalKeyNames.Select(name => principalType.ClrType.GetProperty(name)!.GetValue(row)).ToArray();
                var codes = values.Select(value => Convert.ToString(value)?.Trim() ?? "").ToArray();
                var description = displayName is null ? "" : Convert.ToString(principalType.ClrType.GetProperty(displayName)!.GetValue(row))?.Trim() ?? "";
                var label = string.Join(" / ", codes);
                if (!string.IsNullOrWhiteSpace(description) && !string.Equals(description, label, StringComparison.OrdinalIgnoreCase))
                    label += " - " + description;
                return new ForeignKeyEditorOption(label, values);
            }).ToList();
            var label = string.Join(" / ", propertyNames.Select(name =>
                typeof(TEntity).GetProperty(name)?.GetCustomAttributes(typeof(System.ComponentModel.DisplayNameAttribute), true)
                    .Cast<System.ComponentModel.DisplayNameAttribute>().FirstOrDefault()?.DisplayName ?? name));
            fields.Add(new ForeignKeyEditorField(label, propertyNames, options));
        }
        return fields;
    }

    public async Task SaveAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        await using var db = factory.Create(connectionString);
        var entityType = db.Model.FindEntityType(typeof(TEntity)) ?? throw new InvalidOperationException("Không tìm thấy entity trong DbContext.");
        var key = entityType.FindPrimaryKey() ?? throw new InvalidOperationException("Entity chưa khai báo khóa chính.");
        var entry = db.Entry(entity);
        var keyValues = key.Properties.Select(p => entry.Property(p.Name).CurrentValue).ToArray();
        var existing = await db.Set<TEntity>().FindAsync(keyValues, cancellationToken);
        if (existing is null)
            db.Set<TEntity>().Add(entity);
        else
            db.Entry(existing).CurrentValues.SetValues(entity);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        await using var db = factory.Create(connectionString);
        var entityType = db.Model.FindEntityType(typeof(TEntity)) ?? throw new InvalidOperationException("Không tìm thấy entity trong DbContext.");
        var key = entityType.FindPrimaryKey() ?? throw new InvalidOperationException("Entity chưa khai báo khóa chính.");
        var keyValues = key.Properties.Select(p => db.Entry(entity).Property(p.Name).CurrentValue).ToArray();
        var existing = await db.Set<TEntity>().FindAsync(keyValues, cancellationToken);
        if (existing is null) return;
        db.Set<TEntity>().Remove(existing);
        await db.SaveChangesAsync(cancellationToken);
    }
}

public sealed record LookupItem(string Code, string Name)
{
    public override string ToString() => $"{Code} - {Name}";
}

public sealed record OrderListItem(string MaDon, string TenKH, DateOnly NgayDat, DateOnly NgayGiaoDuKien, string TrangThai, decimal TongTien);
public sealed record OrderDetailItem(string MaSP, string TenSP, int SoLuong, decimal DonGia);
