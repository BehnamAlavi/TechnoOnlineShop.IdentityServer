using System.ComponentModel.DataAnnotations;

namespace TechnoOnlineShop.IdentityServer.Domain
{
    //public enum AuthenticationTransactionStatus
    //{
    //    Pending,
    //    SmsVerified,
    //    ReadyToIssue,
    //    Issuing,
    //    Issued,
    //    Failed
    //}


    public class SmsChallenge //: EntityBase<Guid>
    {
        public Guid Id { get; set; }
        public string? SubjectId { get; set; } = null!;
        public string? PhoneNumber { get; set; } = null!;
        public string? CodeMac { get; set; } = null!;
        public DateTimeOffset? ExpiresAt { get; set; }
        public int? FailedAttempts { get; set; }
        public DateTimeOffset? IssuedAt { get; set; }
        public DateTimeOffset? VerifiedAt { get; set; }

    }

    public class SigningKey //: EntityBase<Guid>
    {
        public Guid Id { get; set; }

        public string KeyId { get; set; } = string.Empty;

        public string Algorithm { get; set; } = "RS256";

        public string PublicKeyPath { get; set; } = string.Empty;

        public string PrivateKeyPath { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }

        public DateTime? RetiredAt { get; set; }

        public bool IsActive { get; set; }
    }

}
