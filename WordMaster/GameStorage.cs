using System.Text.Json;

namespace WordMaster
{
    public static class GameStorage
    {
        private static string GetGameFilePath()
        {
            return Path.Combine(Directory.GetCurrentDirectory(), "Games");
        }

        // Save game result to JSON file
        public static void SaveGameResult(GameResult result)
        {
            string directoryPath = GetGameFilePath();
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            string fileName = $"{result.Date.ToString("yyyy-MM-dd_HH-mm-ss")}.json";
            string filePath = Path.Combine(directoryPath, fileName);

            string json = JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(filePath, json);

            Console.WriteLine($"The game result is saved to file: {filePath}");
        }

        public static GameResult LoadGameResult(string fileName)
        {
            string filePath = Path.Combine(GetGameFilePath(), fileName);

            if (File.Exists(filePath))
            {
                string json = File.ReadAllText(filePath);
                return JsonSerializer.Deserialize<GameResult>(json);
            }

            return null;
        }

        public static List<string> GetSavedGames()
        {
            string directoryPath = GetGameFilePath();
            if (Directory.Exists(directoryPath))
            {
                return Directory.GetFiles(directoryPath, "*.json")
                                .Select(Path.GetFileName)
                                .ToList();
            }
            return new List<string>();
        }
    }
}
