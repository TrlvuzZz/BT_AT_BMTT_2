# BÀI TẬP AN TOÀN VÀ BẢO MẬT THÔNG TIN

Repository lưu trữ nội dung bài tập môn An toàn và Bảo mật Thông tin: hai bài thực hành về mã hoá AES (mã hoá đối xứng) và RSA (mã hoá bất đối xứng), được viết bằng .NET (C#) và thực hiện trên Ubuntu.

## Thông tin sinh viên:
+ Họ và tên: Trần Lâm Vũ
+ Lớp: K59KMT.K01
+ Mã số sinh viên: K235510205299
+ Trường: Đại học Kỹ thuật Công nghiệp Thái Nguyên

## Nội dung
- [Bài tập 1](baitap1.md) - Mã hoá AES: sinh khoá, lưu/import khoá, mã hoá văn bản và mã hoá file lớn (hàng GB), đo thời gian và so sánh dữ liệu theo byte
- [Bài tập 2](baitap2.md) - Mã hoá RSA: sinh cặp khoá, định dạng PEM, mã hoá dữ liệu ≤ 190 byte, đánh giá tốc độ, băm SHA-256 và chữ ký số RSA-PSS

## Deadline
23:59 26/10/2026

## Kết quả

Bài tập 1

Hoàn thành chương trình mã hoá AES-256: sinh khoá, lưu khoá ra file, import khoá, mã hoá văn bản và mã hoá file. Chương trình xử lý được file có kích thước hàng GB, đo được thời gian mã hoá/giải mã và so sánh theo byte cho thấy file sau giải mã giống hệt file nguồn (0 byte khác nhau, mã băm SHA-256 trùng khớp).

Bài tập 2

Hoàn thành chương trình mã hoá RSA-2048 trên .NET: sinh cặp khoá và xuất/nhập file PEM, trích xuất được các thành phần n, e, d, p, q...; mã hoá được dữ liệu tối đa 190 byte (ví dụ khoá AES dạng Base64) và giải mã khớp dữ liệu gốc. Đo tốc độ cho thấy thao tác bằng private key chậm hơn public key nhiều lần. Hàm băm SHA-256 thể hiện rõ hiệu ứng tuyết lở, và chữ ký số bằng RSA-PSS cho chữ ký khác nhau ở mỗi lần ký dù cùng một dữ liệu.

## Cấu trúc repo
```
aes-rsa-baitap/
├── README.md
├── baitap1.md
├── baitap2.md
├── bai1-aes/
│   └── AesApp/            # Source code .NET Bài 1 (Program.cs)
├── bai2-rsa/
│   └── RsaApp/            # Source code .NET Bài 2 (Program.cs)
└── images/                # Ảnh chụp kết quả chạy của 2 bài
    ├── baitap1/
    └── baitap2/
```
