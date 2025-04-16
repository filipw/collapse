#!/usr/bin/env python3
import argparse
import os
import sys
import json
from typing import Dict, List, Tuple, Union, Any, Callable, Optional
from collections import Counter
import math

from qsharp import run, eval

class QSharpRunner:
    def __init__(self, file_path=None, source_code=None):
        self.file_path = file_path
        self.source_code = source_code
        
        # file gets the priority over source_code
        if self.file_path is not None and self.source_code is None:
            with open(self.file_path, 'r') as f:
                self.source_code = f.read()
        
    def run(self, shots: int = 1000) -> List[Any]:
        """
        Run Q# code using the provided run function.
        Returns a list of measurement results.
        """
        if self.source_code is None:
            raise ValueError("No Q# source code provided")
            
        # we need to "compile" the Q# code first
        eval(self.source_code)

        # next we call the entry point. this assumes "Main()" is the entry point
        # we probably should change this because it is a bold assumption
        # but for now, let's just go with it
        results = run(
            entry_expr="Main()",
            shots=shots,
            save_events=False
        )
        
        return results


def format_result(result: Any) -> str:
    if isinstance(result, bool):
        return "1" if result else "0"
    elif hasattr(result, "__str__"):
        # handle Q# Result types (One/Zero)
        result_str = str(result)
        if result_str == "One":
            return "1"
        elif result_str == "Zero":
            return "0"
        elif isinstance(result, tuple):
            # handle tuples of Results or booleans
            return "".join(format_result(bit) for bit in result)
        elif isinstance(result, list):
            # handle lists of Results or booleans
            return "".join(format_result(bit) for bit in result)
        else:
            # give up and return the string representation
            return result_str
    elif isinstance(result, tuple):
        # handle tuples of values
        return "".join(format_result(bit) for bit in result)
    elif isinstance(result, list):
        # handle arrays of values
        return "".join(format_result(bit) for bit in result)
    elif result is None:
        return "None"
    else:
        # ???
        return repr(result)


def format_result_for_display(raw_result: str) -> str:
    # create a ket notation for bit strings
    if all(bit in "01" for bit in raw_result):
        return f"|{raw_result}⟩"
    
    # otherwise nothing to do
    return raw_result


def create_histogram(results: List[Any], max_width: int = 60, colorful: bool = True) -> str:
    formatted_results = [format_result(r) for r in results]
    counter = Counter(formatted_results)
    
    # calculate percentages and find the maximum for scaling
    total = len(formatted_results)
    percentages = {k: (v / total) * 100 for k, v in counter.items()}
    max_percentage = max(percentages.values()) if percentages else 0
    
    # sort results from most frequent to least frequent and convert to ket notation
    sorted_results = sorted(counter.keys(), key=lambda k: (-counter[k], k))
    display_results = {raw: format_result_for_display(raw) for raw in sorted_results}
    
    # calculate the width of the columns
    max_label_width = max(len(display_results[r]) for r in sorted_results) if sorted_results else 0
    max_label_width = max(max_label_width, 8)
    available_width = max_width - max_label_width - 18
    max_bar_length = min(int(available_width * 0.6), 40)  # Reduced from 50
    
    colors = {
        "header": "\033[1;36m",  # cyan bold
        "border": "\033[0;37m",  # light gray
        "label": "\033[0;97m",   # white
        "count": "\033[0;93m",   # yellow
        "percent": "\033[0;92m", # green
        "bar": "\033[0;94m",     # blue
        "reset": "\033[0m"       # reset
    }
    
    if not colorful:
        colors = {k: "" for k in colors}
    
    lines = [f"{colors['header']}Measurement Results Histogram:{colors['reset']}"]
    
    separator_width = min(max_width, max_label_width + 18 + min(20, max_bar_length))
    lines.append(f"{colors['border']}{'-' * separator_width}{colors['reset']}")
    
    header_line = (
        f"{colors['label']}{'Result':<{max_label_width}} {colors['reset']}| "
        f"{colors['count']}{'Count':<6} {colors['reset']}| "
        f"{colors['percent']}{'%':<5} {colors['reset']}| "
        f"{colors['bar']}{'Distribution'}{colors['reset']}"
    )
    lines.append(header_line)
    lines.append(f"{colors['border']}{'-' * separator_width}{colors['reset']}")
    
    # use different bar characters for a more interesting visual
    bar_chars = ["█", "▓", "▒", "░"]
    
    for i, result in enumerate(sorted_results):
        count = counter[result]
        percentage = percentages[result]
        
        # calculate the bar width with a reasonable max length
        bar_width = int((percentage / 100) * max_bar_length) if max_percentage > 0 else 0
        
        # select bar character based on position (cycle through the options)
        bar_char = bar_chars[i % len(bar_chars)]
        
        # format the line with colors, using the ket notation
        line = (
            f"{colors['label']}{display_results[result]:<{max_label_width}} {colors['reset']}| "
            f"{colors['count']}{count:<6} {colors['reset']}| "
            f"{colors['percent']}{percentage:>4.1f}% {colors['reset']}| "
            f"{colors['bar']}{bar_char * bar_width}{colors['reset']}"
        )
        
        lines.append(line)
    
    lines.append(f"{colors['border']}{'-' * separator_width}{colors['reset']}")
    lines.append(f"{colors['header']}Total shots: {colors['count']}{total}{colors['reset']}")
    
    return "\n".join(lines)

def run_from_file(file_path: str, shots: int) -> List[Any]:
    if not os.path.exists(file_path):
        raise FileNotFoundError(f"Q# file not found: {file_path}")
    
    runner = QSharpRunner(file_path=file_path)
    return runner.run(shots=shots)

def run_from_source(source_code: str, shots: int) -> List[Any]:
    runner = QSharpRunner(source_code=source_code)
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
        "-n", "--shots",
        type=int,
        default=1000,
        help="Number of shots to run (default: 1000)"
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
            results = run_from_file(args.file, args.shots)
        elif args.source:
            if args.verbose:
                print("Running Q# code from source string")
            results = run_from_source(args.source, args.shots)
        elif args.stdin:
            if args.verbose:
                print("Reading Q# code from standard input")
            source_code = sys.stdin.read()
            results = run_from_source(source_code, args.shots)
        
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
                import re
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

if __name__ == "__main__":
    main()