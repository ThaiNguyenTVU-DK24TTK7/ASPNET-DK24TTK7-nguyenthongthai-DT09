# Hướng dẫn cài đặt TechStore

## 1. Yêu cầu môi trường

- Windows 10/11
- .NET 8 SDK
- SQL Server
- Visual Studio hoặc Visual Studio Code

## 2. Cài đặt cơ sở dữ liệu

Sử dụng SQL Server Management Studio (SSMS) để thực thi file:

setup/database/db_laptop.sql

Sau khi tạo cơ sở dữ liệu, kiểm tra lại tên database và thông tin SQL Server.

## 3. Cấu hình kết nối cơ sở dữ liệu

Mở file:

scr/EcommerceLaptop/appsettings.json

Cập nhật chuỗi kết nối `DefaultConnection` phù hợp với máy đang cài đặt SQL Server.

## 4. Cấu hình MoMo

Trong file:

scr/EcommerceLaptop/appsettings.json

Thay các giá trị:

- YOUR_PARTNER_CODE
- YOUR_ACCESS_KEY
- YOUR_SECRET_KEY

bằng thông tin MoMo tương ứng khi cần sử dụng chức năng thanh toán.

## 5. Chạy chương trình

Mở Terminal tại thư mục gốc của project và chạy:

dotnet restore

dotnet build EcommerceLaptop.sln

dotnet run --project scr/EcommerceLaptop/EcommerceLaptop.csproj

Sau đó truy cập địa chỉ localhost được hiển thị trong Terminal.

## 6. Công nghệ sử dụng

- ASP.NET Core MVC
- C#
- Entity Framework Core
- SQL Server
- HTML
- CSS
- JavaScript
- Bootstrap
