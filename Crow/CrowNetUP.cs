using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;

using Crow.Core.Backpropagation;

namespace Crow
{
    /// <summary>
    /// CrowNet class, this class defines the basic properties and methods for any CrowNet.
    /// </summary>
    public class CrowNetSOM2DUP
    {
        #region fields
        public string netType { get; set; }
        public int layerWidth { get; set; }
        public int layerHeight { get; set; }
        public bool isCircularRows { get; set; }
        public bool isCircularColumns { get; set; }
        public bool latticeTopology { get; set; }
        public bool neighborhood { get; set; }
        public double learningRate { get; set; }
        #endregion

        #region constructors
        public CrowNetSOM2DUP(string NetType, int LayerWidth, int LayerHeight, bool IsCircularRows, bool IsCircularColumns, bool LatticeTopology, bool Neighborhood, double LearningRate)
        {
            #region normal constructor
            this.netType = NetType;
            this.layerWidth = LayerWidth;
            this.layerHeight = LayerHeight;
            this.isCircularRows = IsCircularRows;
            this.isCircularColumns = IsCircularColumns;
            this.latticeTopology = LatticeTopology;
            this.neighborhood = Neighborhood;
            this.learningRate = LearningRate;
            #endregion
        }


        public CrowNetSOM2DUP()
        {
            this.netType = "";
            this.layerWidth = 0;
            this.layerHeight = 0;
            this.isCircularRows = false;
            this.isCircularColumns = false;
            this.latticeTopology = false;
            this.neighborhood = false;
            this.learningRate = 0;
        }

        public CrowNetSOM2DUP Duplicate()
        {
            CrowNetSOM2DUP dup = new CrowNetSOM2DUP(netType, layerWidth, layerHeight, isCircularRows, isCircularColumns, latticeTopology, neighborhood, learningRate);
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
            return string.Format("CrowNet SOM (unprocessed)");
        }

        #endregion
    }

    public class CrowNetSOMNDUP
    {
        #region fields
        public int[] size { get; set; }
        public bool[] circularity { get; set; }
        public bool latticeTopology { get; set; }
        public bool neighborhood { get; set; }
        public double learningRate { get; set; }
        public double[] initialNodes { get; set; }
        #endregion

        #region constructors
        public CrowNetSOMNDUP(int[] Size, bool[] Circularity, bool LatticeTopology, bool Neighborhood, double LearningRate, double[] InitialNodes)
        {
            #region normal constructor
            this.size = Size;
            this.circularity = Circularity;
            this.latticeTopology = LatticeTopology;
            this.neighborhood = Neighborhood;
            this.learningRate = LearningRate;
            this.initialNodes = InitialNodes;
            #endregion
        }

        public CrowNetSOMNDUP()
        {
            this.size = new int[0];
            this.circularity = new bool[0];
            this.latticeTopology = false;
            this.neighborhood = false;
            this.learningRate = 0;
        }

        public CrowNetSOMNDUP Duplicate()
        {
            CrowNetSOMNDUP dup = new CrowNetSOMNDUP(size, circularity, latticeTopology, neighborhood, learningRate, initialNodes);
            return dup;
        }
        #endregion

        #region properties
        public bool IsValid
        {
            get
            {
                foreach (int s in this.size) if (s < 1) return false;
                return true;
            }
        }
        #endregion

        #region methods
        public override string ToString()
        {
            return string.Format("CrowNet SOG (unprocessed)");
        }

        #endregion
    }

    public class CrowNetBP
    {
        public List<int> layerStructure { get; set; }
        public List<int> neuronCount { get; set; }
        public double learningRate { get; set; }
        public List<ActivationLayer> hiddenLayerList { get; set; }

        public CrowNetBP()
        {
            this.layerStructure = new List<int>();
            this.neuronCount = new List<int>();
            this.learningRate = 0.2;
        }

        public CrowNetBP(List<int> LayerStructure, List<int> NeuronCount, double LearningRate)
        {
            this.layerStructure = LayerStructure;
            this.neuronCount = NeuronCount;
            this.learningRate = Math.Max(LearningRate, 0.01);
        }

        public List<ActivationLayer> HiddenLayerList()
        {
            List<ActivationLayer> ActivationLayerList = new List<ActivationLayer>();

            for (int i = 0; i < neuronCount.Count; i++)
            {
                if (neuronCount[i] < 1) { neuronCount[i] = 1; };

                if (layerStructure[i] == 0) { SigmoidLayer currenthiddenLayer = new SigmoidLayer(neuronCount[i]); ActivationLayerList.Add(currenthiddenLayer); }
                else if (layerStructure[i] == 1) { LinearLayer currenthiddenLayer = new LinearLayer(neuronCount[i]); ActivationLayerList.Add(currenthiddenLayer); }
                else if (layerStructure[i] == 2) { LogarithmLayer currenthiddenLayer = new LogarithmLayer(neuronCount[i]); ActivationLayerList.Add(currenthiddenLayer); }
                else if (layerStructure[i] == 3) { SineLayer currenthiddenLayer = new SineLayer(neuronCount[i]); ActivationLayerList.Add(currenthiddenLayer); }
                else if (layerStructure[i] == 4) { TanhLayer currenthiddenLayer = new TanhLayer(neuronCount[i]); ActivationLayerList.Add(currenthiddenLayer); }
                else { return new List<ActivationLayer>(); }
            }

            return ActivationLayerList;
        }

        public BackpropagationNetwork network(int trainInVectorDimension, int trainOutVectorDimension)
        {
            this.hiddenLayerList = HiddenLayerList();

            ActivationLayer inputLayer = new LinearLayer(trainInVectorDimension);
            ActivationLayer outputLayer = new SigmoidLayer(trainOutVectorDimension);

            BackpropagationConnector bpc0 = new BackpropagationConnector(inputLayer, this.hiddenLayerList[0]);
            for (int i = 1; i < this.hiddenLayerList.Count; i++)
            {
                bpc0 = new BackpropagationConnector(this.hiddenLayerList[i - 1], this.hiddenLayerList[i]);
            }
            bpc0 = new BackpropagationConnector(this.hiddenLayerList[this.hiddenLayerList.Count - 1], outputLayer);

            BackpropagationNetwork network = new BackpropagationNetwork(inputLayer, outputLayer);

            /*ActivationLayer inputLayer = hiddenLayerList[0];
            ActivationLayer outputLayer = hiddenLayerList[hiddenLayerList.Count - 1];

            if(hiddenLayerList.Count != 2)
            {
                BackpropagationConnector bpc0 = new BackpropagationConnector(inputLayer, this.hiddenLayerList[1]);
                for (int i = 2; i < this.hiddenLayerList.Count - 1; i++)
                {
                    bpc0 = new BackpropagationConnector(this.hiddenLayerList[i - 1], this.hiddenLayerList[i]);
                }
                bpc0 = new BackpropagationConnector(this.hiddenLayerList[this.hiddenLayerList.Count - 2], outputLayer);
            }
            
            BackpropagationNetwork network = new BackpropagationNetwork(inputLayer, outputLayer);*/
            network.SetLearningRate(this.learningRate);

            return network;
        }

        public override string ToString()
        {
            return string.Format("CrowNet Backpropagation (untrained)");
        }
    }
}