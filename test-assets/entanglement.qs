import Std.Convert.*;

// try invoking as:
// TestBellState((false, false), (PauliZ, PauliZ));
// TestBellState((false, true), (PauliZ, PauliZ));
// TestBellState((true, false), (PauliZ, PauliZ));
// TestBellState((true, true), (PauliZ, PauliZ));
// TestBellState((false, false), (PauliZ, PauliX));
operation TestBellState(init: (Bool, Bool), measurement_bases : (Pauli, Pauli)) : (Result, Result) {
    let (control_init, target_init) = init;
    use (control, target) = (Qubit(), Qubit());
    ApplyP(control_init ? PauliX | PauliI, control);
    ApplyP(target_init ? PauliX | PauliI, target);

    H(control);
    CNOT(control, target);

    let (control_measurement_basis, target_measurement_basis) = measurement_bases;

    let r1 = Measure([control_measurement_basis], [control]);
    let r2 = Measure([target_measurement_basis], [target]);
    ResetAll([control, target]);

    (r1, r2)
}