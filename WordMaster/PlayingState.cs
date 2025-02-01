namespace WordMaster
{
    public class PlayingState : IGameState
    {
        private readonly GameManager _gameManager;
        private readonly IAnswerAnimationStrategy _animationStrategy;
        private readonly ManualResetEventSlim _pauseEvent = new(true);

        public PlayingState(GameManager gameManager)
        {
            _gameManager = gameManager;
            _animationStrategy = new SimpleAnswerAnimationStrategy();
        }

        public async Task HandleState()
        {
            var cts = new CancellationTokenSource(); 
            var keyTask = Task.Run(() => ListenForKeyPresses(cts)); 

            try
            {
                while (!cts.Token.IsCancellationRequested)
                {
                    _pauseEvent.Wait(); 
                    await _gameManager.PlayRound(cts.Token, _pauseEvent); 
                    cts.Cancel();
                }
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("Game over.");
            }
            finally
            {
                await keyTask;
            }
        }

        private void ListenForKeyPresses(CancellationTokenSource cts)
        {
            while (!cts.Token.IsCancellationRequested)
            {
                if (Console.KeyAvailable)
                {
                    var key = Console.ReadKey(true).Key;
                    if (key == ConsoleKey.Escape)
                    {
                        _pauseEvent.Reset(); // Pause the game
                        _gameManager.PauseTimer(); 

                        bool exitGame = _gameManager.ShowPauseMenu();

                        if (exitGame)
                        {
                            cts.Cancel(); // Stop the game
                        }
                        else
                        {
                            Console.Clear(); 
                            _pauseEvent.Set(); // Resuming the game
                            _gameManager.ResumeTimer(); 
                            _gameManager.RedrawGameInterface();
                        }
                    }
                }

                Task.Delay(100).Wait(); // Small delay to check keys
            }
        }
    }
}
