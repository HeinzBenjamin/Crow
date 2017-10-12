using System;
using System.Runtime.Serialization;
using Crow.Core.Initializers;

namespace Crow.Core.SOM
{
    /// <summary>
    /// A Kohonen Connector is an <see cref="IConnector"/> consisting of a collection of Kohonen
    /// synapses connecting any layer to a Kohonen Layer.
    /// </summary>
    [Serializable]
    public class KohonenConnectorND : Connector<ILayer, KohonenLayerND, KohonenSynapseND>
    {
        /// <summary>
        /// Creates a new Kohonen connector between the given layers.
        /// </summary>
        /// <param name="sourceLayer">
        /// The source layer
        /// </param>
        /// <param name="targetLayer">
        /// The target layer
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// If <c>sourceLayer</c> or <c>targetLayer</c> is <c>null</c>
        /// </exception>
        /*public KohonenConnectorND(ILayer sourceLayer, KohonenLayerND targetLayer)
            : base(sourceLayer, targetLayer, ConnectionMode.Complete)
        {
            this.initializer = new RandomFunction();

            int i = 0;
            foreach (PositionNeuron targetNeuron in targetLayer.Neurons)
            {
                foreach (INeuron sourceNeuron in sourceLayer.Neurons)
                {
                    synapses[i++] = new KohonenSynapseND(sourceNeuron, targetNeuron, this);
                }
            }
        }*/

        /// <summary>
        /// Creates a new Kohonen connector between the given layers.
        /// </summary>
        /// <param name="sourceLayer">
        /// The source layer
        /// </param>
        /// <param name="targetLayer">
        /// The target layer
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// If <c>sourceLayer</c> or <c>targetLayer</c> is <c>null</c>
        /// </exception>
        public KohonenConnectorND(ILayer sourceLayer, KohonenLayerND targetLayer, double[] flattenedGivenInput)
            : base(sourceLayer, targetLayer, ConnectionMode.Complete)
        {
                this.initializer = new RandomFunction();

            int i = 0;
            foreach (PositionNeuron targetNeuron in targetLayer.Neurons)
            {
                foreach (INeuron sourceNeuron in sourceLayer.Neurons)
                {
                    synapses[i++] = new KohonenSynapseND(sourceNeuron, targetNeuron, this);
                }
            }
        }

        /// <summary>
        /// Deserialization Constructor
        /// </summary>
        /// <param name="info">
        /// Serialization information to deserialize and obtain the data
        /// </param>
        /// <param name="context">
        /// Serialization context to use
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// If <c>info</c> is <c>null</c>
        /// </exception>
        public KohonenConnectorND(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            double[] weights = (double[])info.GetValue("weights", typeof(double[]));

            int i = 0;
            foreach (INeuron sourceNeuron in sourceLayer.Neurons)
            {
                foreach (PositionNeuron targetNeuron in targetLayer.Neurons)
                {
                    synapses[i] = new KohonenSynapseND(sourceNeuron, targetNeuron, this);
                    synapses[i].Weight = weights[i];
                    i++;
                }
            }
        }

        /// <summary>
        /// Populates the serialization info with the data needed to serialize the connector
        /// </summary>
        /// <param name="info">
        /// The serialization info to populate the data with
        /// </param>
        /// <param name="context">
        /// The serialization context to use
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// If <c>info</c> is <c>null</c>
        /// </exception>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            double[] weights = new double[synapses.Length];
            for (int i = 0; i < synapses.Length; i++)
            {
                weights[i] = synapses[i].Weight;
            }

            info.AddValue("weights", weights, typeof(double[]));
        }

        /// <summary>
        /// Initializes all synapses in the connector and makes them ready to undergo training
        /// freshly. (Adjusts the weights of synapses using the initializer)
        /// </summary>
        public override void Initialize()
        {
            if (initializer != null)
            {
                initializer.Initialize(this);
            }
        }
    }
}
