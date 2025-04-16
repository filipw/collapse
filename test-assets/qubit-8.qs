import Std.Diagnostics.DumpMachine;
operation Main() : Result[] {
    use qubits = Qubit[2];
    for qubit in qubits[1..Length(qubits) - 1] {
        H(qubit);
    }
    MResetEachZ(qubits)
}