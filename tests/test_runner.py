import unittest
from unittest.mock import patch, mock_open, MagicMock
import os
from collapse.runner import QSharpRunner

class TestQSharpRunner(unittest.TestCase):
    
    def test_init_with_file_path(self):
        mock_file_content = "operation Main() : Result { Zero }"
        with patch("builtins.open", mock_open(read_data=mock_file_content)):
            runner = QSharpRunner(file_path="test.qs")
            
            self.assertEqual(runner.file_path, "test.qs")
            self.assertEqual(runner.source_code, mock_file_content)
            self.assertEqual(runner.entry_point, "Main()")
            
    def test_init_with_source_code(self):
        source_code = "operation Main() : Result { One }"
        runner = QSharpRunner(source_code=source_code)
        
        self.assertIsNone(runner.file_path)
        self.assertEqual(runner.source_code, source_code)
        self.assertEqual(runner.entry_point, "Main()")
        
    def test_init_with_custom_entry_point(self):
        source_code = "operation Custom() : Result { One }"
        runner = QSharpRunner(source_code=source_code, entry_point="Custom()")
        
        self.assertEqual(runner.entry_point, "Custom()")
    
    def test_run_no_source_code(self):
        runner = QSharpRunner()
        
        with self.assertRaises(ValueError) as context:
            runner.run()
            
        self.assertEqual(str(context.exception), "No Q# source code provided")
        
    @patch("collapse.runner.eval")
    @patch("collapse.runner.run")
    def test_run_with_source_code(self, mock_run, mock_eval):
        source_code = "operation Main() : Result { One }"
        runner = QSharpRunner(source_code=source_code)
        
        results = runner.run(shots=100)
        
        # Verify eval was called with the source code
        mock_eval.assert_called_once_with(source_code)
        
        # Verify run was called with correct parameters
        mock_run.assert_called_once_with(
            entry_expr="Main()", 
            shots=100,
            save_events=False
        )
        
    @patch("collapse.runner.eval")
    @patch("collapse.runner.run")
    def test_run_with_custom_entry_point(self, mock_run, mock_eval):
        source_code = "operation Custom() : Result { One }"
        runner = QSharpRunner(source_code=source_code, entry_point="Custom()")
        
        results = runner.run()
        
        mock_eval.assert_called_once_with(source_code)
        mock_run.assert_called_once_with(
            entry_expr="Custom()", 
            shots=1024,
            save_events=False
        )

if __name__ == "__main__":
    unittest.main()