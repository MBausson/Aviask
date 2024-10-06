using AviaskApi.Models.Details;

namespace AviaskApi.Models.Result;

public record UserProfileResult(AviaskUserDetails UserDetails, int AcceptedSuggestions, int AnswersCount);