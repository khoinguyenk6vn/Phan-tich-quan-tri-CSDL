using FashionFlow.DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace FashionFlow.DAL;

public sealed class OrderRepository(FashionFlowContextFactory factory, string connectionString)
{
    public async Task<List<OrderListItem>> GetOrdersAsync(CancellationToken cancellationToken = default)
    {
        await using var db = factory.Create(connectionString);
        var orders = await db.DonHangSanXuats.AsNoTracking()
            .Include(x => x.KhachHang)
            .Include(x => x.ChiTietDonHangs)
            .OrderByDescending(x => x.NgayDat)
            .ThenByDescending(x => x.MaDon)
            .ToListAsync(cancellationToken);

        return orders.Select(x => new OrderListItem(
            x.MaDon, x.KhachHang!.TenKH, x.NgayDat, x.NgayGiaoDuKien, x.TrangThai,
            x.ChiTietDonHangs.Sum(detail => detail.SoLuong * detail.DonGia)))
            .ToList();
    }

    public async Task<List<OrderDetailItem>> GetDetailsAsync(string orderId, CancellationToken cancellationToken = default)
    {
        await using var db = factory.Create(connectionString);
        return await db.ChiTietDonHangs.AsNoTracking().Where(x => x.MaDon == orderId)
            .Select(x => new OrderDetailItem(x.MaSP, x.SanPham!.TenSP, x.SoLuong, x.DonGia)).ToListAsync(cancellationToken);
    }

    public async Task<DonHangSanXuat?> GetOrderAsync(string orderId, CancellationToken cancellationToken = default)
    {
        await using var db = factory.Create(connectionString);
        return await db.DonHangSanXuats.AsNoTracking().Include(x => x.ChiTietDonHangs)
            .FirstOrDefaultAsync(x => x.MaDon == orderId, cancellationToken);
    }

    public async Task SaveOrderAsync(DonHangSanXuat order, IReadOnlyCollection<ChiTietDonHang> details, CancellationToken cancellationToken = default)
    {
        await using var db = factory.Create(connectionString);
        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
        var current = await db.DonHangSanXuats.Include(x => x.ChiTietDonHangs)
            .FirstOrDefaultAsync(x => x.MaDon == order.MaDon, cancellationToken);
        if (current is null)
        {
            order.ChiTietDonHangs = details.ToList();
            db.DonHangSanXuats.Add(order);
        }
        else
        {
            current.MaKH = order.MaKH;
            current.NgayDat = order.NgayDat;
            current.NgayGiaoDuKien = order.NgayGiaoDuKien;
            current.TrangThai = order.TrangThai;
            current.GhiChu = order.GhiChu;
            db.ChiTietDonHangs.RemoveRange(current.ChiTietDonHangs);
            current.ChiTietDonHangs = details.ToList();
        }
        await db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }

    public async Task DeleteOrderAsync(string orderId, CancellationToken cancellationToken = default)
    {
        await using var db = factory.Create(connectionString);
        var order = await db.DonHangSanXuats.FirstOrDefaultAsync(x => x.MaDon == orderId, cancellationToken);
        if (order is null) return;
        db.DonHangSanXuats.Remove(order);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<List<LookupItem>> GetCustomersAsync(CancellationToken cancellationToken = default)
    {
        await using var db = factory.Create(connectionString);
        return await db.KhachHangDatMays.AsNoTracking().OrderBy(x => x.TenKH)
            .Select(x => new LookupItem(x.MaKH, x.TenKH)).ToListAsync(cancellationToken);
    }

    public async Task<List<LookupItem>> GetProductsAsync(CancellationToken cancellationToken = default)
    {
        await using var db = factory.Create(connectionString);
        return await db.SanPhamMays.AsNoTracking().OrderBy(x => x.TenSP)
            .Select(x => new LookupItem(x.MaSP, x.TenSP)).ToListAsync(cancellationToken);
    }
}
