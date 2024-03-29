﻿using System.Collections.Generic;
using System.Threading.Tasks;
using MatchesData;
using MatchesData.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MatchesAPI.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [Authorize]
    public class TeamController : ControllerBase
    {
        private readonly MatchesDbContext _matchesDbContext;

        public TeamController(MatchesDbContext matchesDbContext)
        {
            _matchesDbContext = matchesDbContext;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Team>>> GetAsync()
        {
            return await _matchesDbContext.Teams.ToListAsync();
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(Team), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetAsync(int id)
        {
            var team = await _matchesDbContext.Teams.SingleOrDefaultAsync(t => t.Id == id);

            if (team == null)
            {
                return NotFound();
            }

            return Ok(team);
        }
    }
}