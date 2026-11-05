using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechnoOnlineShop.IdentityServer.Application;
using TechnoOnlineShop.IdentityServer.Application.Contracts;
using TechnoOnlineShop.IdentityServer.Application.Contracts.AppEvents;


namespace TechnoOnlineShop.IdentityServer.Api.Controllers
{
   
    [ApiController]
    [Route("api/token")]
    public sealed class TokenController : ControllerBase
    {
        private readonly ISmsChallengeService _smsChallengeService;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly ILogger<IdentityController> _logger;

        public TokenController(IJwtTokenService jwtTokenService, ILogger<IdentityController> logger)
        {
            _logger = logger;
            _jwtTokenService = jwtTokenService;
        }

        [HttpPost]
        [Route("iss")]
        public async Task<IActionResult> IssueToken([FromBody] IssueTokenCommand issueTokenCommand, CancellationToken cancellationToken)
        {
            var token = await _jwtTokenService.GenerateToken(issueTokenCommand.EnrollmentId.ToString(), issueTokenCommand.UserName, new List<string>() { "orders.create", "orders.read" }).ConfigureAwait(false);

            var tokenRes = new IssueTokenResponse(token, null, 0, null);
           // _logger.LogInformation($"Token:{tokenRes}");
            return new JsonResult(tokenRes);


        }
    }
}
