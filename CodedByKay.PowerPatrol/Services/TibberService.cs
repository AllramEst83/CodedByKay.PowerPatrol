using CodedByKay.PowerPatrol.Interfaces;
using CodedByKay.PowerPatrol.Models;
using Microsoft.Extensions.Options;
using System.Text;

namespace CodedByKay.PowerPatrol.Services
{
    public class TibberService : ITibberService
    {
        private readonly ApplicationSettings _applicationSettings;
        private readonly HttpClient _httpClient;
        public TibberService(IOptions<ApplicationSettings> applicationSettings, IHttpClientFactory httpClientFactory)
        {
            _applicationSettings = applicationSettings.Value;
            _httpClient = httpClientFactory.CreateClient("PowerPatrolClient");

        }

        public async Task<(CurrentSubscription?, string?)> GetCurrentConsumtion()
        {
            var currentEnergyPriceQuery = @"
            {
              viewer {
                homes {
                  timeZone
                  currentSubscription{
                    priceInfo{
                      current{
                        total
                        energy
                        tax
                        startsAt
                      }
                    }
                  }
                }
              }
            }
            ";

            // Preparing the query by escaping necessary characters and removing new lines
            var formattedGraphqlQuery = $"{{\"query\":\"{currentEnergyPriceQuery.Replace("\"", "\\\"").Replace("\n", " ").Replace("\r", "").Trim()}\"}}";

            var request = new HttpRequestMessage(HttpMethod.Post, _applicationSettings.TibberApiUri)
            {
                Content = new StringContent(formattedGraphqlQuery, Encoding.UTF8, "application/json")
            };


            // Add the Authorization header with the access token
            request.Headers.Add("Authorization", $"Bearer {_applicationSettings.TibberApiToken}");

            try
            {
                var response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode(); // Throw an exception if not successful

                var content = await response.Content.ReadAsStringAsync();

                var options = new System.Text.Json.JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true // This makes the deserializer not case-sensitive
                };

                var requestData = System.Text.Json.JsonSerializer.Deserialize<EnergyConsumptionResponse>(content, options);
                CurrentEnergyPrice? tibberConsumtionData = requestData?.Data.Viewer.Homes.FirstOrDefault();


                return (tibberConsumtionData?.CurrentSubscription, tibberConsumtionData?.TimeZone);
            }
            catch (HttpRequestException e)
            {
                return (null, null);
            }
        }

        public async Task<CurrentEnergyPrice?> GetEnergyConsumption()
        {
            var currentEnergyPriceQuery = @"
            {
              viewer {
                homes {
                  timeZone
                  address{
                     address1
                    }
                  currentSubscription{
                    priceInfo{
                      current{
                        total
                        energy
                        tax
                        startsAt
                      }
                      today {
                        total
                        energy
                        tax
                        startsAt
                      }
                      tomorrow {
                        total
                        energy
                        tax
                        startsAt
                      }
                    }
                  }
                }
              }
            }";

            // Preparing the query by escaping necessary characters and removing new lines
            var formattedGraphqlQuery = $"{{\"query\":\"{currentEnergyPriceQuery.Replace("\"", "\\\"").Replace("\n", " ").Replace("\r", "").Trim()}\"}}";

            var request = new HttpRequestMessage(HttpMethod.Post, _applicationSettings.TibberApiUri)
            {
                Content = new StringContent(formattedGraphqlQuery, Encoding.UTF8, "application/json")
            };


            // Add the Authorization header with the access token
            request.Headers.Add("Authorization", $"Bearer {_applicationSettings.TibberApiToken}");

            try
            {
                var response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode(); // Throw an exception if not successful

                var content = await response.Content.ReadAsStringAsync();

                var options = new System.Text.Json.JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true // This makes the deserializer not case-sensitive
                };

                var requestData = System.Text.Json.JsonSerializer.Deserialize<EnergyConsumptionResponse>(content, options);
                CurrentEnergyPrice? tibberConsumtionData = requestData?.Data.Viewer.Homes.FirstOrDefault();

                return tibberConsumtionData;
            }
            catch (HttpRequestException e)
            {
                return null;
            }
        }
    }
}
