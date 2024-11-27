using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project2A
{
    public class SortedWords
    {
        private ObservableCollection<string> _wordListSorted = new ObservableCollection<string>();
        public ObservableCollection<string> WordListSorted
        {
            get => _wordListSorted;
            set
            {
                _wordListSorted = value;
                OnPropertyChanged(nameof(WordListSorted));
            }
        }

        private readonly WordViewModel _wordViewModel;

        public SortedWords(WordViewModel wordViewModel)
        {
            _wordViewModel = wordViewModel;
        }

        public void SortWords()
        {
            if (_wordViewModel.WordList != null)
            {

                Random random = new Random();
                List<string> selectedWords = new List<string>();

                var wordListCopy = new List<string>(_wordViewModel.WordList);

                for (int i = 0; i < 30 && wordListCopy.Count > 0; i++)
                {
                    int randomIndex = random.Next(wordListCopy.Count);
                    selectedWords.Add(wordListCopy[randomIndex]);
                    wordListCopy.RemoveAt(randomIndex);
                }

                selectedWords.Sort(StringComparer.Ordinal);
                WordListSorted = new ObservableCollection<string>(selectedWords);

                OnPropertyChanged(nameof(WordListSorted));
                }
            }

        
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
