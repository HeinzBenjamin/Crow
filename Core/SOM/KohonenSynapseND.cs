namespace Crow.Core.SOM
{
    /// <summary>
    /// A Kohonen Synapse is used to connect a neuron to a Position Neuron. It propagates the data
    /// from input neuron to an output position neuron and self-organizes its weights to match the
    /// input.
    /// </summary>
    public class KohonenSynapseND : ISynapse
    {
        private double weight;
        private KohonenConnectorND parent;
        private INeuron sourceNeuron;
        private PositionNeuron targetNeuron;

        /// <summary>
        /// Gets or sets the weight of the synapse
        /// </summary>
        /// <value>
        /// Weight of the synapse
        /// </value>
        public double Weight
        {
            get { return weight; }
            set { weight = value; }
        }

        /// <summary>
        /// Gets the parent connector
        /// </summary>
        /// <value>
        /// Parent connector containing this synapse. It is never <c>null</c>.
        /// </value>
        public KohonenConnectorND Parent
        {
            get { return parent; }
        }

        IConnector ISynapse.Parent
        {
            get { return parent; }
        }

        INeuron ISynapse.TargetNeuron
        {
            get { return targetNeuron; }
        }

        /// <summary>
        /// Gets the source neuron
        /// </summary>
        /// <value>
        /// The source neuron of the synapse. It is never <c>null</c>.
        /// </value>
        public INeuron SourceNeuron
        {
            get { return sourceNeuron; }
        }

        /// <summary>
        /// Gets the target neuron
        /// </summary>
        /// <value>
        /// The target neuron of the synapse. It is never <c>null</c>.
        /// </value>
        public PositionNeuron TargetNeuron
        {
            get { return targetNeuron; }
        }

        /// <summary>
        /// Creates a new Kohonen Synapse connecting the given neurons
        /// </summary>
        /// <param name="sourceNeuron">
        /// The source neuron
        /// </param>
        /// <param name="targetNeuron">
        /// The target neuron
        /// </param>
        /// <param name="parent">
        /// Parent connector containing this synapse
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// If any of the arguments is <c>null</c>
        /// </exception>
        public KohonenSynapseND(INeuron sourceNeuron, PositionNeuron targetNeuron, KohonenConnectorND parent)
        {
            Helper.ValidateNotNull(sourceNeuron, "sourceNeuron");
            Helper.ValidateNotNull(targetNeuron, "targetNeuron");
            Helper.ValidateNotNull(parent, "parent");

            this.weight = 1d;

            sourceNeuron.TargetSynapses.Add(this);
            targetNeuron.SourceSynapses.Add(this);

            this.sourceNeuron = sourceNeuron;
            this.targetNeuron = targetNeuron;
            this.parent = parent;
        }

        /// <summary>
        /// Propagates the data from source neuron to the target neuron
        /// </summary>
        public void Propagate()
        {
            double similarity = sourceNeuron.Output - weight;
            targetNeuron.value += similarity * similarity;
        }

        /// <summary>
        /// Optimizes the weight to match the input
        /// </summary>
        /// <param name="learningFactor">
        /// Effective learning factor. This is a function of training progress, learning rate and
        /// neighborhood value of target neuron.
        /// </param>
        public void OptimizeWeight(double learningFactor)
        {
            weight += learningFactor * (sourceNeuron.Output - weight);
        }

        /// <summary>
        /// Adds small random noise to weight of this synapse so that the network deviates from
        /// its local optimum position (a local equilibrium state where further learning is of
        /// no use)
        /// </summary>
        /// <param name="jitterNoiseLimit">
        /// Maximum absolute limit to the random noise added
        /// </param>
        public void Jitter(double jitterNoiseLimit)
        {
            this.weight += Helper.GetRandom(-jitterNoiseLimit, jitterNoiseLimit);
        }
    }
}
