using System;
using System.Collections.Generic;
using System.Linq;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Grasshopper.Kernel.Data;
using Rhino.Geometry;

using Crow.Core;
using Crow.Core.Backpropagation;

namespace Crow
{
    public class CrowNetSOMNDP
    {
        public string netType { get; private set; }
        public int[] size { get; set; }
        public bool[] isDimensionCircular { get; set; }
        public bool latticeTopology { get; set; }
        public bool neighborhood { get; set; }
        public int[] winner { get; set; }
        public double[,] trainedVectors { get; set; }
        public int[][] adressBook { get; set; }

        public CrowNetSOMNDP(int[] Size, bool[] IsDimensionCircular, bool LatticeTopology, bool Neighborhood, double[,] Positions, int[][] AdressBook)
        {
            this.netType = "SOMND";
            this.size = Size;
            this.isDimensionCircular = IsDimensionCircular;
            this.latticeTopology = LatticeTopology;
            this.neighborhood = Neighborhood;
            this.winner = new int[0];
            this.trainedVectors = Positions;
            this.adressBook = AdressBook;
        }

        public CrowNetSOMNDP()
        {
            this.netType = "SOMND";
            this.size = new int[0];
            this.isDimensionCircular = new bool[0];
            this.latticeTopology = false;
            this.neighborhood = false;
            this.winner = new int[0];
            this.trainedVectors = new double[0,0];
            this.adressBook = new int[0][];
        }

        public GH_Structure<GH_Number> DataTree()
        {
            GH_Structure<GH_Number> dataTree = new GH_Structure<GH_Number>();
            GH_Structure<GH_Integer> indices = new GH_Structure<GH_Integer>();
            for (int i = 0; i < trainedVectors.GetLength(0); i++)
            {
                GH_Path p = new GH_Path(Utils.ReverseArray(this.adressBook[i]));
                indices.Append(new GH_Integer(i), p);
            }
            indices.Flatten();
            dataTree = new GH_Structure<GH_Number>();
            GH_Structure<GH_Number> valueTree = Utils.MultidimensionalArrayToGHTree(trainedVectors);
            for (int i = 0; i < indices.DataCount; i++)
            {
                dataTree.AppendRange(valueTree.Branches[indices.Branches[0][i].Value], new GH_Path(this.adressBook[i]));
            }
            return dataTree;
        }

        public GH_Structure<GH_Point> PointTree(List<int> xyzD)
        {
            GH_Structure<GH_Point> pointTree = new GH_Structure<GH_Point>();
            GH_Structure<GH_Number> dataTree = this.DataTree();
            for (int i = 0; i < dataTree.Paths.Count; i++)
            {
                GH_Path path = dataTree.Paths[i];
                pointTree.Append(
                    new GH_Point(new Point3d(
                            dataTree.get_DataItem(path, xyzD[0]).Value,
                            dataTree.get_DataItem(path, xyzD[1]).Value,
                            dataTree.get_DataItem(path, xyzD[2]).Value)),
                            path);
            }
            return pointTree;
        }

        public override string ToString()
        {
            return string.Format("CrowNet SOG (processed)");
        }
    }

    public class CrowNetP
    {
        #region fields
        public string netType { get; set; }
        public int layerWidth { get; set; }
        public int layerHeight { get; set; }
        public bool isCircularRows { get; set; }
        public bool isCircularColumns { get; set; }
        public bool latticeTopology { get; set; }
        public bool neighborhood { get; set; }
        public bool[,] isWinner { get; set; }
        public double[][] rowX { get; set; }
        public double[][] rowY { get; set; }
        public double[][] rowZ { get; set; }
        public double[][] columnX { get; set; }
        public double[][] columnY { get; set; }
        public double[][] columnZ { get; set; }
        public double[][] hexagonalX { get; set; }
        public double[][] hexagonalY { get; set; }
        public double[][] hexagonalZ { get; set; }

        public double[] xVal { get; set; }
        public double[] yVal { get; set; }
        public double[] zVal { get; set; }

        public GH_Structure<GH_Number> allTrainingValues { get; set; }

        #endregion

        #region constructors

