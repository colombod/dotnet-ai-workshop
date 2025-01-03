using System.ComponentModel;
using System.Text.Json.Serialization;

namespace Planner;

//[Description("The plan to execute")]
//public record Plan([Description("The list of steps for the plan")] PlanStep[] Steps);

[JsonConverter(typeof(UnionTypeConverter<Plan>))]
public abstract record Plan
{
    [JsonDerivedType(typeof(Plan), typeDiscriminator: "withSteps")]
    [Description("The plan to execute")]
    public record PlanWithSteps([Description("The list of steps for the plan")] PlanStep[] Steps) : Plan;

    [JsonDerivedType(typeof(Plan), typeDiscriminator: "result")]
    [Description("The result of a plan")]
    public record PlanResult([Description("The outcome of the plan")] string Result) : Plan;
}
