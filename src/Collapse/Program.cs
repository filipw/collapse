using Collapse;
using Spectre.Console.Cli;

var app = new CommandApp();
app.Configure(c =>
{
    c.AddCommand<SimulateCommand>("sim");
    c.AddCommand<AzureCommand>("azure");
});
return await app.RunAsync(args);