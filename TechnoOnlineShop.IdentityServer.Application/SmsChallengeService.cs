using System;
using System.Collections.Generic;
using System.Text;

namespace TechnoOnlineShop.IdentityServer.Application
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Storage;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Microsoft.IdentityModel.Tokens;
    using System.ComponentModel.Design;
    using System.Security.Cryptography;
    using System.Text;
    using TechnoOnlineShop.IdentityServer.Application.Contracts;
    using TechnoOnlineShop.IdentityServer.Application.Contracts.AppEvents;
    using TechnoOnlineShop.IdentityServer.Domain;
    using TechnoOnlineShop.IdentityServer.Infrastructure.EF;



    public sealed class SmsChallengeService : ISmsChallengeService
    {
        private readonly IdentityProviderDbContext _db;
        private readonly byte[] _hmacKey;
        private readonly TimeProvider _timeProvider;
        private readonly ILogger<SmsChallengeService> _logger;

        public SmsChallengeService(
            IdentityProviderDbContext db,
            IOptions<SmsChallengeOptions> options,
            TimeProvider timeProvider,
            ILogger<SmsChallengeService> logger)
        {
            _db = db;
            //_hmacKey = Convert.FromBase64String(options.Value.HmacKeyBase64);
            _timeProvider = timeProvider;
            _logger = logger;

            //if (_hmacKey.Length < 32)
            //    throw new InvalidOperationException(
            //        "The SMS HMAC key must be at least 32 bytes.");
        }

        public async Task<SmsChallengeResult> RequestAsync(Guid enrollmentId, string phoneNumber, CancellationToken cancellationToken)
        {

            SmsChallengeResult result = new SmsChallengeResult(enrollmentId, DateTimeOffset.MinValue, null, null);

            if (string.IsNullOrEmpty(phoneNumber))
            {
                throw new ArgumentNullException(nameof(phoneNumber));
            }

            try
            {
                var chalng = await _db.SmsChallenges.AsNoTracking().FirstOrDefaultAsync(itm => itm.Id.Equals(enrollmentId)).ConfigureAwait(false);

                if (chalng == null)
                {
                    var newItem = await CreateSmsChallengeAfterSending(enrollmentId, phoneNumber, null, cancellationToken).ConfigureAwait(false);
                    await _db.SmsChallenges.AddAsync(newItem).ConfigureAwait(false);
                    await _db.SaveChangesAsync().ConfigureAwait(false);
                    result = new SmsChallengeResult(enrollmentId, newItem.ExpiresAt.Value, null, !string.IsNullOrEmpty(newItem.CodeMac));
                    return result;
                }
                else
                {
                    _logger.LogInformation($"chalng exists");
                    if (chalng.ExpiresAt < DateTimeOffset.Now)
                    {
                        _logger.LogInformation($"expires at:{chalng.ExpiresAt}");
                        if (chalng.VerifiedAt.HasValue)
                        {
                            //TODO:Do nothing
                            result = new SmsChallengeResult(enrollmentId, chalng.VerifiedAt.Value, true, false);
                            return result;

                        }
                        else if (chalng.FailedAttempts == 1)
                            {
                            _logger.LogInformation($"FailedAttempts:{chalng.FailedAttempts}");
                            //TODO:if suc, update the code and minus failedattempt, then save the changes.
                            var toBeUpdated = await CreateSmsChallengeAfterSending(enrollmentId, phoneNumber, chalng, cancellationToken);
                                result = new SmsChallengeResult(enrollmentId, toBeUpdated.ExpiresAt.Value, null, false);
                                return result;
                            }
                        else
                        {
                            result = new SmsChallengeResult(enrollmentId, chalng.ExpiresAt.Value, false, !string.IsNullOrEmpty(chalng.CodeMac));
                            return result;
                        }
                    }
                    else
                    {
                        _logger.LogInformation($"chalng.ExpiresAt > now");
                        result = new SmsChallengeResult(enrollmentId, chalng.ExpiresAt.Value, false, !string.IsNullOrEmpty(chalng.CodeMac));
                        return result;
                    }

                }
               
            }
            catch (Exception e)
            {
                _logger.LogError(e, "ERROR in sending: {param}", e.Message);
            }

            return result;
        }


        public async Task<SmsChallenge> CreateSmsChallengeAfterSending(Guid enrollmentId, string phoneNumber, SmsChallenge? smsChallenge, CancellationToken cancellationToken)
        {
            //SmsChallengeResult result = new SmsChallengeResult(enrollmentId, DateTimeOffset.MinValue, null);

            var code = SmsCodeGenerator.Generate();

            bool successful = await SendSMS(phoneNumber);
            //TODO: Calling of SMS service, waiting for successful sending, when we feel confident, adding it into the db. 
            if (successful)
            {
                if (smsChallenge == null)
                {
                    var expireAt = DateTime.UtcNow.AddMinutes(2);
                    var newItem = new SmsChallenge { Id = enrollmentId, ExpiresAt = expireAt, IssuedAt = DateTimeOffset.UtcNow, CodeMac = code };
                    return newItem;
                }
                else //FailedAttempt issue,which the newitem should consider setting.
                {
                    smsChallenge.FailedAttempts--;
                    smsChallenge.CodeMac = code;
                    return smsChallenge;
                }
            }
            else
            {
                if (smsChallenge == null)
                {
                    var newItem = new SmsChallenge { Id = enrollmentId, ExpiresAt = DateTimeOffset.MinValue, IssuedAt = DateTimeOffset.Now, FailedAttempts = 1 };
                    return newItem;
                }
                else
                {
                    smsChallenge.FailedAttempts++;
                    smsChallenge.CodeMac = null;
                    return smsChallenge;
                }
            }

        }

        private async Task<bool> SendSMS(string phoneNumber)
        {
            return true;
        }

        public async Task<SmsVerificationStatus> VerifyAsync(Guid transactionId, string submittedCode, CancellationToken cancellationToken)
        {
            if (transactionId == Guid.Empty || string.IsNullOrWhiteSpace(submittedCode) || submittedCode.Length is < 6 or > 8 ||
             submittedCode.Any(c => c is < '0' or > '9'))
            {
                return SmsVerificationStatus.InvalidOrExpired;
            }

            // Serialize concurrent attempts for this challenge.
            await using var dbTransaction = await _db.Database.BeginTransactionAsync(
                //System.Data.IsolationLevel.Serializable,
                cancellationToken);

            //var challenge = await _db.SmsChallenges
            //    .SingleOrDefaultAsync(
            //        x => x.AuthenticationTransactionId == transactionId,
            //        cancellationToken);

            //if (challenge is null)
            //    return SmsVerificationStatus.TransactionNotFound;

            //var now = _timeProvider.GetUtcNow();

            //if (challenge.LockedUntil is { } lockedUntil && lockedUntil > now)
            //    return SmsVerificationStatus.RateLimited;

            //if (challenge.ExpiresAt <= now || challenge.ConsumedAt is not null)
            //    return SmsVerificationStatus.TransactionExpired;

            //// An already verified challenge is idempotently verified. This is
            //// safe only when the transaction is accessible to trusted BFF callers
            //// and its server-side state remains authoritative.
            //if (challenge.VerifiedAt is not null)
            //{
            //    await dbTransaction.CommitAsync(cancellationToken);
            //    return SmsVerificationStatus.Verified;
            //}

            //if (challenge.AttemptsRemaining <= 0)
            //    return SmsVerificationStatus.RateLimited;

            //var submittedMac = HMACSHA256.HashData(
            //    _hmacKey, Encoding.UTF8.GetBytes(submittedCode));

            //if (!CryptographicOperations.FixedTimeEquals(
            //        challenge.CodeMac, submittedMac))
            //{
            //    challenge.AttemptsRemaining--;

            //    if (challenge.AttemptsRemaining == 0)
            //    {
            //        challenge.LockedUntil = now.AddMinutes(15);
            //    }

            //    await _db.SaveChangesAsync(cancellationToken);
            //    await dbTransaction.CommitAsync(cancellationToken);

            //    return challenge.AttemptsRemaining == 0
            //        ? SmsVerificationStatus.RateLimited
            //        : SmsVerificationStatus.InvalidOrExpired;
            //}

            //challenge.VerifiedAt = now;

            //// Persist verification atomically before returning success.
            //var authTransaction = await _db.AuthenticationTransactions
            //    .SingleOrDefaultAsync(
            //        x => x.Id == transactionId,
            //        cancellationToken);

            //if (authTransaction is null || authTransaction.ExpiresAt <= now)
            //    return SmsVerificationStatus.TransactionExpired;

            //authTransaction.SmsVerifiedAt = now;
            //authTransaction.Status = AuthenticationTransactionStatus.SmsVerified;

            //await _db.SaveChangesAsync(cancellationToken);
            //await dbTransaction.CommitAsync(cancellationToken);

            return SmsVerificationStatus.Verified;
        }
    }


    //public sealed class EnrollmentReadinessStore : IEnrollmentReadinessStore
    //{
    //    private readonly IdentityProviderDbContext _db;

    //    public EnrollmentReadinessStore(IdentityProviderDbContext db)
    //    {
    //        _db = db;
    //    }

    //    public async Task<bool> IsEnrollmentCompleteAsync(
    //        Guid transactionId,
    //        CancellationToken cancellationToken)
    //    {
    //        var transaction = await _db.AuthenticationTransactions
    //            .AsNoTracking()
    //            .Where(x => x.Id == transactionId)
    //            .Select(x => new
    //            {
    //                x.EnrollmentId,
    //                x.IdentitySubject,
    //                x.ExpiresAt
    //            })
    //            .SingleOrDefaultAsync(cancellationToken);

    //        if (transaction is null || transaction.ExpiresAt <= DateTimeOffset.UtcNow)
    //            return false;

    //        return await _db.EnrollmentReadiness
    //            .AsNoTracking()
    //            .AnyAsync(
    //                x => x.EnrollmentId == transaction.EnrollmentId
    //                     && x.IdentitySubject == transaction.IdentitySubject,
    //                cancellationToken);
    //    }



    //public async Task HandleEnrollmentCompletedAsync(
    //UserEnrollmentCompleted message,
    //CancellationToken cancellationToken)
    //    {
    //        await using var tx = await _db.Database.BeginTransactionAsync(
    //            cancellationToken);

    //        var alreadyProcessed = await _db.ProcessedIntegrationEvents
    //            .AnyAsync(x => x.EventId == message.EventId, cancellationToken);

    //        if (alreadyProcessed)
    //            return;

    //        var readiness = await _db.EnrollmentReadiness
    //            .SingleOrDefaultAsync(
    //                x => x.EnrollmentId == message.EnrollmentId,
    //                cancellationToken);

    //        if (readiness is null)
    //        {
    //            _db.EnrollmentReadiness.Add(new EnrollmentReadiness
    //            {
    //                EnrollmentId = message.EnrollmentId,
    //                IdentitySubject = message.IdentitySubject,
    //                EnrollmentVersion = message.EnrollmentVersion,
    //                CompletedAt = message.CompletedAt
    //            });
    //        }
    //        else if (message.EnrollmentVersion > readiness.EnrollmentVersion)
    //        {
    //            readiness.IdentitySubject = message.IdentitySubject;
    //            readiness.EnrollmentVersion = message.EnrollmentVersion;
    //            readiness.CompletedAt = message.CompletedAt;
    //        }

    //        _db.ProcessedIntegrationEvents.Add(new ProcessedIntegrationEvent
    //        {
    //            EventId = message.EventId,
    //            ProcessedAt = DateTimeOffset.UtcNow
    //        });

    //        await _db.SaveChangesAsync(cancellationToken);
    //        await tx.CommitAsync(cancellationToken);
    //    }

    //}
}
