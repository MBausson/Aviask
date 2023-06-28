namespace Aviask.Models.ResponseModels
{
    public record UserStatisticsResponse(
        int LastWeekRecordsCount,
        int CorrectLifetimeCount,
        int FailLifetimeCount,
        float RatioCorrectness,
        StatisticsCategoryResponse MostCorrectCategory,
        List<StatisticsCategoryResponse> MostAnsweredCategories);
}
