using System.Diagnostics;
using System.Text.Json;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Reflection.PortableExecutable;
using System;
using System.Linq.Expressions;

namespace Project2A
{
    public partial class MainPage : ContentPage
    {

        private WordViewModel wordViewModel;
        private SortedWords sortedWords;
        private string selectedWord;
        public int guesses;
        private AudioPlayer player = new AudioPlayer();
        List<string> guessEntries = new List<string>();
        int guessIndex = 0;

        Grid grid;

        private async Task InitializeGame()
        {
            guesses = 0;
            wordGrid.Children.Clear();
            wordGrid.RowDefinitions.Clear();

            await wordViewModel.GetWords();
            sortedWords.SortWords();
            BindingContext = sortedWords;

            selectedWord = await SelectedWordSaving.LoadSelectedWordAsync();
            if (string.IsNullOrEmpty(selectedWord))
            {
                selectedWord = getWord();
            }
            await SelectedWordSaving.SaveSelectedWordAsync(selectedWord); 

            if (string.IsNullOrEmpty(selectedWord))
            {
                Debug.WriteLine("Failed to select a word.");
                await DisplayAlert("Error", "No valid word selected. Restart the game.", "OK");
                return;
            }
            Debug.WriteLine("Selected Word: " + selectedWord);

            guessEntries = await SerializeEntryList.LoadEntriesAsync();
            if (guessEntries.Count > 0)
            {
                foreach (string entry in guessEntries)
                {
                    createWord(selectedWord, entry, true);
                }
            }
        }

        public MainPage()
        {
            InitializeComponent();
            HttpClient client = new HttpClient();
            wordViewModel = new WordViewModel(client);
            sortedWords = new SortedWords(wordViewModel);
            wordGrid = this.wordGrid;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await InitializeGame();
        }

        private String getWord()
        {
            Random random = new Random();
            if (sortedWords.WordListSorted.Count > 0)
            {
                int randomIndex = random.Next(sortedWords.WordListSorted.Count);
                string word = sortedWords.WordListSorted[randomIndex];
                return word;
            }
            else
            {
                Debug.WriteLine("No Words");
                return string.Empty;
            }
        }
        public async void createWord(String selectedWord, String guessedWord, Boolean loadingEntry)
        {
            Debug.WriteLine("createWord called with selectedWord: " + selectedWord);
            selectedWord = selectedWord.ToUpper();
            int index = wordGrid.RowDefinitions.Count;
            string word = guessedWord.ToUpper();
            if ((word.Length == 5 && word.All(c => Char.IsLetter(c))))
            {
                Color BGColor = Color.FromArgb("#ecf7e6");
                wordGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                for (int i = 0; i < word.Length; i++)
                {
                    if (word[i] == selectedWord[i])
                    {
                        BGColor = Color.FromArgb("#66eb23");
                        await player.CreateAudioPlayer("GreenLetter.mp3");
                    }
                    else if (selectedWord.Contains(word[i]))
                    {
                        BGColor = Color.FromArgb("#ebed51");
                        await player.CreateAudioPlayer("YellowLetter.mp3");
                    }
                    else
                    {
                        BGColor = Color.FromArgb("#ecf7e6");
                        await player.CreateAudioPlayer("GrayLetter.mp3");
                    }

                    Frame letterFrame = new Frame
                    {

                        Content = new Label
                        {

                            Text = word[i].ToString(),
                            FontSize = 50,
                            HorizontalTextAlignment = TextAlignment.Center,
                            VerticalTextAlignment = TextAlignment.Center
                        },
                        BorderColor = Colors.Black,
                        Padding = new Thickness(10),
                        CornerRadius = 5,
                        HasShadow = true,
                        BackgroundColor = BGColor,
                        HeightRequest = 100
                    };
                    if (loadingEntry == false)
                    {
                        await Task.Delay(500);
                        wordGrid.Children.Add(letterFrame);
                        await player.PlayAudio();
                    }
                    else if (loadingEntry == true)
                    {
                        wordGrid.Children.Add(letterFrame);
                    }

                    Grid.SetColumn(letterFrame, i);
                    Grid.SetRow(letterFrame, index);
                }
                guesses++;
                if (checkForWin(word, selectedWord) == true)
                {
                    await player.CreateAudioPlayer("LevelPassed.mp3");
                    await player.PlayAudio();
                    await DisplayAlert("Correct Word Guessed!", "You have guessed the correct word, press contine to move on to the next word", "Continue");
                    guessEntries.Clear();
                    await SerializeEntryList.SaveEntriesAsync(guessEntries);
                    selectedWord = getWord();
                    await SelectedWordSaving.SaveSelectedWordAsync(selectedWord);
                    await InitializeGame();
                }
                else if (guesses == 6)
                {
                    await DisplayAlert("No Guesses Remainign", "You have not guessed the correct word: " + selectedWord + " press contine to move on to the next word", "Continue");
                    guessEntries.Clear();
                    await SerializeEntryList.SaveEntriesAsync(guessEntries);
                    selectedWord = getWord();
                    await SelectedWordSaving.SaveSelectedWordAsync(selectedWord);
                    await InitializeGame();
                }
            }
            else
            {
                await DisplayAlert("Invalid Entry", "Please enter a valid 5 letter word", "Try Again");
            }
        }

        private async void Button_Clicked(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(entry1.Text))
            {
                string word = entry1.Text.ToUpper();
                if (word.Length == 5 && word.All(c => Char.IsLetter(c)))
                {
                    guessEntries.Add(word);
                    await SerializeEntryList.SaveEntriesAsync(guessEntries);
                }
                createWord(selectedWord, word, false);
            }
        }

        private bool checkForWin(String word, String selectedWord)
        {
            if (word == selectedWord)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
    public class SerializeEntryList //Saving functions work as if the user just entered the saved entries and works some functions differently to look more fluid, in my last project I tried to seriliase the entire state of the game however I find this works to be much more clean,
    {
        private static string appData = FileSystem.AppDataDirectory;
        private static string filePath = Path.Combine(appData, "gameSettings.json");
        public static async Task SaveEntriesAsync(List<string> guessEntries)
        {
            string jsonarray = JsonSerializer.Serialize(guessEntries);

            using (StreamWriter writer = new StreamWriter(filePath))
            {
                await writer.WriteAsync(jsonarray);
            }
        }
        public static async Task<List<string>> LoadEntriesAsync()
        {
            var loadedEntries = new List<string>();

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
    }
    public class SelectedWordSaving // This will become game settings and scores
    {
        private static string appData = FileSystem.AppDataDirectory;
        private static string selectedWordFilePath = Path.Combine(appData, "selectedWord.json");

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
        public static async Task<string> LoadSelectedWordAsync()
        {
            if (!System.IO.File.Exists(selectedWordFilePath))
            {
                Debug.WriteLine("Selected word file does not exist.");
                return string.Empty;
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
