namespace AviaskApi.Services.FreeQuestionsPool;

public interface IFreeQuestionsPoolService
{
    public void SetPool(IEnumerable<Guid> enumerable);

    public IEnumerable<Guid> GetQuestionsIds();

    public bool IsQuestionInPool(Guid questionId);
}