using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;

using Crow.Core;
using Crow.Core.Initializers;
using Crow.Core.SOM;
using Crow.Core.SOM.NeighborhoodFunctions;

namespace Crow
{
    public class EngineSOMgeneric : GH_Component
    {
        //input
        public GH_Structure<GH_Number> _trainVectors = new GH_Structure<GH_Number>();
        double[][] trainVectors;
        int trainVectorDimension = 0;
        int trainVectorCount = 0;
        private CrowNetSOMNDUP netUP = new CrowNetSOMNDUP();
        int cycles = -1;
        public bool GO = false;

        private bool parallelComputing = false;
        private bool randomTrainingOrder = false;

        DateTime cycleStart = new DateTime();

        // input SOM
        private int[] size;
        private int dimension;
        public bool[] isDimensionCircular;
        private double learningRate;
        public bool latticeTopology = new bool();
        public bool neighborhood = new bool();

        private KohonenNetworkND network;
        private TrainingSet trainingSet = new TrainingSet(1);

        // output SOM
        private double[,] trainedVectors = new double[0,0];
        // return values by position in the grid: first two dimensions define the position in the grid
        // last is the training dimension (row or column or tier whatever)

        private bool[][] isWinnerInDimension;

        CrowNetSOMNDP netP = new CrowNetSOMNDP();
        int counter = 0;

        RandomFunction randomizer = new RandomFunction(0, 1);


        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public EngineSOMgeneric()
            : base("Crow Engine Self-Organizing Grids (Generic)", "Crow SOG",
                "This engine processes n-dimensional Kohnonen network topologies (Self-Organizing Hyper Grids) and m-dimensional training vectors. Use it to fit n-dimensional Kohonen-grids through m-dimensional training vectors.",
                "Crow", "SOM") { }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddIntegerParameter("Training Cycles", "C", "Iteration count over which the engine will attempt self-organization.\nLow values: fast and less reliable - High values: slow and more reliable.", GH_ParamAccess.item, 1000);
            pManager.AddGenericParameter("CrowNet SOG", "Net", "Connect your unprocessed CrowNet to be calculated.", GH_ParamAccess.item);
            pManager.AddNumberParameter("Training vectors", "tv", "Training vectors as 2-d data trees of numbers.", GH_ParamAccess.tree);
            pManager.AddBooleanParameter("Start", "Go", "The calculation when this is set to true. The solution will be reset every time this input is set to 'false", GH_ParamAccess.item, false);
            pManager[0].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Training Cycles and timing", "C", "[0] Number of training cycles executed\n[1] Time spent since start\n[2] Average time per cycle [ms]", GH_ParamAccess.list);
            pManager.Register_GenericParam("CrowNet", "Net", "The calculated network to be passed on to data interpretation", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //get stuff
            if (!DA.GetData(0, ref cycles)) return;
            if (!DA.GetData(1, ref netUP)) return;
            if (!DA.GetDataTree(2, out _trainVectors)) return;
            if (!DA.GetData(3, ref GO)) return;

            trainVectors = Utils.GHTreeToMultidimensionalArray(_trainVectors);
            trainVectorDimension = trainVectors[0].Length;
            trainVectorCount = trainVectors.Length;
            foreach (double[] i in trainVectors) if (i.Length != trainVectorDimension) AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Inconsistant data: Not all training vectors have the same length!");

            trainingSet = new TrainingSet(trainVectorDimension);

            size = netUP.size;                     /// read unprocessed CrowNet
            dimension = size.Length;
            isDimensionCircular = netUP.circularity;
            latticeTopology = netUP.latticeTopology;
            neighborhood = netUP.neighborhood;
            learningRate = netUP.learningRate;

            if (counter == 0)
            {
                isWinnerInDimension = new bool[dimension][];

                // TO DO ! if (isCircularRows) lengthOfRow++;
                // TO DO ! if (isCircularColumns) lengthOfColumn++;
            }
            Task solveTask = new Task(() => Solve());
            if (GO)
            {
                if (counter == 0)
                {
                    cycleStart = System.DateTime.Now;
                    solveTask.Start();
                }
            }
            else
            {
                counter = 0;
                if (solveTask.Status == TaskStatus.Running)
                {
                    solveTask.Dispose();
                }
            }

            if(GO && counter < cycles)
                ExpireSolution(true);
            
            DA.SetData(1, netP);

            _trainVectors = new GH_Structure<GH_Number>();

            // time measures
            TimeSpan totalSpan = new TimeSpan();
            double averageTimePerCycle = 0.0;

            if (GO) totalSpan = System.DateTime.Now - cycleStart;
            if (counter != 0)
                averageTimePerCycle = (double)(totalSpan.Days * 86400 + totalSpan.Hours * 3600 + totalSpan.Minutes * 60 + totalSpan.Seconds) / (double)counter;

            DA.SetDataList(0, new List<string> { counter.ToString(), totalSpan.ToString(), averageTimePerCycle.ToString() });

            
        }

