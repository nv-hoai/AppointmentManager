# AppointmentManager
Để có thể chạy được chương trình, yêu cầu lập trình viên phải cài đặt hệ quản trị cơ sở dữ liệu SQL Server và Mirosoft Visual Studio 2022. Có thể chạy trên VSCode nhưng các bước cài đặt phức tạm hơn nên không đề cập ở đây.

Sau khi clone project về máy, lập trình viên mở Visual Studio 2022 lên, chọn open solution và chọn vào AppointmentManager.sln.

Hệ thống sẽ mở project, sau đó click chuột phải vào project ở solution explorer, chọn build để hệ thống tải các package cần thiết.

Sau khi build thành công, tiếp tục click chuột phải vào project và chọn mở terminal.

Chạy lệnh dotnet ef database update (nếu hệ thống báo lỗi không xác định ef thì bạn cần cài thêm ef vào).

Khi chạy thành công, bạn có thể chạy chương trình.
