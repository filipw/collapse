using Collapse;
using Spectre.Console.Cli;

var app = new CommandApp<SimulateCommand>();
return await app.RunAsync(args);