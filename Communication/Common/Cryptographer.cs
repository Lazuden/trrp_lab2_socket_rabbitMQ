using Newtonsoft.Json;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Communication.Common
{
    public static class Cryptographer
    {
        public static DESCryptoServiceProvider GetDES()
        {
            return new System.Security.Cryptography.DESCryptoServiceProvider();
        }

        public static DESCryptoServiceProvider GetDES(byte[] IV, byte[] Key)
        {
            return new DESCryptoServiceProvider()
            {
                IV = IV,
                Key = Key,
            };
        }

        public static RSACryptoServiceProvider GetRSA()
        {
            return new RSACryptoServiceProvider();
        }

        public static byte[] RSAEncrypt(byte[] byteEncrypt, RSAParameters RSAInfo, bool isOAEP = false)
        {
            try
            {
                byte[] encryptedData;
                using (RSACryptoServiceProvider RSA = new RSACryptoServiceProvider())
                {
                    RSA.ImportParameters(RSAInfo);
                    encryptedData = RSA.Encrypt(byteEncrypt, isOAEP);
                }
                return encryptedData;
            }
            catch (CryptographicException e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
        }


        public static byte[] GetBytesOfPublicRSA(RSAParameters rSAParameters)
        {
            var parameters = new RsaPublicKeyParameters(rSAParameters);
            var str = JsonConvert.SerializeObject(parameters);
            return Encoding.UTF8.GetBytes(str);
        }


        public static RSAParameters GetRSAFromBytes(byte[] bytes)
        {
            var publicKeyJson = Encoding.UTF8.GetString(bytes);
            var publicKey = JsonConvert.DeserializeObject<RsaPublicKeyParameters>(publicKeyJson);
            var publicKeyParameters = publicKey.GetRSAParameters();
            return publicKeyParameters;
        }

        public static DESCryptoServiceProvider GetDesFromBytes(byte[] body, RSAParameters rsaParameters)
        {
            var desParameters = JsonConvert.DeserializeObject<DesParameters>(Encoding.UTF8.GetString(body));
            var iv = RSADecrypt(desParameters.IV, rsaParameters);
            var key = RSADecrypt(desParameters.Key, rsaParameters);

            return GetDES(iv, key);
        }

        public static byte[] EncryptDesByRSA(RSAParameters publicRSA, DESCryptoServiceProvider des)
        {
            var iv_ = new byte[des.IV.Length];
            var key_ = new byte[des.Key.Length];
            Array.Copy(des.IV, 0, iv_, 0, des.IV.Length);
            Array.Copy(des.Key, 0, key_, 0, des.Key.Length);

            var iv = RSAEncrypt(iv_, publicRSA);
            var key = RSAEncrypt(key_, publicRSA);
            var encrypdedDes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new DesParameters(iv, key)));
            return encrypdedDes;
        }

        public static byte[] RSADecrypt(byte[] byteDecrypt, RSAParameters RSAInfo, bool isOAEP = false)
        {
            try
            {
                byte[] decryptedData;
                using (RSACryptoServiceProvider RSA = new RSACryptoServiceProvider())
                {
                    RSA.ImportParameters(RSAInfo);
                    decryptedData = RSA.Decrypt(byteDecrypt, isOAEP);
                }
                return decryptedData;
            }
            catch (CryptographicException e)
            {
                return null;
            }
        }

        public static byte[] SymmetricEncrypt(string strText, SymmetricAlgorithm key)
        {
            MemoryStream ms = new MemoryStream();
            CryptoStream crypstream = new CryptoStream(ms, key.CreateEncryptor(), CryptoStreamMode.Write);
            StreamWriter sw = new StreamWriter(crypstream);
            sw.WriteLine(strText);
            sw.Close();
            crypstream.Close();
            byte[] buffer = ms.ToArray();
            ms.Close();

            return buffer;
        }

        public static string SymmetricDecrypt(byte[] encryptText, SymmetricAlgorithm key)
        {
            MemoryStream ms = new MemoryStream(encryptText);
            CryptoStream crypstream = new CryptoStream(ms, key.CreateDecryptor(), CryptoStreamMode.Read);
            StreamReader sr = new StreamReader(crypstream);
            string val = sr.ReadToEnd();
            sr.Close();
            crypstream.Close();
            ms.Close();

            return val;
        }
    }
}
