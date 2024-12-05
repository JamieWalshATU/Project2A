using System.Diagnostics;

namespace Project2A
{
    //SortedWords creates a collection of words and shuffles them
    public class SortedWords
    {
        //Creates a list to hold sorted words
        private List<string> _wordListSorted = new List<string>();
        public List<string> WordListSorted
        {
            get => _wordListSorted;
            set => _wordListSorted = value;
        }

        private readonly WordViewModel _wordViewModel;

        public SortedWords(WordViewModel wordViewModel)
        {
            _wordViewModel = wordViewModel;
        }

        //Sorts words and creates a subset of 30
        public void SortWords()
        {
            if (_wordViewModel.WordList != null && _wordViewModel.WordList.Count > 0)
            {

                Random random = new Random();
                List<string> selectedWords = new List<string>();
                //Copy of list to avoid modifications
                var wordListCopy = new List<string>(_wordViewModel.WordList);

                //Selects 30 words
                for (int i = 0; i < 30 && wordListCopy.Count > 0; i++)
                {
                    int randomIndex = random.Next(wordListCopy.Count);
                    selectedWords.Add(wordListCopy[randomIndex]);
                    wordListCopy.RemoveAt(randomIndex);
                }
                //Sorts words alphabetically, this is helpful for when debuging but is not nessecary
                selectedWords.Sort(StringComparer.Ordinal);
                WordListSorted = selectedWords;
                Debug.WriteLine($"Sorted {WordListSorted.Count} words.");
            }
            else
            {
                Debug.WriteLine("WordList is empty or null.");
            }
        }
    }
}

