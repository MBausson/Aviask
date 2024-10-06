using AviaskApi.Entities;
using AviaskApi.Models.Details;

namespace AviaskApi.Models.Result;

public record LeaderboardUser(AviaskUserDetails User, int QuestionsCount, int Rank)
{
    public static LeaderboardUser FromData(AviaskUser user, int zeroBasedIndex)
    {
        return new LeaderboardUser(user.GetDetails(), user.Publications.Count, zeroBasedIndex + 1);
    }
}

public class CurrentLeaderboard
{
    public LeaderboardUser[] Users { get; set; } = [];
    public int CurrentUserCount { get; set; }
}