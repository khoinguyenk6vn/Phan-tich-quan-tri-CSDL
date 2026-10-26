using FashionFlow.DAL;
using FashionFlow.DAL.Entities;

namespace FashionFlow.BLL;

public sealed class OrderService(OrderRepository repository)
{
    public Task<List<OrderListItem>> GetOrdersAsync(CancellationToken cancellationToken = default) => repository.GetOrdersAsync(cancellationToken);
    public Task<List<OrderDetailItem>> GetDetailsAsync(string orderId, CancellationToken cancellationToken = default) => repository.GetDetailsAsync(orderId, cancellationToken);
    public Task<DonHangSanXuat?> GetOrderAsync(string orderId, CancellationToken cancellationToken = default) => repository.GetOrderAsync(orderId, cancellationToken);
    public Task<List<LookupItem>> GetCustomersAsync(CancellationToken cancellationToken = default) => repository.GetCustomersAsync(cancellationToken);
    public Task<List<LookupItem>> GetProductsAsync(CancellationToken cancellationToken = default) => repository.GetProductsAsync(cancellationToken);

    public async Task SaveAsync(DonHangSanXuat order, IReadOnlyCollection<ChiTietDonHang> details, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(order.MaDon) || order.MaDon.Length != 8)
            throw new InvalidOperationException("Mã đơn phải gồm 8 ký tự.");
        if (string.IsNullOrWhiteSpace(order.MaKH)) throw new InvalidOperationException("Vui lòng chọn khách hàng.");
        if (order.NgayGiaoDuKien < order.NgayDat) throw new InvalidOperationException("Ngày giao dự kiến phải từ ngày đặt trở đi.");
        if (details.Count == 0) throw new InvalidOperationException("Đơn hàng cần có ít nhất một sản phẩm.");
        if (details.Any(x => string.IsNullOrWhiteSpace(x.MaSP) || x.SoLuong <= 0 || x.DonGia < 0))
            throw new InvalidOperationException("Kiểm tra sản phẩm, số lượng và đơn giá trong chi tiết đơn hàng.");
        if (details.Select(x => x.MaSP).Distinct(StringComparer.OrdinalIgnoreCase).Count() != details.Count)
            throw new InvalidOperationException("Mỗi sản phẩm chỉ được xuất hiện một lần trong một đơn hàng.");
        foreach (var detail in details) detail.MaDon = order.MaDon;
        await repository.SaveOrderAsync(order, details, cancellationToken);
    }

    public Task DeleteAsync(string orderId, CancellationToken cancellationToken = default) => repository.DeleteOrderAsync(orderId, cancellationToken);
}
