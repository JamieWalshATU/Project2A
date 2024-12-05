using System.Diagnostics;
using System.Text.Json;

namespace Project2A
{
    public static class GameStateSerializer
    {
        private static string appData = FileSystem.AppDataDirectory;

        //Saves all user entries
        public static async Task SaveEntriesAsync(List<string> guessEntries)
        {
            string filePath = Path.Combine(appData, "gameSettings.json");
            string jsonarray = JsonSerializer.Serialize(guessEntries);

            using (StreamWriter writer = new StreamWriter(filePath))
            {
                await writer.WriteAsync(jsonarray);
            }
        }
        //Rather than trying to load the previous state of the game, the game reads your previous inputs and tries them again essentially returning to the same position and when the app was closed.
        public static async Task<List<string>> LoadEntriesAsync()
        {
            var loadedEntries = new List<string>();
            string filePath = Path.Combine(appData, "gameSettings.json");

            if (System.IO.File.Exists(filePath))
            {
                try
                {
                    using (StreamReader reader = new StreamReader(filePath))
                    {
                        string jsonstring = await reader.ReadToEndAsync();
                        var entriesFromFile = JsonSerializer.Deserialize<List<String>>(jsonstring);

                        if (entriesFromFile != null)
                        {
                            loadedEntries.AddRange(entriesFromFile);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Failed");
                }
            }
            return loadedEntries;
        }

        private static string selectedWordFilePath = Path.Combine(appData, "selectedWord.json");
        //Saves word to be guessed, will be used for other game settings later
        public static async Task SaveSelectedWordAsync(string selectedWord)
        {
            try
            {
                string json = JsonSerializer.Serialize(selectedWord);
                using (StreamWriter writer = new StreamWriter(selectedWordFilePath))
                {
                    await writer.WriteAsync(json);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error saving selected word");
            }

        }
        //Loads word to be guessed, will be used for other game settings later
        public static async Task<string> LoadSelectedWordAsync()
        {
            string filePath = Path.Combine(appData, "selectedWord.json");
            if (!System.IO.File.Exists(selectedWordFilePath))
            {
                Debug.WriteLine("Selected word file does not exist.");
                return "ERROR";
            }
            try
            {
                using (StreamReader reader = new StreamReader(selectedWordFilePath))
                {
                    string json = await reader.ReadToEndAsync();

                    string selectedWord = JsonSerializer.Deserialize<string>(json);

                    return selectedWord;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error loading selected word");
            }
            return string.Empty;

        }

    }

}
