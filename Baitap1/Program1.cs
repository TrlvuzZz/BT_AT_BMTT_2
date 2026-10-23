using System;
using System.IO;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;

class Program
{
    static string KeyFile = "aes_key.txt";   // file chua Key + IV dang Base64

    static void Main()
    {
        while (true)
        {
            Console.WriteLine("\n===== BAI 1: MA HOA AES =====");
            Console.WriteLine("1. Sinh khoa AES va luu ra file");
            Console.WriteLine("2. Import khoa tu file");
            Console.WriteLine("3. Ma hoa 1 doan van ban");
            Console.WriteLine("4. Ma hoa 1 file");
            Console.WriteLine("5. Giai ma 1 file");
            Console.WriteLine("6. So sanh 2 file theo byte + SHA-256");
            Console.WriteLine("0. Thoat");
            Console.Write("Chon chuc nang: ");
            string chon = Console.ReadLine();
            switch (chon)
            {
                case "1": SinhKhoa(); break;
                case "2": ImportKhoa(show: true); break;
                case "3": MaHoaVanBan(); break;
                case "4": MaHoaGiaiMaFile(maHoa: true); break;
                case "5": MaHoaGiaiMaFile(maHoa: false); break;
                case "6": SoSanhFile(); break;
                case "0": return;
                default: Console.WriteLine("Lua chon khong hop le!"); break;
            }
        }
    }

    // ---- 1. Sinh khoa AES-256, luu Key va IV (Base64) ra file ----
    static void SinhKhoa()
    {
        using (Aes aes = Aes.Create())
        {
            aes.KeySize = 256;
            aes.GenerateKey();
            aes.GenerateIV();
            string[] lines =
            {
                Convert.ToBase64String(aes.Key),
                Convert.ToBase64String(aes.IV)
            };
            File.WriteAllLines(KeyFile, lines);
            Console.WriteLine("Da sinh khoa AES-256 va luu vao: " + KeyFile);
            Console.WriteLine("Key (Base64): " + lines[0]);
            Console.WriteLine("IV  (Base64): " + lines[1]);
        }
    }

    // ---- 2. Import khoa tu file ----
    static (byte[] Key, byte[] IV) ImportKhoa(bool show = false)
    {
        if (!File.Exists(KeyFile))
        {
            Console.WriteLine("Chua co file khoa, tu dong sinh khoa moi...");
            SinhKhoa();
        }
        string[] lines = File.ReadAllLines(KeyFile);
        byte[] key = Convert.FromBase64String(lines[0]);
        byte[] iv  = Convert.FromBase64String(lines[1]);
        if (show)
        {
            Console.WriteLine("Da import khoa tu file: " + KeyFile);
            Console.WriteLine($"Key: {key.Length} byte ({key.Length * 8} bit), IV: {iv.Length} byte");
        }
        return (key, iv);
    }

    // ---- 3. Ma hoa van ban -> in ban ma (Base64) ra man hinh, giai ma kiem tra ----
    static void MaHoaVanBan()
    {
        var (key, iv) = ImportKhoa();
        Console.Write("Nhap van ban can ma hoa: ");
        string plain = Console.ReadLine();
        byte[] cipherBytes;
        using (Aes aes = Aes.Create())
        {
            aes.Key = key; aes.IV = iv;
            using (var enc = aes.CreateEncryptor())
            {
                byte[] p = Encoding.UTF8.GetBytes(plain);
                cipherBytes = enc.TransformFinalBlock(p, 0, p.Length);
            }
        }
        Console.WriteLine("Ban ma (Base64): " + Convert.ToBase64String(cipherBytes));

        using (Aes aes = Aes.Create())
        {
            aes.Key = key; aes.IV = iv;
            using (var dec = aes.CreateDecryptor())
            {
                byte[] d = dec.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);
                Console.WriteLine("Giai ma lai duoc: " + Encoding.UTF8.GetString(d));
            }
        }
    }

    // ---- 4 & 5. Ma hoa / giai ma file theo luong (stream), co do thoi gian ----
    // Dung CryptoStream nen file hang GB van xu ly duoc, khong can nap het vao RAM.
    static void MaHoaGiaiMaFile(bool maHoa)
    {
        var (key, iv) = ImportKhoa();
        Console.Write("Nhap duong dan file dau vao: ");
        string input = Console.ReadLine();
        Console.Write("Nhap duong dan file dau ra: ");
        string output = Console.ReadLine();

        var sw = Stopwatch.StartNew();
        using (Aes aes = Aes.Create())
        {
            aes.Key = key; aes.IV = iv;
            using (FileStream fsIn  = new FileStream(input,  FileMode.Open,   FileAccess.Read))
            using (FileStream fsOut = new FileStream(output, FileMode.Create, FileAccess.Write))
            {
                ICryptoTransform transform = maHoa ? aes.CreateEncryptor() : aes.CreateDecryptor();
                using (CryptoStream cs = new CryptoStream(fsOut, transform, CryptoStreamMode.Write))
                {
                    fsIn.CopyTo(cs);
                }
            }
        }
        sw.Stop();
        var fi = new FileInfo(output);
        Console.WriteLine($"{(maHoa ? "Ma hoa" : "Giai ma")} xong -> {output}");
        Console.WriteLine($"Thoi gian: {sw.ElapsedMilliseconds} ms | Kich thuoc file ra: {fi.Length:N0} byte");
    }

    // ---- 6. So sanh 2 file: dem so byte khac nhau + doi chieu bam SHA-256 ----
    static void SoSanhFile()
    {
        Console.Write("File thu 1: "); string f1 = Console.ReadLine();
        Console.Write("File thu 2: "); string f2 = Console.ReadLine();
        long len1 = new FileInfo(f1).Length, len2 = new FileInfo(f2).Length;
        Console.WriteLine($"Kich thuoc: {len1:N0} byte vs {len2:N0} byte");

        long diff = 0;
        if (len1 == len2)
        {
            byte[] b1 = new byte[81920], b2 = new byte[81920];
            using (var s1 = File.OpenRead(f1))
            using (var s2 = File.OpenRead(f2))
            {
                int r;
                while ((r = s1.Read(b1, 0, b1.Length)) > 0)
                {
                    s2.Read(b2, 0, r);
                    for (int i = 0; i < r; i++) if (b1[i] != b2[i]) diff++;
                }
            }
        }
        Console.WriteLine("So byte khac nhau: " + diff);
        Console.WriteLine("SHA-256 file 1: " + HashFile(f1));
        Console.WriteLine("SHA-256 file 2: " + HashFile(f2));
        Console.WriteLine(len1 == len2 && diff == 0
            ? "=> HAI FILE GIONG HET NHAU"
            : "=> HAI FILE KHAC NHAU");
    }

    static string HashFile(string path)
    {
        using (var sha = SHA256.Create())
        using (var fs = File.OpenRead(path))
            return Convert.ToHexString(sha.ComputeHash(fs));
    }
}
