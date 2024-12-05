using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Project2A
{
    //Event delegates used here to avoid making GuessSubmission() in Mainpage static, will likely be changed later
    public class KeyHandling

    {
        public event Action<string> KeyClickedEvent;

        string enteredWord = "";
        public void OnKeyClicked(object sender, EventArgs e)
        {
            if (sender is Button button)
            {
                string key = button.Text;

                if (key == "Enter") //Enter Pressed
                {
                    if (enteredWord.Length == 5)
                    {
                        KeyClickedEvent.Invoke(enteredWord);
                        enteredWord = "";
                    }

                } 
                else if (key == "Delete") // Delete Pressed
                {
                    if (enteredWord.Length > 0)
                    {
                       enteredWord = enteredWord.Remove(enteredWord.Length - 1);
                       Debug.WriteLine($"Entered Word:{enteredWord}");
                    }
                }
                else // Any Key Pressed
                {
                    enteredWord += key;
                    Debug.WriteLine($"Entered Word:{ enteredWord }");
                }
                    
            }
        }
    }
}
