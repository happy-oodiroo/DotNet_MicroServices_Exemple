using ApisClients.Common;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;

namespace StudentWebPortal.Client
{
    public class CustomAuthentificationStateProvider : Microsoft.AspNetCore.Components.Authorization.AuthenticationStateProvider
    {
        private readonly ILocalStorageService _localStorage;
        private readonly HttpClient _http;
        private readonly StudentInfo.ApiClient.StudentInfoApiServices _StudentInfoApiServices;
        public CustomAuthentificationStateProvider(ILocalStorageService localStorage, HttpClient http, StudentInfo.ApiClient.StudentInfoApiServices studentInfoApiServices)
        {
            _localStorage = localStorage;
            _http = http;
            _StudentInfoApiServices = studentInfoApiServices;
        }
        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var currentLoginOkResponce = _localStorage?.GetItem<StudentInfo.DTO.LoginResponce>("currentLoginOkResponce");
            string? token = null;
            if (currentLoginOkResponce != null)
            {
                var datetime = DateTime.UtcNow;
                if (currentLoginOkResponce.Expiration >= DateTime.UtcNow)                    
                token = currentLoginOkResponce?.Token;
            }
            ClaimsIdentity identity = new ClaimsIdentity();
            _http.DefaultRequestHeaders.Authorization = null;

            if (!string.IsNullOrEmpty(token))
            {
                identity = new ClaimsIdentity(ParseClaimsFromJwt(token), "JWT");
                _http.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token.Replace("\"", ""));
            }

            var user = new ClaimsPrincipal(identity);
            var state = new AuthenticationState(user);

            NotifyAuthenticationStateChanged(Task.FromResult(state));

            return state;
        }

        public static IEnumerable<Claim>? ParseClaimsFromJwt(string? jwt)
        {
            if (string.IsNullOrEmpty(jwt)) return null;

            var payload = jwt.Split('.')[1];
            var jsonBytes = ParseBase64WithoutPadding(payload);
            var keyValuePairs = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonBytes);
            return keyValuePairs?.Select(kvp => new Claim(kvp.Key, kvp.Value.ToString()));
        }

        private static byte[] ParseBase64WithoutPadding(string base64)
        {
            switch (base64.Length % 4)
            {
                case 2: base64 += "=="; break;
                case 3: base64 += "="; break;
            }
            return Convert.FromBase64String(base64);
        }
               
        public async Task Logout()
        {
            _localStorage?.RemoveItem("currentLoginOkResponce");
            _http.DefaultRequestHeaders.Clear();
            await GetAuthenticationStateAsync();
        }
    }

}
