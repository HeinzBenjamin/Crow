using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.Serialization;
using Crow.Core.SOM.NeighborhoodFunctions;
using System.Threading.Tasks;

using Rhino.Geometry;

namespace Crow.Core.SOM
{
    /// <summary>
    /// Kohonen Layer is a layer containing position neurons.
    /// </summary>
    [Serializable]
    public class KohonenLayerND : Layer<PositionNeuron>
    {
        private readonly int[] size;
        public int[][] adressBook;
        public int neuronCount;
        private readonly LatticeTopology topology;
        private bool[] isDimensionCircular;
        private PositionNeuron winnerND;
        private INeighborhoodFunction neighborhoodFunction;

        /// <summary>
        /// Gets the layer size
        /// </summary>
        /// <value>
        /// Size of the layer (Width is number of columns, and Height is number of rows) (In other
        /// words, width is number of neurons in a row and height is number of neurons in a column)
        /// </value>
        public int[] Size
        {
            get { return size; }
        }

        /// <summary>
        /// Gets the lattice topology
        /// </summary>
        /// <value>
        /// Lattice topology of neurons in the layer
        /// </value>
        public LatticeTopology Topology
        {
            get { return topology; }
        }

        /// <summary>
        /// Gets or sets a boolean representing whether the neuron rows are circular
        /// </summary>
        /// <value>
        /// A boolean representing whether the neuron rows are circular
        /// </value>
        public bool[] IsDimensionCircular
        {
            get { return isDimensionCircular; }
            set { isDimensionCircular = value; }
        }

        /// <summary>
        /// Gets the winner neuron of the layer
        /// </summary>
        /// <value>
        /// Winner Neuron
        /// </value>
        public PositionNeuron WinnerND
        {
            get { return winnerND; }
        }

        /// <summary>
        /// Gets or sets the neighborhood function
        /// </summary>
        /// <value>
        /// Neighborhood Function
        /// </value>
        public INeighborhoodFunction NeighborhoodFunction
        {
            get { return neighborhoodFunction; }
            set { neighborhoodFunction = value; }
        }

        /// <summary>
        /// Position Neuron indexer
        /// </summary>
        /// <param name="posVector">
        /// n-dimensional position vector of the neuron
        /// </param>
        /// <returns>
        /// The neuron at given co-ordinates
        /// </returns>
        /// <exception cref="IndexOutOfRangeException">
        /// If any of the indices are out of range
        /// </exception>
        public PositionNeuron this[int[] posVector]
        {
            get { return neurons[MakePositionHelper(posVector)]; }
        }

        /// <summary>
        /// Returns the continuous index of a neuron from its indices in the position space
        /// </summary>
        /// <param name="posVector"></param>
        /// <returns></returns>
        private int MakePositionHelper(int[] posVector)
        {
            int p = posVector[0];
            for (int i = 1; i < posVector.Length; i++)
            {
                int f = 1;
                for (int j = 0; j < i; j++) f *= size[j];
                p += posVector[i] * f;
            }
            return p;
        }


        /// <summary>
        /// returns the count of the neurons in the layer
        /// </summary>
        /// <returns></returns>
        private void calculateNeuronCount()
        {
            if (size.Length == 0) neuronCount = 0;
            else
            {
                neuronCount = 1;
                for (int i = 0; i < size.Length; i++) neuronCount *= size[i];
            }
        }

        /// <summary>
        /// Creates a Kohonen layer with the specified size and neighborhood function
        /// </summary>
        /// <param name="size">
        /// Size of the layer
        /// </param>
        /// <param name="neighborhoodFunction">
        /// Neighborhood function to use
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// If <c>neighborhoodFunction</c> is <c>null</c>
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If layer width or layer height is not positive
        /// </exception>
        public KohonenLayerND(int[] size, INeighborhoodFunction neighborhoodFunction)
            : this(size, neighborhoodFunction, LatticeTopology.Rectangular)
        {
        }

