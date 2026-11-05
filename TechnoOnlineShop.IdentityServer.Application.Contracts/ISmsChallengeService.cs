using System;
using System.Collections.Generic;
using System.Text;
using TechnoOnlineShop.IdentityServer.Application.Contracts.AppEvents;

namespace TechnoOnlineShop.IdentityServer.Application.Contracts
{
    public sealed record SmsChallengeResult(Guid AuthenticationTransactionId, DateTimeOffset ExpiresAt, bool? alreadyConfimed, bool? codeSent);

    public interface ISmsChallengeService
    {
       Task<SmsChallengeResult> RequestAsync(Guid enrollmentId, string phoneNumber, CancellationToken cancellationToken);
       Task<SmsVerificationStatus> VerifyAsync(Guid challengeId, string submittedCode,CancellationToken cancellationToken);
    }


}
