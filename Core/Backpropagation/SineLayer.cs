using System;
using System.Runtime.Serialization;
using Crow.Core.Initializers;

namespace Crow.Core.Backpropagation
{
    /// <summary>
    /// An <see cref="ActivationLayer"/> using sine activation function
    /// </summary>
    [Serializable]
    public class SineLayer : ActivationLayer
    {
        /// <summary>
        /// Constructs a new SineLayer containing specified number of neurons
        /// </summary>
        /// <param name="neuronCount">
        /// The number of neurons
        /// </param>
        /// <exception cref="ArgumentException">
        /// If <c>neuronCount</c> is zero or negative
        /// </exception>
        public SineLayer(int neuronCount)
            : base(neuronCount)
        {
            this.initializer = new NguyenWidrowFunction();
        }

        /// <summary>
        /// Sine activation function
        /// </summary>
        /// <param name="input">
        /// Current input to the neuron
        /// </param>
        /// <param name="previousOutput">
        /// The previous output at the neuron
        /// </param>
        /// <returns>
        /// The activated value
        /// </returns>
        public override double Activate(double input, double previousOutput)
        {
            return Math.Sin(input);
        }

        /// <summary>
        /// Derivative of sine function
        /// </summary>
        /// <param name="input">
        /// Current input to the neuron
        /// </param>
        /// <param name="output">
        /// Current output (activated) at the neuron
        /// </param>
        /// <returns>
        /// The result of derivative of activation function
        /// </returns>
        public override double Derivative(double input, double output)
        {
            return Math.Sqrt(1 - output * output);
        }

        /// <summary>
        /// Deserialization constructor
        /// </summary>
        /// <param name="info">
        /// The info to deserialize
        /// </param>
        /// <param name="context">
        /// The serialization context to use
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// If <c>info</c> is <c>null</c>
        /// </exception>
        public SineLayer(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}