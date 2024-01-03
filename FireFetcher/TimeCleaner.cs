using System.Text;

namespace FireFetcher
{
    internal class TimeCleaner
    {
        public string Clean(string DirtyTime)
        {
            // Dirtytime is represented in the format PTxxHxxMxx.xxxS
            // Hours, Minutes and Miliseconds can also be missing

            StringBuilder StringBuild = new();

            // First we go through the DirtyTime to make sure minutes and seconds always have 2 digits
            int ChunkCharIndex = 0;
            int CharIndex = 0;

            foreach (char Char in DirtyTime)
            {
                if (Char == 'H' || Char == 'M' || Char == '.' || Char == 'S')
                {
                    // If there is only one 0 in Minute or Second
                    if (ChunkCharIndex == 1 && Char != 'H' && Char != 'S')
                    {
                        StringBuild.Insert(CharIndex - 1, '0');
                        CharIndex++;
                    }

                    ChunkCharIndex = 0;
                }
                else
                {
                    ChunkCharIndex++;
                }

                StringBuild.Append(Char);

                CharIndex++;
            }

            return StringBuild.ToString().Replace("PT", "").Replace("H", ":").Replace("M", ":").Replace("S", "");
        }
    }
}
