namespace TechnoOnlineShop.IdentityServer.Api.Controllers
{
    public sealed record IssueSmsVerificationCodeRequest(Guid EnrollmentId, string PhoneNumber);

    public sealed record IssueSmsVerificationCodeResponse(Guid AuthenticationTransactionId, DateTimeOffset ExpiresAt);

    public sealed record IssueCodeVerification(Guid EnrollmentId, string VerificationCode);


    public sealed record IssueTokenCommand(Guid EnrollmentId, string? UserName, string? PhoneNumber);

    public sealed record IssueTokenResponse(
        string AccessToken,
        string TokenType,
        int ExpiresIn,
        string? RefreshToken);
}
