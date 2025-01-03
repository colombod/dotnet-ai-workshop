using System.ClientModel;
using Azure.AI.OpenAI;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace Planner.Tests;

public class PlanEvaluatorTests
{
    private readonly IConfigurationRoot _configuration;

    public PlanEvaluatorTests()
    {
        var builder = new ConfigurationBuilder()
            .AddUserSecrets<StructuredChatClientTests>();
        _configuration = builder.Build();
    }

    [Fact]
    public async Task generates_result_if_all_steps_are_performed()
    {
        string endpoint = _configuration["meai:endpoint"] ?? string.Empty;
        string key = _configuration["meai:apikey"] ?? string.Empty;
        var chatClient = new AzureOpenAIClient(
                new Uri(endpoint),
                new ApiKeyCredential(key))

            .AsChatClient("gpt-4o-mini");
        var planEvaluator = new PlanEvaluator(chatClient);

        var plan = new Plan([
            new PlanStep("calculate necessary fuel for spaceship to cover distance between earth and the moon")
        ]);

        string task = "find how much fuel a spaceship needs to reach the moon from earth";

        PanStepExecutionResult[] previousSteps = [
            new ("find distance from earth to the moon", "The distance from earth to the moon is 384,400 km"),
            new ("Find out ship fuel consumptions", "The spaceship needs 1 gallons of fuel per 1000km"),
            new("calculate necessary fuel for spaceship to cover distance between earth and the moon", "The ship will consume 384.4 gallons to cover the distance between the earth adn the moon")
        ];

        var planOrResult = await planEvaluator.EvaluatePlanAsync(task, plan, previousSteps);

        using var scope = new AssertionScope();
        planOrResult.Result.Should().NotBeNull();
        planOrResult.Result.Outcome.Should().NotBeNullOrWhiteSpace();
        planOrResult.Result.Outcome.Should().Contain("384.4");
    }

    [Fact]
    public async Task generates_updated_plan()
    {
        string endpoint = _configuration["meai:endpoint"] ?? string.Empty;
        string key = _configuration["meai:apikey"] ?? string.Empty;
        var chatClient = new AzureOpenAIClient(
                new Uri(endpoint),
                new ApiKeyCredential(key))

            .AsChatClient("gpt-4o-mini");
        var planEvaluator = new PlanEvaluator(chatClient);

        var plan = new Plan([
            new PlanStep("find distance from earth to the moon"),
            new PlanStep("Find out ship fuel consumptions"),
            new PlanStep("calculate necessary fuel for spaceship")
        ]);

        string task = "find how much fuel a spaceship needs to reach the moon from earth";

        PanStepExecutionResult[] previousSteps = [
            new PanStepExecutionResult("find distance from earth to the moon", "The distance from earth to the moon is 384,400 km")
        ];

        var planOrResult = await planEvaluator.EvaluatePlanAsync(task, plan, previousSteps);

        planOrResult.Plan.Should().NotBeNull();
    }
}
