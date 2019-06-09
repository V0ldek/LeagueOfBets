using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace LeagueOfBets.Pages
{
    public class IndexModel : PageModel
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly ILogger<IndexModel> _logger;

        public string Values { get; private set; }
        public int RegisterStatusCode { get; private set; }

        public IndexModel(IHttpClientFactory clientFactory, ILogger<IndexModel> logger)
        {
            _clientFactory = clientFactory;
            _logger = logger;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var client = _clientFactory.CreateClient("Matches");
            var token = await HttpContext.GetTokenAsync("access_token");

            _logger.LogInformation(token);

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var response = await client.GetAsync("/values");

            if (response.IsSuccessStatusCode)
            {
                Values = await response.Content.ReadAsStringAsync();
            }
            else
            {
                _logger.LogWarning(response.ToString());
            }

            return Page();
        }
    }
}
