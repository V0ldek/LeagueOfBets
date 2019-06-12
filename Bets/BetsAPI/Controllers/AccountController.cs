using System;
using System.Security.Claims;
using System.Threading.Tasks;
using BetsData;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace BetsAPI.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [Authorize]
    public class AccountController : ControllerBase
    {
        private readonly BetsDbContext _betsDbContext;
        private readonly ILogger<AccountController> _logger;

        public AccountController(BetsDbContext betsDbContext, ILogger<AccountController> logger)
        {
            _betsDbContext = betsDbContext;
            _logger = logger;
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(long), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<long>> GetAsync(string id)
        {
            var subject = User.GetSubjectClaim();

            if (subject != id)
            {
                _logger.LogInformation($"User subject claim {subject} does not match requested id {id}.");
                return Unauthorized();
            }

            var account = await _betsDbContext.Accounts.SingleOrDefaultAsync(a => a.UserId == id);

            return account != null ? Convert.ToInt64(account.Balance) : (await _betsDbContext.GetAccountConfigurationAsync()).BaseBalance;
        }
    }
}
