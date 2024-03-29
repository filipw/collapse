using Spectre.Console.Cli;
using System.ComponentModel;

namespace Collapse;

public sealed class SimulateCommandSettings : CommandSettings
    {
        [Description("Path to search for the Q# program. Defaults to current directory.")]
        [CommandArgument(0, "<searchPath>")]
        public string Path { get; init; }

        [Description("Number of shots. Defaults to 10")]
        [CommandOption("--shots")]
        [DefaultValue(10)]
        public int Shots { get; init; }

        [Description("Use QIR. Defaults to false")]
        [CommandOption("--qir")]
        [DefaultValue(false)]
        public bool Qir { get; init; }

        [Description("QIR simulator path. Defaults to empty (taken from PATH).")]
        [CommandOption("--qir-runner")]
        public string QirRunner { get; init; }

        [Description("Suppress building the application. Defaults to false.")]
        [CommandOption("--skip-build")]
        [DefaultValue(false)]
        public bool SkipBuild { get; init; }

        [Description("Determines if collapse should run the runner multiple times, or yield to the runner to handle the shots internally. Defaults to true.")]
        [CommandOption("--no-orchestration")]
        [DefaultValue(true)]
        public bool NoOrchestration { get; init;}
    }
