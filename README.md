📝 Game Competition Analysis System (GCAS)
Hệ thống tự động thu thập và phân tích bảng xếp hạng game (Võ Lâm Truyền Kỳ) sử dụng Airtest, AI (Groq/Llama) và .NET 8.
🚀 Tính năng chính
Auto-Capture: Tự động chụp màn hình game theo định kỳ bằng Airtest.

AI Analysis: Sử dụng mô hình Llama 3 (qua Groq API) để bóc tách dữ liệu từ ảnh (OCR & Structured Data).

Leaderboard Tracking: Tự động quản lý danh sách Player, Server, Guild và biến động bảng xếp hạng.

Cloud Deployment: Hệ thống Backend được Dockerize và triển khai trên Render.

🏗 Kiến trúc hệ thống (System Architecture)
Hệ thống vận hành theo mô hình Hybrid Automation:

Client (Python/Airtest): Chạy tại máy local, điều khiển giả lập game, chụp ảnh và POST ảnh lên Server.

Server (.NET 8 API): Tiếp nhận dữ liệu, điều phối luồng phân tích AI.

AI Layer (Groq): Xử lý hình ảnh và trả về dữ liệu định dạng JSON.

Database (SQL Server): Lưu trữ lịch sử phân tích và thông tin người chơi.

🛠 Công nghệ sử dụng
Backend: ASP.NET Core 8 (Web API)

Database: Entity Framework Core, SQL Server

Automation: Airtest Project (Python)
📦 Hướng dẫn cài đặt
1. Cấu hình Backend (.NET)
Clone dự án và mở bằng Visual Studio 2022.

Cấu hình appsettings.json:
JSON
{
  "Groq": {
    "ApiKey": "YOUR_GROQ_API_KEY"
  },
  "ConnectionStrings": {
    "DefaultConnection": "YOUR_SQL_SERVER_CONNECTION_STRING"
  }
}
Update Database: dotnet ef database update

2. Cấu hình Bot tự động (Airtest)
Cài đặt thư viện: pip install airtest requests

Chạy file bot_capture.py (Script Python bạn dùng để chụp màn hình).

📡 API Endpoints
Gemini đã nói
Việc viết một file README.md chuyên nghiệp là "vũ khí" cực mạnh để ghi điểm với hội đồng Review 2. Nó cho thấy bạn có tư duy làm sản phẩm thực tế chứ không chỉ là code bài tập.

Dưới đây là mẫu README được thiết kế riêng cho dự án Game Competition Analysis System của bạn, kết hợp cả .NET Core, AI và Airtest.

📝 Game Competition Analysis System (GCAS)
Hệ thống tự động thu thập và phân tích bảng xếp hạng game (Võ Lâm Truyền Kỳ) sử dụng Airtest, AI (Groq/Llama) và .NET 8.

🚀 Tính năng chính
Auto-Capture: Tự động chụp màn hình game theo định kỳ bằng Airtest.

AI Analysis: Sử dụng mô hình Llama 3 (qua Groq API) để bóc tách dữ liệu từ ảnh (OCR & Structured Data).

Leaderboard Tracking: Tự động quản lý danh sách Player, Server, Guild và biến động bảng xếp hạng.

Cloud Deployment: Hệ thống Backend được Dockerize và triển khai trên Render.

🏗 Kiến trúc hệ thống (System Architecture)
Hệ thống vận hành theo mô hình Hybrid Automation:

Client (Python/Airtest): Chạy tại máy local, điều khiển giả lập game, chụp ảnh và POST ảnh lên Server.

Server (.NET 8 API): Tiếp nhận dữ liệu, điều phối luồng phân tích AI.

AI Layer (Groq): Xử lý hình ảnh và trả về dữ liệu định dạng JSON.

Database (SQL Server): Lưu trữ lịch sử phân tích và thông tin người chơi.

🛠 Công nghệ sử dụng
Backend: ASP.NET Core 8 (Web API)

Database: Entity Framework Core, SQL Server

Automation: Airtest Project (Python)

AI: Groq Cloud API (Model: llama-3.2-11b-vision-preview hoặc tương đương)

DevOps: Docker, Render Cloud

📦 Hướng dẫn cài đặt
1. Cấu hình Backend (.NET)
Clone dự án và mở bằng Visual Studio 2022.

Cấu hình appsettings.json:

JSON
{
  "Groq": {
    "ApiKey": "YOUR_GROQ_API_KEY"
  },
  "ConnectionStrings": {
    "DefaultConnection": "YOUR_SQL_SERVER_CONNECTION_STRING"
  }
}
Update Database: dotnet ef database update

2. Cấu hình Bot tự động (Airtest)
Cài đặt thư viện: pip install airtest requests

Chạy file bot_capture.py (Script Python bạn dùng để chụp màn hình).

📡 API Endpoints
Method	Endpoint	Description
POST	/api/Analysis/process	Tiếp nhận ảnh từ Bot và bắt đầu phân tích AI
GET	/api/Analysis	Lấy danh sách các lượt phân tích đã thực hiện
GET	/api/Leaderboard/top	Xem bảng xếp hạng hiện tại
AI: Groq Cloud API (Model: llama-3.2-11b-vision-preview hoặc tương đương)

DevOps: Docker, Render Cloud
