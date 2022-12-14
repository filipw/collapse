using System.Text.Json;

namespace Collapse;

record AzureExecutionResponse 
{
    public JsonElement[] Histogram { get; init; }
}