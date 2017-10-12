using System;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows.Forms;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;

using Crow.Core.Backpropagation;
using Crow.Core.SOM;
using Crow.Core;

namespace Crow
{
    public class EngineBP_new : GH_Component
    {

        private BackpropagationNetwork Network;
        private bool networkLoaded = true;

        /// <summary>
        /// Initializes a new instance of the DecomposePlankton class.
        /// </summary>
        public EngineBP_new()
            : base("Crow Engine Backpropagation", "Crow BP",
                "Create an artificial neural network, that minimizes errors using the backpropagation method.",
                "Crow", "Backpropagation")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddIntegerParameter("Cycles", "C", "Number of training cycles per training [int]", GH_ParamAccess.item, 1000);
            pManager.AddGenericParameter("CrowNet Backpropagation", "Net", "Connect your backpropagation netwrok here", GH_ParamAccess.item);
            pManager.AddNumberParameter("Training Input Vectors", "trainIn", "Supply a set of input vectors as list of list to be mapped into the output vectors using the given backpropagation network structure.", GH_ParamAccess.tree);
            pManager.AddNumberParameter("Training Output Vectors", "trainOut", "Supply a set of output vectors as list of list into which the input vectors are mapped into.", GH_ParamAccess.tree);
            pManager[0].Optional = true;
            pManager[1].Optional = true;
            pManager[2].Optional = true;
            pManager[3].Optional = true;
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.Register_DoubleParam("Mean Square Error", "mse", "The mean square error that still remains.", GH_ParamAccess.list);
            pManager.Register_GenericParam("Network", "Net", "The network established to be passed on to the next component", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            CrowNetBP net = new CrowNetBP();
            if (!networkLoaded)
            {
                int cycles = 1000;
                
                GH_Structure<GH_Number> tiv = new GH_Structure<GH_Number>();
                GH_Structure<GH_Number> tov = new GH_Structure<GH_Number>();

                DA.GetData(0, ref cycles);
                DA.GetData(1, ref net);
                DA.GetDataTree(2, out tiv);
                DA.GetDataTree(3, out tov);

                double[][] trainInVectors = Utils.GHTreeToMultidimensionalArray(tiv);
                double[][] trainOutVectors = Utils.GHTreeToMultidimensionalArray(tov);


                int trainVectorCount = trainInVectors.Length;
                if (trainVectorCount != trainOutVectors.Length) AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Please supply an equal amount of input and output training vectors!");

                int trainInVectorDimension = trainInVectors[0].Length;
                int trainOutVectorDimension = trainOutVectors[0].Length;

                BackpropagationNetwork network = net.network(trainInVectorDimension, trainOutVectorDimension);


                // set Trainingset
                TrainingSet trainingSet = new TrainingSet(trainInVectorDimension, trainOutVectorDimension);

                for (int i = 0; i < trainVectorCount; i++)
                {
                    trainingSet.Add(new TrainingSample(trainInVectors[i], trainOutVectors[i]));
                }

                // train
                network.Learn(trainingSet, cycles);
                this.Network = network;
            }
            if(this.Network != null)
            {
                DA.SetData(0, this.Network.MeanSquaredError.ToString("0.0000000000"));

                CrowNetBPP nn = new CrowNetBPP(this.Network);
                nn.hiddenLayerList = net.hiddenLayerList;
                nn.layerStructure = net.layerStructure;
                nn.neuronCount = net.neuronCount;
                DA.SetData(1, nn);
            }
            
            networkLoaded = false;
        }

        protected override void AppendAdditionalComponentMenuItems(ToolStripDropDown menu)
        {
            ToolStripMenuItem item1 = Menu_AppendItem(menu, "Save network", save_btn_Clicked);
            ToolStripMenuItem item2 = Menu_AppendItem(menu, "Load network", load_btn_Clicked);
            item2.ToolTipText = "When you load a .cnw file the Grasshopper input of this component will be ignored.";
        }

        private void save_btn_Clicked(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "CNW files (*.cnw)|*.cnw";
            saveFileDialog.InitialDirectory = ".";

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    System.IO.Stream stream = System.IO.File.Open(saveFileDialog.FileName, System.IO.FileMode.Create);

                    BinaryFormatter bFormatter = new BinaryFormatter();
                    bFormatter.Serialize(stream, this.Network);
                    stream.Close();
                }
                catch (Exception ex) { AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Couldn't save the crow net:\n" + ex.Message); }
            }
        }

        private void load_btn_Clicked(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "CNW files (*.cnw)|*.cnw";
            openFileDialog.InitialDirectory = ".";
            if(openFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    System.IO.Stream stream = System.IO.File.Open(openFileDialog.FileName, System.IO.FileMode.Open);

                    BinaryFormatter bFormatter = new BinaryFormatter();
                    this.Network = bFormatter.Deserialize(stream) as BackpropagationNetwork;
                    stream.Close();
                    networkLoaded = true;
                    ExpireSolution(true);
                }
                catch (Exception ex) { AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Couldn't load the crow net:\n" + ex.Message); }
            }
        }


        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                // return Resources.IconForThisComponent;                
                return Crow.Properties.Resources.crowengines;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("{3719A58C-D362-4F7B-88FE-54FAA01F84FA}"); }
        }
    }
}