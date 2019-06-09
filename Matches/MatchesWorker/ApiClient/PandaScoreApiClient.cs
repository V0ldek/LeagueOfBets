using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using MatchesData.Entities;
using MatchesData.Entities.Enums;
using Newtonsoft.Json;

namespace MatchesWorker.ApiClient
{
    internal class PandaScoreApiClient : IApiClient
    {
        private readonly PandaScoreConfiguration _configuration;
        private readonly HttpClient _httpClient = new HttpClient();

        public PandaScoreApiClient(PandaScoreConfiguration configuration)
        {
            _configuration = configuration;
            _httpClient.BaseAddress = new Uri($"{configuration.ApiUri}");
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _configuration.AppSecret);
        }

        public async Task<IEnumerable<Match>> FetchMatchesBetweenAsync(DateTime fetchSince, DateTime fetchTo)
        {
            var responses = await GetPagesAsync(
                $"/leagues/{_configuration.LeagueId}/" +
                $"matches?range[begin_at]={fetchSince:yyyy-MM-dd},{fetchTo:yyyy-MM-dd}");
            return responses.SelectMany(ParseMatches);
        }

        private async Task<IEnumerable<string>> GetPagesAsync(string url)
        {
            var responses = new List<string>();

            var firstResponse = await GetAsync(url);
            responses.Add(await firstResponse.Content.ReadAsStringAsync());

            for (var i = 2; i <= GetNumberOfPages(firstResponse); ++i)
            {
                responses.Add(await (await GetAsync($"{url}&page={i}")).Content.ReadAsStringAsync());
            }

            return responses;
        }

        private static int GetNumberOfPages(HttpResponseMessage response)
        {
            var headers = response.Headers.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            var total = int.Parse(headers["X-Total"].Single());
            var perPage = int.Parse(headers["X-Per-Page"].Single());
            return (perPage + total - 1) / total;
        }

        private async Task<HttpResponseMessage> GetAsync(string url)
        {
            var response = await _httpClient.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                return response;
            }

            throw new InvalidOperationException(
                $"PandaScore responded with code {response.StatusCode}. " +
                $"Response body:\n {await response.Content.ReadAsStringAsync()}");
        }

        private static IEnumerable<Match> ParseMatches(string jsonResponse)
        {
            dynamic json = JsonConvert.DeserializeObject(jsonResponse);
            var result = new List<Match>();

            foreach (var entry in json)
            {
                result.Add(ParseMatch(entry));
            }

            return result;
        }

        private static Match ParseMatch(dynamic json)
        {
            var blueParticipation = new MatchParticipation
            {
                Side = Side.Blue,
                TeamId = json.opponents[0].opponent.id,
                MatchId = json.id
            };
            var redParticipation = new MatchParticipation
            {
                Side = Side.Red,
                TeamId = json.opponents[1].opponent.id,
                MatchId = json.id
            };
            var match = new Match
            {
                Id = json.id,
                BlueScore = json.results[0].team_id == blueParticipation.TeamId
                    ? json.results[0].score
                    : json.results[1].score,
                RedScore = json.results[0].team_id == redParticipation.TeamId
                    ? json.results[0].score
                    : json.results[1].score,
                Format = json.number_of_games,
                IsFinished = json.end_at != null,
                Participations = new List<MatchParticipation> {blueParticipation, redParticipation},
                StartDateTime = json.begin_at
            };

            return match;
        }
    }
}