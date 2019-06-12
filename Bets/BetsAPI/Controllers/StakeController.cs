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
    public class StakeController : ControllerBase
    {
        private readonly BetsDbContext _betsDbContext;

        public StakeController(BetsDbContext betsDbContext)
        {
            _betsDbContext = betsDbContext;
        }
        
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<Stake>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAsync()
        {
            var stakes = await _betsDbContext.Stakes.ToListAsync();
            return Ok(stakes.Select(s => new
            {
                s.Id,
                s.MatchId,
                s.BlueScore,
                s.RedScore,
                s.Ratio
            }));
        }
    }
}
