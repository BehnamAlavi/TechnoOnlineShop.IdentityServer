using System;
using System.Collections.Generic;
using System.Text;

namespace TechnoOnlineShop.IdentityServer.Application.Contracts.AppEvents
{
    public sealed record UserEnrollmentCompleted(Guid EventId, Guid EnrollmentId, string IdentitySubject, DateTimeOffset CompletedAt, long EnrollmentVersion);


    public enum SmsVerificationStatus
    {
        Verified,
        InvalidOrExpired,
        RateLimited,
        TransactionNotFound,
        TransactionExpired
    }

    public sealed class SmsChallengeOptions
    {
        // Base64-encoded random 32-byte secret.
        public required string HmacKeyBase64 { get; init; }
    }

    public interface ISmsChallengeStore
    {
        Task<SmsVerificationStatus> VerifyAndMarkVerifiedAsync(
            Guid transactionId,
            string submittedCode,
            CancellationToken cancellationToken);
    }

    public interface IEnrollmentReadinessStore
    {
        Task<bool> IsEnrollmentCompleteAsync(
            Guid transactionId,
            CancellationToken cancellationToken);
    }

    public interface IAuthenticationTransactionStore
    {
        Task<bool> IsExpiredAsync(
            Guid transactionId,
            CancellationToken cancellationToken);

        // Atomically reserves issuance so concurrent requests cannot
        // issue multiple independent token sets for the same transaction.
        Task<bool> TryBeginIssuanceAsync(
            Guid transactionId,
            CancellationToken cancellationToken);

        Task MarkIssuedAsync(
            Guid transactionId,
            CancellationToken cancellationToken);

        Task ReleaseIssuanceAsync(
            Guid transactionId,
            CancellationToken cancellationToken);
    }

    public interface IOidcTokenIssuer
    {
        Task<IssuedTokens> IssueForTransactionAsync(
            Guid transactionId,
            CancellationToken cancellationToken);
    }

    public sealed record IssuedTokens(
        string AccessToken,
        string? RefreshToken,
        int ExpiresIn);

}
