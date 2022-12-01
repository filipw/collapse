namespace EntanglementSwapping {

    open Microsoft.Quantum.Canon;
    open Microsoft.Quantum.Intrinsic;
    open Microsoft.Quantum.Measurement;

    @EntryPoint()
    operation EnganglementSwapping() : (Result, Result) {
        
        // Alice
        use (q1, q2) = (Qubit(), Qubit());
        H(q1);
        CNOT(q1, q2);
        Message("foo");
        // at this point Alice has an entangled pair q1, q2 = |00⟩+|11⟩/√2

        // Bob
        use (q3, q4) = (Qubit(), Qubit());
        H(q3);
        CNOT(q3, q4);
        // at this point Bob has an entangled pair q3, q4 = |00⟩+|11⟩/√2

        // Bell measurement - done by Carol
        CNOT(q2, q4);
        H(q2);
        let q2Result = M(q2);
        let q4Result = M(q4);

        // at this point q1 and q3 are entangled, but in one of the four possible Bell states
        // Bob needs to fix up the state between q1 and q3 to become |00⟩+|11⟩/√2
        if (q2Result == Zero and q4Result == Zero) { 
            I(q3); 
        } 

        if (q2Result == Zero and q4Result == One) { 
            X(q3); 
        } 

        if (q2Result == One and q4Result == Zero) { 
            Z(q3); 
        } 

        if (q2Result == One and q4Result == One) { 
            X(q3); 
            Z(q3); 
        } 

        // optional - this will return qubits to state |00⟩
        //Adjoint PrepareEntangledState([q1], [q3]);

        // at this point the entangled pair q1, q3 is |00⟩+|11⟩/√2
        let c1 = M(q1);
        let c3 = M(q3);

        return (c1, c3);
    }
}