        /// <summary>
        /// Creates a new processed CrowNet in the design of an Self Organizing Map (Kohonen Network)
        /// </summary>
        public CrowNetP(string NetType, int LayerWidth, int LayerHeight, bool IsCircularRows, bool IsCircularColumns, bool LatticeTopology, bool Neighborhood, bool[,] IsWinner, double[][] RowX, double[][] RowY, double[][] RowZ, double[][] ColumnX, double[][] ColumnY, double[][] ColumnZ, double[][] HexagonalX, double[][] HexagonalY, double[][] HexagonalZ)
        {
            #region normal constructor
            this.netType = NetType;
            this.layerWidth = LayerWidth;
            this.layerHeight = LayerHeight;
            this.isCircularRows = IsCircularRows;
            this.isCircularColumns = IsCircularColumns;
            this.latticeTopology = LatticeTopology;
            this.neighborhood = Neighborhood;
            this.isWinner = IsWinner;
            this.rowX = RowX;
            this.rowY = RowY;
            this.rowZ = RowZ;
            this.columnX = ColumnX;
            this.columnY = ColumnY;
            this.columnZ = ColumnZ;
            this.hexagonalX = HexagonalX;
            this.hexagonalY = HexagonalY;
            this.hexagonalZ = HexagonalZ;
            this.allTrainingValues = new GH_Structure<GH_Number>();

            #endregion
        }

        /// <summary>
        /// Creates a new processed CrowNet in the design of an Self Organizing Map (Kohonen Network)
        /// </summary>
        public CrowNetP(string NetType, int LayerWidth, int LayerHeight, bool IsCircularRows, bool IsCircularColumns, bool LatticeTopology, bool Neighborhood, bool[,] IsWinner, double[][] RowX, double[][] RowY, double[][] RowZ, double[][] ColumnX, double[][] ColumnY, double[][] ColumnZ, double[][] HexagonalX, double[][] HexagonalY, double[][] HexagonalZ, GH_Structure<GH_Number> AllValuesAsTree)
        {
            #region normal constructor
            this.netType = NetType;
            this.layerWidth = LayerWidth;
            this.layerHeight = LayerHeight;
            this.isCircularRows = IsCircularRows;
            this.isCircularColumns = IsCircularColumns;
            this.latticeTopology = LatticeTopology;
            this.neighborhood = Neighborhood;
            this.isWinner = IsWinner;
            this.rowX = RowX;
            this.rowY = RowY;
            this.rowZ = RowZ;
            this.columnX = ColumnX;
            this.columnY = ColumnY;
            this.columnZ = ColumnZ;
            this.hexagonalX = HexagonalX;
            this.hexagonalY = HexagonalY;
            this.hexagonalZ = HexagonalZ;
            this.allTrainingValues = AllValuesAsTree;

            #endregion
        }

        /// <summary>
        /// Creates a new processed CrowNet to solve the travelling salesman problem
        /// </summary>
        public CrowNetP(string NetType, double[] XVal, double[] YVal, double[] ZVal, bool IsCircularRows)
        {
            this.netType = NetType;
            this.xVal = XVal;
            this.yVal = YVal;
            this.zVal = ZVal;
            this.isCircularRows = IsCircularRows;
            this.allTrainingValues = new GH_Structure<GH_Number>();
        }

        public CrowNetP()
        {
            #region blank constructor
            this.layerWidth = 0;
            this.layerHeight = 0;
            this.isCircularRows = false;
            this.isCircularColumns = false;
            this.latticeTopology = false;
            this.neighborhood = false;
            this.isWinner = null;
            this.rowX = null;
            this.rowY = null;
            this.rowZ = null;
            this.columnX = null;
            this.columnY = null;
            this.columnZ = null;
            this.hexagonalX = null;
            this.hexagonalY = null;
            this.hexagonalZ = null;
            this.allTrainingValues = new GH_Structure<GH_Number>();

            #endregion
        }

        public CrowNetP Duplicate()
        {
            CrowNetP dup = new CrowNetP(netType, layerWidth, layerHeight, isCircularRows, isCircularColumns, latticeTopology, neighborhood, isWinner, rowX, rowY, rowZ, columnX, columnY, columnZ, hexagonalX, hexagonalY, hexagonalZ, allTrainingValues);
            return dup;
        }
        #endregion

