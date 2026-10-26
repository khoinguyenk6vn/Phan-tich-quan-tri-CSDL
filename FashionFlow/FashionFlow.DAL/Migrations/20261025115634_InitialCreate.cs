using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FashionFlow.DAL.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CongDoanSanXuat",
                columns: table => new
                {
                    MaCD = table.Column<string>(type: "char(5)", maxLength: 5, nullable: false),
                    TenCD = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ThuTu = table.Column<int>(type: "int", nullable: false),
                    DonGiaCong = table.Column<decimal>(type: "decimal(18,0)", precision: 18, scale: 0, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CongDoanSanXuat", x => x.MaCD);
                    table.CheckConstraint("CK_CongDoanSanXuat_ThuTu", "[ThuTu] > 0");
                });

            migrationBuilder.CreateTable(
                name: "KhachHangDatMay",
                columns: table => new
                {
                    MaKH = table.Column<string>(type: "char(6)", maxLength: 6, nullable: false),
                    TenKH = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SDT = table.Column<string>(type: "varchar(15)", maxLength: 15, nullable: false),
                    Email = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    DiaChi = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KhachHangDatMay", x => x.MaKH);
                });

            migrationBuilder.CreateTable(
                name: "NguyenPhuLieuVai",
                columns: table => new
                {
                    MaNPL = table.Column<string>(type: "char(6)", maxLength: 6, nullable: false),
                    TenNPL = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LoaiNPL = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DonViTinh = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    DonGia = table.Column<decimal>(type: "decimal(18,0)", precision: 18, scale: 0, nullable: false),
                    SoLuongTon = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NguyenPhuLieuVai", x => x.MaNPL);
                    table.CheckConstraint("CK_NguyenPhuLieuVai_DonGia", "[DonGia] >= 0");
                    table.CheckConstraint("CK_NguyenPhuLieuVai_SoLuongTon", "[SoLuongTon] >= 0");
                });

            migrationBuilder.CreateTable(
                name: "NhanCongMay",
                columns: table => new
                {
                    MaNC = table.Column<string>(type: "char(6)", maxLength: 6, nullable: false),
                    HoTen = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SDT = table.Column<string>(type: "varchar(15)", maxLength: 15, nullable: true),
                    TayNghe = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    NgayVaoLam = table.Column<DateOnly>(type: "date", nullable: false, defaultValueSql: "CONVERT(date, GETDATE())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NhanCongMay", x => x.MaNC);
                });

            migrationBuilder.CreateTable(
                name: "SanPhamMay",
                columns: table => new
                {
                    MaSP = table.Column<string>(type: "char(6)", maxLength: 6, nullable: false),
                    TenSP = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    MoTa = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    DonGiaBan = table.Column<decimal>(type: "decimal(18,0)", precision: 18, scale: 0, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SanPhamMay", x => x.MaSP);
                    table.CheckConstraint("CK_SanPhamMay_DonGiaBan", "[DonGiaBan] >= 0");
                });

            migrationBuilder.CreateTable(
                name: "DonHangSanXuat",
                columns: table => new
                {
                    MaDon = table.Column<string>(type: "char(8)", maxLength: 8, nullable: false),
                    MaKH = table.Column<string>(type: "char(6)", maxLength: 6, nullable: false),
                    NgayDat = table.Column<DateOnly>(type: "date", nullable: false, defaultValueSql: "CONVERT(date, GETDATE())"),
                    NgayGiaoDuKien = table.Column<DateOnly>(type: "date", nullable: false),
                    TrangThai = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Chờ xử lý"),
                    GhiChu = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DonHangSanXuat", x => x.MaDon);
                    table.CheckConstraint("CK_DonHangSanXuat_TrangThai", "[TrangThai] IN (N'Chờ xử lý', N'Đang sản xuất', N'Hoàn thành', N'Đã hủy')");
                    table.ForeignKey(
                        name: "FK_DonHangSanXuat_KhachHangDatMay_MaKH",
                        column: x => x.MaKH,
                        principalTable: "KhachHangDatMay",
                        principalColumn: "MaKH",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DinhMucNPL",
                columns: table => new
                {
                    MaSP = table.Column<string>(type: "char(6)", nullable: false),
                    MaNPL = table.Column<string>(type: "char(6)", nullable: false),
                    SoLuongDinhMuc = table.Column<decimal>(type: "decimal(10,3)", precision: 10, scale: 3, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DinhMucNPL", x => new { x.MaSP, x.MaNPL });
                    table.CheckConstraint("CK_DinhMucNPL_SoLuong", "[SoLuongDinhMuc] > 0");
                    table.ForeignKey(
                        name: "FK_DinhMucNPL_NguyenPhuLieuVai_MaNPL",
                        column: x => x.MaNPL,
                        principalTable: "NguyenPhuLieuVai",
                        principalColumn: "MaNPL",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DinhMucNPL_SanPhamMay_MaSP",
                        column: x => x.MaSP,
                        principalTable: "SanPhamMay",
                        principalColumn: "MaSP",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CapPhatNPL",
                columns: table => new
                {
                    MaCapPhat = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaDon = table.Column<string>(type: "char(8)", maxLength: 8, nullable: false),
                    MaNPL = table.Column<string>(type: "char(6)", maxLength: 6, nullable: false),
                    SoLuong = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    NgayCap = table.Column<DateOnly>(type: "date", nullable: false, defaultValueSql: "CONVERT(date, GETDATE())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CapPhatNPL", x => x.MaCapPhat);
                    table.CheckConstraint("CK_CapPhatNPL_SoLuong", "[SoLuong] > 0");
                    table.ForeignKey(
                        name: "FK_CapPhatNPL_DonHangSanXuat_MaDon",
                        column: x => x.MaDon,
                        principalTable: "DonHangSanXuat",
                        principalColumn: "MaDon",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CapPhatNPL_NguyenPhuLieuVai_MaNPL",
                        column: x => x.MaNPL,
                        principalTable: "NguyenPhuLieuVai",
                        principalColumn: "MaNPL",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ChiTietDonHang",
                columns: table => new
                {
                    MaDon = table.Column<string>(type: "char(8)", maxLength: 8, nullable: false),
                    MaSP = table.Column<string>(type: "char(6)", maxLength: 6, nullable: false),
                    SoLuong = table.Column<int>(type: "int", nullable: false),
                    DonGia = table.Column<decimal>(type: "decimal(18,0)", precision: 18, scale: 0, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChiTietDonHang", x => new { x.MaDon, x.MaSP });
                    table.CheckConstraint("CK_ChiTietDonHang_DonGia", "[DonGia] >= 0");
                    table.CheckConstraint("CK_ChiTietDonHang_SoLuong", "[SoLuong] > 0");
                    table.ForeignKey(
                        name: "FK_ChiTietDonHang_DonHangSanXuat_MaDon",
                        column: x => x.MaDon,
                        principalTable: "DonHangSanXuat",
                        principalColumn: "MaDon",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ChiTietDonHang_SanPhamMay_MaSP",
                        column: x => x.MaSP,
                        principalTable: "SanPhamMay",
                        principalColumn: "MaSP",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PhanCong",
                columns: table => new
                {
                    MaPC = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaDon = table.Column<string>(type: "char(8)", maxLength: 8, nullable: false),
                    MaSP = table.Column<string>(type: "char(6)", maxLength: 6, nullable: false),
                    MaCD = table.Column<string>(type: "char(5)", maxLength: 5, nullable: false),
                    MaNC = table.Column<string>(type: "char(6)", maxLength: 6, nullable: false),
                    NgayLam = table.Column<DateOnly>(type: "date", nullable: false),
                    SoLuongHoanThanh = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PhanCong", x => x.MaPC);
                    table.CheckConstraint("CK_PhanCong_SoLuong", "[SoLuongHoanThanh] > 0");
                    table.ForeignKey(
                        name: "FK_PhanCong_ChiTietDonHang_MaDon_MaSP",
                        columns: x => new { x.MaDon, x.MaSP },
                        principalTable: "ChiTietDonHang",
                        principalColumns: new[] { "MaDon", "MaSP" },
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PhanCong_CongDoanSanXuat_MaCD",
                        column: x => x.MaCD,
                        principalTable: "CongDoanSanXuat",
                        principalColumn: "MaCD",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PhanCong_NhanCongMay_MaNC",
                        column: x => x.MaNC,
                        principalTable: "NhanCongMay",
                        principalColumn: "MaNC",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CapPhatNPL_MaDon",
                table: "CapPhatNPL",
                column: "MaDon");

            migrationBuilder.CreateIndex(
                name: "IX_CapPhatNPL_MaNPL",
                table: "CapPhatNPL",
                column: "MaNPL");

            migrationBuilder.CreateIndex(
                name: "IX_ChiTietDonHang_MaSP",
                table: "ChiTietDonHang",
                column: "MaSP");

            migrationBuilder.CreateIndex(
                name: "IX_DinhMucNPL_MaNPL",
                table: "DinhMucNPL",
                column: "MaNPL");

            migrationBuilder.CreateIndex(
                name: "IX_DonHangSanXuat_MaKH",
                table: "DonHangSanXuat",
                column: "MaKH");

            migrationBuilder.CreateIndex(
                name: "IX_KhachHangDatMay_SDT",
                table: "KhachHangDatMay",
                column: "SDT",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PhanCong_MaCD",
                table: "PhanCong",
                column: "MaCD");

            migrationBuilder.CreateIndex(
                name: "IX_PhanCong_MaDon_MaSP",
                table: "PhanCong",
                columns: new[] { "MaDon", "MaSP" });

            migrationBuilder.CreateIndex(
                name: "IX_PhanCong_MaNC",
                table: "PhanCong",
                column: "MaNC");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CapPhatNPL");

            migrationBuilder.DropTable(
                name: "DinhMucNPL");

            migrationBuilder.DropTable(
                name: "PhanCong");

            migrationBuilder.DropTable(
                name: "NguyenPhuLieuVai");

            migrationBuilder.DropTable(
                name: "ChiTietDonHang");

            migrationBuilder.DropTable(
                name: "CongDoanSanXuat");

            migrationBuilder.DropTable(
                name: "NhanCongMay");

            migrationBuilder.DropTable(
                name: "DonHangSanXuat");

            migrationBuilder.DropTable(
                name: "SanPhamMay");

            migrationBuilder.DropTable(
                name: "KhachHangDatMay");
        }
    }
}
