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
        private string selectedWord;         // Selected word is a random chosen word from the sorted list, this is the word the user must guess.
        public int guesses;
        private AudioPlayer player = new AudioPlayer(); //Audioplayer for sound
        List<string> guessEntries = new List<string>(); // List of all user-entries

        Grid grid;
        public MainPage()
        {
            InitializeComponent();
            HttpClient client = new HttpClient();
            wordViewModel = new WordViewModel(client);
            sortedWords = new SortedWords(wordViewModel);//Sorted list of 30 words
            wordGrid = this.wordGrid; // Grid that displays guesses
        }

        private async Task InitializeGame()
        {
            guesses = 0; 
            wordGrid.Children.Clear(); // Clears Grid
            wordGrid.RowDefinitions.Clear(); // CLears row definitons on Grid

            // Gets words from viewModel, sorts then binds them
            await wordViewModel.GetWords(); 
            sortedWords.SortWords();
            BindingContext = sortedWords; 

            //  Loads saved selectedWord
            selectedWord = await GameStateSerializer.LoadSelectedWordAsync();
            
            //Loads previous guesses
            await InitializeGuesses();
        }
        private async Task<string> GetSelectedWord()
        {
            selectedWord = await GameStateSerializer.LoadSelectedWordAsync();
            if (string.IsNullOrEmpty(selectedWord)) //CHecks if there is a valid selected word, if not it creates one
            {
                selectedWord = GetRandomWord();
            }
            await GameStateSerializer.SaveSelectedWordAsync(selectedWord); //Saves selected word
            return selectedWord;
        }
        //method for fetching a random word
        private string GetRandomWord()
        {
            Random random = new Random();
            if (sortedWords.WordListSorted.Count > 0)
            {
                int randomIndex = random.Next(sortedWords.WordListSorted.Count);
                return sortedWords.WordListSorted[randomIndex];
            }
            else
            {
                Debug.WriteLine("No Words");
                return string.Empty;
            }
        }
        //Initializes previous guesses, tnis is how the game saves/loads
        private async Task InitializeGuesses()
        {
            guessEntries = await GameStateSerializer.LoadEntriesAsync();
            if (guessEntries.Count > 0)
            {
                foreach (string entry in guessEntries)
                {
                    CreateWord(selectedWord, entry, true); //Rather than trying to load the previous state of the game, the game reads your previous inputs and tries them again essentially returning to the same position and when the app was closed.
                }
            }
        }
        //Checks if the entry is 5 alpabetic letters for error handling
        private bool IsValidWord(string word)
        {
            return word.Length == 5 && word.All(c => Char.IsLetter(c));
        }

        //Initializes or Re-Initializes the game on page appearing
        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await InitializeGame();
        }

        //User Entry to Grid-UI
        public async void CreateWord(String selectedWord, String guessedWord, Boolean loadingEntry)
        {
            Debug.WriteLine("createWord called with selectedWord: " + selectedWord);
            selectedWord = selectedWord.ToUpper();
            int index = wordGrid.RowDefinitions.Count;
            string word = guessedWord.ToUpper();

            if (IsValidWord(word))
            {
                Color BGColor = Color.FromArgb("#ecf7e6"); // Gray BG (Default)
                wordGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

                //Displays each letter 
                for (int i = 0; i < word.Length; i++)
                {
                    BGColor = GetLetterColor(word[i], selectedWord, i, ref BGColor);//Gets Colour based on letter
                    Frame letterFrame = CreateLetterFrame(word[i], BGColor);

                    wordGrid.Children.Add(letterFrame);
                    Grid.SetColumn(letterFrame, i);
                    Grid.SetRow(letterFrame, index);

                    if (!loadingEntry)// if this is an actual user input, the game will play audio and delay to inputs to form an animation, if the game is just laoding up these functions are ignored
                    {
                        await Task.Delay(500);
                        await player.PlayAudio();
                    }
                }
                guesses++;
                await HandleGuessResult(word, selectedWord);
            }
            else
            {
                await DisplayAlert("Invalid Entry", "Please enter a valid 5 letter word", "Try Again");
            }
        }

        private async Task HandleGuessResult(string word, string selectedWord)
        {
            if (CheckForWin(word, selectedWord)) // If word is correct
            {
                await player.CreateAudioPlayer("LevelPassed.mp3");
                await player.PlayAudio();
                await DisplayAlert("Correct Word Guessed!", "You have guessed the correct word, press continue to move on to the next word", "Continue");
                await RestartGame();
            }
            else if (guesses == 6) // if all guesses are used up
            {
                await DisplayAlert("No Guesses Remaining", $"You have not guessed the correct word: {selectedWord}. Press continue to move on to the next word.", "Continue");
                await RestartGame();
            }
        }
        //Clears entries and selects a new word
        private async Task RestartGame()
        {
            guessEntries.Clear();
            await GameStateSerializer.SaveEntriesAsync(guessEntries);
            selectedWord = GetRandomWord();
            await GameStateSerializer.SaveSelectedWordAsync(selectedWord);
            await InitializeGame();
        }
        //Checks if guessed word is the same as the selected word
        private bool CheckForWin(string word, string selectedWord)
        {
            return word == selectedWord;
        }
        // Create a frame for displaying a letter, with specified background color
        private Frame CreateLetterFrame(char letter, Color bgColor)
        {
            return new Frame
            {
                Content = new Label
                {
                    Text = letter.ToString(),
                    FontSize = 50,
                    HorizontalTextAlignment = TextAlignment.Center,
                    VerticalTextAlignment = TextAlignment.Center
                },
                BorderColor = Colors.Black,
                Padding = new Thickness(10),
                CornerRadius = 5,
                HasShadow = true,
                BackgroundColor = bgColor,
                HeightRequest = 100
            };
        }
        //gets background & audio colour for letter
        private Color GetLetterColor(char letter, string selectedWord, int index, ref Color bgColor)
        {
            if (letter == selectedWord[index])
            {
                bgColor = Color.FromArgb("#66eb23"); // Green for correct letter
                player.CreateAudioPlayer("GreenLetter.mp3");
            }
            else if (selectedWord.Contains(letter))
            {
                bgColor = Color.FromArgb("#ebed51");  // Yellow for letter in word but wrong position
                player.CreateAudioPlayer("YellowLetter.mp3");
            }
            else
            {
                bgColor = Color.FromArgb("#ecf7e6");// Gray for incorrect letter
                player.CreateAudioPlayer("GrayLetter.mp3");
            }
            return bgColor;
        }
        //Handler for submitting a guess
        private async void Button_Clicked(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(entry1.Text))
            {
                string word = entry1.Text.ToUpper();
                if (IsValidWord(word))
                {
                    guessEntries.Add(word);
                    await GameStateSerializer.SaveEntriesAsync(guessEntries);
                }
                CreateWord(selectedWord, word, false);
            }
        }
    }
}
