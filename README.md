# TechStore - Website bán laptop

## 1. Giới thiệu

TechStore là website thương mại điện tử hỗ trợ kinh doanh laptop trực tuyến.
Hệ thống cung cấp các chức năng cho khách hàng và quản trị viên như xem sản phẩm,
tìm kiếm, quản lý giỏ hàng, đặt hàng, thanh toán và quản lý sản phẩm.

## 2. Công nghệ sử dụng

- ASP.NET Core MVC
- C#
- Entity Framework Core
- SQL Server
- HTML/CSS/JavaScript
- Bootstrap

## 3. Cấu trúc repository

```text
.
├── setup/
│   ├── database/              # Cơ sở dữ liệu và dữ liệu thử nghiệm
│   └── README.md              # Hướng dẫn cài đặt và chạy hệ thống
├── scr/
│   └── EcommerceLaptop/      # Mã nguồn ASP.NET Core MVC
├── progress-report/
│   ├── report/                # Báo cáo tiến độ theo tuần
│   └── README.md
├── thesis/
│   ├── doc/                   # Báo cáo khóa luận dạng DOC/DOCX
│   ├── pdf/                   # Báo cáo khóa luận dạng PDF
│   ├── html/                  # Tài liệu dạng web
│   ├── abs/                   # Slide và tài liệu thuyết trình
│   └── refs/                  # Tài liệu tham khảo
├── soft/                      # Phần mềm liên quan (nếu có)
├── docker/                    # Tệp triển khai Docker (nếu có)
├── EcommerceLaptop.sln       # Solution của dự án
└── README.md
```

## 4. Cài đặt và chạy chương trình

Xem hướng dẫn chi tiết tại [setup/README.md](setup/README.md). Tóm tắt:

1. Cài .NET 8 SDK, SQL Server và Visual Studio/Visual Studio Code.
2. Dùng SSMS chạy [setup/database/db_laptop.sql](setup/database/db_laptop.sql).
3. Cập nhật chuỗi kết nối trong `scr/EcommerceLaptop/appsettings.json`.
4. Tại thư mục gốc, chạy:

   ```bash
   dotnet restore
   dotnet build EcommerceLaptop.sln
   dotnet run --project scr/EcommerceLaptop/EcommerceLaptop.csproj
   ```

5. Mở địa chỉ localhost được hiển thị trong Terminal.

Không commit thông tin bí mật thật của MoMo hoặc thông tin kết nối cá nhân vào repository. Khi cần thanh toán MoMo, cấu hình các khóa theo hướng dẫn trong [setup/README.md](setup/README.md).

## 5. Tài liệu dự án

- Báo cáo tiến độ: [progress-report/README.md](progress-report/README.md)
- Báo cáo dạng DOCX: [thesis/doc/BAOCAO ASPNET NGUYENTHONGTHAI_Final.docx](thesis/doc/BAOCAO%20ASPNET%20NGUYENTHONGTHAI_Final.docx)
- Báo cáo dạng PDF: [thesis/pdf/BAOCAO ASPNET NGUYENTHONGTHAI_Final.pdf](thesis/pdf/BAOCAO%20ASPNET%20NGUYENTHONGTHAI_Final.pdf)
- Mã nguồn: [scr/EcommerceLaptop](scr/EcommerceLaptop)
- Dữ liệu khởi tạo và thử nghiệm: [setup/database](setup/database)

## 6. Thông tin liên hệ

- Họ tên: Nguyễn Thông Thái
- Email: thaint180998@tvu-onschool.edu.vn
- Số điện thoại: 0393535696
