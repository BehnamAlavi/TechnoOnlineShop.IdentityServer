using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System;
using System.Collections.Generic;
using System.Text;

namespace TechnoOnlineShop.IdentityServer.Infrastructure.EF
{
    public sealed class IdentityProviderDbContextFactory
    : IDesignTimeDbContextFactory<IdentityProviderDbContext>
    {
        public IdentityProviderDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder =
                new DbContextOptionsBuilder<IdentityProviderDbContext>();

            optionsBuilder.UseSqlServer(
                "Server=localhost,1433;Initial Catalog=TechnoOnlineShopIdentity;User ID=sa;Password=YourStrongPassword123!;Encrypt=false;TrustServerCertificate=True");

            return new IdentityProviderDbContext(optionsBuilder.Options);
        }
    }
}
