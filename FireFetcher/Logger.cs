
namespace FireFetcher
{
    internal class Logger
    {
        private const string LogFolder = "Logs/";

        public void GeneralLog(string Text)
        {
            // Write general logging info to console
            Console.WriteLine(Text);

            if (!Directory.Exists(LogFolder))
            {
                Directory.CreateDirectory(LogFolder);
            }

            // Write to log
            using (StreamWriter OutputFile = new(Path.Combine(LogFolder, "General-Log.txt")))
            {
                OutputFile.WriteLine(Text);
            }
        }

        public void JsonLog(string Data, string File)
        {
            if (!Directory.Exists(LogFolder))
            {
                Directory.CreateDirectory(LogFolder);
            }

            // Write to log
            using (StreamWriter OutputFile = new(Path.Combine(LogFolder, "Json-Log.txt")))
            {
                OutputFile.WriteLine($"[{DateTime.Now}] Wrote the following data to {File}:\n{Data}");
            }
        }

        public void CommandLog(string CommandName, string User)
        {
            // Write command logging to console
            Console.WriteLine($"[{DateTime.Now}] {User} used the command {CommandName}");

            if (!Directory.Exists(LogFolder))
            {
                Directory.CreateDirectory(LogFolder);
            }

            // Write to log
            using (StreamWriter OutputFile = new(Path.Combine(LogFolder, "Command-Log.txt")))
            {
                OutputFile.WriteLine($"[{DateTime.Now}] {User} used the command {CommandName}");
            }
        }
    }
}
