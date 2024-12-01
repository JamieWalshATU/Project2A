﻿using System.Diagnostics;

namespace Project2A
{
    public partial class MainPage : ContentPage
    {

        private WordViewModel wordViewModel;
        private SortedWords sortedWords;
        private string selectedWord;
        public int guesses;
        private AudioPlayer player = new AudioPlayer();

        Grid grid;

        private async void InitializeGame()
        {

            guesses = 0;
            wordGrid.Children.Clear();
            wordGrid.RowDefinitions.Clear();

            await wordViewModel.GetWords();
            sortedWords.SortWords();
            BindingContext = sortedWords;

            selectedWord = getWord();
            Debug.WriteLine("Selected Word: " + selectedWord);
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
            InitializeGame();
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
        public async void createWord(String selectedWord)
        {
            Debug.WriteLine("createWord called with selectedWord: " + selectedWord);
            selectedWord = selectedWord.ToUpper();
            int index = wordGrid.RowDefinitions.Count;
            string word = entry1.Text.ToUpper();
            if ((word.Length == 5 && word.All(c => Char.IsLetter(c)))) {
                Color BGColor = Color.FromArgb("#ecf7e6");
                wordGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                for (int i = 0; i < word.Length; i++)
                {
                    if (word[i] == selectedWord[i])
                    {
                        BGColor = Color.FromArgb("#66eb23");
                        await player.CreateAudioPlayer("GreenLetter.mp3");
                    }
                    else if (selectedWord.Contains(word[i])) {
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
                    await Task.Delay(500);
                    wordGrid.Children.Add(letterFrame);
                    await player.PlayAudio();

                    Grid.SetColumn(letterFrame, i);
                    Grid.SetRow(letterFrame, index);
                }
                guesses++;
                if (checkForWin(word, selectedWord) == true)
                {
                    await player.CreateAudioPlayer("LevelPassed.mp3");
                    await player.PlayAudio();
                    await DisplayAlert("Correct Word Guessed!", "You have guessed the correct word, press contine to move on to the next word", "Continue");
                    InitializeGame();
                }
                else if (guesses == 6)
                {
                    await DisplayAlert("No Guesses Remainign", "You have not guessed the correct word: " + selectedWord + " press contine to move on to the next word", "Continue");
                    InitializeGame();
                }
            }
            else
            {
                await DisplayAlert("Invalid Entry", "Please enter a valid 5 letter word","Try Again");
            } 
        }

        private void Button_Clicked(object sender, EventArgs e)
        {
            ;
            createWord(selectedWord);
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

}
