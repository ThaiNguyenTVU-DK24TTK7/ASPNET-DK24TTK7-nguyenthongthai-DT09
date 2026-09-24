using System;
using Ecommerce.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace Ecommerce.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20251103140845_AddPhieuNhapTables")]
    partial class AddPhieuNhapTables
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "9.0.6")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("Ecommerce.Models.ChiTietDonHang", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int?>("DonHangId")
                        .HasColumnType("int");

                    b.Property<decimal>("GiaMua")
                        .HasColumnType("decimal(18, 2)");

                    b.Property<int>("KichThuocSanPhamId")
                        .HasColumnType("int");

                    b.Property<DateTime>("NgayTao")
                        .HasColumnType("datetime2");

                    b.Property<int?>("SanPhamId")
                        .HasColumnType("int");

                    b.Property<int>("SoLuong")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("DonHangId");

                    b.HasIndex("KichThuocSanPhamId");

                    b.HasIndex("SanPhamId");

                    b.ToTable("ChiTietDonHangs");
                });

            modelBuilder.Entity("Ecommerce.Models.ChiTietPhieuNhap", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("GhiChu")
                        .HasMaxLength(200)
                        .HasColumnType("nvarchar(200)");

                    b.Property<decimal>("GiaNhap")
                        .HasColumnType("decimal(18,2)");

                    b.Property<string>("KichThuoc")
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<string>("MauSac")
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<int>("PhieuNhapId")
                        .HasColumnType("int");

                    b.Property<int>("SanPhamId")
                        .HasColumnType("int");

                    b.Property<int>("SoLuongNhap")
                        .HasColumnType("int");

                    b.Property<decimal>("ThanhTien")
                        .HasColumnType("decimal(18,2)");

                    b.HasKey("Id");

                    b.HasIndex("PhieuNhapId");

                    b.HasIndex("SanPhamId");

                    b.ToTable("ChiTietPhieuNhaps");
                });

            modelBuilder.Entity("Ecommerce.Models.DanhGia", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("BinhLuan")
                        .HasMaxLength(1000)
                        .HasColumnType("nvarchar(1000)");

                    b.Property<int?>("DonHangId")
                        .HasColumnType("int");

                    b.Property<DateTime>("NgayTao")
                        .HasColumnType("datetime2");

                    b.Property<int?>("NguoiDungId")
                        .HasColumnType("int");

                    b.Property<int?>("SanPhamId")
                        .HasColumnType("int");

                    b.Property<int?>("SoSao")
                        .HasColumnType("int");

                    b.Property<string>("TrangThai")
                        .IsRequired()
                        .HasMaxLength(20)
                        .HasColumnType("nvarchar(20)");

                    b.HasKey("Id");

                    b.HasIndex("DonHangId");

                    b.HasIndex("NguoiDungId");

                    b.HasIndex("SanPhamId");

                    b.ToTable("DanhGias");
                });

            modelBuilder.Entity("Ecommerce.Models.DanhMuc", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("DuongDanAnh")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("MoTa")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("NgayTao")
                        .HasColumnType("datetime2");

                    b.Property<string>("Ten")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.HasKey("Id");

                    b.ToTable("DanhMucs");
                });

            modelBuilder.Entity("Ecommerce.Models.DonHang", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("DiaChiNhan")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("nvarchar(200)");

                    b.Property<string>("GhiChu")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("NgayTao")
                        .HasColumnType("datetime2");

                    b.Property<int?>("NguoiDungId")
                        .HasColumnType("int");

                    b.Property<string>("SoDienThoaiNhan")
                        .IsRequired()
                        .HasMaxLength(20)
                        .HasColumnType("nvarchar(20)");

                    b.Property<string>("TenNguoiNhan")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<decimal>("TongGiaTri")
                        .HasColumnType("decimal(18, 2)");

                    b.Property<string>("TrangThai")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.HasKey("Id");

                    b.HasIndex("NguoiDungId");

                    b.ToTable("DonHangs");
                });

            modelBuilder.Entity("Ecommerce.Models.GioHang", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("KichThuocSanPhamId")
                        .HasColumnType("int");

                    b.Property<DateTime>("NgayTao")
                        .HasColumnType("datetime2");

                    b.Property<int>("NguoiDungId")
                        .HasColumnType("int");

                    b.Property<int>("SoLuong")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("KichThuocSanPhamId");

                    b.HasIndex("NguoiDungId");

                    b.ToTable("GioHangs");
                });

            modelBuilder.Entity("Ecommerce.Models.KichThuocSanPham", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("KichThuoc")
                        .IsRequired()
                        .HasMaxLength(10)
                        .HasColumnType("nvarchar(10)");

                    b.Property<string>("MauSac")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<int>("SanPhamId")
                        .HasColumnType("int");

                    b.Property<int>("TonKho")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("SanPhamId");

                    b.ToTable("KichThuocSanPhams");
                });

            modelBuilder.Entity("Ecommerce.Models.LienHe", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("ChuDe")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("nvarchar(200)");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("HoTen")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<DateTime>("NgayGui")
                        .HasColumnType("datetime2");

                    b.Property<int?>("NguoiDungId")
                        .HasColumnType("int");

                    b.Property<string>("NoiDung")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("PhanHoiAdmin")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("TrangThai")
                        .IsRequired()
                        .HasMaxLength(20)
                        .HasColumnType("nvarchar(20)");

                    b.HasKey("Id");

                    b.HasIndex("NguoiDungId");

                    b.ToTable("LienHes");
                });

            modelBuilder.Entity("Ecommerce.Models.NguoiDung", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<string>("HoTen")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<string>("MatKhau")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("nvarchar(255)");

                    b.Property<DateTime>("NgayTao")
                        .HasColumnType("datetime2");

                    b.Property<string>("SoDienThoai")
                        .IsRequired()
                        .HasMaxLength(20)
                        .HasColumnType("nvarchar(20)");

                    b.Property<string>("TenDangNhap")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<string>("VaiTro")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.HasKey("Id");

                    b.ToTable("NguoiDungs");
                });

            modelBuilder.Entity("Ecommerce.Models.PhieuNhap", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("GhiChu")
                        .HasMaxLength(500)
                        .HasColumnType("nvarchar(500)");

                    b.Property<string>("MaPhieuNhap")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<DateTime>("NgayNhap")
                        .HasColumnType("datetime2");

                    b.Property<DateTime>("NgayTao")
                        .HasColumnType("datetime2");

                    b.Property<int>("NguoiTaoId")
                        .HasColumnType("int");

                    b.Property<string>("NhaCungCap")
                        .HasMaxLength(200)
                        .HasColumnType("nvarchar(200)");

                    b.Property<decimal>("TongGiaTri")
                        .HasColumnType("decimal(18,2)");

                    b.Property<string>("TrangThai")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.HasKey("Id");

                    b.HasIndex("NguoiTaoId");

                    b.ToTable("PhieuNhaps");
                });

            modelBuilder.Entity("Ecommerce.Models.SanPham", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("ChatLieu")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<int>("DanhMucId")
                        .HasColumnType("int");

                    b.Property<string>("DuongDanAnh")
                        .HasMaxLength(500)
                        .HasColumnType("nvarchar(500)");

                    b.Property<decimal>("Gia")
                        .HasColumnType("decimal(18, 2)");

                    b.Property<decimal?>("GiaGiam")
                        .HasColumnType("decimal(18, 2)");

                    b.Property<string>("MoTa")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("NgayTao")
                        .HasColumnType("datetime2");

                    b.Property<int>("SoLuongBan")
                        .HasColumnType("int");

                    b.Property<string>("Ten")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.HasKey("Id");

                    b.HasIndex("DanhMucId");

                    b.ToTable("SanPhams");
                });

            modelBuilder.Entity("Ecommerce.Models.SanPhamAnh", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("DuongDan")
                        .IsRequired()
                        .HasMaxLength(500)
                        .HasColumnType("nvarchar(500)");

                    b.Property<bool>("IsMain")
                        .HasColumnType("bit");

                    b.Property<int>("SanPhamId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("SanPhamId");

                    b.ToTable("SanPhamAnhs");
                });

            modelBuilder.Entity("Ecommerce.Models.ThanhToan", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int?>("DonHangId")
                        .HasColumnType("int");

                    b.Property<DateTime>("NgayTao")
                        .HasColumnType("datetime2");

                    b.Property<string>("PhuongThuc")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<decimal>("SoTien")
                        .HasColumnType("decimal(18, 2)");

                    b.Property<string>("TrangThai")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.HasKey("Id");

                    b.HasIndex("DonHangId");

                    b.ToTable("ThanhToans");
                });

            modelBuilder.Entity("Ecommerce.Models.YeuThich", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<DateTime>("NgayTao")
                        .HasColumnType("datetime2");

                    b.Property<int>("NguoiDungId")
                        .HasColumnType("int");

                    b.Property<int>("SanPhamId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("NguoiDungId");

                    b.HasIndex("SanPhamId");

                    b.ToTable("YeuThichs");
                });

            modelBuilder.Entity("Ecommerce.Models.ChiTietDonHang", b =>
                {
                    b.HasOne("Ecommerce.Models.DonHang", "DonHang")
                        .WithMany("ChiTietDonHangs")
                        .HasForeignKey("DonHangId");

                    b.HasOne("Ecommerce.Models.KichThuocSanPham", "KichThuocSanPham")
                        .WithMany("ChiTietDonHangs")
                        .HasForeignKey("KichThuocSanPhamId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Ecommerce.Models.SanPham", "SanPham")
                        .WithMany("ChiTietDonHangs")
                        .HasForeignKey("SanPhamId");

                    b.Navigation("DonHang");

                    b.Navigation("KichThuocSanPham");

                    b.Navigation("SanPham");
                });

            modelBuilder.Entity("Ecommerce.Models.ChiTietPhieuNhap", b =>
                {
                    b.HasOne("Ecommerce.Models.PhieuNhap", "PhieuNhap")
                        .WithMany("ChiTietPhieuNhaps")
                        .HasForeignKey("PhieuNhapId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Ecommerce.Models.SanPham", "SanPham")
                        .WithMany()
                        .HasForeignKey("SanPhamId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("PhieuNhap");

                    b.Navigation("SanPham");
                });

            modelBuilder.Entity("Ecommerce.Models.DanhGia", b =>
                {
                    b.HasOne("Ecommerce.Models.DonHang", "DonHang")
                        .WithMany()
                        .HasForeignKey("DonHangId");

                    b.HasOne("Ecommerce.Models.NguoiDung", "NguoiDung")
                        .WithMany("DanhGias")
                        .HasForeignKey("NguoiDungId");

                    b.HasOne("Ecommerce.Models.SanPham", "SanPham")
                        .WithMany("DanhGias")
                        .HasForeignKey("SanPhamId");

                    b.Navigation("DonHang");

                    b.Navigation("NguoiDung");

                    b.Navigation("SanPham");
                });

            modelBuilder.Entity("Ecommerce.Models.DonHang", b =>
                {
                    b.HasOne("Ecommerce.Models.NguoiDung", "NguoiDung")
                        .WithMany("DonHangs")
                        .HasForeignKey("NguoiDungId");

                    b.Navigation("NguoiDung");
                });

            modelBuilder.Entity("Ecommerce.Models.GioHang", b =>
                {
                    b.HasOne("Ecommerce.Models.KichThuocSanPham", "KichThuocSanPham")
                        .WithMany("GioHangs")
                        .HasForeignKey("KichThuocSanPhamId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Ecommerce.Models.NguoiDung", "NguoiDung")
                        .WithMany("GioHangs")
                        .HasForeignKey("NguoiDungId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("KichThuocSanPham");

                    b.Navigation("NguoiDung");
                });

            modelBuilder.Entity("Ecommerce.Models.KichThuocSanPham", b =>
                {
                    b.HasOne("Ecommerce.Models.SanPham", "SanPham")
                        .WithMany("KichThuocSanPhams")
                        .HasForeignKey("SanPhamId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("SanPham");
                });

            modelBuilder.Entity("Ecommerce.Models.LienHe", b =>
                {
                    b.HasOne("Ecommerce.Models.NguoiDung", "NguoiDung")
                        .WithMany("LienHes")
                        .HasForeignKey("NguoiDungId");

                    b.Navigation("NguoiDung");
                });

            modelBuilder.Entity("Ecommerce.Models.PhieuNhap", b =>
                {
                    b.HasOne("Ecommerce.Models.NguoiDung", "NguoiTao")
                        .WithMany()
                        .HasForeignKey("NguoiTaoId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("NguoiTao");
                });

            modelBuilder.Entity("Ecommerce.Models.SanPham", b =>
                {
                    b.HasOne("Ecommerce.Models.DanhMuc", "DanhMuc")
                        .WithMany("SanPhams")
                        .HasForeignKey("DanhMucId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("DanhMuc");
                });

            modelBuilder.Entity("Ecommerce.Models.SanPhamAnh", b =>
                {
                    b.HasOne("Ecommerce.Models.SanPham", "SanPham")
                        .WithMany("SanPhamAnhs")
                        .HasForeignKey("SanPhamId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("SanPham");
                });

            modelBuilder.Entity("Ecommerce.Models.ThanhToan", b =>
                {
                    b.HasOne("Ecommerce.Models.DonHang", "DonHang")
                        .WithMany("ThanhToans")
                        .HasForeignKey("DonHangId");

                    b.Navigation("DonHang");
                });

            modelBuilder.Entity("Ecommerce.Models.YeuThich", b =>
                {
                    b.HasOne("Ecommerce.Models.NguoiDung", "NguoiDung")
                        .WithMany("YeuThichs")
                        .HasForeignKey("NguoiDungId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Ecommerce.Models.SanPham", "SanPham")
                        .WithMany("YeuThichs")
                        .HasForeignKey("SanPhamId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("NguoiDung");

                    b.Navigation("SanPham");
                });

            modelBuilder.Entity("Ecommerce.Models.DanhMuc", b =>
                {
                    b.Navigation("SanPhams");
                });

            modelBuilder.Entity("Ecommerce.Models.DonHang", b =>
                {
                    b.Navigation("ChiTietDonHangs");

                    b.Navigation("ThanhToans");
                });

            modelBuilder.Entity("Ecommerce.Models.KichThuocSanPham", b =>
                {
                    b.Navigation("ChiTietDonHangs");

                    b.Navigation("GioHangs");
                });

            modelBuilder.Entity("Ecommerce.Models.NguoiDung", b =>
                {
                    b.Navigation("DanhGias");

                    b.Navigation("DonHangs");

                    b.Navigation("GioHangs");

                    b.Navigation("LienHes");

                    b.Navigation("YeuThichs");
                });

            modelBuilder.Entity("Ecommerce.Models.PhieuNhap", b =>
                {
                    b.Navigation("ChiTietPhieuNhaps");
                });

            modelBuilder.Entity("Ecommerce.Models.SanPham", b =>
                {
                    b.Navigation("ChiTietDonHangs");

                    b.Navigation("DanhGias");

                    b.Navigation("KichThuocSanPhams");

                    b.Navigation("SanPhamAnhs");

                    b.Navigation("YeuThichs");
                });
#pragma warning restore 612, 618
        }
    }
}
