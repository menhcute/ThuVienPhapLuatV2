# ThuVienPhapLuatV2

Đây là chương trình Crawl dữ liệu như Văn bản pháp luật, Công văn... từ trang web Thư viện pháp luật (https://thuvienphapluat.vn/). Sau khi Crawl dữ liệu xong, chương trình sẽ lưu dữ liệu vào database, chương trình này hiện hỗ trợ lưu vào một Folder trong máy, văn bản được Crawl về sẽ ở dạng .txt. 

- Tính năng mới: chương trình sẽ Crawl văn bản được ra mắt ngày đầu tiên, và sẽ Crawl cho tới văn bản mới nhất, ngoài ra chương trình kiểm tra được những link nào đã cào, những link nào chưa cào, tránh cào trùng.

How to use:

Mở chương trình này trong Visual Studio 2022.
CTRL + F, search biến string filePath, ở đó sẽ có một cái đường dẫn vô một folder để lưu File công văn, hãy chỉnh sửa để lưu vào máy bạn (thường là bỏ vô Folder Storage cùng Folder chung của chương trình này).
Sau đó Clean, Build chương trình.
Sau đó Run chương trình và chương trình sẽ Crawl dữ liệu và lưu vào Folder filePath đó.
