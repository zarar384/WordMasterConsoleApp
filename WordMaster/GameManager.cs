using System.Collections;
using System.Reflection;
using System.Resources;

namespace WordMaster
{
    public class GameManager
    {
        private Dictionary<string, string> _wordDictionary;
        private int _score = 0;
        private int _mistakes = 0;
        private int _roundNumber = 1;
        private HashSet<string> _words;
        private HashSet<string> _playedWords;
        private HashSet<string> _wrongWords;
        private Dictionary<string, int> _mistakeTracker;  // Track mistakes per word
        private readonly ManualResetEventSlim _timerPauseEvent = new ManualResetEventSlim(true);
        private System.Timers.Timer _timer;
        private int _timeLimitInSeconds = 10; 
        private bool _timeIsUp;

        public GameManager()
        {
            LoadDictionary();
            _words = _wordDictionary.Keys.ToList().ToHashSet();
            _playedWords = new HashSet<string>();
            _wrongWords = new HashSet<string>();
            _mistakeTracker = new Dictionary<string, int>(); 
        }

        public void SetGameSettings(int typeSpeed, int timeLimitInSeconds)
        {
            _timeLimitInSeconds = timeLimitInSeconds;
        }

        // Loading words from resx
        public void LoadDictionary()
        {
            _wordDictionary = new Dictionary<string, string>();
            ResourceManager resourceManager = new ResourceManager($"{typeof(Program).Namespace}.Words", Assembly.GetExecutingAssembly());

            foreach (var entry in resourceManager.GetResourceSet(System.Globalization.CultureInfo.CurrentCulture, true, true).Cast<DictionaryEntry>())
            {
                var word = entry.Key.ToString();
                var translation = entry.Value.ToString();
                _wordDictionary.Add(word, translation);
            }
        }

        public async Task StartGame(IGameState initialState)
        {
            Console.Clear();

            await initialState.HandleState(); // start the round

            if (_mistakeTracker.Any())
            {
                ShowMostMistakes();
            }
        }

        // Gameplay for one round
        public async Task PlayRound(CancellationToken cancellationToken, ManualResetEventSlim pauseEvent)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var wordsToPlay = _wrongWords.Any() ? _wrongWords : _words;
                ShuffleWords(wordsToPlay);

                int totalWords = wordsToPlay.Count;
                List<string> wordsToAddToWrongWords = new List<string>();

                foreach (var word in wordsToPlay.ToList())
                {
                    pauseEvent.Wait(); // Wait if the game is paused
                    cancellationToken.ThrowIfCancellationRequested(); // Interrupt game if canceled

                    _timeIsUp = false;
                    _wordsToPlay = totalWords--;
                    _currentWord = word;
                    Console.Clear(); 
                    Console.WriteLine($"[ESC] Pause. Points: {_score}, Mistakes: {_mistakes}, Round {_roundNumber}");
                    Console.WriteLine($"Words left: {_wordsToPlay}");
                    Console.WriteLine($"\nWhat does the word '{_currentWord}' mean?");

                    var correctTranslation = _wordDictionary[_currentWord];
                    var wrongTranslations = _wordDictionary.Values
                        .Where(v => v != correctTranslation)
                        .OrderBy(x => Guid.NewGuid())
                        .Take(3)
                        .ToList();
                    var allTranslations = new List<string>(wrongTranslations) { correctTranslation };
                    allTranslations.Shuffle();

                    for (int i = 0; i < allTranslations.Count; i++)
                    {
                        Console.WriteLine($"{i + 1}. {allTranslations[i]}");
                    }

                    StartTimer();

                    int userChoice = InputUtils.GetUserChoice(allTranslations, cancellationToken);

                    if (_timeIsUp)
                    {
                        Console.WriteLine("\nYou didn't have time to answer!");
                        if (!wordsToAddToWrongWords.Contains(_currentWord))
                        {
                            wordsToAddToWrongWords.Add(_currentWord);
                        }
                        StopTimer();
                        continue;
                    }

                    await ProcessUserChoice(userChoice, allTranslations, correctTranslation, _currentWord, cancellationToken); // Передаем cancellationToken

                    StopTimer();

                    DateTime startTime = DateTime.Now;
                    while (DateTime.Now - startTime < TimeSpan.FromMilliseconds(1500))
                    {
                        if (Console.KeyAvailable && Console.ReadKey(true).Key == ConsoleKey.Enter)
                        {
                            break;
                        }

                       //cancellationToken.ThrowIfCancellationRequested();
                       //await Task.Delay(100, cancellationToken); /
                    }

                    cancellationToken.ThrowIfCancellationRequested(); // Final check after the round
                }

                foreach (var wrongWord in wordsToAddToWrongWords)
                {
                    _wrongWords.Add(wrongWord);
                }

                if(_wrongWords.Count == 0)
                {
                    break;
                }

