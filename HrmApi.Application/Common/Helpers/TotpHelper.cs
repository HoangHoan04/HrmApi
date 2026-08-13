using System.Security.Cryptography;
using System.Text;

namespace HrmApi.Application.Common.Helpers
{

    public static class TotpHelper
    {
        private const string Base32Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";

        public static string GenerateSecret(int numBytes = 20)
        {
            byte[] bytes = RandomNumberGenerator.GetBytes(numBytes);
            return ToBase32(bytes);
        }

        public static string BuildOtpAuthUri(string secret, string accountName, string issuer = "HRM")
        {
            string label = Uri.EscapeDataString($"{issuer}:{accountName}");
            string iss = Uri.EscapeDataString(issuer);
            return $"otpauth://totp/{label}?secret={secret}&issuer={iss}&algorithm=SHA1&digits=6&period=30";
        }

        public static bool VerifyCode(string secretBase32, string code, int window = 1)
        {
            if (string.IsNullOrWhiteSpace(secretBase32) || string.IsNullOrWhiteSpace(code))
                return false;

            string normalized = code.Trim().Replace(" ", "");
            if (normalized.Length != 6 || !normalized.All(char.IsDigit))
                return false;

            byte[] key;
            try
            {
                key = FromBase32(secretBase32.Trim().Replace(" ", "").ToUpperInvariant());
            }
            catch
            {
                return false;
            }

            long timestep = DateTimeOffset.UtcNow.ToUnixTimeSeconds() / 30;
            for (int i = -window; i <= window; i++)
            {
                string expected = ComputeTotp(key, timestep + i);
                if (CryptographicOperations.FixedTimeEquals(
                        Encoding.ASCII.GetBytes(expected),
                        Encoding.ASCII.GetBytes(normalized)))
                {
                    return true;
                }
            }

            return false;
        }

        private static string ComputeTotp(byte[] key, long timestep)
        {
            byte[] counter = BitConverter.GetBytes(timestep);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(counter);

            using HMACSHA1 hmac = new(key);
            byte[] hash = hmac.ComputeHash(counter);
            int offset = hash[^1] & 0x0F;
            int binary =
                ((hash[offset] & 0x7F) << 24)
                | ((hash[offset + 1] & 0xFF) << 16)
                | ((hash[offset + 2] & 0xFF) << 8)
                | (hash[offset + 3] & 0xFF);
            int otp = binary % 1_000_000;
            return otp.ToString("D6");
        }

        public static string ToBase32(byte[] data)
        {
            if (data.Length == 0) return string.Empty;
            StringBuilder sb = new((data.Length * 8 + 4) / 5);
            int buffer = data[0];
            int next = 1;
            int bitsLeft = 8;
            while (bitsLeft > 0 || next < data.Length)
            {
                if (bitsLeft < 5)
                {
                    if (next < data.Length)
                    {
                        buffer <<= 8;
                        buffer |= data[next++] & 0xFF;
                        bitsLeft += 8;
                    }
                    else
                    {
                        int pad = 5 - bitsLeft;
                        buffer <<= pad;
                        bitsLeft += pad;
                    }
                }

                int index = (buffer >> (bitsLeft - 5)) & 0x1F;
                bitsLeft -= 5;
                sb.Append(Base32Alphabet[index]);
            }

            return sb.ToString();
        }

        public static byte[] FromBase32(string input)
        {
            string cleaned = input.Trim().TrimEnd('=').ToUpperInvariant();
            if (cleaned.Length == 0) return [];

            List<byte> output = new((cleaned.Length * 5) / 8);
            int buffer = 0;
            int bitsLeft = 0;
            foreach (char c in cleaned)
            {
                int val = Base32Alphabet.IndexOf(c);
                if (val < 0)
                    throw new FormatException("Invalid Base32 character.");
                buffer = (buffer << 5) | val;
                bitsLeft += 5;
                if (bitsLeft >= 8)
                {
                    output.Add((byte)((buffer >> (bitsLeft - 8)) & 0xFF));
                    bitsLeft -= 8;
                }
            }

            return output.ToArray();
        }
    }
}
