using System.Text.Json.Serialization;

namespace FireFetcher
{
    internal class ApiRequester
    {
        #region speedrun.com

        public class SrcResponse
        {
            public List<Data> data { get; set; }
        }

        public class Data
        {
            public int place { get; set; }
            public Run run { get; set; }
        }

        public class Run
        {
            public string game { get; set; }
            public string category { get; set; }
            public List<Player> players { get; set; }
            public Times times { get; set; }
            public Values values { get; set; }
        }

        public class Player
        {
            public Uri uri { get; set; }
        }

        public class Times
        {
            public string primary { get; set; }
        }

        public class Values
        {
            [JsonPropertyName("9l7x7xzn")]
            public string sla { get; set; }
            [JsonPropertyName("38dj54e8")]
            public string amc { get; set; }
            [JsonPropertyName("wl333p9l")]
            public string MelInbounds { get; set; }
        }

        public class SrcProfileResponse
        {
            public ProfileData data { get; set; }
        }

        public class ProfileData
        {
            public Names names { get; set; }
        }

        public class Names
        {
            public string international { get; set; }
        }

        #endregion

        #region boards.portal2.sr

        public class BoardsResponse
        {
            public BoardsTimes times { get; set; }
        }

        public class BoardsTimes
        {
            public SP SP { get; set; }
        }

        public class SP
        {
            public Chambers chambers { get; set; }
        }

        public class Chambers
        {
            public BestRank bestRank { get; set; }
        }

        public class BestRank
        {
            public ScoreData scoreData { get; set; }
            public object map { get; set; }
        }

        public class ScoreData
        {
            public string playerRank { get; set; }
        }

        #endregion

        #region lp.nekz.me

        public class LpResponse
        {
            public List<LpData> data { get; set; }
        }

        public class LpData
        {
            public string name { get; set; }
            public int score { get; set; }
            public int rank { get; set; }
        }

        #endregion

        private string ReturnData = "";

        public string RequestData(int Site, string? User)
        {
            GetData(Site, User);

            return ReturnData;
        }

        public async void GetData(int Site, string? User)
        {
            var Client = new HttpClient();

            string Url = "";

            // Create Url from where to grab data
            UriBuilder UriBuilder = new();
            UriBuilder.Scheme = "http";
            switch (Site)
            {
                case 0:
                    UriBuilder.Host = "www.speedrun.com";
                    UriBuilder.Path = "api/v1/users/";
                    UriBuilder.Path += $"{User}/";
                    UriBuilder.Path += "personal-bests";
                    Url = UriBuilder.ToString();
                    break;
                case 1:
                    UriBuilder.Host = "board.portal2.sr";
                    UriBuilder.Path = "profile/";
                    UriBuilder.Path += $"{User}/";
                    UriBuilder.Path += "json";
                    Url = UriBuilder.ToString();
                    break;
                case 2:
                    Url = "https://lp.nekz.me/api/v1/sp";
                    break;
            }

            var Response = await Client.GetAsync(Url);

            // If success
            if (Response.IsSuccessStatusCode)
            {
                ReturnData = await Response.Content.ReadAsStringAsync();
            }
        }
    }
}
