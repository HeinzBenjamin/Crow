using System;
using System.Collections.Generic;
using System.Drawing;

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
    public class NetConstructorSOMND : GH_Component
    {

        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public NetConstructorSOMND()
            : base("CrowNet Constructor - Self-Organizing Grid (generic)", "SOG",
                "Construct an n-dimensional Kohonen network topology",
                "Crow", "Unsupervised") { }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddNumberParameter("Nodes)
            pManager.AddNumberParameter("Initial Nodes (optional)", "IN", "Initial nodes. If not supplied zeroes will be used. Tree structure has to match layer dimensions.", GH_ParamAccess.tree);
            pManager.AddBooleanParameter("Neighborhood Function", "NF", "0 = Gaussian Function, 1 = Mexican Hat Function", GH_ParamAccess.item, false);
            pManager.AddNumberParameter("Learning Rate", "LR", "Set the learning rate (between 0 and 1)", GH_ParamAccess.item, 0.2);
            pManager[1].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.Register_GenericParam("CrowNet", "Net", "The unprocessed CrowNet to be passed to the Crow Engine for unsupersised learning.");
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {

            #region initialize empty variables
            List<int> size = new List<int>();
            List<bool> circularity = new List<bool>();
            bool latticeTopology = false;
            bool neighborhood = false;
            bool neighborDistance = true;
            double learningRate = 0.2d;
            GH_Structure<GH_Number> gi = new GH_Structure<GH_Number>();
            # endregion


            if (!DA.GetDataList(0, size)) return;
            //if (!DA.GetDataList(1, circularity)) return;
            //if (!DA.GetData(2, ref latticeTopology)) return;
            if (!DA.GetData(1, ref neighborhood)) return;
            DA.GetData(2, ref neighborDistance);
            if (!DA.GetData(3, ref learningRate)) return;
            List<double> initialNodes = new List<double>();

            if (DA.GetDataTree(4, out gi) && gi.DataCount > 0)
            {
                int branchCount = 1;
                foreach (int b in size) branchCount *= b;
                if (gi.Branches.Count != branchCount) { AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "The tree structure of given input data and network size don't match!"); return; }
                foreach (GH_Number n in gi.FlattenData()) initialNodes.Add(n.Value);           
            }
            CrowNetSOMNDUP NetUP = new CrowNetSOMNDUP(size.ToArray(), circularity.ToArray(), latticeTopology, neighborhood, neighborDistance, learningRate, initialNodes.ToArray());

            DA.SetData(0, NetUP);
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
                return Crow.Properties.Resources.selforgND;
            }
        }
        # endregion

        #region Guid
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("{1C13E01A-8A8C-40E2-8BEE-ED20C32C10BF}"); }
        }
        #endregion
    }
}