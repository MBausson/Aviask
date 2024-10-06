using System.Globalization;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.RateLimiting;
using AviaskApi.Entities;
using AviaskApi.Identity;
using AviaskApi.Jobs;
using AviaskApi.Models;
using AviaskApi.Repositories;
using AviaskApi.Services.Attachment;
using AviaskApi.Services.Filterable;
using AviaskApi.Services.FreeQuestionsPool;
using AviaskApi.Services.HtmlContentSanitizer;
using AviaskApi.Services.Jwt;
using AviaskApi.Services.Mailer;
using AviaskApi.Services.MockExaminer;
using AviaskApi.Services.RecoveryToken;
using AviaskApi.Services.StripeService;
using AviaskApi.Validators;
using Bogus;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Microsoft.VisualBasic;
using Newtonsoft.Json;
using Quartz;
using Stripe;
using File = System.IO.File;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace AviaskApi.Configuration;

public static class DependencyInjection
{
    public static IServiceCollection AddLogger(this IServiceCollection services)
    {
        services.AddLogging(logging =>
        {
            logging.ClearProviders();
            logging.AddConsole();
            logging.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.Warning);
        });

        return services;
    }

    public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddControllers().AddNewtonsoftJson(options =>
        {
            options.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Utc;
            options.SerializerSettings.DateFormatHandling = DateFormatHandling.IsoDateFormat;
            options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
        });

        services.Configure<ApiBehaviorOptions>(options => { options.SuppressModelStateInvalidFilter = true; });

        services.AddScoped<AviaskApiContext>();
        services.AddDbContext<AviaskApiContext>(optionsAction =>
        {
            optionsAction.UseNpgsql(Env.Get("CONNECTIONSTRINGS_AVIASKAPICONTEXT"));
        });

        services.AddScoped<IFilterableService, FilterableService>();

        return services;
    }

    public static IServiceCollection AddStripe(this IServiceCollection services, IConfiguration configuration)
    {
        StripeConfiguration.ApiKey = Env.Get("STRIPE_SECRET_KEY");

        var appInfo = new AppInfo
        {
            Name = "Aviask",
            Version = "0.1.0"
        };

        StripeConfiguration.AppInfo = appInfo;

        services.AddHttpClient("Stripe");
        services.AddTransient<IStripeClient, StripeClient>(s =>
        {
            var clientFactory = s.GetRequiredService<IHttpClientFactory>();
            var httpClient = new SystemNetHttpClient(
                clientFactory.CreateClient("Stripe"),
                3,
                appInfo);

            return new StripeClient(StripeConfiguration.ApiKey, null, httpClient);
        });

        return services;
    }

    public static IServiceCollection AddJobs(this IServiceCollection services)
    {
        if (SetupConfiguration.IsTest) return services;

        services.AddQuartz(q =>
        {
            q.AddJob<FreeQuestionsRefreshJob>(options => options.WithIdentity(nameof(FreeQuestionsRefreshJob)));
            q.AddTrigger(options =>
            {
                options
                    .ForJob(nameof(FreeQuestionsRefreshJob))
                    .WithIdentity(nameof(FreeQuestionsRefreshJob) + "_trigger")
                    .StartNow()
                    .WithSimpleSchedule(s => s
                        .WithIntervalInHours(24)
                        .RepeatForever()
                    )
                    .WithDescription(
                        "Trigger that specifies questions available to non-premium users, refreshed everyday");
            });
        });

        services.AddQuartzHostedService(q => q.WaitForJobsToComplete = false);
        services.AddScoped<IJob, FreeQuestionsRefreshJob>();
        services.AddScoped<IJob, MockExamTimerJob>();

        return services;
    }

    public static IServiceCollection AddSwagger(this IServiceCollection services)
    {
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
                { Title = "Aviask API", Version = "v1" });
        });

        return services;
    }

    public static IServiceCollection AddIdentity(this IServiceCollection services)
    {
        services.AddIdentity<AviaskUser, AviaskUserRole>(options =>
        {
            options.Password.RequiredLength = 6;
            options.Password.RequireDigit = false;
            options.Password.RequireUppercase = false;
            options.Password.RequireLowercase = false;
            options.Password.RequireNonAlphanumeric = false;
        }).AddEntityFrameworkStores<AviaskApiContext>();

        return services;
    }

    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IAviaskRepository<Question, Guid>, QuestionRepository>();
        services.AddScoped<IAviaskRepository<AviaskUser, Guid>, UserRepository>();
        services.AddScoped<IAviaskRepository<AnswerRecord, Guid>, AnswerRecordRepository>();
        services.AddScoped<IAviaskRepository<QuestionReport, Guid>, QuestionReportRepository>();
        services.AddScoped<IAviaskRepository<Attachment, Guid>, AttachmentRepository>();
        services.AddScoped<IAviaskRepository<RecoveryToken, Guid>, RecoveryTokenRepository>();
        services.AddScoped<IAviaskRepository<MockExam, Guid>, MockExamRepository>();

        return services;
    }

    public static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddScoped<IFreeQuestionsPoolService, FreeQuestionsPoolService>();
        services.AddScoped<IAttachmentService, AttachmentService>();
        services.AddScoped<IHtmlContentSanitizerService, HtmlContentSanitizerService>();
        services.AddScoped<IRecoveryTokenService, RecoveryTokenService>();
        services.AddScoped<IStripeService, StripeService>();
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IMailerService, MailerService>();
        services.AddScoped<IMockExaminer, MockExaminer>();

        return services;
    }

    public static IServiceCollection AddValidators(this IServiceCollection services)
    {
        ValidatorOptions.Global.LanguageManager.Culture = CultureInfo.GetCultureInfo("en-Us");

        services.AddScoped<IValidator<Question>, QuestionValidator>();
        services.AddScoped<IValidator<LoginModel>, LoginModelValidator>();
        services.AddScoped<IValidator<CreateUserModel>, CreateUserModelValidator>();
        services.AddScoped<IValidator<EditUserModel>, EditUserModelValidator>();
        services.AddScoped<IValidator<CreateQuestionReportModel>, CreateQuestionReportValidator>();
        services.AddScoped<IValidator<PasswordResetModel>, PasswordResetModelValidator>();

        return services;
    }

    public static IServiceCollection AddRateLimitServices(this IServiceCollection services)
    {
        if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")?.ToLower() == "test") return services;

        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            options.AddPolicy("fixed", context => RateLimitPartition.GetFixedWindowLimiter(
                context.Connection.RemoteIpAddress?.ToString(), _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 15,
                    Window = TimeSpan.FromSeconds(2)
                }));
        });

        return services;
    }

    public static IServiceCollection AddCorsPolicies(this IServiceCollection services)
    {
        services.AddCors(options =>
        {
            //  Todo: be more restrictive on these
            options.AddPolicy("AllowAnyOrigin", builder =>
            {
                builder.AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowAnyOrigin();
            });
        });

        return services;
    }

    public static IServiceCollection AddAuth(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(o =>
        {
            o.TokenValidationParameters = new TokenValidationParameters
            {
                ValidIssuer = Env.Get("JWT_ISSUER"),
                ValidAudience = Env.Get("JWT_AUDIENCE"),
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Env.Get("JWT_KEY"))),
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true
            };
        });

        services.AddAuthorization(options =>
        {
            options.AddPolicy(AuthorizationPolicyConstants.Administration, policy => policy.RequireRole("admin"));
            options.AddPolicy(AuthorizationPolicyConstants.QuestionModeration,
                policy => policy.RequireRole("manager", "admin"));
            options.AddPolicy(AuthorizationPolicyConstants.AuthenticatedUsers,
                policy => policy.RequireAuthenticatedUser());

            options.DefaultPolicy = options.GetPolicy(AuthorizationPolicyConstants.AuthenticatedUsers)!;
        });

        return services;
    }

    public static async Task<IServiceProvider> AddRolesIfNotExist(this IServiceProvider services)
    {
        using (var serviceScope = services.CreateScope())
        {
            var roleManager = serviceScope.ServiceProvider.GetService<RoleManager<AviaskUserRole>>()!;

            var availableRoles = new[] { "member", "manager", "admin" };

            foreach (var role in availableRoles)
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new AviaskUserRole
                    {
                        Name = role
                    });
        }

        return services;
    }

    public static async Task<IServiceProvider> StartOngoingMockExamsTimer(this IServiceProvider services)
    {
        if (SetupConfiguration.IsTest) return services;

        using (var serviceScope = services.CreateScope())
        {
            var mockExaminer = serviceScope.ServiceProvider.GetService<IMockExaminer>()!;

            await mockExaminer.StartOngoingMockExamTimersAsync();
        }

        return services;
    }

    public static async Task<IServiceProvider> CreateDatabaseIfNotExists(this IServiceProvider services)
    {
        using (var serviceScope = services.CreateScope())
        {
            var dbContext = serviceScope.ServiceProvider.GetService<AviaskApiContext>();
            var logger = serviceScope.ServiceProvider.GetService<ILogger<AviaskApiContext>>();

            //  If any error occurs whilst creating a new DB, delete it
            //  CanConnectAsync indicates whether or not the DB was present before migrating
            var exists = await dbContext.Database.CanConnectAsync();

            try
            {
                logger.LogInformation("Migrating the databse...");
                await dbContext.Database.MigrateAsync();
            }
            catch
            {
                if (!exists)
                {
                    logger.LogWarning("An error occured whilst creating the database. It has been deleted.");
                    await dbContext.Database.EnsureDeletedAsync();
                }

                throw;
            }
        }

        return services;
    }

    public static async Task<IServiceProvider> SeedDatabase(this IServiceProvider services)
    {
        /*
         * For each user, create 10 questions for them
         */

        using var serviceScope = services.CreateScope();
        var dbContext = serviceScope.ServiceProvider.GetRequiredService<AviaskApiContext>();

        var questionAnswerFaker = new Faker<QuestionAnswers>()
            .RuleFor(o => o.Answer1, f => f.Vehicle.Manufacturer())
            .RuleFor(o => o.Answer2, f => f.Vehicle.Manufacturer())
            .RuleFor(o => o.Answer3, f => f.Vehicle.Manufacturer())
            .RuleFor(o => o.Answer4, f => f.Vehicle.Manufacturer())
            .RuleFor(o => o.CorrectAnswer, (f, m) =>
            {
                if (f.Random.Bool()) return m.Answer1;

                return m.Answer2;
            })
            .RuleFor(o => o.Explications, f => f.Lorem.Sentences(2));

        var questionFaker = new Faker<Question>().RuleFor(o => o.Id, f => f.Random.Uuid())
            .RuleFor(o => o.Title, f => f.Name.FullName())
            .RuleFor(o => o.Description, f => Strings.Join(f.Lorem.Words(12)))
            .RuleFor(o => o.Category, f => f.PickRandomWithout(Category.NULL))
            .RuleFor(o => o.Source, f => f.Company.CompanyName())
            .RuleFor(o => o.QuestionAnswers, _ => questionAnswerFaker.Generate())
            .RuleFor(o => o.QuestionAnswersId, (_, m) => m.QuestionAnswers.Id)
            .RuleFor(o => o.Publisher, _ => null)
            .RuleFor(o => o.PublisherId, (_, _) => null)
            .RuleFor(o => o.PublishedAt, f => f.Date.Recent().ToUniversalTime())
            .RuleFor(o => o.Visibility, _ => QuestionVisibility.PRIVATE);

        foreach (var user in await dbContext.AviaskUsers.ToArrayAsync())
        {
            var questions = questionFaker.Generate(10).Select(q =>
            {
                q.Publisher = user;
                q.PublisherId = user.Id;

                return q;
            });

            await dbContext.AddRangeAsync(questions);
            await dbContext.SaveChangesAsync();
        }

        return services;
    }
}
