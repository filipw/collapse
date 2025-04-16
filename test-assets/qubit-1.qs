operation Main() : Result {
    use qubit = Qubit();
    H(qubit);
    MResetZ(qubit)
}