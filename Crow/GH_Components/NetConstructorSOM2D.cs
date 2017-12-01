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
    public class NetConstructorSOM2D : GH_Component
    {
        private int layerWidth;
        private int layerHeight;
        public bool isCircularRows = new bool();
        public bool isCircularColumns = new bool();
        public bool latticeTopology = new bool();
        public bool neighborhood = new bool();
        public bool neighborDistance = true;
        private double learningRate;

        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public NetConstructorSOM2D()
            : base("CrowNet Constructor - Self-Organizing Map (2D)", "SOM-2D",
                "Construct a 2-dimensional Kohonen network topology",
                "Crow", "Unsupervised") { }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddIntegerParameter("Layer Width", "W", "The number of neurons in u direction of the output net", GH_ParamAccess.item, 12);
            pManager.AddIntegerParameter("Layer Height", "H", "The number of neurons in v direction of the output net", GH_ParamAccess.item, 12);
            pManager.AddBooleanParameter("Circular Rows", "c Row", "Use a circular neuron layout in the rows?", GH_ParamAccess.item, false);
            pManager.AddBooleanParameter("Circular Columns", "c Col", "Use a circular neuron layout in the columns?", GH_ParamAccess.item, false);
            pManager.AddBooleanParameter("Lattice Topology", "L", "0 = Rectangular Lattice, 1 = Hexagonal Lattice", GH_ParamAccess.item, true);
            pManager.AddBooleanParameter("Neighborhood Distance Type", "ND", "0 = Manhattan distance, 1 = Euclidean Grid Distance", GH_ParamAccess.item, true);
            pManager.AddBooleanParameter("Neighborhood Function", "NF", "0 = Gaussian Function, 1 = Mexican Hat Function", GH_ParamAccess.item, false);
            pManager.AddNumberParameter("Learning Rate", "LR", "set the learning rate (between 0 and 1)", GH_ParamAccess.item, 0.2);
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
            GH_Integer i1 = new GH_Integer();                    /// layerWidth
            GH_Integer i2 = new GH_Integer();                    /// layerHeight

            if (!DA.GetData(0, ref i1)) return;
            if (!DA.GetData(1, ref i2)) return;
            if (!DA.GetData(2, ref isCircularRows)) return;
            if (!DA.GetData(3, ref isCircularColumns)) return;
            if (!DA.GetData(4, ref latticeTopology)) return;
            if (!DA.GetData(5, ref neighborhood)) return;
            if (!DA.GetData(6, ref neighborDistance)) return;
            if (!DA.GetData(7, ref learningRate)) return;


            i1.CastTo<int>(out layerWidth);
            i2.CastTo<int>(out layerHeight);

            CrowNetSOM2DUP NetUP = new CrowNetSOM2DUP("som", layerWidth, layerHeight, isCircularRows, isCircularColumns, latticeTopology, neighborhood, neighborDistance, learningRate);
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
                return Crow.Properties.Resources.selforg3D;
            }
        }
        # endregion

        #region Guid
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("{B19FF105-8C40-482F-BEF7-0B14E021DA7E}"); }
        }
        #endregion
    }
}