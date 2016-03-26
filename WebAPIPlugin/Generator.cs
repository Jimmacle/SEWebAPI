using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebAPIPlugin
{
    public class Generator
    {
        /// <summary>
        /// Generates a pseudo-random alphanumeric key of specified length.
        /// </summary>
        /// <param name="length"></param>
        /// <returns></returns>
        public static string GenerateWeakKey(int length)
        {
            string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            StringBuilder sb = new StringBuilder();
            Random r = new Random();
            for (int i = 0; i < length; i++)
            {
                sb.Append(chars[r.Next(0, chars.Length - 1)]);
            }

            return sb.ToString();
        }
    }
}
