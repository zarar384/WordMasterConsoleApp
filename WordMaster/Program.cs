
namespace WordMaster
{
    class Program
    {
        static GameManager gameManager;

        static async Task Main(string[] args)
        {
            gameManager = new GameManager();

            Console.WriteLine("Do you want to continue your last game or start a new one?");
            Console.WriteLine("1. Load Game");
            Console.WriteLine("2. New Game");

            int choice = InputUtils.GetUserChoice();
            IGameState gameState = null;

            if (choice == 1)
            {
                // Show list of saved games
                List<string> savedGames = GameStorage.GetSavedGames();
                if (savedGames.Any())
                {
                    Console.WriteLine("Select a game to continue:");
                    for (int i = 0; i < savedGames.Count; i++)
                    {
                        Console.WriteLine($"{i + 1}. {savedGames[i]}");
                    }

                    int gameChoice = InputUtils.GetUserChoice(savedGames.Count);
                    string selectedGame = savedGames[gameChoice - 1];

                    // Loading a saved game
                    GameResult gameResult = GameStorage.LoadGameResult(selectedGame);
                    if (gameResult != null)
                    {
                        Console.WriteLine($"Game loaded: {selectedGame}");
                        gameManager.LoadGameState(gameResult);
                        gameState = new PlayingState(gameManager);
                    }
                    else
                    {
                        Console.WriteLine("Failed to load game.");
                        gameState = new PlayingState(gameManager);
                    }
                }
                else
                {
                    Console.WriteLine("No saved games.");
                    gameState = new PlayingState(gameManager);
                }
            }
            else
            {
                // Start a new game
                gameState = new PlayingState(gameManager);
            }

            // Запуск игры
            await gameManager.StartGame(gameState);

            Console.WriteLine("Game over. Return to menu...");
            await Main(args);
        }
    }
}