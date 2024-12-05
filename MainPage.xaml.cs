using System.Diagnostics;

namespace Project2A
{
    public partial class MainPage : ContentPage
    {
        private KeyHandling keyHandling;
        
        private WordViewModel wordViewModel;
        private SortedWords sortedWords;
        private string selectedWord;         // Selected word is a random chosen word from the sorted list, this is the word the user must guess.
        
        public int guesses;
        List<string> guessEntries = new List<string>(); // List of all user-entries

        private AudioPlayer player = new AudioPlayer(); //Audioplayer for sound

        Grid grid;
        public MainPage()
        {
            InitializeComponent();

            keyHandling = new KeyHandling();
            keyHandling.KeyClickedEvent += GuessSubmission;

            HttpClient client = new HttpClient();
            wordViewModel = new WordViewModel(client);
            sortedWords = new SortedWords(wordViewModel);//Sorted list of 30 words
            wordGrid = this.wordGrid; // Grid that displays guesses
        }

        private async Task InitializeGame()
        {
            guesses = 0;

            InitializeBlankGrid();

            Debug.WriteLine("Initializing game...");

            // Gets words from viewModel, sorts then binds them
            await wordViewModel.GetWords();
            Debug.WriteLine($"WordList has {wordViewModel.WordList.Count} words.");

            sortedWords.SortWords();
            Debug.WriteLine($"Sorted words list has {sortedWords.WordListSorted.Count} words.");
            BindingContext = sortedWords;

            //  Loads saved selectedWord
            selectedWord = await GameStateSerializer.LoadSelectedWordAsync();
            Debug.WriteLine($"Selected word: {selectedWord}");

            //Loads previous guesses
            await InitializeGuesses();
        }
        
        //Creates a 6x5 Grid
        private void InitializeBlankGrid()
        {
            wordGrid.Children.Clear();
            wordGrid.RowDefinitions.Clear();

            for (int row = 0; row < 6; row++)  
            {
                wordGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

                for (int col = 0; col < 5; col++) // 5 columns for letters
                {
                    Frame blankFrame = CreateLetterFrame(' ', Color.FromArgb("#ecf7e6")); // Gray for blank
                    wordGrid.Children.Add(blankFrame);
                    Grid.SetColumn(blankFrame, col);
                    Grid.SetRow(blankFrame, row);
                }
            }

        }
        private async Task<string> GetSelectedWord()
        {
            selectedWord = await GameStateSerializer.LoadSelectedWordAsync();
            if (selectedWord.Length > 5 )
            {
                selectedWord = GetRandomWord();
            }
            if (string.IsNullOrEmpty(selectedWord)) //CHecks if there is a valid selected word, if not it creates one
            {
                selectedWord = GetRandomWord();
                await GameStateSerializer.SaveSelectedWordAsync(selectedWord);
            } //Saves selected word
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
                Debug.WriteLine("No words available in sorted list.");
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
            string word = guessedWord.ToUpper();

            if (IsValidWord(word))
            {
                Color BGColor = Color.FromArgb("#ecf7e6"); // Gray BG (Default)
                int row = guesses;

                //Displays each letter 
                for (int i = 0; i < word.Length; i++)
                {
                    BGColor = await GetLetterColorAsync(word[i], selectedWord, i);//Gets Colour based on letter
                    Frame existingFrame = GetFrameAtPosition(row, i);

                    if (existingFrame != null)
                    {
                        // Update the frame's content and background color
                        ((Label)existingFrame.Content).Text = word[i].ToString();
                        existingFrame.BackgroundColor = BGColor;
                    }

                    if (existingFrame != null)

                        if (!loadingEntry)// if this is an actual user input, the game will play audio and delay to inputs to form an animation, if the game is just laoding up these functions are ignored
                    {
                        await player.PlayAudio();
                        await Task.Delay(500);
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
        Frame GetFrameAtPosition(int row, int column)
        {
            foreach (var child in wordGrid.Children)
            {
                if (wordGrid.GetRow(child) == row && wordGrid.GetColumn(child) == column && child is Frame frame)
                {
                    return frame;
                }
            }
            return null;
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
                    FontSize = 30,
                    HorizontalTextAlignment = TextAlignment.Center,
                    VerticalTextAlignment = TextAlignment.Center
                },
                BorderColor = Colors.Black,
                Padding = new Thickness(10),
                CornerRadius = 5,
                HasShadow = true,
                BackgroundColor = bgColor,
                HeightRequest = 80,
            };
        }
        //gets background & audio colour for letter
        private async Task<Color> GetLetterColorAsync(char letter, string selectedWord, int index)

        {
            Color bgColor;
            if (letter == selectedWord[index])
            {
                bgColor = Color.FromArgb("#66eb23"); // Green for correct letter
                await player.CreateAudioPlayer("GreenLetter.mp3");
            }
            else if (selectedWord.Contains(letter))
            {
                bgColor = Color.FromArgb("#ebed51");  // Yellow for letter in word but wrong position
                await player.CreateAudioPlayer("YellowLetter.mp3");
            }
            else
            {
                bgColor = Color.FromArgb("#ecf7e6");// Gray for incorrect letter
                await player.CreateAudioPlayer("GrayLetter.mp3");
            }
            return bgColor;
        }
        //Handler for submitting a guess
        public async void GuessSubmission(string enteredWord)
        {
            if (!string.IsNullOrWhiteSpace(enteredWord))
            {
                string word = enteredWord.ToUpper();
                if (IsValidWord(word))
                {
                    guessEntries.Add(word);
                    await GameStateSerializer.SaveEntriesAsync(guessEntries);
                }
                CreateWord(selectedWord, word, false);
            }
        }
        private void OnKeyClicked(object sender, EventArgs e)
        {
            keyHandling.OnKeyClicked(sender, e);
        }
    }
}
