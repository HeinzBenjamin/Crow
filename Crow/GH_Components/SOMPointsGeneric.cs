using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;

using System.Windows.Forms;

namespace Crow.GH_Components
{
    public class SOMPointsGeneric : GH_Component
    {
        GH_Structure<GH_Number> adressTree = new GH_Structure<GH_Number>();
        bool showPoints = true;

        public SOMPointsGeneric()
            : base("Generic SOG Display", "SOG Data",
                "Displays your SOG data. Position data can be displayed as a cloud of points. Winners are highlighted. Chose data by 'address' in grid topology.",
                "Crow", "SOM")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Crow Net", "Net", "Connect your processed SOG", GH_ParamAccess.item);
            pManager.AddIntegerParameter("x,y,z dimension indices (Optional)", "(xyzD)", "Indices of the dimension in the training vectors that encode position data (x,y,z). By default the first three vector values are used", GH_ParamAccess.list);
            pManager[1].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.Register_DoubleParam("Data vectors", "data", "Raw data of trained vectors", GH_ParamAccess.tree);
            pManager.AddPointParameter("Points", "points", "All the points forming the Crow point cloud", GH_ParamAccess.tree);
            pManager.Register_IntegerParam("Adress structure", "adresses", "Adress structure of your point cloud", GH_ParamAccess.tree);
            pManager.Register_IntegerParam("Winner Index", "winIndex", "Index of winner point.", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            CrowNetSOMNDP net = new CrowNetSOMNDP();
            List<int> xyzD = new List<int> ();

            DA.GetData(0, ref net);
            if (DA.GetDataList(1, xyzD)) showPoints = true;
            else xyzD = new List<int> { 0, 1, 2 };
            
            if (net.trainedVectors.GetLength(0) > 0)
            {
                GH_Structure<GH_Number> dataTree = net.DataTree();
                GH_Structure<GH_Point> pointTree = net.PointTree(xyzD);

                DA.SetDataTree(0, dataTree);
                if(showPoints) DA.SetDataTree(1, pointTree);
                if (net.adressBook.GetLength(0) != adressTree.Branches.Count || net.adressBook[0].Length != adressTree.Branches[0].Count) adressTree = Utils.MultidimensionalArrayToGHTree(net.adressBook);
                DA.SetDataTree(2, adressTree);

                //SUPER SHITTY SOLUTION TO FIND WINNER
                int[] winner = net.winner;
                if (net.winner.Length == 2) winner = new int[3] { net.winner[0], net.winner[1], 0 };
                else if (net.winner.Length == 1) winner = new int[3] { net.winner[0], 0, 0 };
                DA.SetData(3, Utils.AdressToIndex(net.adressBook, winner));
            }
        }


        protected override void AppendAdditionalComponentMenuItems(ToolStripDropDown menu)
        {

            ToolStripMenuItem item1 = Menu_AppendItem(menu, "Show Points", item1_Clicked, true, showPoints);
            item1.ToolTipText = "Tick to display the first three dimensions of the trained vectors as points.";
        }

        private void item1_Clicked(object sender, EventArgs e)
        {
            RecordUndoEvent("showPoints");
            showPoints = !showPoints;
            ExpireSolution(true);
        }

        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Crow.Properties.Resources.crowpointsnd;
            }              
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("{2a3d923a-3197-4256-a190-120578dfa30d}"); }
        }
    }
}