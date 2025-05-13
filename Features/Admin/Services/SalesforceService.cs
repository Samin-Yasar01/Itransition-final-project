using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using FormsApp.Features.Admin.Models;
using FormsApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Collections.Generic;

namespace FormsApp.Features.Admin.Services
{
    public interface ISalesforceService
    {
        Task<bool> CreateSalesforceAccount(ApplicationUser user, SalesforceIntegrationModel model);
    }

    public class SalesforceService : ISalesforceService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly UserManager<ApplicationUser> _userManager;

        public SalesforceService(
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration,
            UserManager<ApplicationUser> userManager)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _userManager = userManager;
        }

        public async Task<bool> CreateSalesforceAccount(ApplicationUser user, SalesforceIntegrationModel model)
        {
            try
            {
               
                var authResponse = await GetSalesforceAuthToken();
                if (!authResponse.IsSuccessStatusCode)
                {
                    return false;
                }

                var authContent = await authResponse.Content.ReadAsStringAsync();
                var authJson = JsonDocument.Parse(authContent);
                var accessToken = authJson.RootElement.GetProperty("access_token").GetString();
                var instanceUrl = authJson.RootElement.GetProperty("instance_url").GetString();

                var client = _httpClientFactory.CreateClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                
                var accountData = new
                {
                    Name = model.CompanyName,
                    Industry = model.Industry,
                    AnnualRevenue = model.AnnualRevenue,
                    Phone = model.PhoneNumber
                };

                var accountResponse = await client.PostAsync(
                    $"{instanceUrl}/services/data/v56.0/sobjects/Account",
                    new StringContent(JsonSerializer.Serialize(accountData), Encoding.UTF8, "application/json"));

                if (!accountResponse.IsSuccessStatusCode)
                {
                    var error = await accountResponse.Content.ReadAsStringAsync();
                    
                    return false;
                }

                var accountResult = await accountResponse.Content.ReadAsStringAsync();
                var accountJson = JsonDocument.Parse(accountResult);
                var accountId = accountJson.RootElement.GetProperty("id").GetString();

                
                var contactData = new
                {
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    Phone = model.PhoneNumber,
                    Title = model.JobTitle,
                    AccountId = accountId
                };

                var contactResponse = await client.PostAsync(
                    $"{instanceUrl}/services/data/v56.0/sobjects/Contact",
                    new StringContent(JsonSerializer.Serialize(contactData), Encoding.UTF8, "application/json"));

                if (!contactResponse.IsSuccessStatusCode)
                {
                    var error = await contactResponse.Content.ReadAsStringAsync();
                   
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                
                return false;
            }
        }

        private async Task<HttpResponseMessage> GetSalesforceAuthToken()
        {
            var client = _httpClientFactory.CreateClient();
            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("grant_type", "password"),
                new KeyValuePair<string, string>("client_id", _configuration["Salesforce:ClientId"]),
                new KeyValuePair<string, string>("client_secret", _configuration["Salesforce:ClientSecret"]),
                new KeyValuePair<string, string>("username", _configuration["Salesforce:Username"]),
                new KeyValuePair<string, string>("password", _configuration["Salesforce:Password"] + _configuration["Salesforce:SecurityToken"])
            });

            return await client.PostAsync("https://login.salesforce.com/services/oauth2/token", content);
        }
    }
}