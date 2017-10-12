using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Grasshopper.Kernel.Data;
using Rhino.Geometry;

namespace Crow
{
    public class SOMPoints2D : GH_Component
    {

        /// <summary>
        /// Initializes a new instance of the CrowPoints class.
        /// </summary>
        public SOMPoints2D()
            : base("2D SOM Point Display", "SOM-2D Points",
                "Displays your SOM-2D data as a cloud of points highlighting the propagation 'winners'",
                "Crow", "SOM")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Crow Net", "Net", "Connect your processed SOM-2D Network.", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.Register_PointParam("All Points", "all", "All the points forming the Crow point cloud", GH_ParamAccess.list);
            pManager.Register_PointParam("Winner Points", "winners", "The points propagated as winners (closest to input)", GH_ParamAccess.list);
            pManager.Register_DoubleParam("All values", "values", "All values as a data tree", GH_ParamAccess.tree);
            pManager.AddPathParameter("Winner path", "wp", "Path to winner neuron", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            GH_Structure<GH_Point> allPoints = new GH_Structure<GH_Point>();
            List<GH_Point> winnerPoints = new List<GH_Point>();
            GH_Structure<GH_Number> allValuesAsTree = new GH_Structure<GH_Number>();
            GH_Path winnerPath = new GH_Path();
            CrowNetP Net = new CrowNetP();



            if (!DA.GetData<CrowNetP>(0, ref Net)) return;

            if (Net.netType == "som")
            {
                for (int i = 0; i < Net.layerWidth; i++)
                {
                    for (int j = 0; j < Net.layerHeight; j++)
                    {
                        allPoints.Append(new GH_Point(new Point3d(Net.columnX[i][j], Net.columnY[i][j], Net.columnZ[i][j])), new GH_Path(i));
                        if (Net.isWinner[i, j])
                        { 
                            winnerPoints.Add(new GH_Point(new Point3d(Net.columnX[i][j], Net.columnY[i][j], Net.columnZ[i][j])));
                            winnerPath = new GH_Path(i, j);
                        }
                    }
                }
            }

            DA.SetDataList(0, allPoints);
            DA.SetDataList(1, winnerPoints);
            DA.SetDataTree(2, Net.allTrainingValues);
            DA.SetData(3, winnerPath);
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
                return Crow.Properties.Resources.crowpoints;
            }
        }
        # endregion

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("{77E06921-2A26-49F3-A6DA-9106DDB82993}"); }
        }
    }
}