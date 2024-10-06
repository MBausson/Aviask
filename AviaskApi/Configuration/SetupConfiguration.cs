using dotenv.net;

namespace AviaskApi.Configuration;

public static class SetupConfiguration
{
    public static bool IsTest => Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")?.ToLower() == "test";

    public static IConfigurationManager AddDotEnv(this IConfigurationManager manager, bool isDevelopment = false)
    {
        manager.AddEnvironmentVariables();

        List<string> envFiles = [".env"];

        if (isDevelopment)
            envFiles.Add(".env.development");
        else if (IsTest) envFiles.Add(".env.testing");

        DotEnv.Fluent()
            .WithExceptions()
            .WithEnvFiles(envFiles.ToArray())
            .WithOverwriteExistingVars()
            .WithoutExceptions()
            .Load();

        return manager;
    }
}