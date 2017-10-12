namespace Crow.Core.SOM
{
    /// <summary>
    /// Lattice Topology of the position neurons in a Kohonen Layer
    /// </summary>
    public enum LatticeTopology
    {
        // Arrangement of neurons in a rectangular lattice
        //
        //            0 0 0 0 0 0
        //            0 0 0 * 0 0
        //            0 0 * O * 0
        //            0 0 0 * 0 0
        //            0 0 0 0 0 0
        //
        // The four immediate neighbors of 'O' are shown as '*'

        /// <summary>
        /// Each neuron has four immediate neighbors
        /// </summary>
        Rectangular = 0,



        // Arrangement of neurons in a hexagonal lattice
        //
        //            0 0 0 0 0
        //             0 0 * * 0
        //            0 0 * O * 0
        //             0 0 * * 0 0
        //              0 0 0 0 0
        //
        // The six immediate neighbors of 'O' are shown as '*'

        /// <summary>
        /// Each neuron has six immediate neighbors
        /// </summary>
        Hexagonal = 1,
    }
}
