using System.Text;

namespace FireFetcher
{
    internal class EmbedTextBuilder
    {
        public string BuildText(List<Program.CleanedResponse> List, bool Singleplayer, bool Cm)
        {
            if (Singleplayer)
            {
                if (Cm)
                {
                    // If empty list
                    if (List.Count == 0)
                    {
                        return "No runs available";
                    }
                    // Else, meaning list has content
                    else
                    {
                        int LastPlace = 0;
                        int Offset = 0;

                        StringBuilder Sb = new("");
                        for (int i = 0; i < List.Count; i++)
                        {
                            if (List[i].Place == LastPlace)
                            {
                                Offset++;
                            }

                            int Position = i - Offset;

                            switch (Position)
                            {
                                case 0:
                                    Sb.Append($"1st - {List[Position + Offset].Runner} - {ConvertPlace(List[Position + Offset].Place)} on {List[Position + Offset].Map}");
                                    break;
                                case 1:
                                    Sb.Append($"\n2nd - {List[Position + Offset].Runner} - {ConvertPlace(List[Position + Offset].Place)} on {List[Position + Offset].Map}");
                                    break;
                                case 2:
                                    Sb.Append($"\n3rd - {List[Position + Offset].Runner} - {ConvertPlace(List[Position + Offset].Place)} on {List[Position + Offset].Map}");
                                    break;
                                case > 2:
                                    Sb.Append($"\n{Position + 1}th - {List[Position + Offset].Runner} - {ConvertPlace(List[Position + Offset].Place)} on {List[Position + Offset].Map}");
                                    break;
                            }

                            LastPlace = List[i].Place;
                        }

                        // Return the newly created leaderboards text
                        return Sb.ToString();
                    }
                }
                else
                {
                    // If empty list
                    if (List.Count == 0)
                    {
                        return "No runs available";
                    }
                    // Else, meaning list has content
                    else
                    {
                        StringBuilder Sb = new("");
                        for (int i = 0; i < List.Count; i++)
                        {
                            switch (i)
                            {
                                case 0:
                                    Sb.Append($"1st - {List[i].Runner} - {List[i].Time}");
                                    break;
                                case 1:
                                    Sb.Append($"\n2nd - {List[i].Runner} - {List[i].Time}");
                                    break;
                                case 2:
                                    Sb.Append($"\n3rd - {List[i].Runner} - {List[i].Time}");
                                    break;
                                case > 2:
                                    Sb.Append($"\n{i + 1}th - {List[i].Runner} - {List[i].Time}");
                                    break;
                            }
                        }

                        // Return the newly created leaderboards text
                        return Sb.ToString();
                    }
                }
            }
            // If Coop
            else
            {
                // If empty list
                if (List.Count == 0)
                {
                    return "No runs available";
                }
                // Else, meaning list has content
                else
                {
                    StringBuilder Sb = new("");
                    for (int i = 0; i < List.Count; i++)
                    {
                        switch (i)
                        {
                            case 0:
                                Sb.Append($"1st - {List[i].Runner} & {List[i].Partner} - {List[i].Time}");
                                break;
                            case 1:
                                Sb.Append($"\n2nd - {List[i].Runner} & {List[i].Partner} - {List[i].Time}");
                                break;
                            case 2:
                                Sb.Append($"\n3rd - {List[i].Runner} & {List[i].Partner} - {List[i].Time}");
                                break;
                            case > 2:
                                Sb.Append($"\n{i + 1}th - {List[i].Runner} & {List[i].Partner} - {List[i].Time}");
                                break;
                        }
                    }

                    // Return the newly created leaderboards text
                    return Sb.ToString();
                }
            }
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
