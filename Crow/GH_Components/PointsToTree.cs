using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Grasshopper.Kernel.Data;

using Rhino.Geometry;

namespace Crow.GH_Components
{
    public class PointsToTree : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the PointsToTree class.
        /// </summary>
        public PointsToTree()
          : base("PointsToTree", "P2T",
              "Turns a list of points into a tree of numbers",
              "Crow", "Util")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddPointParameter("Points", "Pts", "", GH_ParamAccess.list);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddNumberParameter("Tree", "T", "Each tree branch consists of the point coordinates as a list.", GH_ParamAccess.tree);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            GH_Structure<GH_Number> tree = new GH_Structure<GH_Number>();
            List<Point3d> pts = new List<Point3d>();

            if(!DA.GetDataList(0, pts)) return;

            for(int i = 0; i < pts.Count; i++)
            {
                GH_Path p = new GH_Path(i);
                tree.Append(new GH_Number(pts[i].X), p);
                tree.Append(new GH_Number(pts[i].Y), p);
                tree.Append(new GH_Number(pts[i].Z), p);
            }
            DA.SetDataTree(0, tree);
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Crow.Properties.Resources.point2tree;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("{120142f2-bf53-4861-ad45-b87cfbea7f1d}"); }
        }
    }
}