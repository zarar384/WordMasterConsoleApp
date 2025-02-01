namespace WordMaster
{
    public interface IAnswerAnimationStrategy
    {
        Task DisplayAnswerResult(bool isCorrect);
    }
}
