using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;
using TechnoOnlineShop.IdentityServer.Domain;

namespace TechnoOnlineShop.IdentityServer.Infrastructure.EF
{
    public sealed class IdentityProviderDbContext(DbContextOptions<IdentityProviderDbContext> options)
     : DbContext(options)
    {
        public DbSet<SmsChallenge> SmsChallenges => Set<SmsChallenge>();
        public DbSet<SigningKey> SigningKeys => Set<SigningKey>();
        //public DbSet<EnrollmentReadiness> EnrollmentReadiness => Set<EnrollmentReadiness>();
        //public DbSet<AuthenticationTransaction> AuthenticationTransactions => Set<AuthenticationTransaction>();
        //public DbSet<ProcessedIntegrationEvent> ProcessedIntegrationEvents => Set<ProcessedIntegrationEvent>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            //modelBuilder.Entity<SmsChallenge>(b =>
            //{
            //    b.HasKey(x => x.Id);
            //    b.Property(x => x.CodeMac).HasMaxLength(32);
            //});


        }
    }
}
