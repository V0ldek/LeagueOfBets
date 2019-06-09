using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MatchesData.Entities;

namespace MatchesWorker.ApiClient
{
    internal interface IApiClient
    {
        Task<IEnumerable<Match>> FetchMatchesBetweenAsync(DateTime fetchSince, DateTime fetchTo);
    }
}