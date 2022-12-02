using Collapse;
using Spectre.Console.Cli;

var app = new CommandApp();
app.SetDefaultCommand<SimulateCommand>();
app.Configure(c =>
{
    c.AddCommand<SimulateCommand>("sim");
});
return await app.RunAsync(args);