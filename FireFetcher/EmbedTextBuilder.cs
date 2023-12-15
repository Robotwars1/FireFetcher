using System.Text;

namespace FireFetcher
{
    internal class EmbedTextBuilder
    {
        public string BuildText(List<Program.CleanedResponse> List, bool Singleplayer)
        {
            if (Singleplayer)
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
    }
}
