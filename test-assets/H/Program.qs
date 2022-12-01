namespace Hadamard {

    open Microsoft.Quantum.Canon;
    open Microsoft.Quantum.Intrinsic;
    open Microsoft.Quantum.Measurement;

    @EntryPoint()
    operation OneGate() : Result {
        use q = Qubit();
        H(q);
        return M(q);
    }
}

