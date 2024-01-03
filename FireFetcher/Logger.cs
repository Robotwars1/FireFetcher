
namespace FireFetcher
{
    internal class Logger
    {
        private const string LogFolder = "Logs/";

        private static void CheckLogFolder()
        {
            if (!Directory.Exists(LogFolder))
            {
                Directory.CreateDirectory(LogFolder);
            }
        }

        public void GeneralLog(string Text)
        {
            // Write general logging info to console
            Console.WriteLine(Text);

            CheckLogFolder();

            // Write to log
            using StreamWriter OutputFile = new(Path.Combine(LogFolder, "General-Log.txt"));
            OutputFile.WriteLine(Text);
            OutputFile.Close();
        }

        public void JsonLog(string Data, string File)
        {
            CheckLogFolder();

            // Write to log
            using StreamWriter OutputFile = new(Path.Combine(LogFolder, "Json-Log.txt"));
            OutputFile.WriteLine($"[{DateTime.Now}] Wrote the following data to {File}:\n{Data}");
            OutputFile.Close();
        }

        public void CommandLog(string CommandName, string User)
        {
            // Write command logging to console
            Console.WriteLine($"[{DateTime.Now}] {User} used the command {CommandName}");

            CheckLogFolder();

            // Write to log
            using StreamWriter OutputFile = new(Path.Combine(LogFolder, "Command-Log.txt"));
            OutputFile.WriteLine($"[{DateTime.Now}] {User} used the command {CommandName}");
            OutputFile.Close();
        }
    }
}
