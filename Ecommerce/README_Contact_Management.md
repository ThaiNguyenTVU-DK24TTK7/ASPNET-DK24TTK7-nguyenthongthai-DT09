# Quản lý Liên hệ - Admin Panel

## Tổng quan
Chức năng quản lý liên hệ cho phép admin xem, phản hồi và quản lý các tin nhắn liên hệ từ khách hàng.

## Tính năng chính

### 1. Dashboard
- Hiển thị thống kê tổng số tin nhắn liên hệ
- Hiển thị số tin nhắn mới chưa đọc
- Card thống kê với gradient màu xanh nhạt

### 2. Danh sách liên hệ (ContactList)
- Hiển thị tất cả tin nhắn liên hệ theo thứ tự mới nhất
- Thông tin hiển thị:
  - Người gửi (tên và tài khoản nếu có)
  - Email
  - Chủ đề
  - Nội dung (rút gọn)
  - Trạng thái (Mới/Đã đọc/Đã phản hồi)
  - Ngày gửi
- Các thao tác:
  - Xem chi tiết
  - Đánh dấu đã đọc (cho tin nhắn mới)
  - Xóa tin nhắn

### 3. Chi tiết liên hệ (ContactDetail)
- Hiển thị đầy đủ thông tin tin nhắn
- Form phản hồi cho khách hàng
- Hiển thị phản hồi hiện tại (nếu có)
- Cập nhật trạng thái thành "Đã phản hồi" khi gửi phản hồi

## Trạng thái tin nhắn
- **Mới**: Tin nhắn vừa được gửi, chưa đọc
- **Đã đọc**: Admin đã xem tin nhắn
- **Đã phản hồi**: Admin đã gửi phản hồi

## Cách sử dụng

### Truy cập
1. Đăng nhập với tài khoản Admin
2. Vào menu "Quản lý liên hệ" trong sidebar

### Xem danh sách
- Tất cả tin nhắn được hiển thị trong bảng
- Tin nhắn mới có nền màu vàng nhạt
- Sử dụng các badge để phân biệt trạng thái

### Xem chi tiết
- Click vào icon "mắt" để xem chi tiết
- Thông tin được hiển thị trong layout 2 cột
- Cột trái: Thông tin tin nhắn
- Cột phải: Form phản hồi

### Phản hồi
1. Mở chi tiết tin nhắn
2. Nhập nội dung phản hồi vào textarea
3. Click "Gửi phản hồi"
4. Trạng thái tự động chuyển thành "Đã phản hồi"

### Quản lý
- **Đánh dấu đã đọc**: Click icon "check" cho tin nhắn mới
- **Xóa**: Click icon "thùng rác" và xác nhận

## Bảo mật
- Chỉ Admin mới có thể truy cập
- Sử dụng `[Authorize(Roles = "Admin")]` attribute
- Tất cả form đều có CSRF protection

## Giao diện
- Responsive design
- Sử dụng Bootstrap 5
- Custom CSS cho styling đẹp mắt
- Icons từ Bootstrap Icons
- Gradient backgrounds cho cards

## Database
- Sử dụng bảng `LienHes` có sẵn
- Quan hệ với bảng `NguoiDungs` (nếu có)
- Các trường chính:
  - Id, HoTen, Email, ChuDe, NoiDung
  - TrangThai, PhanHoiAdmin, NgayGui
  - NguoiDungId (foreign key)

## Files đã tạo/cập nhật
- `ViewModels/AdminContactListViewModel.cs`
- `ViewModels/AdminContactDetailViewModel.cs`
- `Controllers/AdminController.cs` (thêm actions)
- `Views/Admin/ContactList.cshtml`
- `Views/Admin/ContactDetail.cshtml`
- `Views/Admin/Dashboard.cshtml` (thêm thống kê)
- `Views/Shared/_AdminLayout.cshtml` (thêm menu)
- `ViewModels/DashboardViewModel.cs` (thêm properties)
- `wwwroot/css/AdminContact.css` 