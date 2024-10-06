namespace AviaskApi.Services.FreeQuestionsPool;

public class FreeQuestionsPoolService : IFreeQuestionsPoolService
{
    private static IEnumerable<Guid> _questionIdsPool = Enumerable.Empty<Guid>();

    public void SetPool(IEnumerable<Guid> enumerable)
    {
        _questionIdsPool = enumerable;
    }

    public virtual IEnumerable<Guid> GetQuestionsIds()
    {
        return _questionIdsPool;
    }

    public virtual bool IsQuestionInPool(Guid questionId)
    {
        return _questionIdsPool.Contains(questionId);
    }
}