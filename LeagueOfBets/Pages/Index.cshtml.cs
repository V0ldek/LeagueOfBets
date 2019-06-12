using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using LeagueOfBets.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace LeagueOfBets.Pages
{
    [Authorize]
    [AutoValidateAntiforgeryToken]
    public class IndexModel : PageModel
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly ILogger<IndexModel> _logger;

        public AccountViewModel Account { get; private set; }

        public IEnumerable<MatchViewModel> Matches { get; private set; }

        public Dictionary<int, List<StakeViewModel>> StakesByMatchId { get; private set; }

        [BindProperty]
        public int SelectedStakeId { get; set; }

        public IndexModel(IHttpClientFactory clientFactory, ILogger<IndexModel> logger)
        {
            _clientFactory = clientFactory;
            _logger = logger;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var token = await HttpContext.GetTokenAsync("access_token");
            var matchesClient = GetClient("Matches", token);
            var betsClient = GetClient("Bets", token);

            var accountTask = GetAccountAsync(betsClient);
            var matchesTask = GetMatchesAsync(matchesClient);
            var stakesTask = GetStakesByMatchIdAsync(betsClient);

            await Task.WhenAll(accountTask, matchesTask, stakesTask);

            Account = await accountTask;
            Matches = await matchesTask;
            StakesByMatchId = await stakesTask;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var token = await HttpContext.GetTokenAsync("access_token");
            var betsClient = GetClient("Bets", token);

            _logger.LogInformation($"Posting bet for subject {User.GetSubjectClaim()}");

            foreach (var claim in User.Claims)
            {
                _logger.LogInformation($"Claim:\n{claim.Type} = {claim.Value}");
            }

            var json = JsonConvert.SerializeObject(
                new
                {
                    userId = User.GetSubjectClaim(),
                    stakeId = SelectedStakeId,
                    amount = 100
                });
            var result = await betsClient.PutAsync(
                "/bet/",
                new StringContent(json, Encoding.UTF8, "application/json"));

            if (!result.IsSuccessStatusCode)
            {
                _logger.LogError(
                    $"Error posting a bet for stake {SelectedStakeId}, Bets responded with {result.StatusCode}.\n{result}");
            }

            return RedirectToPage();
        }

        private HttpClient GetClient(string name, string token)
        {
            var client = _clientFactory.CreateClient(name);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return client;
        }

        private async Task<AccountViewModel> GetAccountAsync(HttpClient betsClient)
        {
            var response = await betsClient.GetAsync($"/account/{User.GetSubjectClaim()}");

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError($"Bets service responded with {response.StatusCode}.\n{response}");
                return null;
            }

            if (!long.TryParse(await response.Content.ReadAsStringAsync(), out var balance))
            {
                _logger.LogError($"Unable to parse Bets service response as account balance.\n{response}");
            }

            return new AccountViewModel(User.Claims.FirstOrDefault(c => c.Type == "email")?.Value, balance);
        }

        private async Task<IEnumerable<MatchViewModel>> GetMatchesAsync(HttpClient matchesClient)
        {
            var response = await matchesClient.GetAsync($"/match");

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError($"Matches service responded with {response.StatusCode}.\n{response}");
                return null;
            }

            try
            {
                return await ParseMatchesAsync(response);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Error while parsing Matches service response.");
                return null;
            }
        }

        private async Task<IEnumerable<MatchViewModel>> ParseMatchesAsync(HttpResponseMessage response)
        {
            var json = await response.Content.ReadAsStringAsync();
            var matchesJson = (IEnumerable<dynamic>) JsonConvert.DeserializeObject(json);

            return matchesJson.Select(matchJson => (MatchViewModel) ParseMatch(matchJson)).ToList();
        }

        private MatchViewModel ParseMatch(dynamic matchJson)
        {
            var blueTeamJson = matchJson.participations[0].team;
            var redTeamJson = matchJson.participations[1].team;

            return new MatchViewModel(
                int.Parse((string) matchJson.id),
                (string) blueTeamJson.name,
                (string) redTeamJson.name,
                int.Parse((string) matchJson.blueScore),
                int.Parse((string) matchJson.redScore),
                int.Parse((string) matchJson.bestOf),
                (string) blueTeamJson.logoUrl,
                (string) redTeamJson.logoUrl,
                DateTime.Parse((string) matchJson.startDateTime)
            );
        }

        private async Task<Dictionary<int, List<StakeViewModel>>> GetStakesByMatchIdAsync(HttpClient betsClient)
        {
            var response = await betsClient.GetAsync($"/stake");

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError($"Bets service responded to /stake with {response.StatusCode}.\n{response}");
                return null;
            }

            try
            {
                return (await ParseStakesAsync(response)).GroupBy(s => s.MatchId)
                    .ToDictionary(g => g.Key, g => g.ToList());
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Error while parsing Bets/stake service response.");
                return null;
            }
        }

        private async Task<IEnumerable<StakeViewModel>> ParseStakesAsync(HttpResponseMessage response)
        {
            var json = await response.Content.ReadAsStringAsync();
            var stakesJson = (IEnumerable<dynamic>) JsonConvert.DeserializeObject(json);

            return stakesJson.Select(stakeJson => (StakeViewModel) ParseStake(stakeJson)).ToList();
        }

        private static StakeViewModel ParseStake(dynamic stakeJson) =>
            new StakeViewModel(
                int.Parse((string) stakeJson.id),
                int.Parse((string) stakeJson.matchId),
                int.Parse((string) stakeJson.blueScore),
                int.Parse((string) stakeJson.redScore),
                float.Parse((string) stakeJson.ratio));
    }
}