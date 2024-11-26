
namespace Project2A
{
    public partial class MainPage : ContentPage
    {

        private WordViewModel wordViewModel;
        private SortedWords sortedWords;
        private string selectedWord;

        Grid grid;


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

            await wordViewModel.GetWords();
            sortedWords.SortWords();
            BindingContext = sortedWords;
            
            selectedWord = getWord();
            Console.WriteLine("Selected Word: " + selectedWord);
        }

        private String getWord()
        {
            Random random = new Random();
            if (sortedWords.WordListSorted.Count > 0)
            {
                int randomIndex = random.Next(sortedWords.WordListSorted.Count);
                return sortedWords.WordListSorted[randomIndex];
            }
            else
            {
                Console.WriteLine("No Words");
                return string.Empty;
            }
        }
        public void createWord(String selectedWord)
        {
            int index = wordGrid.RowDefinitions.Count;
            string word = entry1.Text;
            string wordFormat = word.ToUpper();
            Color BGColor = Color.FromArgb("#ecf7e6");
            wordGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });


            for (int i = 0; i < word.Length; i++)
            {
                if (wordFormat[i] == selectedWord[i])
                {
                    BGColor = Color.FromArgb("#66eb23");
                }
                else if (selectedWord.Contains(wordFormat[i])) {
                    BGColor = Color.FromArgb("#ebed51");
                }            
                else
                {
                    BGColor = Color.FromArgb("#ecf7e6");
                }                
                    
                Frame letterFrame = new Frame
                {

                    Content = new Label
                    {

                        Text = wordFormat[i].ToString(),
                        FontSize = 24,
                        HorizontalTextAlignment = TextAlignment.Center,
                        VerticalTextAlignment = TextAlignment.Center
                    },
                    BorderColor = Colors.Black,
                    Padding = new Thickness(10),
                    CornerRadius = 5,
                    HasShadow = true,
                    BackgroundColor = BGColor,
                };

                wordGrid.Children.Add(letterFrame);

                Grid.SetColumn(letterFrame, i);
                Grid.SetRow(letterFrame, index);
            }
        }

        private void Button_Clicked(object sender, EventArgs e)
        {;
            createWord(selectedWord);
        }
    }

}
