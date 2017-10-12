using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;

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
    public class EngineSOM2D : GH_Component
    {
        //input
        public List<GH_Point> pointsList = new List<GH_Point>();
        GH_Structure<GH_Number> trainingVectorTree = new GH_Structure<GH_Number>();
        bool trainDataArePoints = true;
        CrowNetSOM2DUP netUP = null;
        int cycles = -1;
        public bool GO = false;

        // inpout SOM
        private int layerWidth;
        private int layerHeight;
        private double learningRate;
        public bool isCircularRows = new bool();
        public bool isCircularColumns = new bool();
        public bool latticeTopology = new bool();
        public bool neighborhood = new bool();

        private KohonenNetwork network;
        private TrainingSet trainingSet = new TrainingSet(3);

        // output SOM
        private bool[,] isWinner;
        private double[][] rowX;
        private double[][] rowY;
        private double[][] rowZ;
        private double[][] columnX;
        private double[][] columnY;
        private double[][] columnZ;
        private double[][] hexagonalX;
        private double[][] hexagonalY;
        private double[][] hexagonalZ;

        // variables TSP
        public bool circle = new bool();
        public int citiesCount;
        public int neuronCount;
        private double[] xVal;
        private double[] yVal;
        private double[] zVal;

        CrowNetP netP = null;
        int counter = 0;

        RandomFunction randomizer = new RandomFunction(0, 1);
        CrowOptions opt = new CrowOptions();

        GH_Structure<GH_Number> allValuesTree = new GH_Structure<GH_Number>();
        //GivenInput givenInput = new GivenInput();


        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public EngineSOM2D()
            : base("Crow Engine Self-Organizing Maps (2D)", "Crow SOM-2D",
                "This engine processes two dimensional Kohnonen network topologies (Self-Organizing Maps) and 3-dimensional training vectors (points). Use it to fit meshes through point clouds.",
                "Crow", "SOM") { }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddIntegerParameter("Training Cycles", "C", "Iteration count over which the engine will attempt self-organization.\nLow values: fast and less reliable - High values: slow and more reliable.", GH_ParamAccess.item, 1000);
            pManager.AddGenericParameter("CrowNet SOM 2D", "Net", "Connect your unprocessed two-dimensional SOM network.", GH_ParamAccess.item);
            pManager.AddGenericParameter("Options", "Opt", "General Crow Options", GH_ParamAccess.item);
            pManager.AddPointParameter("Points", "P", "Training data organized in 3-dimensional points.", GH_ParamAccess.list);
            pManager.AddNumberParameter("Traingin Vectors", "TV", "Instead of points you can also supply a data tree of numbers representing your training data", GH_ParamAccess.tree);
            pManager.AddBooleanParameter("Go", "Go", "The calculation when this is set to true. The solution will be reset every time this input is set to 'false", GH_ParamAccess.item, false);
            pManager[0].Optional = true;
            pManager[2].Optional = true;
            pManager[3].Optional = true;
            pManager[4].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.Register_IntegerParam("Training Cycles", "C", "Number of training cycles executed", GH_ParamAccess.list);
            pManager.Register_GenericParam("CrowNet", "Net", "The calculated network to be passed on to the geometric interpretation", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //get stuff
            trainDataArePoints = true;

            if (!DA.GetData(0, ref cycles)) return;
            if (!DA.GetData(1, ref netUP)) return;
            DA.GetData(2, ref opt);
            if (!DA.GetDataList(3, pointsList))
            {
                trainDataArePoints = false;
                if (!DA.GetDataTree(4, out trainingVectorTree)) return;
            }
            if (!DA.GetData(5, ref GO)) return;



            layerHeight = netUP.layerHeight;                      /// read unprocessed CrowNet
            layerWidth = netUP.layerWidth;
            isCircularRows = netUP.isCircularRows;
            isCircularColumns = netUP.isCircularColumns;
            latticeTopology = netUP.latticeTopology;
            neighborhood = netUP.neighborhood;
            learningRate = netUP.learningRate;

            if (counter == 0)
            {
                #region initialize processing params

                isWinner = new bool[layerWidth, layerHeight];
                rowX = new double[layerHeight][];
                rowY = new double[layerHeight][];
                rowZ = new double[layerHeight][];
                columnX = new double[layerWidth][];
                columnY = new double[layerWidth][];
                columnZ = new double[layerWidth][];
                hexagonalX = new double[layerWidth][];
                hexagonalY = new double[layerWidth][];
                hexagonalZ = new double[layerWidth][];

                circle = netUP.isCircularRows;
                xVal = new double[pointsList.Count * 2 + 1];
                yVal = new double[pointsList.Count * 2 + 1];
                zVal = new double[pointsList.Count * 2 + 1];
                #endregion


                #region prepare for NetP
                int lengthOfRow = layerWidth;
                int lengthOfColumn = layerHeight;

                if (isCircularRows) lengthOfRow++;
                if (isCircularColumns) lengthOfColumn++;

                for (int i = 0; i < layerWidth; i++)
                {
                    columnX[i] = new double[lengthOfColumn];
                    columnY[i] = new double[lengthOfColumn];
                    columnZ[i] = new double[lengthOfColumn];
                    hexagonalX[i] = new double[lengthOfColumn];
                    hexagonalY[i] = new double[lengthOfColumn];
                    hexagonalZ[i] = new double[lengthOfColumn];
                }

                for (int i = 0; i < layerHeight; i++)
                {
                    rowX[i] = new double[lengthOfRow];
                    rowY[i] = new double[lengthOfRow];
                    rowZ[i] = new double[lengthOfRow];
                }
                # endregion
            }

            if (GO)
            {
                if (counter == 0)
                {
                    Task.Factory.StartNew(() => Solve());
                }
                DA.SetData(0, counter);
                netP = new CrowNetP("som", layerWidth, layerHeight, isCircularRows, isCircularColumns, latticeTopology, neighborhood, isWinner, rowX, rowY, rowZ, columnX, columnY, columnZ, hexagonalX, hexagonalY, hexagonalZ, allValuesTree);
                ExpireSolution(true);
            }
            else counter = 0;
            
            
            DA.SetData(1, netP);



            pointsList = new List<GH_Point>();
        }

        void Solve()
        {

            CrowNetP NetP = new CrowNetP();

            if (netUP.netType == "som")
            {
                #region self organizing maps

                #region prepare and assign
                trainingSet.Clear();
                int trainVectorDimension = 3;
                if (trainDataArePoints)
                {
                    for (int i = 0; i < pointsList.Count; i++)
                    {
                        trainingSet.Add(new TrainingSample(new double[] { pointsList[i].Value.X, pointsList[i].Value.Y, pointsList[i].Value.Z }));
                    }
                }
                else
                {
                    trainVectorDimension = trainingVectorTree.Branches[0].Count;
                    trainingSet = new TrainingSet(trainVectorDimension);
                    for (int i = 0; i < trainingVectorTree.Branches.Count; i++)
                    {
                        double[] values = new double[trainVectorDimension];

                        for (int j = 0; j < trainVectorDimension; j++) 
                            values[j] = trainingVectorTree.Branches[i][j].Value;

                        trainingSet.Add(new TrainingSample(values));
                    }
                }
                

                ///  process
                ///  start learning

                int learningRadius = Math.Max(layerWidth, layerHeight) / 2;

                INeighborhoodFunction neighborhoodFunction = new GaussianFunction(learningRadius) as INeighborhoodFunction;
                if (neighborhood) neighborhoodFunction = new MexicanHatFunction(learningRadius) as INeighborhoodFunction;

                LatticeTopology topology = LatticeTopology.Rectangular;
                if (latticeTopology) topology = LatticeTopology.Hexagonal;

                KohonenLayer inputLayer = new KohonenLayer(trainVectorDimension);
                KohonenLayer outputLayer = new KohonenLayer(new Size(layerWidth, layerHeight), neighborhoodFunction, topology);
                KohonenConnector connector = new KohonenConnector(inputLayer, outputLayer);
                connector.Initializer = randomizer;

                outputLayer.SetLearningRate(learningRate, 0.05d);
                outputLayer.IsRowCircular = isCircularRows;
                outputLayer.IsColumnCircular = isCircularColumns;
                network = new KohonenNetwork(inputLayer, outputLayer);
                network.useRandomTrainingOrder = opt.UseRandomTraining;
                #endregion

                #region delegates
                network.BeginEpochEvent += new TrainingEpochEventHandler(
                    delegate(object senderNetwork, TrainingEpochEventArgs args)
                    {
                        #region TrainingCycle
                        if (network == null || !GO) { return; }


                        int iPrev = layerWidth - 1;
                        allValuesTree = new GH_Structure<GH_Number>();
                        for (int i = 0; i < layerWidth; i++)
                        {
                            for (int j = 0; j < layerHeight; j++)
                            {
                                IList<ISynapse> synapses = (network.OutputLayer as KohonenLayer)[i, j].SourceSynapses;
                                double x = synapses[0].Weight;
                                double y = synapses[1].Weight;
                                double z = synapses[2].Weight;

                                for (int k = 0; k < trainVectorDimension; k++) allValuesTree.Append(new GH_Number(synapses[k].Weight), new GH_Path(i,j));

                                rowX[j][i] = x;
                                rowY[j][i] = y;
                                rowZ[j][i] = z;
                                columnX[i][j] = x;
                                columnY[i][j] = y;
                                columnZ[i][j] = z;

                                if (j % 2 == 1)
                                {
                                    hexagonalX[i][j] = x;
                                    hexagonalY[i][j] = y;
                                    hexagonalZ[i][j] = z;
                                }
                                else
                                {
                                    hexagonalX[iPrev][j] = x;
                                    hexagonalY[iPrev][j] = y;
                                    hexagonalZ[iPrev][j] = z;
                                }
                            }
                            iPrev = i;
                        }

                        if (isCircularRows)
                        {
                            for (int i = 0; i < layerHeight; i++)
                            {
                                rowX[i][layerWidth] = rowX[i][0];
                                rowY[i][layerWidth] = rowY[i][0];
                                rowZ[i][layerWidth] = rowZ[i][0];
                            }
                        }

                        if (isCircularColumns)
                        {
                            for (int i = 0; i < layerWidth; i++)
                            {
                                columnX[i][layerHeight] = columnX[i][0];
                                columnY[i][layerHeight] = columnY[i][0];
                                columnZ[i][layerHeight] = columnZ[i][0];
                                hexagonalX[i][layerHeight] = hexagonalX[i][0];
                                hexagonalY[i][layerHeight] = hexagonalY[i][0];
                                hexagonalZ[i][layerHeight] = hexagonalZ[i][0];
                            }
                        }

                        Array.Clear(isWinner, 0, layerHeight * layerWidth);

                        #endregion
                        NetP = new CrowNetP("som", layerWidth, layerHeight, isCircularRows, isCircularColumns, latticeTopology, neighborhood, isWinner, rowX, rowY, rowZ, columnX, columnY, columnZ, hexagonalX, hexagonalY, hexagonalZ, allValuesTree);
                        counter++;
                    });

                network.EndSampleEvent += new TrainingSampleEventHandler(
                    delegate(object senderNetwork, TrainingSampleEventArgs args)
                    {
                        isWinner[network.Winner.Coordinate.X, network.Winner.Coordinate.Y] = true;
                    });
                #endregion

                #endregion
            }

            network.Learn(trainingSet, cycles);
        }

        #region Icon
        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                // return Resources.IconForThisComponent;
                return Crow.Properties.Resources.crowengineus;
            }
        }
        # endregion

        #region Guid
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("{12B46317-64F9-4105-A24C-CC522FC5E052}"); }
        }
        #endregion
    }
}