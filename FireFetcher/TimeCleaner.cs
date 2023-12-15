using System.Text;

namespace FireFetcher
{
    internal class TimeCleaner
    {
        public string Clean(string DirtyTime)
        {
            // First we go through the DirtyTime to make sure minutes and seconds always have 2 digits
            StringBuilder StringBuild = new();

            string Hour = "";
            string Minute = "";
            string Second = "";
            string MiliSecond = "";

            int CharIndex = 0;

            foreach (char Char in DirtyTime)
            {
                StringBuild.Append(Char);

                if (StringBuild.ToString().EndsWith("H"))
                {
                    Hour = StringBuild.ToString();
                    StringBuild = new();
                }
                else if (StringBuild.ToString().EndsWith("M"))
                {
                    Minute = StringBuild.ToString();
                    StringBuild = new();
                }
                // Extra check incase there are no MiliSeconds
                else if (StringBuild.ToString().EndsWith(".") || CharIndex == DirtyTime.Length)
                {
                    Second = StringBuild.ToString();
                    StringBuild = new();
                }
                else if (StringBuild.ToString().EndsWith("S"))
                {
                    MiliSecond = StringBuild.ToString();
                    StringBuild = new();
                }

                CharIndex++;
            }

            // Check if Minute and Second are missing a 0 in front (basicly if they are less than 10
            if (Minute.Length == 2)
            {
                Minute = Minute.Insert(0, "0");
            }
            if (Second.Length == 2)
            {
                Second = Second.Insert(0, "0");
            }

            DirtyTime = "" + Hour + Minute + Second + MiliSecond;

            // Clean the time
            string CleanTime = DirtyTime.Replace("PT", "").Replace("H", ":").Replace("M", ":").Replace("S", "");

            return CleanTime;
        }
    }
}
