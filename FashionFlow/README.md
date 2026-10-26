# FashionFlow - Quản lý xưởng may

Ứng dụng desktop WinForms dùng EF Core Code First, SQL Server Express và kiến trúc 3 tầng.

## Cấu trúc

- `FashionFlow`: WinForms, đăng nhập, điều hướng, quản lý danh mục và đơn hàng.
- `FashionFlow.BLL`: kiểm tra nghiệp vụ, xác thực đăng nhập và lưu đơn hàng.
- `FashionFlow.DAL`: entity, `DbContext`, cấu hình EF Core và repository.
- `Database`: script T-SQL cho login, role, view, stored procedure, function và trigger.

Luồng phụ thuộc: WinForms -> BLL -> DAL. Ứng dụng không dùng ADO.NET để CRUD; các truy vấn, thao tác CRUD và transaction đều dùng EF Core.

## Chạy trên SQL Server Express

1. Cài SQL Server Express với instance `SQLEXPRESS`, bật chế độ xác thực SQL Server and Windows Authentication, sau đó khởi động dịch vụ `SQL Server (SQLEXPRESS)`.
2. Mở `Database/00_Security.sql` bằng SSMS và chạy bằng tài khoản SQL Server có quyền tạo database và login. Script tạo database `FashionFlowDb`, role `FashionFlowAdmin` và `FashionFlowUser`, cùng tài khoản demo:
   - Admin: `admin` / `123`
   - User: `user` / `123`
3. Mở `FashionFlow.slnx` bằng Visual Studio 2026, restore NuGet packages, build và chạy project `FashionFlow`.
4. Đăng nhập lần đầu bằng tài khoản Admin. EF Core Code First chạy migration `InitialCreate` để tạo 10 bảng, sau đó thêm dữ liệu mẫu. Tài khoản user chỉ có quyền đọc, thêm và sửa dữ liệu; quyền xóa thuộc Admin.
5. Sau lần chạy đầu, mở và chạy `Database/01_ViewsProceduresFunctionsTriggers.sql` trong SSMS bằng Admin. Script tạo hai view, hai stored procedure, một function và ba trigger.
6. Đăng xuất/chạy lại ứng dụng và đăng nhập `user` / `123` để xem giới hạn quyền.

Nếu SQL Server Express của bạn dùng instance hoặc tên database khác, đổi chuỗi kết nối trong `FashionFlow.BLL/LoginService.cs`. Không lưu mật khẩu trong mã nguồn; mật khẩu được nhập ở màn hình đăng nhập.

## Chức năng

- CRUD danh mục: khách hàng, sản phẩm, nguyên phụ liệu, công đoạn, nhân công, định mức, cấp phát và phân công.
- Quản lý đơn hàng dạng Main-Sub: lưu thông tin đơn và các sản phẩm trong cùng một EF Core transaction.
- CSDL giữ khóa ngoại, khóa chính ghép, unique/check constraints; trigger kiểm tra ngày giao, ngày làm và cập nhật tồn kho cấp phát.
- Mỗi form có kích thước khung 1024x576 và bố cục dùng layout/dock để tiện chỉnh sửa giao diện.

## Các đối tượng T-SQL

- `V_DanhSachChuaXuLy`, `V_ThongKeTongHop`
- `P_DanhSachGiaoDich`, `P_ThemSuaXoaKhachHang`
- `F_TinhTongGiaTri`
- `TR_DonHang_KiemTraNgayGiao`, `TR_CapPhatNPL_CapNhatTonKho`, `TR_PhanCong_KiemTraNgayLam`

Ghi chú: migrations được tự động áp dụng khi đăng nhập bằng tài khoản Admin. Để tạo migration khi sửa model, cài công cụ một lần bằng `dotnet tool install --global dotnet-ef --version 10.0.12`, sau đó chạy `dotnet ef migrations add TenMigration --project FashionFlow.DAL --startup-project FashionFlow`.
