import argparse
import os
import sys
import json
import re
from typing import List, Any, Counter

from collapse.core import format_result, create_histogram
from collapse.runner import QSharpRunner

def run_from_file(file_path: str, shots: int, entrypoint: str) -> List[Any]:
    if not os.path.exists(file_path):
        raise FileNotFoundError(f"Q# file not found: {file_path}")
    
    runner = QSharpRunner(file_path=file_path, entry_point=entrypoint)
    return runner.run(shots=shots)


def run_from_source(source_code: str, shots: int, entrypoint: str) -> List[Any]:
    runner = QSharpRunner(source_code=source_code, entry_point=entrypoint)
    return runner.run(shots=shots)


def main():
    parser = argparse.ArgumentParser(
        description="Collapse - Run Q# code and visualize results in a histogram"
    )
    
    input_group = parser.add_mutually_exclusive_group(required=True)
    input_group.add_argument(
        "-f", "--file", 
        help="Path to the Q# file to execute"
    )
    input_group.add_argument(
        "-s", "--source", 
        help="Q# source code as a string"
    )
    input_group.add_argument(
        "--stdin", 
        action="store_true",
        help="Read Q# source code from standard input"
    )
    
    parser.add_argument(
        "-e", "--entrypoint",
        default="Main()",
        help="Entry point expression to run (default: Main())"
    )

    parser.add_argument(
        "-n", "--shots",
        type=int,
        default=1024,
        help="Number of shots to run (default: 1024)"
    )
    
    parser.add_argument(
        "--width",
        type=int,
        default=80,
        help="Maximum width of the histogram (default: 80)"
    )
    
    parser.add_argument(
        "--output",
        choices=["text", "json"],
        default="text",
        help="Output format (default: text)"
    )
    
    parser.add_argument(
        "--save",
        help="Save results to the specified file"
    )
    
    parser.add_argument(
        "--verbose",
        action="store_true",
        help="Show detailed information about the Q# execution"
    )
    
    parser.add_argument(
        "--no-color",
        action="store_true", 
        help="Disable colorized output"
    )
    
    parser.add_argument(
        "--max-bar",
        type=int,
        default=50,
        help="Maximum bar length in characters (default: 50)"
    )
    
    args = parser.parse_args()
    
    try:
        use_colors = not args.no_color and sys.stdout.isatty()
        
        if args.verbose:
            print(f"Running Q# code with {args.shots} shots...")
        
        if args.file:
            if args.verbose:
                print(f"Loading Q# code from file: {args.file}")
            results = run_from_file(args.file, args.shots, args.entrypoint)
        elif args.source:
            if args.verbose:
                print("Running Q# code from source string")
            results = run_from_source(args.source, args.shots, args.entrypoint)
        elif args.stdin:
            if args.verbose:
                print("Reading Q# code from standard input")
            source_code = sys.stdin.read()
            results = run_from_source(source_code, args.shots, args.entrypoint)
        
        if args.verbose:
            print(f"Execution completed. Processing {len(results)} results...")
        
        histogram = create_histogram(results, max_width=args.width, colorful=use_colors)
        
        if args.output == "text":
            output = histogram
        else:  # json
            formatted_results = [format_result(r) for r in results]
            counter = Counter(formatted_results)
            output = json.dumps({
                "results": {k: v for k, v in counter.items()},
                "total_shots": args.shots,
                "result_count": len(results)
            }, indent=2)
        
        if args.save:
            if args.output == "text" and use_colors:
                ansi_escape = re.compile(r'\x1B(?:[@-Z\\-_]|\[[0-?]*[ -/]*[@-~])')
                output = ansi_escape.sub('', output)
                
            with open(args.save, "w") as f:
                f.write(output)
            print(f"Results saved to {args.save}")
        else:
            print(output)
            
    except Exception as e:
        print(f"Error: {e}", file=sys.stderr)
        if args.verbose:
            import traceback
            traceback.print_exc()
        sys.exit(1)