using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BetsData;
using BetsData.Entities;
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
    public class BetController : ControllerBase
    {
        private readonly BetsDbContext _betsDbContext;
        private readonly ILogger<BetController> _logger;

        public BetController(BetsDbContext betsDbContext, ILogger<BetController> logger)
        {
            _betsDbContext = betsDbContext;
            _logger = logger;
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<Bet>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetAsync(string userId)
        {
            var subject = User.GetSubjectClaim();

            if (userId != subject)
            {
                return Unauthorized();
            }

            return Ok(await _betsDbContext.Bets.Where(b => b.UserId == userId).ToListAsync());
        }

        [HttpPut]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> PutAsync(Bet bet)
        {
            _logger.LogInformation($"Bet:\n{JsonConvert.SerializeObject(bet)}");

            var subject = User.GetSubjectClaim();

            if (bet.UserId != subject)
            {
                _logger.LogError($"Unauthorized claim {subject} for userId = {bet.UserId}.");
                return Unauthorized();
            }

            var stake = await _betsDbContext.Stakes
                .Include(s => s.Match)
                .SingleOrDefaultAsync(s => s.Id == bet.StakeId);

            if (stake == null)
            {
                return BadRequest($"Specified stake ${bet.StakeId} does not exist.");
            }

            if (stake.Match.IsFinished)
            {
                return BadRequest($"Match ${stake.MatchId} already finished.");
            }

            _betsDbContext.Bets.Add(bet);

            await _betsDbContext.SaveChangesAsync();

            return Ok();
        }
    }
}
