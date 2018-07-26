using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace #%namespace%#.Business
{
    public class MD5Helper
    {
        public static string ComputeMD5Hash(string text)
        {
            var bytes = new UTF8Encoding().GetBytes(text);
            var hash = MD5.Create().ComputeHash(bytes);
            return BitConverter.ToString(hash).Replace("-", "");
        }
    }
}
