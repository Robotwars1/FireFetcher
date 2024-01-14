using System.Text;

namespace FireFetcher
{
    internal class EmbedTextBuilder
    {
        public string BuildText(List<Program.CleanedResponse> List, bool Singleplayer, bool Cm, bool LP)
        {
            // If empty list
            if (List.Count == 0)
            {
                return "No runs available";
            }

            int LastPlace = 0;
            int Offset = 0;

            StringBuilder Sb = new("");

            if (Singleplayer)
            {
                if (Cm)
                {
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
                                if (i == 0)
                                {
                                    Sb.Append($"1st - {List[Position + Offset].Runner} - {ConvertPlace(List[Position + Offset].Place)} on {List[Position + Offset].Map}");
                                }
                                // If there is a tie at 1st
                                else
                                {
                                    Sb.Append($"\n1st - {List[Position + Offset].Runner} - {ConvertPlace(List[Position + Offset].Place)} on {List[Position + Offset].Map}");
                                }
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
                }
                else if (LP)
                {
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
                                if (i == 0)
                                {
                                    Sb.Append($"1st - {List[Position + Offset].Runner} - {List[Position + Offset].PortalCount} Portals");
                                }
                                // If there is a tie at 1st
                                else
                                {
                                    Sb.Append($"\n1st - {List[Position + Offset].Runner} - {List[Position + Offset].PortalCount} Portals");
                                }
                                break;
                            case 1:
                                Sb.Append($"\n2nd - {List[Position + Offset].Runner} - {List[Position + Offset].PortalCount} Portals");
                                break;
                            case 2:
                                Sb.Append($"\n3rd - {List[Position + Offset].Runner} - {List[Position + Offset].PortalCount} Portals");
                                break;
                            case > 2:
                                Sb.Append($"\n{Position + 1}th - {List[Position + Offset].Runner} - {List[Position + Offset].PortalCount} Portals");
                                break;
                        }

                        LastPlace = List[i].Place;
                    }
                }
                // If a fullgame run
                else
                {
                    for (int i = 0; i < List.Count; i++)
                    {
                        switch (i)
                        {
                            case 0:
                                if (i == 0)
                                {
                                    Sb.Append($"1st - {List[i].Runner} - {List[i].Time}");
                                }
                                // If there is a tie at 1st
                                else
                                {
                                    Sb.Append($"\n1st - {List[i].Runner} - {List[i].Time}");
                                }
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
                }
            }
            // If Coop
            else
            {
                for (int i = 0; i < List.Count; i++)
                {
                    switch (i)
                    {
                        case 0:
                            if (i == 0)
                            {
                                Sb.Append($"1st - {List[i].Runner} & {List[i].Partner} - {List[i].Time}");
                            }
                            // If there is a tie at 1st
                            else
                            {
                                Sb.Append($"\n1st - {List[i].Runner} & {List[i].Partner} - {List[i].Time}");
                            }
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
