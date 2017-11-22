using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace SEW_Skills_Test
{
    public class Program
    {
        public static Dictionary<char, List<string>> WordsByStartingLetter = new Dictionary<char, List<string>>();  
        public static void Main(string[] args)
        {
            var wordList = JsonConvert.DeserializeObject<List<string>>(File.ReadAllText(Environment.CurrentDirectory + @"\Data\words.json"));
            
            var keys = "abcdefghijklmnopqrstuvwxyz".ToCharArray();
            foreach (var key in keys)
            {
                WordsByStartingLetter.Add(key, wordList.Where(x=> x.StartsWith(key.ToString())).ToList());
            }
            wordList.Sort((x, y) => x.Length - y.Length);
            
            var smallest = wordList[0].Length + wordList[0].Length; //This is the smallest possible length of a word that can contain other words from the list.
            var shortenedWordList = wordList.Where(x => x.Length >= smallest).ToList(); //check only the words large enough to contain other words.

            var output = new List<string>();
            object resultsLock = new object();
            var status = Parallel.ForEach(shortenedWordList, word =>
            {
                if (SubSearch(word, WordsByStartingLetter[word[0]].Where(x => x.Length < word.Length).ToList(), wordList))
                {
                    lock (resultsLock)
                    {
                        output.Add(word);
                    }            
                }
            });

            if (!status.IsCompleted || status.LowestBreakIteration.HasValue)
            {
                Console.WriteLine("Hm. Something went wrong."); //Parallel exited without completing.
                Console.Write("Press any key to exit."); 
                Console.ReadKey();
                return;
            }

            var orderedOutput = output.OrderByDescending(x => x.Length).ThenBy(x => x).ToList();
            if (orderedOutput.Count > 1)
                Console.WriteLine($"We've found our first long word composed of other words! The word: {orderedOutput[0]}, the word length: {orderedOutput[0].Length} ");
            if (orderedOutput.Count > 2)
                Console.WriteLine($"We've found our second long word composed of other words! The word: {orderedOutput[1]}, the word length: {orderedOutput[1].Length} ");

            Console.WriteLine($"All Done! Total big words composed of other words found: {orderedOutput.Count} ");
            Console.Write("Press any key to exit.");
            Console.ReadKey();
        }


        public static bool SubSearch(string currentWord, List<string> filteredSubWords, List<string> wordList)
        {
            foreach (var word in filteredSubWords)
            {
                if (currentWord.StartsWith(word))
                {
                    if (currentWord == word)
                    {
                        return true;
                    }

                    var nextWord = currentWord.Remove(0, word.Length);
                    if (SubSearch(nextWord, WordsByStartingLetter[nextWord[0]].Where(x => x.Length <= nextWord.Length).ToList(), wordList))
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