        void Solve()
        {

            #region prepare and assign
            trainingSet.Clear();
            for (int i = 0; i < trainVectorCount; i++)
            {
                List<double> dl = new List<double>();
                for (int j = 0; j < trainVectorDimension; j++) dl.Add(trainVectors[i][j]);
                trainingSet.Add(new TrainingSample(dl.ToArray()));
            }

            ///  process
            ///  start learning

            ///  get learning radius for neighborhood function
            int learningRadius = 0;
            for (int i = 0; i < dimension; i++) if (size[i] > learningRadius) learningRadius = size[i];
            learningRadius /= 2;

            INeighborhoodFunction neighborhoodFunction = new GaussianFunction(learningRadius, netUP.neighborDistance) as INeighborhoodFunction;
            if (neighborhood) neighborhoodFunction = new MexicanHatFunction(learningRadius) as INeighborhoodFunction;

            LatticeTopology topology = LatticeTopology.Rectangular;
            if (latticeTopology) topology = LatticeTopology.Hexagonal;
            /// instantiate relevant network layers
            KohonenLayer inputLayer = new KohonenLayer(trainVectorDimension);
            KohonenLayerND outputLayer = new KohonenLayerND(size, neighborhoodFunction, topology);
            KohonenConnectorND connector = new KohonenConnectorND(inputLayer, outputLayer, netUP.initialNodes);
            if (netUP.initialNodes.Length != 0)
                connector.Initializer = new GivenInput(netUP.initialNodes);
            else
                connector.Initializer = new RandomFunction(0.0, 1.0);
            outputLayer.SetLearningRate(learningRate, 0.05d);
            outputLayer.IsDimensionCircular = isDimensionCircular;
            network = new KohonenNetworkND(inputLayer, outputLayer);
            network.useRandomTrainingOrder = randomTrainingOrder;
            inputLayer.ParallelComputation = false;
            outputLayer.ParallelComputation = parallelComputing;
            #endregion

            #region delegates
            network.BeginEpochEvent += new TrainingEpochEventHandler(
                delegate(object senderNetwork, TrainingEpochEventArgs args)
                {
                    #region trainingCylce
                    if (network == null || !GO) { return; }
                    trainedVectors = new double[outputLayer.neuronCount, trainVectorDimension];

                    for (int i = 0; i < outputLayer.neuronCount; i++)
                    {
                        IList<ISynapse> synapses = (network.OutputLayer as KohonenLayerND)[outputLayer.adressBook[i]].SourceSynapses;
                        for (int j = 0; j < trainVectorDimension; j++) trainedVectors[i, j] = synapses[j].Weight;
                    }

                    //make new net here
                    netP = new CrowNetSOMNDP(size, isDimensionCircular, latticeTopology, neighborhood, trainedVectors, outputLayer.adressBook);

                    counter++;
                    
                    #endregion
                });

            network.EndSampleEvent += new TrainingSampleEventHandler(
                    delegate(object senderNetwork, TrainingSampleEventArgs args)
                    {
                        netP.winner = outputLayer.WinnerND.CoordinateND;
                    });
            #endregion


            
            network.Learn(trainingSet, cycles);

        }


        protected override void AppendAdditionalComponentMenuItems(ToolStripDropDown menu)
        {

            ToolStripMenuItem item1 = Menu_AppendItem(menu, "Boost", item1_Clicked, true, parallelComputing);
            item1.ToolTipText = "Tick for parallel computing. This is especially benefitial in neuronCounts > 100 and trainingVectorCounts > 500.";
            ToolStripMenuItem item2 = Menu_AppendItem(menu, "Random Training Order", item2_Clicked, true, randomTrainingOrder);
            item2.ToolTipText = "Iterate through training samples randomly.";
        }

        private void item1_Clicked(object sender, EventArgs e)
        {
            RecordUndoEvent("parallelComputing");
            parallelComputing = !parallelComputing;
            ExpireSolution(true);
        }

        private void item2_Clicked(object sender, EventArgs e)
        {
            RecordUndoEvent("randomTrainingOrder");
            randomTrainingOrder = !randomTrainingOrder;
            ExpireSolution(true);
        }

        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                // return Resources.IconForThisComponent;
                return Crow.Properties.Resources.crowengineus_g;
            }
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("{71D006D6-E16B-4401-B6C1-3F4898677CCF}"); }
        }
    }
}