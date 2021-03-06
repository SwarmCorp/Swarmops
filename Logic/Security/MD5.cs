using System.Security.Cryptography;
using System.Text;

namespace Swarmops
{
    /// <summary>
    /// Summary description for MD5.
    /// </summary>
    public class MD5
    {
        public static string Hash (string input)
        {
            byte[] data = Encoding.GetEncoding(1252).GetBytes(input);

            System.Security.Cryptography.MD5 md5 = new MD5CryptoServiceProvider();
            byte[] hash = md5.ComputeHash(data);

            // Write the resulting hash to a string of hex values.

            var result = new StringBuilder();

            foreach (byte oneByte in hash)
            {
                result.Append(oneByte.ToString("X02") + " ");
            }

            return result.ToString().TrimEnd();
        }
    }
}