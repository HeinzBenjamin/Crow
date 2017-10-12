using System.Runtime.Serialization;

namespace Crow.Core
{
    /// <summary>
    /// Learning Rate Function interface. This interface defines the way in which learning rate
    /// changes from its initial value to its final value as the training progresses.
    /// </summary>
    public interface ILearningRateFunction : ISerializable
    {
        /// <summary>
        /// Gets the initial value of learning rate
        /// </summary>
        /// <value>
        /// Initial Learning Rate
        /// </value>
        double InitialLearningRate { get; }

        /// <summary>
        /// Gets the final value of learning rate
        /// </summary>
        /// <value>
        /// Final Learning Rate
        /// </value>
        double FinalLearningRate { get; }

        /// <summary>
        /// Gets effective learning rate for current training iteration. No validation is performed
        /// on the arguments.
        /// </summary>
        /// <param name="currentIteration">
        /// Current training iteration
        /// </param>
        /// <param name="trainingEpochs">
        /// Total number of training epochs
        /// </param>
        /// <returns>
        /// The effective learning rate for current training iteration
        /// </returns>
        /// <exception cref="System.ArgumentException">
        /// If <c>trainingEpochs</c> is zero or negative
        /// </exception>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// If <c>currentIteration</c> is negative or, if it is not less than <c>trainingEpochs</c>
        /// </exception>
        double GetLearningRate(int currentIteration, int trainingEpochs);
    }
}