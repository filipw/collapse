import unittest
from unittest.mock import patch
from collapse.core import format_result, format_result_for_display, create_histogram
from qsharp._native import Result

class TestCore(unittest.TestCase):
    
    def test_format_result_boolean(self):
        self.assertEqual(format_result(True), "1")
        self.assertEqual(format_result(False), "0")
    
    def test_format_result_qsharp_results(self):
        self.assertEqual(format_result(Result.One), "1")
        self.assertEqual(format_result(Result.Zero), "0")
        
    def test_format_result_tuples(self):
        self.assertEqual(format_result((True, False, True)), "101")
        self.assertEqual(format_result((Result.One, Result.Zero)), "10")
        self.assertEqual(format_result((True, Result.One, False)), "110")
    
    def test_format_result_lists(self):
        self.assertEqual(format_result([True, False, True]), "101")
        self.assertEqual(format_result([Result.One, Result.One, Result.Zero]), "110")
    
    def test_format_result_none(self):
        self.assertEqual(format_result(None), "None")
    
    def test_format_result_other(self):
        class CustomObject:
            def __repr__(self):
                return "CustomObj"
        
        self.assertEqual(format_result(CustomObject()), "CustomObj")
    
    def test_format_result_for_display_bit_string(self):
        self.assertEqual(format_result_for_display("01"), "|01⟩")
        self.assertEqual(format_result_for_display("1011"), "|1011⟩")
    
    def test_format_result_for_display_non_bit_string(self):
        self.assertEqual(format_result_for_display("hello"), "hello")
        self.assertEqual(format_result_for_display("01A"), "01A")
    
    def test_create_histogram_simple(self):
        results = [True, True, False, True]
        histogram = create_histogram(results, colorful=False)
        
        self.assertIn("Measurement Results Histogram", histogram)
        self.assertIn("Total shots: 4", histogram)
        self.assertIn("|1⟩", histogram)
        self.assertIn("|0⟩", histogram)
        self.assertIn("75.0%", histogram)
        self.assertIn("25.0%", histogram)
    
    def test_create_histogram_complex(self):
        results = [
            (Result.One, Result.Zero),
            (Result.Zero, Result.One),
            (Result.One, Result.One),
            (Result.One, Result.Zero)
        ]
        
        histogram = create_histogram(results, colorful=False)
        
        self.assertIn("|10⟩", histogram)
        self.assertIn("|01⟩", histogram)
        self.assertIn("|11⟩", histogram)
        self.assertIn("50.0%", histogram)
        self.assertIn("25.0%", histogram)
        
if __name__ == "__main__":
    unittest.main()