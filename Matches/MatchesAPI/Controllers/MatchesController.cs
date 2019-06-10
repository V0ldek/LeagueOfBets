using System.Collections.Generic;
using System.Threading.Tasks;
using MatchesData;
using MatchesData.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MatchesAPI.Controllers
{
    [Route("")]
    [Route("[controller]")]
    [ApiController]
    [Authorize]
    public class MatchesController : ControllerBase
    {
        private readonly MatchesDbContext _matchesDbContext;

        public MatchesController(MatchesDbContext matchesDbContext)
        {
            _matchesDbContext = matchesDbContext;
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<Match>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<Match>>> GetAsync() =>
            await _matchesDbContext.Matches.Include(m => m.Participations).ToListAsync();

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(Match), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetAsync(int id)
        {
            var match = await _matchesDbContext.Matches.SingleOrDefaultAsync(m => m.Id == id);

            if (match == null)
            {
                return NotFound();
            }

            return Ok(match);
        }
    }
}