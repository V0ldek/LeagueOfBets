using System;
using System.Threading.Tasks;
using BetsData;
using BetsData.Entities;
using BetsData.Entities.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BetsAPI.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [Authorize]
    public class StakeController : ControllerBase
    {
        private readonly BetsDbContext _betsDbContext;

        public StakeController(BetsDbContext betsDbContext)
        {
            _betsDbContext = betsDbContext;
        }

        [HttpGet]
        [ProducesResponseType(typeof(Stake), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetAsync(int matchId, string side, int losersScore)
        {
            if(!Enum.TryParse(side, out Side parsedSide))
            {
                return BadRequest($"Invalid parameter {nameof(side)}={side}.");
            }

            var stake = await _betsDbContext.Stakes.SingleOrDefaultAsync(
                s => s.MatchId == matchId && s.WinningSide == parsedSide && s.LosersScore == losersScore);

            if (stake == null)
            {
                return NotFound();
            }

            return Ok(stake);
        }
    }
}
