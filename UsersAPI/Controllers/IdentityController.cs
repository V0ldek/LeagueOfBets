using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using UsersAPI.Data;
using UsersAPI.Model;

namespace UsersAPI.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class IdentityController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<IdentityController> _logger;
        private readonly UsersContext _context;

        public IdentityController(UserManager<ApplicationUser> userManager, ILogger<IdentityController> logger, UsersContext context)
        {
            _userManager = userManager;
            _logger = logger;
            _context = context;
        }

        [HttpPost("/register")]
        public async Task<IActionResult> RegisterAsync(RegisterModel registerModel)
        {
            _logger.LogInformation($"Received request {{email = {registerModel.Email}, password = {registerModel.Password}}}.");
            var user = new ApplicationUser
            {
                UserName = registerModel.Email,
                Email = registerModel.Email
            };

            var result = await _userManager.CreateAsync(user, registerModel.Password);

            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            _logger.LogInformation($"User {registerModel.Email} created successfully.");

            var userInDb = await _context.Users.SingleAsync(u => u.Email == registerModel.Email);

            _logger.LogInformation($"User in db:\n {JsonConvert.SerializeObject(userInDb)}");

            return Ok();
        }
    }
}
