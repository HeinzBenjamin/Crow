using System;
using System.Runtime.Serialization;
using Crow.Core.Initializers;

namespace Crow.Core.Backpropagation
{
    /// <summary>
    /// An <see cref="ActivationLayer"/> using sigmoid activation function
    /// </summary>
    [Serializable]
    public class SigmoidLayer : ActivationLayer
    {
        /// <summary>
        /// Constructs a new SigmoidLayer containing specified number of neurons
        /// </summary>
        /// <param name="neuronCount">
        /// The number of neurons
        /// </param>
        /// <exception cref="ArgumentException">
        /// If <c>neuronCount</c> is zero or negative
        /// </exception>
        public SigmoidLayer(int neuronCount)
            : base(neuronCount)
        {
            this.initializer = new NguyenWidrowFunction();
        }

        /// <summary>
        /// Sigmoid activation function
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
            return 1d / (1 + Math.Exp(-input));
        }

        //public override double ActivateBackward(double input)

        /// <summary>
        /// Derivative of sigmoid function
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
            return output * (1 - output);
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
        public SigmoidLayer(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}