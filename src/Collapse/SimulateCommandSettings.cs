using Spectre.Console.Cli;
using System.ComponentModel;

namespace Collapse;

public sealed class SimulateCommandSettings : CommandSettings
    {
        [Description("Path to search for the Q# program. Defaults to current directory.")]
        [CommandArgument(0, "[searchPath]")]
        public string Path { get; init; }

        [Description("Number of shots. Defaults to 10")]
        [CommandOption("--shots")]
        [DefaultValue(10)]
        public int Shots { get; init; }

        [Description("Suppress building the application. Defaults to false.")]
        [CommandOption("--skip-build")]
        [DefaultValue(false)]
        public bool SkipBuild { get; init; }
    }
