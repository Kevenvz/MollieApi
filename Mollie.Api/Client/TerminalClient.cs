﻿using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Mollie.Api.Client.Abstract;
using Mollie.Api.Extensions;
using Mollie.Api.Models.List;
using Mollie.Api.Models.Terminal;
using Mollie.Api.Models.Url;

namespace Mollie.Api.Client
{    /// <summary>
     /// Calls in this class are documented in https://docs.mollie.com/reference/v2/terminals-api/overview
     /// </summary>
    public class TerminalClient : BaseMollieClient, ITerminalClient
    {
        public TerminalClient(string apiKey, HttpClient httpClient = null) : base(apiKey, httpClient) { }
        /// <summary>
        /// retrieve the data for one specific terminal
        /// </summary>
        /// <param name="terminalId">id of the specific terminal</param>
        /// <returns></returns>
        public async Task<TerminalResponse> GetTerminalAsync(string terminalId) {
            return await this.GetAsync<TerminalResponse>($"terminals/{terminalId}").ConfigureAwait(false);
        }
        /// <summary>
        /// retrieve all data for all terminals
        /// </summary>
        /// <param name="from">used for pagination</param>
        /// <param name="limit">used for pagination</param>
        /// <param name="profileId"> profile that requests the ListResponse element</param>
        /// <returns></returns>
        public async Task<ListResponse<TerminalResponse>> GetTerminalListAsync(string from = null, int? limit = null, string profileId = null) {
            var queryParameters = this.BuildQueryParameters(
                profileId: profileId);
            return await this.GetListAsync<ListResponse<TerminalResponse>>("terminals", from, limit, queryParameters).ConfigureAwait(false);
        }
        
        public async Task<ListResponse<TerminalResponse>> GetTerminalListAsync(UrlObjectLink<ListResponse<TerminalResponse>> url) {
            return await this.GetAsync(url).ConfigureAwait(false);
        }
        
        private Dictionary<string, string> BuildQueryParameters(string profileId = null)
        {
            var result = new Dictionary<string, string>();
            result.AddValueIfNotNullOrEmpty(nameof(profileId), profileId);
            return result;
        }
    }
}