namespace WordMaster
{
    public class GameResult
    {
        public int Score { get; set; }
        public int Mistakes { get; set; }
        public DateTime Date { get; set; }
        public int RoundNumber { get; set; }
        public HashSet<string> Words { get; set; }
        public HashSet<string> WrongWords { get; set; } 

        
        public GameResult(int score, int mistakes, int roundNumber, HashSet<string> words, HashSet<string> wrongWords)
        {
            Score = score;
            Mistakes = mistakes;
            RoundNumber = roundNumber;
            Date = DateTime.Now;
            Words = words;
            WrongWords = wrongWords; 
        }
    }
}
