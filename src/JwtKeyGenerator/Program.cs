using System;
using System.Security.Cryptography;

namespace JwtKeyGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            int keySize = 64; // 64 bytes = 512 bits (good for HS256/HS512)
            string jwtSecret = GenerateSecureKey(keySize);

            Console.WriteLine("Your JWT Secret Key:");
            Console.WriteLine(jwtSecret);
            Console.WriteLine("\nKeep this key secret and store it securely!");
            Console.ReadLine();
        }

        static string GenerateSecureKey(int size)
        {
            var key = new byte[size];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(key);
            }
            return Convert.ToBase64String(key);
        }
    }
}
