using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MatchesData;
using MatchesData.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace MatchesAPI.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [Authorize]
    public class MatchController : ControllerBase
    {
        private readonly MatchesDbContext _matchesDbContext;
        private readonly ILogger<MatchController> _logger;

        public MatchController(MatchesDbContext matchesDbContext, ILogger<MatchController> logger)
        {
            _matchesDbContext = matchesDbContext;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAsync()
        {
            var result = (await _matchesDbContext.Matches
                    .Include(m => m.Participations)
                    .ThenInclude(p => p.Team)
                    .ToListAsync())
                .Select(
                    m => new
                    {
                        m.Id,
                        m.BestOf,
                        m.BlueScore,
                        m.RedScore,
                        m.StartDateTime,
                        Participations = m.Participations.Select(
                            p => new
                            {
                                p.MatchId,
                                p.Side,
                                Team = new
                                {
                                    p.Team.Id,
                                    p.Team.Name,
                                    p.Team.LogoUrl
                                }
                            })
                    });

            _logger.LogInformation($"Returning matches:\n{JsonConvert.SerializeObject(result)}");

            return Ok(result);
        }


        [HttpGet("{id}")]
        [ProducesResponseType(typeof(Match), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetAsync(int id)
        {
            var match = await _matchesDbContext.Matches
                .Include(m => m.Participations)
                .ThenInclude(p => p.Team)
                .SingleOrDefaultAsync(m => m.Id == id);

            if (match == null)
            {
                return NotFound();
            }

            return Ok(match);
        }
    }
}