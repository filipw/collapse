namespace RNG {

    open Microsoft.Quantum.Arithmetic;
    open Microsoft.Quantum.Canon;
    open Microsoft.Quantum.Convert;
    open Microsoft.Quantum.Diagnostics;
    open Microsoft.Quantum.Intrinsic;
    open Microsoft.Quantum.Measurement;
    open Microsoft.Quantum.Preparation;

    @EntryPoint()
    operation Start() : Result[] {
        use qubits = Qubit[2];
        ApplyToEach(H, qubits);
        let results = MultiM(qubits);
        return results;
    }
}