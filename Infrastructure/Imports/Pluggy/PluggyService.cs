using Application.Abstractions.Pluggy;
using Application.Configuration.Pluggy;
using Application.DTOs.Auth;
using Application.DTOs.Banks;
using Domain.Entities.banks;
using Domain.ValueObjects;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;
using System.Text.Json;

namespace Infrastructure.Imports.Pluggy
{
    public class PluggyService : IPluggyService
    {
        private readonly HttpClient _httpClient;
        private readonly PluggySettings _settings;

        public PluggyService(
            HttpClient httpClient,
            IOptions<PluggySettings> options)
        {
            _httpClient = httpClient;
            _settings = options.Value;
        }

        public async Task<string> GetApiKeyAsync()
        {
            var response = await _httpClient.PostAsJsonAsync(
                "https://api.pluggy.ai/auth",
                new
                {
                    clientId = _settings.ClientId,
                    clientSecret = _settings.ClientSecret
                });

            var content = await response.Content.ReadAsStringAsync();

            return content;
        }

        public async Task<ConnectorsResponseDto> GetConnectorsAsync()
        {
            var apiKeyJson = await GetApiKeyAsync();

            var auth = JsonSerializer.Deserialize<AuthPluggy>(apiKeyJson,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("X-API-KEY", auth.ApiKey);

            var response = await _httpClient.GetAsync("https://api.pluggy.ai/connectors");

            var responseContent = await response.Content.ReadAsStringAsync();

            var result = JsonSerializer.Deserialize<ConnectorsResponseDto>(responseContent,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

            return result ?? new ConnectorsResponseDto();
        }

        public async Task<string> GetAccountsAsync(string externalConnectionId)
        {
            var apiKeyJson = await GetApiKeyAsync();

            var auth = JsonSerializer.Deserialize<AuthPluggy>(
                apiKeyJson,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

            _httpClient.DefaultRequestHeaders.Clear();

            _httpClient.DefaultRequestHeaders.Add(
                "X-API-KEY",
                auth!.ApiKey);

            var response = await _httpClient.GetAsync(
                $"https://api.pluggy.ai/accounts?itemId={externalConnectionId}");

            return await response.Content.ReadAsStringAsync();
        }

        public async Task<ConnectTokenResponse> CreateConnectTokenAsync(string clientUserId)
        {
            var apiKeyJson = await GetApiKeyAsync();

            var auth = JsonSerializer.Deserialize<AuthPluggy>(
                apiKeyJson,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

            _httpClient.DefaultRequestHeaders.Clear();

            _httpClient.DefaultRequestHeaders.Add(
                "X-API-KEY",
                auth!.ApiKey);

            var response = await _httpClient.PostAsJsonAsync(
                "https://api.pluggy.ai/connect_token",
                new
                {
                    clientUserId
                });

            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();

            var result = JsonSerializer.Deserialize<ConnectTokenDto>(
                json,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

            return new ConnectTokenResponse(
                result!.AccessToken
            );
        }
    }
}
