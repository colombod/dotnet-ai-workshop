using Microsoft.Extensions.AI;

namespace Planner;

public class PlanExecutor(IChatClient chatClient)
{
    public async Task<PanStepExecutionResult> ExecutePlanStep(Plan plan, CancellationToken cancellationToken = default)
    {
        string planString = string.Join("\n", plan.Steps.Select((step,i) => $"{i+1}. {step.Action}"));
        var task = plan.Steps[0];
        string prompt = $"""
                         For the following plan:
                         {planString}

                         You are tasked with executing step 1, {task.Action}.
                         """;
        var response = await chatClient.CompleteAsync([new ChatMessage(ChatRole.User, prompt)], cancellationToken: cancellationToken);
        string? output = response.Message.Text;
        return new PanStepExecutionResult(task.Action, Output:output??string.Empty);

    }
}