        /// <summary>
        /// Creates a Kohonen layer with the specified size, topology and neighborhood function
        /// </summary>
        /// <param name="size">
        /// Size of the layer
        /// </param>
        /// <param name="neighborhoodFunction">
        /// Neighborhood function to use
        /// </param>
        /// <param name="topology">
        /// Lattice topology of neurons
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// If <c>neighborhoodFunction</c> is <c>null</c>
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If layer width or layer height is not positive, or if <c>topology</c> is invalid
        /// </exception>
        public KohonenLayerND(int[] size, INeighborhoodFunction neighborhoodFunction, LatticeTopology topology)
            : base(SizeHelper(size))
        {
            // The product can be positive when both width and height are negative. So, we need to check one.
            Helper.ValidatePositive(size[0], "size.FirstDimension");

            Helper.ValidateNotNull(neighborhoodFunction, "neighborhoodFunction");
            Helper.ValidateEnum(typeof(LatticeTopology), topology, "topology");

            this.size = size;
            this.neighborhoodFunction = neighborhoodFunction;
            this.topology = topology;

            calculateNeuronCount();

            k = 0;
            int[] counters = new int[size.Length];
            adressBook = new int[neuronCount][];

            FillAdressBookRecursively(counters, ReverseArray(size), size.Length-1);
            for (int i = 0; i < neuronCount; i++) neurons[i] = new PositionNeuron(adressBook[i], this);

        }

        // get neuron count
        static int SizeHelper(int[] size)
        {
            if (size.Length == 0 || size[0] == 0) return 0;

            int r = 1;
            for (int i = 0; i < size.Length; i++) r *= size[i];
            return r;
        }

        private int k; //neuron index

        void FillAdressBookRecursively(int[] counters, int[] size, int level)
        {
            if (level == -1) { adressBook[k] = ReverseArray(counters); k++; }
            else
            {
                for (counters[level] = 0; counters[level] < size[level]; counters[level]++)
                    FillAdressBookRecursively(counters, size, level - 1);
            }
        }

        public static int[] ReverseArray(int[] array)
        {
            int[] rArray = new int[array.Length];
            for (int i = 0; i < array.Length; i++) rArray[i] = array[array.Length - i - 1];
            return rArray;
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
        public KohonenLayerND(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            this.topology = (LatticeTopology)info.GetValue("topology", typeof(LatticeTopology));

            this.neighborhoodFunction
                = info.GetValue("neighborhoodFunction", typeof(INeighborhoodFunction))
                as INeighborhoodFunction;

            for (int i = 0; i < this.isDimensionCircular.Length; i++)
            {
                this.size[i] = info.GetInt32("size.Dimension" + i);
                this.isDimensionCircular[i] = info.GetBoolean("isDimension" + i + "Circular");
            }
        }

        /// <summary>
        /// Populates the serialization info with the data needed to serialize the layer
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

            info.AddValue("topology", topology);
            for (int i = 0; i < size.Length; i++)
            {
                info.AddValue("size.Dimension" + i, size[i]);
                info.AddValue("isDimension" + i + "Circular", isDimensionCircular[i]);
            }
            info.AddValue("neighborhoodFunction", neighborhoodFunction, typeof(INeighborhoodFunction));
        }

        /// <summary>
        /// Initializes all neurons and makes them ready to undergo fresh training.
        /// </summary>
        public override void Initialize()
        {
            //Since there are no initializable parameters in this layer, this is a do-nothing function
        }

        /// <summary>
        /// Runs all neurons in the layer and finds the winner
        /// </summary>
        public override void Run()
        {
            this.winnerND = neurons[0];
            if (!base.ParallelComputation)
                for (int i = 0; i < neurons.Length; i++)
                {
                    neurons[i].Run();
                    if (neurons[i].value < winnerND.value)
                    {
                        winnerND = neurons[i];
                    }
                }
            else
            {
                ParallelOptions opt = new ParallelOptions();
                opt.MaxDegreeOfParallelism = System.Environment.ProcessorCount * 2;
                Parallel.For(0, neurons.Length, opt, i =>
                {
                    neurons[i].Run();
                    if (neurons[i].value < winnerND.value)
                    {
                        winnerND = neurons[i];
                    }
                });
            }
        }

        /// <summary>
        /// All neurons and their are source connectors are allowed to learn. This method assumes a
        /// learning environment where inputs, outputs and other parameters (if any) are appropriate.
        /// </summary>
        /// <param name="currentIteration">
        /// Current learning iteration
        /// </param>
        /// <param name="trainingEpochs">
        /// Total number of training epochs
        /// </param>
        /// <exception cref="ArgumentException">
        /// If <c>trainingEpochs</c> is zero or negative
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// If <c>currentIteration</c> is negative or, if it is not less than <c>trainingEpochs</c>
        /// </exception>
        public override void Learn(int currentIteration, int trainingEpochs)
        {
            // Validation Delegated
            neighborhoodFunction.EvaluateNeighborhood(this, currentIteration, trainingEpochs);
            base.Learn(currentIteration, trainingEpochs);
        }
    }
}