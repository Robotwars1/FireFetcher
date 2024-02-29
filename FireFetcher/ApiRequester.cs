
namespace FireFetcher
{
    internal class ApiRequester
    {
        private string ReturnData = "";

        public async Task<string> RequestData(int Site, string? User)
        {
            await GetData(Site, User);

            return ReturnData;
        }

        private async Task GetData(int Site, string? User)
        {
            var Client = new HttpClient();

            string Url = "";

            // Create Url from where to grab data
            UriBuilder UriBuilder = new();
            UriBuilder.Scheme = "https";
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
            else
            {
                Console.WriteLine($"Failed to get data from site index: {Site}");
            }
        }
    }
}
