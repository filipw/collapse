import unittest
import sys
import io
import os
import json
from pathlib import Path
from collapse.cli import run_from_file, run_from_source, main

ASSETS_DIR = Path(os.path.abspath(os.path.dirname(__file__))) / ".." / "test-assets"

class TestCLI(unittest.TestCase):
    
    def setUp(self):
        self.assertTrue(ASSETS_DIR.exists(), f"Test assets directory not found: {ASSETS_DIR}")
    
    def test_run_from_file(self):
        results = run_from_file(str(ASSETS_DIR / "entanglement.qs"), 10, "Main()")
        self.assertIsInstance(results, list)
        self.assertEqual(len(results), 10)
    
    def test_run_from_file_not_found(self):
        with self.assertRaises(FileNotFoundError):
            run_from_file("nonexistent.qs", 100, "Main()")
    
    def test_run_from_source(self):
        with open(str(ASSETS_DIR / "entanglement.qs"), 'r') as f:
            source_code = f.read()
            
        results = run_from_source(source_code, 10, "Main()")
        self.assertIsInstance(results, list)
        self.assertEqual(len(results), 10)
    
    def test_main_from_file(self):
        captured_output = io.StringIO()
        sys.stdout = captured_output
        
        sys.argv = ["collapse", "-f", str(ASSETS_DIR / "one.qs"), "-n", "5"]
        try:
            main()
        except SystemExit:
            pass
        
        sys.stdout = sys.__stdout__
        
        output = captured_output.getvalue()
        self.assertIn("|1⟩", output)
    
    def test_main_from_source(self):
        with open(str(ASSETS_DIR / "one.qs"), 'r') as f:
            test_source = f.read()
        
        captured_output = io.StringIO()
        sys.stdout = captured_output
        
        sys.argv = ["collapse", "-s", test_source, "-n", "5"]
        try:
            main()
        except SystemExit:
            pass
        
        sys.stdout = sys.__stdout__
        
        output = captured_output.getvalue()
        self.assertIn("|1⟩", output)
    
    def test_main_json_output(self):
        captured_output = io.StringIO()
        sys.stdout = captured_output
        
        sys.argv = ["collapse", "-f", str(ASSETS_DIR / "one.qs"), "--output", "json", "-n", "5"]
        try:
            main()
        except SystemExit:
            pass
        
        sys.stdout = sys.__stdout__
        
        output = json.loads(captured_output.getvalue())
        self.assertIn("results", output)
        self.assertEqual(output["total_shots"], 5)
        self.assertEqual(output["result_count"], 5)
        self.assertTrue('1' in output["results"])
    
    def test_main_verbose_mode(self):
        captured_output = io.StringIO()
        sys.stdout = captured_output
        
        file_path = str(ASSETS_DIR / "qubit-1.qs")
        sys.argv = ["collapse", "-f", file_path, "--verbose", "-n", "5"]
        try:
            main()
        except SystemExit:
            pass
        
        sys.stdout = sys.__stdout__
        
        output = captured_output.getvalue()
        self.assertIn("Running Q# code with", output)
        self.assertIn(f"Loading Q# code from file: {file_path}", output)
        self.assertIn("Execution completed", output)

if __name__ == "__main__":
    unittest.main()