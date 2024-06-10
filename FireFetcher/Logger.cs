
namespace FireFetcher
{
    internal class Logger
    {
        private const string LogFolder = "Logs/";

        private static void CheckLogFolderExists()
        {
            if (!Directory.Exists(LogFolder))
            {
                Directory.CreateDirectory(LogFolder);
            }
        }

        public void GeneralLog(string Text)
        {
            // Write general logging info to console
            Console.WriteLine($"[{DateTime.Now}] {Text}");

            CheckLogFolderExists();

            string FilePath = Path.Combine(LogFolder, "General-Log.txt");

            // Write to log
            using StreamWriter OutputFile = File.AppendText(FilePath);
            OutputFile.WriteLine($"[{DateTime.Now}] {Text}");
            OutputFile.Close();
        }

        public void JsonLog(string Data, string File)
        {
            CheckLogFolderExists();

            string FilePath = Path.Combine(LogFolder, "Json-Log.txt");

            // Write to log
            using StreamWriter OutputFile = System.IO.File.AppendText(FilePath);
            OutputFile.WriteLine($"[{DateTime.Now}] Wrote the following data to {File}:\n{Data}");
            OutputFile.Close();
        }

        public void CommandLog(string User)
        {
            // Write command logging to console
            Console.WriteLine($"[{DateTime.Now}] {User} used a command");

            CheckLogFolderExists();

            string FilePath = Path.Combine(LogFolder, "Command-Log.txt");

            // Write to log
            using StreamWriter OutputFile = File.AppendText(FilePath);
            OutputFile.WriteLine($"[{DateTime.Now}] {User} used a command");
            OutputFile.Close();
        }
    }
}
