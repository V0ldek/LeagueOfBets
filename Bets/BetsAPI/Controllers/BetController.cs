using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BetsData;
using BetsData.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BetsAPI.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [Authorize]
    public class BetController : ControllerBase
    {
        private readonly BetsDbContext _betsDbContext;

        public BetController(BetsDbContext betsDbContext)
        {
            _betsDbContext = betsDbContext;
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

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(Bet), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetAsync(int id)
        {
            var bet = await _betsDbContext.Bets.SingleOrDefaultAsync(b => b.Id == id);

            if (bet == null)
            {
                return NotFound();
            }

            var subject = User.GetSubjectClaim();

            if (bet.UserId != subject)
            {
                return Unauthorized();
            }

            return Ok(bet);
        }

        [HttpPut]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> PutAsync(string userId, int stakeId, uint amount)
        {
            var subject = User.GetSubjectClaim();

            if (userId != subject)
            {
                return Unauthorized();
            }

            var stake = await _betsDbContext.Stakes
                .Include(s => s.Match)
                .SingleOrDefaultAsync(s => s.Id == stakeId);

            if (stake == null)
            {
                return BadRequest($"Specified stake ${stakeId} does not exist.");
            }

            if (stake.Match.IsFinished)
            {
                return BadRequest($"Match ${stake.MatchId} already finished.");
            }

            var bet = new Bet
            {
                Amount = amount,
                Stake = stake,
                UserId = userId
            };
            _betsDbContext.Bets.Add(bet);

            await _betsDbContext.SaveChangesAsync();

            return Ok();
        }
    }
}
