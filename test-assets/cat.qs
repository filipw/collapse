operation Main() : Result[] {
    // create a GHZ state (cat state) with 8 qubits
    use qubits = Qubit[8];

    // apply Hadamard gate to the first qubit
    H(qubits[0]);

    // apply CNOT gates to create entanglement
    for qubit in qubits[1..Length(qubits) - 1] {
        CNOT(qubits[0], qubit);
    }
    // return measurement results
    MResetEachZ(qubits)
}