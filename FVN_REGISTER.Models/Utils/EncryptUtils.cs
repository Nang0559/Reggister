
using System.Security.Cryptography;
using System.Text;


namespace FVN_REGISTER.Contract.Util
{
    public static class EncryptUtils
    {
        private static string privateKeyz = "NangDV";

        // 1. Mã hóa MD5 (Dùng cho mật khẩu)
        public static string MD5(this string data)
        {
            if (string.IsNullOrEmpty(data)) return string.Empty;

            using (var md5 = System.Security.Cryptography.MD5.Create())
            {
                byte[] hashedBytes = md5.ComputeHash(Encoding.UTF8.GetBytes(data));
                return BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
            }
        }

        // 2. So sánh mật khẩu
        public static bool PwdCompare(string plainPass, string encryptPass)
        {
            if (string.IsNullOrEmpty(plainPass) || string.IsNullOrEmpty(encryptPass))
                return false;

            return encryptPass.Trim().Equals(plainPass.Trim().MD5());
        }

        // 3. Mã hóa TripleDES (Dùng cho dữ liệu cần bảo mật như chuỗi kết nối, token...)
        public static string MD5Encrypt(string source)
        {
            if (string.IsNullOrEmpty(source)) return string.Empty;
            try
            {
                byte[] keyArray;
                byte[] toEncryptArray = Encoding.UTF8.GetBytes(source);

                using (var hashmd5 = System.Security.Cryptography.MD5.Create())
                {
                    keyArray = hashmd5.ComputeHash(Encoding.UTF8.GetBytes(privateKeyz));
                }

                using (var tdes = TripleDES.Create())
                {
                    tdes.Key = keyArray;
                    tdes.Mode = CipherMode.ECB;
                    tdes.Padding = PaddingMode.PKCS7;

                    using (var cTransform = tdes.CreateEncryptor())
                    {
                        byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);
                        return Convert.ToBase64String(resultArray, 0, resultArray.Length);
                    }
                }
            }
            catch { return string.Empty; }
        }

        // 4. Giải mã TripleDES
        public static string MD5Decrypt(string source)
        {
            if (string.IsNullOrEmpty(source)) return string.Empty;
            try
            {
                byte[] keyArray;
                byte[] toEncryptArray = Convert.FromBase64String(source);

                using (var hashmd5 = System.Security.Cryptography.MD5.Create())
                {
                    keyArray = hashmd5.ComputeHash(Encoding.UTF8.GetBytes(privateKeyz));
                }

                using (var tdes = TripleDES.Create())
                {
                    tdes.Key = keyArray;
                    tdes.Mode = CipherMode.ECB;
                    tdes.Padding = PaddingMode.PKCS7;

                    using (var cTransform = tdes.CreateDecryptor())
                    {
                        byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);
                        return Encoding.UTF8.GetString(resultArray);
                    }
                }
            }
            catch { return string.Empty; }
        }
    }
}
