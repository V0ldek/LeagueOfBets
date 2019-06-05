using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
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
            /*var client = _clientFactory.CreateClient("Users");
            var response = await client.GetAsync("/api/values");

            if (response.IsSuccessStatusCode)
            {
                Values = await response.Content.ReadAsStringAsync();
            }
            else
            {
                _logger.LogWarning(response.ToString());
            }

            var payload = new
            {
                email = "Twoja_stara",
                password = "passwordTwojejStarej"
            };
            var registerResponse = await client.PostAsync(
                "/register", new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json"));

            RegisterStatusCode = (int) registerResponse.StatusCode;*/

            return Page();
        }
    }
}
