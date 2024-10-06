using dotenv.net.Utilities;

namespace AviaskApi.Configuration;

public static class Env
{
    /// <summary>
    ///     Tries to get the value of an environment variable
    /// </summary>
    /// <param name="key">Variable name</param>
    /// <param name="fallback">Fallback value if the variable is not found</param>
    /// <remarks>This function searches in .env files first, then in Environment variables</remarks>
    /// <returns>Env variable value</returns>
    public static string Get(string key, string? fallback = null)
    {
        EnvReader.TryGetStringValue(key, out var value);

        return value ?? Environment.GetEnvironmentVariable(key) ?? fallback;
    }
}