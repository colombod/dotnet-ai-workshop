﻿using System.ClientModel;
using Azure.AI.OpenAI;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace Planner.Tests;

public class PlanExecutorTests
{
    private readonly IConfiguration _configuration;

    public PlanExecutorTests()
    {
        IConfigurationBuilder builder = new ConfigurationBuilder()
            .AddUserSecrets<StructuredChatClientTests>();
        _configuration = builder.Build();
    }

    [Fact]
    public async Task executes_a_single_step_of_a_plan()
    {
        string endpoint = _configuration["AzureOpenAI:Endpoint"] ?? string.Empty;
        string key = _configuration["AzureOpenAI:Key"] ?? string.Empty;
        IChatClient chatClient = new AzureOpenAIClient(
                new Uri(endpoint),
                new ApiKeyCredential(key))

            .AsChatClient("gpt-4o-mini");
        

        PlanExecutor executor = new(chatClient);
        Plan plan = new([
            new PlanStep("find distance from earth to the moon"),
            new PlanStep("calculate necessary fuel for spaceship")
        ]);

        PanStepExecutionResult result = await executor.ExecutePlanStep(plan);
        using AssertionScope scope = new();

        result.Should().NotBeNull();
        result.StepAction.Should().Be("find distance from earth to the moon");
        result.Output.Should().NotBeNullOrWhiteSpace();
    }
}