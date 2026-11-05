using System;
using System.Collections.Generic;
using System.Text;

namespace TechnoOnlineShop.IdentityServer.Application.Contracts.Options
{
    public class JwtOptions
    {
        public string Issuer { get; set; }
        public string Audience { get; set; }
        public string Key { get; set; }
        public string PrivateKeyPath { get; set; }

    }
}
