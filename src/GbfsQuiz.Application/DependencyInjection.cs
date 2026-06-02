using GbfsQuiz.Application.Features.Auth;
using GbfsQuiz.Application.Features.Auth.Interfaces;
using GbfsQuiz.Application.Features.Games;
using GbfsQuiz.Application.Features.Games.Interfaces;
using GbfsQuiz.Application.Features.Leaderboard;
using GbfsQuiz.Application.Features.Leaderboard.Interfaces;
using GbfsQuiz.Application.Features.Players;
using GbfsQuiz.Application.Features.Players.Interfaces;
using GbfsQuiz.Application.Features.Quiz;
using GbfsQuiz.Application.Features.Quiz.Interfaces;
using GbfsQuiz.Application.Features.Quiz.Strategies;
using Microsoft.Extensions.DependencyInjection;

namespace GbfsQuiz.Application;

/// <summary>Registers application services: the game loop, quiz engine and question strategies.</summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IGameService, GameService>();
        services.AddScoped<IQuizService, QuizService>();
        services.AddScoped<ILeaderboardService, LeaderboardService>();
        services.AddScoped<IAvatarService, AvatarService>();
        AddQuestionStrategies(services);
        return services;
    }

    private static void AddQuestionStrategies(IServiceCollection services)
    {
        services.AddScoped<IQuestionStrategy, MostBikesCityStrategy>();
        services.AddScoped<IQuestionStrategy, BiggestNetworkStrategy>();
        services.AddScoped<IQuestionStrategy, BikesInCityStrategy>();
        services.AddScoped<IQuestionStrategy, StationCountStrategy>();
        services.AddScoped<IQuestionStrategy, NearestStationStrategy>();
    }
}