                StopTimer();
                _roundNumber++;
            }
        }

        private int _wordsToPlay;
        private string _currentWord;
        public void RedrawGameInterface()
        {
            if (!string.IsNullOrEmpty(_currentWord)) 
            {
                Console.Clear();
                Console.WriteLine($"[ESC] Pause. Points: {_score}, Mistakes: {_mistakes}, Раунд {_roundNumber}");
                Console.WriteLine($"Words left: {_wordsToPlay}");
                Console.WriteLine($"\nWhat does the word  '{_currentWord}' mean?");

                var correctTranslation = _wordDictionary[_currentWord];
                var wrongTranslations = _wordDictionary.Values
                    .Where(v => v != correctTranslation)
                    .OrderBy(x => Guid.NewGuid())
                    .Take(3)
                    .ToList();
                var allTranslations = new List<string>(wrongTranslations) { correctTranslation };
                allTranslations.Shuffle();

                for (int i = 0; i < allTranslations.Count; i++)
                {
                    Console.WriteLine($"{i + 1}. {allTranslations[i]}");
                }

                if (_timeIsUp)
                {
                    Console.WriteLine("");
                    Console.WriteLine("Time is up!");
                }
            }
        }

        private void ShuffleWords(HashSet<string> words)
        {
            var rng = new Random();
            var wordList = words.ToList(); 
            int n = wordList.Count;

            // Fisher-Yates shuffle algorithm
            for (int i = n - 1; i > 0; i--)
            {
                int j = rng.Next(i + 1);
                (wordList[i], wordList[j]) = (wordList[j], wordList[i]);
            }

            words.Clear();
            foreach (var word in wordList)
            {
                words.Add(word);
            }
        }


        private void ShowMostMistakes()
        {
            // Show words with the most mistakes
            var mostMistakes = _mistakeTracker.OrderByDescending(kv => kv.Value)
                                               .Take(5)
                                               .ToList();

            Console.Clear();
            Console.WriteLine($"You did it in {_roundNumber} rounds!");
            Console.WriteLine("Words with the most errors:");
            foreach (var entry in mostMistakes)
            {
                Console.WriteLine($"{entry.Key}: {entry.Value} mistakes");
            }
            Console.WriteLine();
        }

        public bool ShowPauseMenu()
        {
            Console.Clear();
            Console.ResetColor();
            Console.WriteLine("Pause");
            Console.WriteLine("1. Continue");
            Console.WriteLine("2. Save");
            Console.WriteLine("3. Exit to menu");

            while (true)
            {
                int choice = InputUtils.GetUserChoice(3);

                if (choice == 2)
                {
                    SaveGameResult();
                }
                else if (choice == 3)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        // Timer for each question
        private void StartTimer()
        {
            _timeIsUp = false;
            int timeRemaining = _timeLimitInSeconds;

            _timer = new System.Timers.Timer(1000); 
            object consoleLock = new object(); // Lock for synchronization

            _timer.Elapsed += (sender, e) =>
            {
                _timerPauseEvent.Wait(); // Wait if the timer is paused

                lock (consoleLock)
                {
                    if (_timeIsUp) return; // Preventing re-execution

                    if (timeRemaining > 0)
                    {
                        // Save the current cursor position
                        int currentCursorLeft = Console.CursorLeft;
                        int currentCursorTop = Console.CursorTop;

                        Console.SetCursorPosition(0, 1); // Coordinates for displaying the timer
                        Console.Write($"Time left: {timeRemaining--} sec   ");

                        // Восстанавливаем позицию курсора
                        Console.SetCursorPosition(currentCursorLeft, currentCursorTop);
                    }
                    else
                    {
                        _timeIsUp = true;
                        Console.SetCursorPosition(0, Console.CursorTop + 1);
                        Console.WriteLine("Time is up!");
                        StopTimer();
                    }
                }
            };
            _timer.Start();
        }

        public void PauseTimer()
        {
            _timerPauseEvent.Reset(); 
        }

        public void ResumeTimer()
        {
            _timerPauseEvent.Set(); 
        }
        private void StopTimer()
        {
            if (_timer != null)
            {
                _timer.Stop();
                _timer.Dispose();
                _timer = null;
            }
        }

        private async Task ProcessUserChoice(int userChoice, List<string> allTranslations, string correctTranslation, string word, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            string chosenTranslation = allTranslations[userChoice - 1];
            bool isCorrect = chosenTranslation == correctTranslation;

            var animationStrategy = new SimpleAnswerAnimationStrategy();
            await animationStrategy.DisplayAnswerResult(isCorrect);

            if (isCorrect)
            {
                _score++;
                _wrongWords.Remove(word); 
            }
            else
            {
                _mistakes++;

                if (_mistakeTracker.ContainsKey(word))
                {
                    _mistakeTracker[word]++;
                }
                else
                {
                    _mistakeTracker[word] = 1;
                }

                _wrongWords.Add(word);  
            }

            Console.WriteLine($"Points: {_score}, Mistakes: {_mistakes}");
        }

        public void LoadGameState(GameResult gameResult)
        {
            _score = gameResult.Score;
            _mistakes = gameResult.Mistakes;
            _roundNumber = gameResult.RoundNumber;
            _wrongWords = gameResult.WrongWords;
        }

        public void SaveGameResult()
        {
            GameResult result = new GameResult(_score, _mistakes, _roundNumber, _playedWords, _wrongWords);
            GameStorage.SaveGameResult(result);
        }
    }
}
