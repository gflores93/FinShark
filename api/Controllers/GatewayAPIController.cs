using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


namespace api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GatewayAPIController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public GatewayAPIController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;

        }

        [HttpGet]
        [Route("externalAPIRequest")]
        public async Task<IActionResult> ExternalAPIRequest()
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                // client.Timeout = TimeSpan.FromMinutes(5); // Set a timeout of 5 minutes (or adjust as needed)

                // var client = new HttpClient();
                var request = new HttpRequestMessage(HttpMethod.Get, "https://www.apple.com/search-services/suggestions/defaultlinks/?src=globalnav&locale=es_MX");
                request.Headers.Add("sec-ch-ua", "\"Not A(Brand\";v=\"99\", \"Google Chrome\";v=\"121\", \"Chromium\";v=\"121\"");
                request.Headers.Add("Referer", "https://www.apple.com/mx/");
                request.Headers.Add("sec-ch-ua-mobile", "?0");
                request.Headers.Add("User-Agent", "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/121.0.0.0 Safari/537.36");
                request.Headers.Add("sec-ch-ua-platform", "\"macOS\"");
                request.Headers.Add("Cookie", "geo=MX");
                var response = await client.SendAsync(request);
                response.EnsureSuccessStatusCode();
                var jsonString = await response.Content.ReadAsStringAsync();

                // option 1
                var jsonObject = JsonConvert.DeserializeObject<ApiResponse>(jsonString);

                // option 2
                // var jsonObject = JObject.Parse(jsonString); // need to access each object with its property name

                return Ok(jsonObject);


            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"HTTP Request Error: {ex.Message}");
                return StatusCode(500, "Internal Server Error");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return StatusCode(500, "Internal Server Error");
            }
        }

        [HttpGet]
        [Route("execute")]
        public async Task<IActionResult> ExecuteApi([FromQuery] string apiUrl, [FromQuery] string? model = null)
        {
            try
            {
                var client = _httpClientFactory.CreateClient();

                var response = await client.GetAsync(apiUrl);

                response.EnsureSuccessStatusCode();

                var jsonString = await response.Content.ReadAsStringAsync();

                //model passed requires namespace.className (e.g. api.Controllers.ApiResponse)
                if (!string.IsNullOrEmpty(model))
                {
                    try
                    {
                        // If a model is provided, try to parse it
                        var modelType = Type.GetType(model);
                        if (modelType != null)
                        {
                            var modelObject = JsonConvert.DeserializeObject(jsonString, modelType);
                            return Ok(modelObject);
                        }
                    }
                    catch (Exception ex)
                    {
                        // Log or handle the deserialization exception
                        return BadRequest($"Error deserializing response: {ex.Message}");
                    }
                }

                // If no model is provided or parsing fails, return the raw JSON string
                return Ok(jsonString);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error: {ex.Message}");
            }
        }

    }

    // Classes to map externalAPI responses
    public class SectionResult
    {
        public string Label { get; set; } = String.Empty;
        public string Url { get; set; } = String.Empty;
    }

    public class Section
    {
        public string SectionName { get; set; } = String.Empty;
        public List<SectionResult> SectionResults { get; set; } = new List<SectionResult>();
    }

    public class ApiResponse
    {
        public string Id { get; set; } = String.Empty;
        public List<Section> Results { get; set; } = new List<Section>();
    }

}