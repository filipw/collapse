# Collapse 🌀

A CLI tool for simulating Q# quantum programs and visualizing measurement results with histograms.

## Features

- 🚀 Execute Q# code from files, strings, or stdin
- 📊 Generate histograms of measurement results
- 🎯 Customizable number of shots and output formats
- 🔧 Flexible entry point configuration
- 📄 JSON output support for programmatic use

## Installation

### From Source

```bash
git clone <repository-url>
cd collapse
pip install -e .
```

## Usage

### Basic Usage

Run a Q# file:
```bash
collapse -f your_quantum_program.qs
```

Run Q# code from a string:
```bash
collapse -s "operation Main() : Result { use q = Qubit(); H(q); MResetZ(q) }"
```

Read Q# code from stdin:
```bash
echo "operation Main() : Result { use q = Qubit(); H(q); MResetZ(q) }" | collapse --stdin
```

### Advanced Options

#### Number of Shots
Control how many times to run the quantum program:
```bash
collapse -f program.qs -n 2048
```

#### Custom Entry Point
Specify a different entry point (default is `Main()`):
```bash
collapse -f program.qs -e "TestBellState((false, false), (PauliZ, PauliZ))"
```

#### Output Formats
Get results as JSON:
```bash
collapse -f program.qs --output json
```

### Complete Example

```bash
collapse -f entanglement.qs -e "TestBellState((false, false), (PauliZ, PauliZ))" -n 1024 --verbose
```

## Example Output

```
Measurement Results Histogram:
----------------------------------------
Result   | Count  | %    | Distribution
----------------------------------------
|00⟩     | 512    | 50.0% | ████████████████████████████████████████
|11⟩     | 512    | 50.0% | ▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓
----------------------------------------
Total shots: 1024
```

## Command Line Options

| Option | Short | Description | Default |
|--------|-------|-------------|---------|
| `--file` | `-f` | Path to Q# file to execute | - |
| `--source` | `-s` | Q# source code as string | - |
| `--stdin` | - | Read Q# code from stdin | false |
| `--entrypoint` | `-e` | Entry point expression | `Main()` |
| `--shots` | `-n` | Number of shots to run | 1024 |
| `--width` | - | Maximum histogram width | 80 |
| `--output` | - | Output format (text/json) | text |
| `--verbose` | - | Show detailed execution info | false |
| `--no-color` | - | Disable colorized output | false |
| `--max-bar` | - | Maximum bar length in chars | 50 |

## Development

### Running Tests

```bash
python -m pytest tests/
```

## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.