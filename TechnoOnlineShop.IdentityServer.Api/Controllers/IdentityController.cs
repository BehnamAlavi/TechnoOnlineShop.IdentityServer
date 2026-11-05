using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechnoOnlineShop.IdentityServer.Application;
using TechnoOnlineShop.IdentityServer.Application.Contracts;
using TechnoOnlineShop.IdentityServer.Application.Contracts.AppEvents;


namespace TechnoOnlineShop.IdentityServer.Api.Controllers
{
   
    [ApiController]
    [Route("api/identity")]
    public sealed class IdentityController : ControllerBase
    {
        private readonly ISmsChallengeService _smsChallengeService;
        private readonly IJwtTokenService jwtTokenService;
        private readonly ILogger<IdentityController> _logger;

        public IdentityController(
            ISmsChallengeService smsChallengeService ,

            ILogger<IdentityController> logger
            //IIdentityTokenService identityTokenService

            )
        {
            _smsChallengeService = smsChallengeService;
            _logger = logger;
            _logger.LogInformation("inside the IdentityController");
            //_identityTokenService = identityTokenService;
        }

        [HttpPost]
        [Route("smscde")]
        public async Task<ActionResult<IssueSmsVerificationCodeResponse>> IssueTheSmsVerfCode([FromBody] IssueSmsVerificationCodeRequest command)
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            var cts = new CancellationTokenSource();

            //_logger.LogInformation("inside the smscde, before calling _smsChallengeService.RequestAsync");

            var result = await _smsChallengeService.RequestAsync(
                command.EnrollmentId,
                command.PhoneNumber,
                cts.Token);

            return Ok(result);
        }


        [HttpPost]
        [Route("vrfcde")]
        public async Task<ActionResult<IssueSmsVerificationCodeResponse>> VerifyCode([FromBody] IssueCodeVerification command)
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            var cts = new CancellationTokenSource();

            //_logger.LogInformation("inside the VerifyCode, before calling _smsChallengeService.RequestAsync");

            var result = await _smsChallengeService.VerifyAsync(
                command.EnrollmentId,
                command.VerificationCode,
                cts.Token);

            return Ok(result);
        }

    }
}
