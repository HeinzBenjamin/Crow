using System.Runtime.Serialization;

namespace Crow.Core.SOM
{
    /// <summary>
    /// This interface represents a neighborhood function. A neighborhood function determines
    /// the neighborhood of every neuron with respect to winner neuron. This function depends
    /// on the the shape of the layer and also on the training progress.
    /// </summary>
    public interface INeighborhoodFunction : ISerializable
    {
        /// <summary>
        /// Determines the neighborhood of every neuron in the given Kohonen layer with respect
        /// to winner neuron.
        /// </summary>
        /// <param name="layer">
        /// The Kohonen Layer containing neurons
        /// </param>
        /// <param name="currentIteration">
        /// Current training iteration
        /// </param>
        /// <param name="trainingEpochs">
        /// Training Epochs
        /// </param>
        /// <exception cref="System.ArgumentException">
        /// If <c>trainingEpochs</c> is zero or negative
        /// </exception>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// If <c>currentIteration</c> is negative or, if it is not less than <c>trainingEpochs</c>
        /// </exception>
        void EvaluateNeighborhood(KohonenLayer layer, int currentIteration, int trainingEpochs);

        /// <summary>
        /// Determines the neighborhood of every neuron in the given Kohonen layer with respect
        /// to winner neuron.
        /// </summary>
        /// <param name="layer">
        /// The Kohonen Layer containing neurons
        /// </param>
        /// <param name="currentIteration">
        /// Current training iteration
        /// </param>
        /// <param name="trainingEpochs">
        /// Training Epochs
        /// </param>
        /// <exception cref="System.ArgumentException">
        /// If <c>trainingEpochs</c> is zero or negative
        /// </exception>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// If <c>currentIteration</c> is negative or, if it is not less than <c>trainingEpochs</c>
        /// </exception>
        void EvaluateNeighborhood(KohonenLayerND layer, int currentIteration, int trainingEpochs);
    }
}
