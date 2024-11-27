using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace Project2A
{
    public class WordViewModel : INotifyPropertyChanged
    {
        private  ObservableCollection<string> _wordList = new ObservableCollection<string>();

        public ObservableCollection<string> WordList
        {
            get => _wordList;
            set
            {
                _wordList = value;
                OnPropertyChanged(nameof(WordList));
            }
        }
        private HttpClient httpClient;
        
        public WordViewModel(HttpClient httpClient)
        {
            this.httpClient = httpClient ?? new HttpClient();
        }
        public async Task GetWords()
        {
            string appData = FileSystem.AppDataDirectory;
            string filePath = Path.Combine(appData, "wordlist.json");
             
            if (System.IO.File.Exists(filePath))
            {
                using (StreamReader reader = new StreamReader(filePath))
                {
                    string jsonstring = await reader.ReadToEndAsync();
                    var wordsFromFile = JsonSerializer.Deserialize<List<string>>(jsonstring);

                    if (wordsFromFile != null)
                    {
                        WordList = new ObservableCollection<string>(wordsFromFile);

                        //foreach (string word in WordList)
                        //{
                            //Debug.WriteLine(word);
                        //}
                    }
                }
                     
            }

            else
            {
                var response = await httpClient.GetAsync("https://raw.githubusercontent.com/DonH-ITS/jsonfiles/main/words.txt");
                if (response.IsSuccessStatusCode)
                {
                    string contents = await response.Content.ReadAsStringAsync();

                    string[] words = contents.Split(separator: new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

                    foreach (var word in words)
                    {
                        WordList.Add(word);
                    }
                    string jsonarray = JsonSerializer.Serialize(WordList);

                    using (StreamWriter writer = new StreamWriter(filePath))
                    {
                        await writer.WriteAsync(jsonarray);
                    }
                }            
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

