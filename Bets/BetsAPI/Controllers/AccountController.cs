using System.Threading.Tasks;
using BetsData;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BetsAPI.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [Authorize]
    public class AccountController : ControllerBase
    {
        private readonly BetsDbContext _betsDbContext;

        public AccountController(BetsDbContext betsDbContext)
        {
            _betsDbContext = betsDbContext;
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(long), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<long>> GetAsync(string id)
        {
            var subject = User.GetSubjectClaim();

            if (subject != id)
            {
                return Unauthorized();
            }

            var account = await _betsDbContext.Accounts.SingleOrDefaultAsync(a => a.UserId == id);

            return account?.Balance ?? (await _betsDbContext.GetAccountConfigurationAsync()).BaseBalance;
        }
    }
}
