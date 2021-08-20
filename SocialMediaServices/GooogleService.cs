using Newtonsoft.Json;
using SocialMediaServices.Modal;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace SocialMediaServices
{
    public class GooogleService : IMediaService
    {
        public async Task<GoogleResponseModal> FetchUserProfileByAccessToken(string AccessToken)
        {
            GoogleResponseModal googleResponseModal = default;
            try
            {
                HttpClient httpClient = new HttpClient();
                string Url = $"https://www.googleapis.com/oauth2/v1/userinfo?access_token={AccessToken}";

                httpClient.CancelPendingRequests();
                HttpResponseMessage output = await httpClient.GetAsync(Url);

                if (output.IsSuccessStatusCode)
                {
                    string outputData = await output.Content.ReadAsStringAsync();
                    googleResponseModal = JsonConvert.DeserializeObject<GoogleResponseModal>(outputData);
                }
            }
            catch (Exception ex)
            {
                return null;
            }
            return googleResponseModal;
        }
    }
}
