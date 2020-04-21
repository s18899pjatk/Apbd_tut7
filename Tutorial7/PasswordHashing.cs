using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Tutorial7
{
    public class PasswordHashing
    {
        private string _password;
        private string _salt;
        public PasswordHashing(string password, string salt)
        {
            _password = password;
            _salt = salt;
        }
        public bool CheckHash()
        {
            
            var str = Create(_password, _salt);
            if (!Validate(_password, _salt, str))
            {
                return false;
            }
            return true;
        }

        private static string Create(string value, string salt)
        {
            var valueBytes = KeyDerivation.Pbkdf2(
                                               password: value,
                                               salt: Encoding.UTF8.GetBytes(salt),
                                               prf: KeyDerivationPrf.HMACSHA512,
                                               iterationCount: 10000,
                                               numBytesRequested: 256 / 8);
            return Convert.ToBase64String(valueBytes);
        }

        public static bool Validate(string value, string salt, string hash)
            => Create(value, salt) == hash;

       /* private static string GenerateSalt()
        {
            byte[] randomBytes = new byte[128 / 8];
            using (var generator = RandomNumberGenerator.Create())
            {
                generator.GetBytes(randomBytes);
                return Convert.ToBase64String(randomBytes);
            }
        }*/
    }
}
