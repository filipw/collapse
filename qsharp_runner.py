from qsharp import eval, run
from typing import Any, List

class QSharpRunner:
    def __init__(self, file_path=None, source_code=None, entry_point="Main()"):
        self.file_path = file_path
        self.source_code = source_code
        self.entry_point = entry_point

        # file gets the priority over source_code
        if self.file_path is not None and self.source_code is None:
            with open(self.file_path, 'r') as f:
                self.source_code = f.read()

    def run(self, shots: int = 1024) -> List[Any]:
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
            entry_expr=self.entry_point,
            shots=shots,
            save_events=False
        )

        return results