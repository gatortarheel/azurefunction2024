using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net.Http;

namespace AzureWaffles.Function
{
    public static class WeatherFunction
    {
        private static readonly HttpClient httpClient = new HttpClient();
        private const string WeatherApiKey = "f1713d9f5c23f3ad64eaafe3a9060c95";
        private const string WeatherApi = "http://api.openweathermap.org/data/2.5/weather";


        [FunctionName("GetWeatherByZipCode")]
        public static async Task<IActionResult> Run ([HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req, ILogger log)
        {
            string zipCode = req.Query["zipCode"];
           
            if(string.IsNullOrEmpty(zipCode))
            {
                return new BadRequestObjectResult("Need a zip code");
            }

            try
            {
                string weatherData = await GetWeatherDataAsync(zipCode);
                return new OkObjectResult(weatherData);
            }
            catch(Exception ex)
            {
                log.LogError($"Error: {ex.Message}");
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }

        private static async Task<string> GetWeatherDataAsync(string zipCode)
        {
            string requestUrl = $"{WeatherApi}?zip={zipCode},us&appid={WeatherApiKey}&units=imperial";
             HttpResponseMessage response = await httpClient.GetAsync(requestUrl);
            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();
            WeatherResponse weatherResponse = JsonConvert.DeserializeObject<WeatherResponse>(responseBody);
            return $"Current weather in {weatherResponse.name}: {weatherResponse.main.temp} {weatherResponse.weather[0].description}";
        }

        
      
    }
}
