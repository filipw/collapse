from typing import List, Any
from collections import Counter

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