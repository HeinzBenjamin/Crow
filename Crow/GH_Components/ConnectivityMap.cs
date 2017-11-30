using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;

namespace Crow.GH_Components
{
    public class ConnectivityMap : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the ConnectivityMap class.
        /// </summary>
        public ConnectivityMap()
          : base("Connectivity Map", "cMap",
              "Computes a vertex-based neighborhood map from lines",
              "Crow", "Util")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddIntegerParameter("Line Start Index", "indA", "", GH_ParamAccess.list);
            pManager.AddIntegerParameter("Line End Index", "indB", "", GH_ParamAccess.list);
            pManager.AddLineParameter("Lines", "lines", "Supply lines directly, inputs indA and indB are overwritten, if this is supplied", GH_ParamAccess.list);
            pManager[0].Optional = true;
            pManager[1].Optional = true;
            pManager[2].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddIntegerParameter("Connectivity Map", "CM", "First tree layer refers to ", GH_ParamAccess.tree);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<int> indA = new List<int>();
            List<int> indB = new List<int>();
            List<Line> lines = new List<Line>();

            DA.GetDataList(0, indA);
            DA.GetDataList(1, indB);
            DA.GetDataList(2, lines);



            List<int> edgeList = new List<int>();
            int numVertices = 0;
            GH_Structure<GH_Integer> tree = new GH_Structure<GH_Integer>();

            //weave
            if (lines.Count == 0)
            {
                if (indA.Count != indB.Count)
                    throw new Exception("indA and indB don't match!");

                for (int i = 0; i < indA.Count; i++)
                {
                    edgeList.Add(indA[i]);
                    edgeList.Add(indB[i]);
                    if (indA[i] > numVertices)
                        numVertices = indA[i];
                    if (indB[i] > numVertices)
                        numVertices = indB[i];
                }
                numVertices++;

            }
            else
            {
                //.... TO DO!!!
            }


            Utils.ConnectivityMap map = new Utils.ConnectivityMap(numVertices, edgeList.ToArray());

            for (int i = 0; i < map.Lists.Length; i++)
            {
                for (int j = 0; j < map.Lists[i].Count; j++)
                {
                    GH_Path path = new GH_Path(i, j);
                    for (int k = 0; k < map.Lists[i][j].Count; k++)
                        tree.Append(new GH_Integer(map.Lists[i][j][k]), path);
                }
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
                return Crow.Properties.Resources.connectivitymap;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("653b9a8e-c52e-4d37-ae3a-74ce87712591"); }
        }
    }
}