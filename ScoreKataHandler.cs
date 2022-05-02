using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System;
using System.Net;
using MelonLoader;

namespace ScoreKata
{
    public class ScoreKataHandler
    {
        HttpClient client = new HttpClient()
        {
            DefaultRequestHeaders =
            {
                { "Accept", "application/json" },
            },
            BaseAddress = new System.Uri("https://scorekata.com/wp-json/api/")
        };

        ScoreKataLeaderboardEntry[] previousWinners = new ScoreKataLeaderboardEntry[0];
        bool hasPreviousWinners = false;

        public async Task<ScoreKataLeaderboardEntry[]> GetLeaderboard()
        {
            try
            {
                System.Net.ServicePointManager.SecurityProtocol =
                    SecurityProtocolType.Tls12 |
                    SecurityProtocolType.Tls11 |
                    SecurityProtocolType.Tls;

                var request = new HttpRequestMessage(HttpMethod.Get, "leaderboard");
                var response = await client.SendAsync(request);
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();
                var responseObject = JsonConvert.DeserializeObject<ScoreKataLeaderboardEntry[]>(responseBody);
                return responseObject;
            }
            catch (HttpRequestException e)
            {
                MelonLogger.Msg(e.Message + "\n" + e.InnerException.Message);
                return new ScoreKataLeaderboardEntry[0];
            }
            catch (Exception ex)
            {
                MelonLogger.Msg(ex.Message + "\n" + ex.InnerException.Message);
                return new ScoreKataLeaderboardEntry[0];
            }
        }
        
        public async Task<ScoreKataLeaderboardEntry[]> GetPreviousWinners()
        { 
            //If we have the data already, just return it. Winners change once a month and its not worth covering the edge case.
            if (hasPreviousWinners)
                return previousWinners;
            
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get,"winner");
                var response = await client.SendAsync(request);
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();
                var responseObject = JsonConvert.DeserializeObject<ScoreKataLeaderboardEntry[]>(responseBody);
                hasPreviousWinners = true;
                previousWinners = responseObject;
                return previousWinners;
            }
            catch (HttpRequestException e)
            {
                MelonLogger.Msg(e.Message + "\n" + e.InnerException.Message);
                return new ScoreKataLeaderboardEntry[0];
            }
            catch (Exception ex)
            {
                MelonLogger.Msg(ex.Message);
                return new ScoreKataLeaderboardEntry[0];
            }
        }
        
        public bool IsPreviousWinner(ScoreKataLeaderboardEntry user)
        {
            for (var i = 0; i < previousWinners.Length; i++)
            {
                if(user.user_id == previousWinners[i].user_id)
                    return true;
            }
            return false;
        }

    }
}