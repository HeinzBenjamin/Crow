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
    public class NetConstructorSOG : GH_Component
    {

        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public NetConstructorSOG()
            : base("CrowNet Constructor - Self-Organizing Grid (generic)", "SOG",
                "Construct an n-dimensional Kohonen network topology",
                "Crow", "Unsupervised") { }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddIntegerParameter("Layer Dimensions", "D", "The extend of the network in each dimension as a list of integers.\n(e.g. a 3d-cube with an edge length of ten neurons would be {10, 10, 10})", GH_ParamAccess.list);
            pManager.AddNumberParameter("Initial Node Values (optional)", "V", "Optional initial node values. You have three options here:\n1) Leave empty: all nodes are initiliazed with starting values 0.0\nSupply a list containing two numbers: the lower and upper bound of random initialisation (same in every dimension)\n3)Supply a tree structure matching the layer dimensions in which every node's values are defined.", GH_ParamAccess.tree);
            //pManager.AddBooleanParameter("Dimension Circularity", "C", "Use a circular neuron layout in the rows?", GH_ParamAccess.list);
            //pManager.AddBooleanParameter("Lattice Topology", "L", "0 = Rectangular Lattice, 1 = Hexagonal Lattice", GH_ParamAccess.item, false);
            pManager.AddBooleanParameter("Neighborhood Function", "NF", "0 = Gaussian Function, 1 = Mexican Hat Function", GH_ParamAccess.item, false);
            pManager.AddBooleanParameter("Neighborhood Distance Type", "ND", "0 = Manhattan distance, 1 = Euclidean Grid Distance", GH_ParamAccess.item, true);
            pManager.AddNumberParameter("Learning Rate", "LR", "Set the learning rate (between 0.0 and 1.0)", GH_ParamAccess.item, 0.2);
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
            if (!DA.GetData(2, ref neighborhood)) return;
            DA.GetData(3, ref neighborDistance);
            if (!DA.GetData(4, ref learningRate)) return;
            List<double> initialNodes = new List<double>();

            if (DA.GetDataTree(1, out gi) && gi.DataCount > 0)
            {
                int branchCount = 1;
                foreach (int b in size) branchCount *= b;
                if (gi.Branches.Count != branchCount) { AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "The tree structure of given input data and network size don't match!"); return; }
                foreach (GH_Number n in gi.FlattenData()) initialNodes.Add(n.Value);           
            }
            CrowNetSOMNDUP NetUP = new CrowNetSOMNDUP(size.ToArray(), circularity.ToArray(), latticeTopology, neighborhood, neighborDistance, learningRate, initialNodes.ToArray());

            DA.SetData(0, NetUP);
        }

        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Crow.Properties.Resources.selforgND;
            }
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("{9B8C3000-DD5B-467E-A7AA-A40D11511E1C}"); }
        }
    }
}