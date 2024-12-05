using System.Text.Json;
using System.Diagnostics;

namespace Project2A
{
    //ViewModel class handles loading/saving of words
    public class WordViewModel
    {
        private List<string> _wordList = new List<string>();

        public List<string> WordList
        {
            get => _wordList;
            set => _wordList = value;
        }
        private HttpClient httpClient;

        public WordViewModel(HttpClient httpClient)
        {
            this.httpClient = httpClient ?? new HttpClient();
        }
        // Method to load words from a local file or fetch them from API
        public async Task GetWords()
        {
            string appData = FileSystem.AppDataDirectory;
            string filePath = Path.Combine(appData, "wordlist.json");
            //Checks if file is saved
            if (System.IO.File.Exists(filePath))
            {
                // if it exits, read words from file
                using (StreamReader reader = new StreamReader(filePath))
                {
                    string jsonstring = await reader.ReadToEndAsync();
                    var wordsFromFile = JsonSerializer.Deserialize<List<string>>(jsonstring);

                    if (wordsFromFile != null)
                    {
                        //updates wordList
                        WordList = new List<string>(wordsFromFile);
                        Debug.WriteLine($"Loaded {WordList.Count} words from file.");

                    }
                }

            }

            else // if file does not exist, create file from api
            {
                Debug.WriteLine("File not found. Fetching words from API.");
                var response = await httpClient.GetAsync("https://raw.githubusercontent.com/DonH-ITS/jsonfiles/main/words.txt");
                if (response.IsSuccessStatusCode)
                {
                    string contents = await response.Content.ReadAsStringAsync();
                    //splits content with line breaks
                    string[] words = contents.Split(separator: new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

                    foreach (var word in words)
                    {
                        WordList.Add(word);
                    }
                    Debug.WriteLine($"Loaded {WordList.Count} words from API.");
                    string jsonarray = JsonSerializer.Serialize(WordList);
                    //serializes file and saves
                    using (StreamWriter writer = new StreamWriter(filePath))
                    {
                        await writer.WriteAsync(jsonarray);
                    }
                }
            }
        }
    }
}

