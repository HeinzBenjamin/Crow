using System;
using System.Runtime.Serialization;
using Crow.Core.Backpropagation;
using Crow.Core.SOM;

namespace Crow.Core.Initializers
{
    /// <summary>
    /// An <see cref="IInitializer"/> using random function
    /// </summary>
    [Serializable]
    public class GivenInput : IInitializer
    {
        private readonly double[] inputs;

        public double[] Inputs
        {
            get { return inputs; }
        }

        public GivenInput(double[] inputs)
        {
            this.inputs = inputs;
        }

        public GivenInput(SerializationInfo info, StreamingContext context)
        {
            Helper.ValidateNotNull(info, "info");

            this.inputs = (double[]) info.GetValue("inputs", typeof(double[]));
        }

        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            Helper.ValidateNotNull(info, "info");

            info.AddValue("inputs", inputs, typeof(double[]));
        }

        /// <summary>
        /// Initializes bias values of activation neurons in the activation layer.
        /// </summary>
        /// <param name="activationLayer">
        /// The activation layer to initialize
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// If <c>activationLayer</c> is <c>null</c>
        /// </exception>
        public void Initialize(ActivationLayer activationLayer)
        {
            //do nothing
        }

        /// <summary>
        /// Initializes weights of all backpropagation synapses in the backpropagation connector.
        /// </summary>
        /// <param name="connector">
        /// The backpropagation connector to initialize.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// If <c>connector</c> is <c>null</c>
        /// </exception>
        public void Initialize(BackpropagationConnector connector)
        {
            //do nothing
        }

        /// <summary>
        /// Initializes weights of all spatial synapses in a Kohonen connector.
        /// </summary>
        /// <param name="connector">
        /// The Kohonen connector to initialize.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// If <c>connector</c> is <c>null</c>
        /// </exception>
        public void Initialize(KohonenConnector connector)
        {
            /*Helper.ValidateNotNull(connector, "connector");
            foreach (KohonenSynapse synapse in connector.Synapses)
            {
                synapse.Weight = Helper.GetRandom(minLimit, maxLimit);
            }*/
        }

        /// <summary>
        /// Initializes weights of all spatial synapses in a Kohonen connector 3D.
        /// </summary>
        /// <param name="connector">
        /// The Kohonen connector to initialize.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// If <c>connector</c> is <c>null</c>
        /// </exception>
        public void Initialize(KohonenConnectorND connector)
        {
            /*Helper.ValidateNotNull(connector, "connector");
            foreach (KohonenSynapseND synapse in connector.Synapses)
            {
                synapse.Weight = Helper.GetRandom(minLimit, maxLimit);

            }*/
        }
    }
}
