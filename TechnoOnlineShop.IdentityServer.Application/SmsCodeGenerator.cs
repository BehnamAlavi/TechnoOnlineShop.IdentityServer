using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace TechnoOnlineShop.IdentityServer.Application
{
    public class SmsCodeGenerator
    {
        public static string Generate()
        {
            var code = RandomNumberGenerator.GetInt32(0, 1_000_000);
            return code.ToString("D6");
        }
    }
}
