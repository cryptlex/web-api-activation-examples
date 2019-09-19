using System;
using System.Text;
using System.Security.Cryptography;
using System.IO;
using Microsoft.IdentityModel.Tokens;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Security;

namespace Cryptlex
{
    public class LexEncryptionService
    {
        public static string Sha256(string rawData)
        {
            // Create a SHA256   
            using (SHA256 sha256Hash = SHA256.Create())
            {
                // ComputeHash - returns byte array  
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));

                // Convert byte array to a string   
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }

        public static RsaSecurityKey PublicKeyFromPem(string publicKeyPem)
        {
            PemReader pemReader = new PemReader(new StringReader(publicKeyPem));
            RsaKeyParameters publicKeyParameters = (RsaKeyParameters)pemReader.ReadObject();
            RSAParameters rsaParameters = DotNetUtilities.ToRSAParameters(publicKeyParameters);
            return new RsaSecurityKey(rsaParameters);
        }
    }

    class PasswordFinder : IPasswordFinder
    {
        private string _password;

        public PasswordFinder(string password)
        {
            this._password = password;
        }


        public char[] GetPassword()
        {
            return _password.ToCharArray();
        }
    }
}