using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

using Crow.Core;
using Crow.Core.Backpropagation;

namespace Crow.GH_Components
{
    public class NetConstructorBP : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the NetbuilderBP class.
        /// </summary>
        public NetConstructorBP()
            : base("CrowNet Constructor - Backpropagation Network", "Backprop",
                "Construct a feed forward network, that employs backpropagation for error reduction",
                "Crow", "Supervised") { }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddIntegerParameter("Layer Structure", "LS", "The amount and type of neuron layers as integer list.   0: Sigmoid Layer  -  1: Linear Layer  -  2: Logarithmic Layer  -  3: Sine Layer  -  4: TanH Layer", GH_ParamAccess.list);
            pManager.AddIntegerParameter("Neuron Count", "NC", "Amount of neurons per layer provided in layer structure.", GH_ParamAccess.list);
            pManager.AddNumberParameter("Learning Rate", "LR", "Learning rate (0.0 to 2.0)", GH_ParamAccess.item, 0.2);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("CrowNet", "Net", "The unprocessed CrowNet to be passed to the backpropagation engine.", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<int> layerStructure = new List<int>();
            List<int> neuronCount = new List<int>();
            double learningRate = 0.2;

            DA.GetDataList(0, layerStructure);
            DA.GetDataList(1, neuronCount);
            DA.GetData(2, ref learningRate);

            if (neuronCount.Count != layerStructure.Count) AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "The inputs layer structure and neuron count must have the same amount of items");
            foreach (int l in layerStructure) if (l < 0 || l > 4) AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "The input layer structure only accepts integers between 0 and 4.");

            

            DA.SetData(0, new CrowNetBP(layerStructure, neuronCount, learningRate));

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
                return Crow.Properties.Resources.backprop;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("{9323cd94-338b-41b0-8906-8616477efe11}"); }
        }
    }
}