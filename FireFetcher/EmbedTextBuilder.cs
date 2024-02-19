using System.Text;

namespace FireFetcher
{
    internal class EmbedTextBuilder
    {
        public string BuildText(List<Program.CleanedResponse> List, int CategoryIndex)
        {
            // If empty list, use early exit
            if (List.Count == 0)
            {
                return "No runs available";
            }

            int LastPlace = 0;
            int Offset = 0;

            StringBuilder Sb = new("");

            for (int i = 0; i < List.Count; i++)
            {
                // Black magic to solve ties
                if (List[i].Place == LastPlace)
                {
                    Offset++;
                }
                int Position = i - Offset;

                // If not first line, make sure everything gets a newline
                if (i > 0)
                {
                    Sb.Append('\n');
                }

                // Add universal info
                Sb.Append($"{ConvertPlace(Position + 1)} - {List[Position + Offset].Runner}");

                // If Coop, add partner
                if (CategoryIndex == 1)
                {
                    Sb.Append($" & {List[Position + Offset].Partner}");
                }
                
                // If LP, add PortalCount, If CM, add BestPlace, otherwise add time
                if (CategoryIndex == 5)
                {
                    Sb.Append($" - {List[Position + Offset].PortalCount} Portals");
                }
                else if (CategoryIndex == 4)
                {
                    Sb.Append($" - {ConvertPlace(List[Position + Offset].Place)} on {List[Position + Offset].Map}");
                }
                else
                {
                    Sb.Append($" - {List[Position + Offset].Time}");
                }

                LastPlace = List[i].Place;
            }

            // Return the newly created leaderboards text
            return Sb.ToString();
        }

        private static string ConvertPlace(int Place)
        {
            // Modulo 10 since we want end digit
            // 11th, 12th, 13th are expetions from "standard"
            if (Place % 10 == 1 && Place != 11)
            {
                return $"{Place}st";
            }
            else if (Place % 10 == 2 && Place != 12)
            {
                return $"{Place}nd";
            }
            else if (Place % 10 == 3 && Place != 13)
            {
                return $"{Place}rd";
            }
            else
            {
                return $"{Place}th";
            }
        }
    }
}
