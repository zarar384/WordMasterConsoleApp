namespace WordMaster
{
    public static class InputUtils
    {
        public static int GetUserChoice(int numberOfChoices = 2)
        {
            int choice;
            while (!int.TryParse(Console.ReadLine(), out choice) || choice < 1 || choice > numberOfChoices)
            {
                Console.WriteLine($"Please select 1 or {numberOfChoices}.");
            }
            return choice;
        }

        public static int GetUserChoice(List<string> allTranslations, CancellationToken cancellationToken = default)
        {
            return GetUserChoice(allTranslations, 4, cancellationToken);
        }
           
        public static int GetUserChoice(List<string> allTranslations, int numberOfChoices = 4, CancellationToken cancellationToken = default)
        {
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (Console.KeyAvailable)
                {
                    var key = Console.ReadKey(intercept: true).Key;

                    if (key == ConsoleKey.Escape)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                    }

                    if (char.IsDigit((char)key))
                    {
                        int choice = (int)char.GetNumericValue((char)key);
                        if (choice >= 1 && choice <= numberOfChoices)
                        {
                            return choice;
                        }
                    }
                }

                // A small delay to reduce CPU load
                Task.Delay(50, cancellationToken).Wait(cancellationToken);
            }
        }
    }
}
