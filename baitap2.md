# BÀI TẬP 2
Bài tập thực hành mã hoá RSA — thuật toán mã hoá bất đối xứng (một cặp khoá: khoá công khai để mã hoá/xác minh, khoá bí mật để giải mã/ký). Chương trình được viết bằng .NET (C#) dạng console, thực hiện trên Ubuntu, gồm các chức năng theo đúng thứ tự yêu cầu: sinh cặp khoá RSA, xuất khoá ra định dạng PEM, tìm hiểu các byte trong file PEM, import khoá từ file PEM, mã hoá và giải mã dữ liệu, đánh giá tốc độ, băm SHA-256 với hiệu ứng tuyết lở và chữ ký số.
Source code của bài nằm trong thư mục `Baitap2` (file `Program.cs`), chạy bằng lệnh:
```
dotnet run
```
Chương trình dùng RSA-2048 (khoá dài 2048 bit), padding OAEP-SHA256 cho mã hoá và SHA-256 cho chữ ký số.
## 2.1. Sinh cặp khoá RSA
Sử dụng lớp `RSA` của .NET để sinh cặp khoá 2048 bit:
```csharp
using (RSA rsa = RSA.Create(2048))
{
    // xuat 2 khoa ra file PEM o buoc sau
}
```
Kết quả: chương trình sinh được một cặp khoá gồm khoá công khai (public key) và khoá bí mật (private key) liên kết với nhau về mặt toán học.
![Sinh cặp khoá RSA](images/baitap2/rsa-sinh-khoa.png)

## 2.2. Xuất khoá ra định dạng PEM
Cặp khoá được xuất ra 2 file văn bản theo định dạng PEM: `public_key.pem` và `private_key.pem`:
```csharp
File.WriteAllText("public_key.pem",  rsa.ExportRSAPublicKeyPem());
File.WriteAllText("private_key.pem", rsa.ExportRSAPrivateKeyPem());
```
Kết quả: thư mục chứa 2 file PEM, file khoá công khai nhẹ hơn (~392 byte) so với file khoá bí mật (~1,7 KB) vì khoá bí mật chứa nhiều thành phần hơn.
![Hai file PEM sau khi xuất](images/baitap2/rsa-xuat-pem.png)

## 2.3. Tìm hiểu các byte trong file PEM
Mở file PEM cho thấy cấu trúc: dòng đầu là `-----BEGIN PUBLIC KEY-----` (hoặc `-----BEGIN RSA PRIVATE KEY-----`), dòng cuối là `-----END ...-----`, phần giữa là dữ liệu nhị phân DER (mã hoá theo chuẩn ASN.1) được mã hoá Base64 và ngắt dòng mỗi 64 ký tự.
Giải mã Base64 của khoá công khai thu được 270 byte DER, bắt đầu bằng dãy byte:
```
30 82 01 0A  02 82 01 01 00  <256 byte của n>  02 03 01 00 01
```
Ý nghĩa: `30` là thẻ SEQUENCE, `02` là thẻ INTEGER; số nguyên đầu tiên dài 257 byte (thừa 1 byte `00` ở đầu để đảm bảo số dương) chính là n (modulus, 2048 bit); số nguyên cuối `01 00 01` = 65537 chính là e. File khoá bí mật chứa tới 9 số nguyên theo thứ tự: version, n, e, d, p, q, dP, dQ, qInv.
![Nội dung file PEM](images/baitap2/rsa-pem.png)

## 2.4. Import khoá từ file PEM
Đọc lại file PEM để khôi phục cặp khoá và trích xuất các thành phần:
```csharp
RSA rsa = RSA.Create();
rsa.ImportFromPem(File.ReadAllText("private_key.pem"));
RSAParameters p = rsa.ExportParameters(true);
```
Kết quả: từ khoá công khai lấy được n (2048 bit) và e = 65537; từ khoá bí mật lấy được đầy đủ n, e, d, p, q, dP, dQ, qInv, trong đó d cũng dài 2048 bit.
![Các tham số khoá trích xuất từ PEM](images/baitap2/rsa-tham-so-khoa.png)

## 2.5. Mã hoá dữ liệu
RSA không mã hoá trực tiếp được dữ liệu dài: với RSA-2048 và OAEP-SHA256, mỗi lần mã hoá tối đa `256 − 2×32 − 2 = 190 byte`. Vì vậy dữ liệu được chọn ở đây là khoá AES-256 của Bài tập 1 (32 byte, dạng Base64 gồm 44 ký tự — nằm gọn trong giới hạn 190 byte):
```csharp
byte[] cipher = pub.Encrypt(data, RSAEncryptionPadding.OaepSHA256);
File.WriteAllText("encrypted.txt", Convert.ToBase64String(cipher));
```
Kết quả: dữ liệu 44 byte được mã hoá thành bản mã 256 byte, lưu vào file `encrypted.txt` dưới dạng Base64 (344 ký tự).
![Mã hoá dữ liệu bằng RSA](images/baitap2/rsa-ma-hoa.png)

## 2.6. Giải mã dữ liệu và so sánh
Đọc bản mã từ `encrypted.txt`, giải mã bằng khoá bí mật và so sánh từng byte với dữ liệu gốc:
```csharp
byte[] plain = priv.Decrypt(cipher, RSAEncryptionPadding.OaepSHA256);
```
Kết quả: dữ liệu giải mã được giống hệt dữ liệu gốc, chứng tỏ cặp khoá hoạt động đúng và đây cũng là cơ chế của mã hoá lai — dùng RSA để bảo vệ khoá AES, dùng AES để mã hoá dữ liệu lớn.
![Giải mã và so sánh với dữ liệu gốc](images/baitap2/rsa-giai-ma.png)

## 2.7. Đánh giá tốc độ mã hoá và giải mã
Chương trình mã hoá/giải mã cùng một dữ liệu 1000 lần bằng mỗi loại khoá và tính thời gian trung bình cho một lần:
Thao tác	Khoá dùng	Thời gian trung bình (ms/lần)
Mã hoá	Public key (e nhỏ)	...
Giải mã	Private key (d lớn)	...
Nhận xét: giải mã chậm hơn mã hoá vài chục lần. Nguyên nhân là phép toán cốt lõi của RSA là luỹ thừa modulo: mã hoá dùng số mũ e = 65537 rất nhỏ (chỉ có 2 bit 1), còn giải mã dùng số mũ d dài tới 2048 bit nên tốn nhiều phép nhân số lớn hơn hẳn.
![Đo tốc độ mã hoá và giải mã RSA](images/baitap2/rsa-toc-do.png)

## 2.8. Hàm băm SHA-256 và hiệu ứng tuyết lở
Chương trình cho nhập 2 chuỗi ký tự, tính mã băm SHA-256 (256 bit, hiển thị 64 ký tự hex) của từng chuỗi và đếm số bit khác nhau giữa hai mã băm:
```csharp
byte[] h = SHA256.HashData(Encoding.UTF8.GetBytes(text));
```
Thử với 2 chuỗi chỉ khác nhau đúng 1 ký tự ở cuối: `Dep trai moi co nguoi yeu` và `Dep trai moi co nguoi yeu!` — hai mã băm cho ra hoàn toàn khác nhau, số bit khác nhau là .../256 (khoảng 50%). Đây là hiệu ứng tuyết lở (avalanche effect): thay đổi rất nhỏ ở đầu vào làm khoảng một nửa số bit của mã băm đảo ngược, không thể đoán được mã băm mới từ mã băm cũ.
![SHA-256 và hiệu ứng tuyết lở](images/baitap2/sha256-tuyet-lo.png)

## 2.9. Chữ ký số
Yêu cầu đặc biệt của bài: ký cùng một dữ liệu, cùng một người ký (cùng khoá bí mật) nhưng hai lần ký phải cho hai chữ ký khác nhau. Chương trình ký một dữ liệu bằng hai cơ chế padding rồi đối chiếu:
PKCS#1 v1.5: ký 2 lần cho chữ ký giống hệt nhau (cơ chế xác định — không đạt yêu cầu đặc biệt).
RSA-PSS: ký 2 lần cho chữ ký khác nhau, vì mỗi lần ký PSS trộn thêm một đoạn salt ngẫu nhiên; cả hai chữ ký đều xác minh hợp lệ (True) bằng khoá công khai.
```csharp
byte[] sig = priv.SignData(data, HashAlgorithmName.SHA256, RSASignaturePadding.Pss);
bool ok = pub.VerifyData(data, sig, HashAlgorithmName.SHA256, RSASignaturePadding.Pss);
```

Kết luận: để đáp ứng yêu cầu "cùng dữ liệu, cùng người ký, chữ ký khác nhau mỗi lần", phải dùng RSA-PSS thay cho PKCS#1 v1.5.
![Chữ ký số PKCS#1 v1.5 và RSA-PSS](images/baitap2/rsa-chu-ky-so.png)

---

## Kết quả
Hoàn thành các yêu cầu của Bài tập 2:
- Sinh được cặp khoá RSA-2048, xuất ra file PEM và import lại đầy đủ các tham số (n, e, d, p, q, dP, dQ, qInv).
- Hiểu được cấu trúc file PEM: phần giữa là Base64 của dữ liệu DER/ASN.1; khoá công khai gồm n và e = 65537, khoá bí mật gồm 9 thành phần.
- Mã hoá được dữ liệu trong giới hạn 190 byte của RSA-2048 + OAEP-SHA256 (minh hoạ bằng khoá AES 32 byte của Bài tập 1), lưu bản mã Base64 ra file và giải mã lại khớp hoàn toàn.
- Đánh giá được tốc độ: giải mã bằng khoá bí mật chậm hơn mã hoá bằng khoá công khai vài chục lần do số mũ d lớn hơn e rất nhiều.
- Thấy rõ hiệu ứng tuyết lở của SHA-256: đổi 1 ký tự làm khoảng 50% số bit của mã băm thay đổi.
- Tạo được chữ ký số đáp ứng yêu cầu đặc biệt — cùng dữ liệu, cùng người ký nhưng chữ ký khác nhau mỗi lần — nhờ cơ chế RSA-PSS với salt ngẫu nhiên.
Kết luận chung của cả hai bài tập: RSA rất chậm và giới hạn kích thước dữ liệu nên không dùng để mã hoá trực tiếp dữ liệu lớn; trong thực tế người ta kết hợp cả hai — mã hoá lai: dùng AES (nhanh) để mã hoá dữ liệu, dùng RSA để mã hoá và trao đổi khoá AES, dùng SHA-256 để kiểm tra tính toàn vẹn và chữ ký số RSA-PSS để xác thực người gửi.
