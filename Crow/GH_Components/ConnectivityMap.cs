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
            pManager.AddLineParameter("Lines", "Lines", "Supply lines directly, inputs indA and indB are overwritten, if this is supplied", GH_ParamAccess.list);
            pManager.AddMeshParameter("Mesh", "Mesh", "If you supply a mesh, the first three inputs will be ignored.", GH_ParamAccess.item);
            pManager[0].Optional = true;
            pManager[1].Optional = true;
            pManager[2].Optional = true;
            pManager[3].Optional = true;
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
            Mesh mesh = new Mesh();

            DA.GetDataList(0, indA);
            DA.GetDataList(1, indB);
            DA.GetDataList(2, lines);
            DA.GetData(3, ref mesh);

            List<int> edgeList = new List<int>();
            int numVertices = 0;
            GH_Structure<GH_Integer> tree = new GH_Structure<GH_Integer>();

            //isIndices
            if (lines.Count == 0 && mesh.Vertices.Count == 0)
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
            //isLines
            else if (mesh.Vertices.Count == 0)
            {
                List<Point3d> vertices = new List<Point3d>();
                List<int> startIndices = new List<int>();
                List<int> endIndices = new List<int>();
                LinesToVertexPairs(lines, 0.001, out vertices, out startIndices, out endIndices);
                for(int i = 0; i <  startIndices.Count; i++)
                {
                    edgeList.Add(startIndices[i]);
                    edgeList.Add(endIndices[i]);
                    numVertices = vertices.Count;
                }
            }
            //isMesh
            else
            {
                mesh.Vertices.CombineIdentical(true, true);
                numVertices = mesh.Vertices.Count;
                for(int i = 0; i < mesh.TopologyEdges.Count; i++)
                {
                    edgeList.Add(mesh.TopologyEdges.GetTopologyVertices(i).I);
                    edgeList.Add(mesh.TopologyEdges.GetTopologyVertices(i).J);
                }                  
            }

            if (numVertices == 0 || edgeList.Count == 0)
                return;
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

        public void LinesToVertexPairs(List<Line> lines, double pointDuplicateThreshold, out List<Point3d> vertices, out List<int> startIndices, out List<int> endIndices)
        {
            List<Line> cleanLineList = new List<Line>();
            double ptDuplThrSquared = pointDuplicateThreshold * pointDuplicateThreshold;
            startIndices = new List<int>();
            endIndices = new List<int>();

            #region clean up line list
            List<Line> lHist = new List<Line>();
            for (int j = 0; j < lines.Count; j++)
            {
                //Clean list from duplicate lines
                Line lCand = lines[j];
                Point3d ptCandA = lCand.From;
                Point3d ptCandB = lCand.To;
                bool lineExistsAlready = false;
                foreach (Line lh in lHist)
                {
                    Line tempL = new Line(lCand.From, lCand.To);
                    if ((Utils.SquareDistance(tempL.From, lh.From) < ptDuplThrSquared && Utils.SquareDistance(tempL.To, lh.To) < ptDuplThrSquared) ||
                        (Utils.SquareDistance(tempL.From, lh.To) < ptDuplThrSquared && Utils.SquareDistance(tempL.To, lh.From) < ptDuplThrSquared))
                        lineExistsAlready = true;
                }

                //Clean list from too short lines
                if (!(Utils.SquareDistance(ptCandA, ptCandB) < ptDuplThrSquared || lineExistsAlready))
                {
                    lHist.Add(lCand);
                    cleanLineList.Add(lCand);
                }
                else
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Line nr. " + j + " is either invalid (too short) or appeared for the second time. It is ignored.");
            }
            #endregion

            //find unique line start indices
            vertices = new List<Point3d>();
            int advance = 0;
            for (int item = 0; item < cleanLineList.Count; item++)
            {
                Point3d ptCand = cleanLineList[item].From;
                int alreadyExistsAs = -1;
                for (int k = 0; k < vertices.Count; k++)
                {
                    //simple squared distance
                    if (Utils.SquareDistance(vertices[k], ptCand) < ptDuplThrSquared)
                    {
                        alreadyExistsAs = k;
                        startIndices.Add(alreadyExistsAs);
                        break;
                    }
                }
                if (alreadyExistsAs == -1)
                {
                    vertices.Add(ptCand);
                    startIndices.Add(advance);
                    advance++;
                }
            }

            //find unique line end indices
            for (int item = 0; item < cleanLineList.Count; item++)
            {
                Point3d ptCand = cleanLineList[item].To;
                int alreadyExistsAs = -1;
                for (int k = 0; k < vertices.Count; k++)
                {
                    if (Utils.SquareDistance(vertices[k], ptCand) < ptDuplThrSquared)
                    {
                        alreadyExistsAs = k;
                        endIndices.Add(alreadyExistsAs);
                        break;
                    }
                }
                if (alreadyExistsAs == -1)
                {
                    vertices.Add(ptCand);
                    endIndices.Add(advance);
                    advance++;
                }
            }
        }
    }
}