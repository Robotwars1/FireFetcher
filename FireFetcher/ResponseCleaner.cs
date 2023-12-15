
namespace FireFetcher
{
    internal class ResponseCleaner
    {
        // Function to remove any extra runs a given user might have
        public List<Program.CleanedResponse> GetBestRunFromEachUser(List<Program.CleanedResponse> InputList)
        {
            // Check if user already has a run on the boards
            List<string> UserHasBetterScore = new();
            List<int> IndexToRemove = new();
            for (int i = 0; i < InputList.Count; i++)
            {
                if (!UserHasBetterScore.Contains(InputList[i].Runner))
                {
                    UserHasBetterScore.Add(InputList[i].Runner);
                }
                else
                {
                    IndexToRemove.Add(i);
                }
            }

            // Removing by going through List backwards to avoid errors
            IndexToRemove = IndexToRemove.OrderByDescending(o => o).ToList();
            foreach (int Index in IndexToRemove)
            {
                InputList.RemoveAt(Index);
            }

            return InputList;
        }

        // Function to remove exact duplicates when 2 users have the same best coop time
        public List<Program.CleanedResponse> RemoveDuplicate(List<Program.CleanedResponse> InputList)
        {
            List<int> IndexToRemove = new();

            for (int i = 0; i < InputList.Count; i++)
            {
                for (int j = 0; j < InputList.Count; j++)
                {
                    // Dont compare a run with itself && dont compare with runs that should already be removed
                    if (i != j && !IndexToRemove.Contains(j) && !IndexToRemove.Contains(i))
                    {
                        // Check there are no exact duplicates in Amc (with both ways to flip users)
                        if ((InputList[i].Runner == InputList[j].Partner && InputList[i].Partner == InputList[j].Runner) || 
                            (InputList[i].Runner == InputList[j].Runner && InputList[i].Partner == InputList[j].Partner))
                        {
                            IndexToRemove.Add(j);
                        }
                    }
                }
            }

            // Remove duplicate entries
            //IndexToRemove = IndexToRemove.Distinct().ToList();

            // Removing by going through List backwards to avoid errors
            IndexToRemove = IndexToRemove.OrderByDescending(o => o).ToList();
            foreach (int Index in IndexToRemove)
            {
                InputList.RemoveAt(Index);
            }

            // Return the now cleaned list
            return InputList;
        }
    }
}
