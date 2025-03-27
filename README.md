# Person Detection App

Ứng dụng WPF sử dụng camera máy tính để phát hiện người và điều khiển màn hình, được phát triển với .NET 8.

## Công nghệ sử dụng

- **WPF (Windows Presentation Foundation)**: Framework UI của Microsoft để tạo giao diện người dùng
- **C#**: Ngôn ngữ lập trình chính
- **MVVM (Model-View-ViewModel)**: Mẫu kiến trúc để tách biệt logic và giao diện
- **OpenCvSharp4**: Thư viện wrapper .NET cho OpenCV, cung cấp các công cụ xử lý hình ảnh và thị giác máy tính
- **CommunityToolkit.Mvvm**: Thư viện hỗ trợ triển khai MVVM
- **Windows Registry API**: Để tự động khởi động cùng Windows

## Tính năng chính

- Phát hiện người trong tầm nhìn camera
- Tự động bật màn hình khi phát hiện người
- Nhận diện khuôn mặt và chỉ bật màn hình khi nhận diện người quen
- Đăng ký khuôn mặt mới
- Điều chỉnh độ nhạy phát hiện người và khuôn mặt
- Tự động khởi động cùng Windows

## Cài đặt

1. Đảm bảo bạn đã cài đặt .NET 8 SDK
2. Clone repository hoặc tải về các file
3. Build và chạy ứng dụng
4. Các file cascade cần thiết sẽ được tự động tải về nếu chưa có

## Yêu cầu hệ thống

- Windows 10/11
- .NET 8 Runtime
- Webcam hoặc camera tích hợp