        #region properties
        public bool IsValid
        {
            get
            {
                if (this.layerHeight < 1) { return false; }
                if (this.layerWidth < 1) { return false; }
                return true;
            }
        }
        #endregion

        #region methods
        public override string ToString()
        {
            return string.Format("CrowNet SOM (processed)");
        }

        #endregion
    }

    public class CrowNetBPP
    {
        public BackpropagationNetwork network { get; set; }

        public List<int> layerStructure { get; set; }
        public List<int> neuronCount { get; set; }
        public double learningRate { get; set; }
        public List<ActivationLayer> hiddenLayerList { get; set; }

        public CrowNetBPP()
        {
            LinearLayer inputNull = new LinearLayer(1);
            LinearLayer outputNull = new LinearLayer(1);
            BackpropagationConnector nullConnector = new BackpropagationConnector(inputNull, outputNull);
            this.network = new BackpropagationNetwork(inputNull, outputNull);
        }

        public CrowNetBPP(BackpropagationNetwork Network)
        {
            
            this.network = Network;
        }

        /*public CrowNetBPP Flip()
        {
            ActivationLayer[] newLayerList = new ActivationLayer[hiddenLayerList.Count + 2];

            newLayerList[0] = new LinearLayer(network.OutputLayer.NeuronCount);
            newLayerList[hiddenLayerList.Count + 1] = new LinearLayer(network.InputLayer.NeuronCount);

            for (int i = 0; i < hiddenLayerList.Count; i++)
            {
                newLayerList[i + 1] = hiddenLayerList[hiddenLayerList.Count - 1 - i];
                if(hiddenLayerList[hiddenLayerList.Count - 1 - i] is SigmoidLayer)
                    newLayerList[i + 1] = new SigmoidLayer(hiddenLayerList[hiddenLayerList.Count - 1 - i].NeuronCount);

                else if (hiddenLayerList[hiddenLayerList.Count - 1 - i] is LinearLayer)
                    newLayerList[i + 1] = new LinearLayer(hiddenLayerList[hiddenLayerList.Count - 1 - i].NeuronCount);

                else if (hiddenLayerList[hiddenLayerList.Count - 1 - i] is LogarithmLayer)
                    newLayerList[i + 1] = new LogarithmLayer(hiddenLayerList[hiddenLayerList.Count - 1 - i].NeuronCount);

                else if (hiddenLayerList[hiddenLayerList.Count - 1 - i] is TanhLayer)
                    newLayerList[i + 1] = new TanhLayer(hiddenLayerList[hiddenLayerList.Count - 1 - i].NeuronCount);

                else if (hiddenLayerList[hiddenLayerList.Count - 1 - i] is SineLayer)
                    newLayerList[i + 1] = new SineLayer(hiddenLayerList[hiddenLayerList.Count - 1 - i].NeuronCount);
            }


            //CrowNetBP newNet = new CrowNetBP(newLS.ToList(), newNC.ToList(), learningRate);

            var connectors = network.Connectors.ToList();
            for (int i = 0; i < network.ConnectorCount; i++)
            {
                BackpropagationConnector bpc = new BackpropagationConnector(newLayerList[i], newLayerList[i+1]);
                var synapses = bpc.Synapses.ToList();
                var oldSynapses = connectors[network.ConnectorCount - 1 - i].Synapses.ToList();
                for (int j = 0; j < bpc.SynapseCount; j++)
                {
                    synapses[j].Weight = oldSynapses[j].Weight;

                    //backward counting synapses in each connector
                    //(bpc.Synapses as List<BackpropagationSynapse>)[j].Weight = ((network.Connectors as List<BackpropagationConnector>)[i].Synapses as List<BackpropagationSynapse>)[bpc.SynapseCount - j].Weight;
                }
            }

            BackpropagationNetwork newNet = new BackpropagationNetwork(newLayerList[0], newLayerList[newLayerList.Length-1]);
            return new CrowNetBPP(newNet);
        }*/

        public override string ToString()
        {
            return string.Format("CrowNet Backpropagation (trained)");
        }
    }    
}