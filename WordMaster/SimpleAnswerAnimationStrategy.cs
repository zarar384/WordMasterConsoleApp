namespace WordMaster
{
    public class SimpleAnswerAnimationStrategy : IAnswerAnimationStrategy
    {
        public async Task DisplayAnswerResult(bool isCorrect)
        {
            if (isCorrect)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                await DisplayAnimation("Right!");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                await DisplayAnimation("Wrong!");
            }
            Console.ResetColor();
        }

        private async Task DisplayAnimation(string message)
        {
            int originalCursorTop = Console.CursorTop;
            int originalCursorLeft = Console.CursorLeft;

            foreach (var c in message)
            {
                if (Console.KeyAvailable && Console.ReadKey(intercept: true).Key == ConsoleKey.Enter)
                {
                    // Move cursor to beginning of line and overwrite current line
                    Console.SetCursorPosition(originalCursorLeft, originalCursorTop);
                    Console.Write(new string(' ', message.Length)); 
                    Console.SetCursorPosition(originalCursorLeft, originalCursorTop); 
                    Console.WriteLine(message);
                    return;
                }

                Console.Write(c);
                await Task.Delay(50); // Pause for animation
            }
            Console.WriteLine();
        }
    }
}
