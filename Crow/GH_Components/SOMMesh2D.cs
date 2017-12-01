using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;

namespace Crow
{
    public class SOMMesh2D : GH_Component
    {

        /// <summary>
        /// Initializes a new instance of the CrowDisplay class.
        /// </summary>
        public SOMMesh2D()
            : base("2D SOM Mesh Display", "SOM-2D Mesh",
                "Displays your SOM-2D topology as a mesh.",
                "Crow", "Unsupervised")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Crow Net", "Net", "Connect your processed Crow Network to be shown as a mesh", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.Register_MeshParam("Crow Mesh", "Mesh", "Crow neural network shown as a mesh", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {

            GH_Mesh CMesh = new GH_Mesh();
            CrowNetP Net = new CrowNetP();

            if (!DA.GetData<CrowNetP>(0, ref Net)) return;

            int H = Net.layerHeight;
            int W = Net.layerWidth;
            Mesh RhinoMesh = new Mesh();


            for (int i = 0; i < W; i++)
            {
                for (int j = 0; j < H; j++)
                {
                    if (!Net.latticeTopology) RhinoMesh.Vertices.Add(new Point3d(Net.columnX[i][j], Net.columnY[i][j], Net.columnZ[i][j]));
                    else RhinoMesh.Vertices.Add(new Point3d(Net.hexagonalX[i][j], Net.hexagonalY[i][j], Net.hexagonalZ[i][j]));
                }
            }


            int pointCount = H * W;


            if (!Net.latticeTopology)   // quad mesh
            {

                #region quad mesh
                if (!Net.isCircularRows && !Net.isCircularColumns)         // no circular rows or columns
                    for (int i = 0; i < pointCount; i++)
                    {
                        if (i % H != H - 1 && !(i > pointCount - H - 1))
                        {
                            RhinoMesh.Faces.AddFace(i, i + 1, i + H + 1, i + H);
                        }
                    }

                if (!Net.isCircularRows && Net.isCircularColumns)         // circular columns but no circular rows
                    for (int i = 0; i < pointCount; i++)
                    {
                        if (i % H != H - 1 && !(i > pointCount - H - 1))
                        {
                            RhinoMesh.Faces.AddFace(i, i + 1, i + H + 1, i + H);
                        }
                        else
                        {
                            if (!(i > pointCount - H - 1)) { RhinoMesh.Faces.AddFace(i, i - (H - 1), i + 1, i + H); }
                        }
                    }

                if (Net.isCircularRows && !Net.isCircularColumns)         // no circular columns but circular rows
                    for (int i = 0; i < pointCount; i++)
                    {
                        if (i % H != H - 1 && !(i > pointCount - H - 1))
                        {
                            RhinoMesh.Faces.AddFace(i, i + 1, i + H + 1, i + H);
                        }
                        else
                        {
                            if (i > (pointCount - H - 1) && !(i == pointCount - 1)) { RhinoMesh.Faces.AddFace(i, i + 1, (i + 1) - (H * (W - 1)), i - (H * (W - 1))); }
                        }
                    }


                if (Net.isCircularRows && Net.isCircularColumns)          // circular rows and circular columns
                    for (int i = 0; i < pointCount; i++)
                    {
                        if (i % H != H - 1 && !(i > pointCount - H - 1))  // if is not in upper or left side
                        {
                            RhinoMesh.Faces.AddFace(i, i + 1, i + H + 1, i + H);
                        }
                        else
                        {
                            if (!(i > pointCount - H - 1)) { RhinoMesh.Faces.AddFace(i, i - (H - 1), i + 1, i + H); } //if is in upper corner
                            if (i > (pointCount - H - 1) && !(i == pointCount - 1)) { RhinoMesh.Faces.AddFace(i, i + 1, (i + 1) - (H * (W - 1)), i - (H * (W - 1))); } // if is in left side
                            if (i == pointCount - 1) { RhinoMesh.Faces.AddFace(i, i - H + 1, 0, H - 1); }
                        }
                    }
                #endregion
            }

            else                        // tri mesh
            {
                #region tri mesh
                //int o = 1; //flip orientation with 0 and 1

                if (!Net.isCircularRows && !Net.isCircularColumns)         // no circular rows or columns
                    for (int i = 0; i < pointCount; i++)
                    {
                        if (i % H != H - 1 && !(i > pointCount - 2 * H - 1))
                        {
                            RhinoMesh.Faces.AddFace(i, i + 1, i + H + 1, i + H);
                        }
                    }

                if (!Net.isCircularRows && Net.isCircularColumns)         // circular columns but no circular rows
                {
                    for (int w = 0; w < W; w++)
                    {
                        for (int h = 0; h < H; h++)
                        {
                            int i = w * H + h;
                            if (i < pointCount - 2 * H - 1)
                            {
                                if (h % 2 == 0)
                                {
                                    if (h != H - 1)
                                    {
                                        RhinoMesh.Faces.AddFace(i, i + 1, i + H + 1);
                                        RhinoMesh.Faces.AddFace(i, i + H + 1, i + H);
                                    }
                                    else
                                    {
                                        RhinoMesh.Faces.AddFace(i, i + 1, i + H);
                                        RhinoMesh.Faces.AddFace(i + 1, i + H + 1, i + H);
                                    }
                                }
                                else
                                {
                                    RhinoMesh.Faces.AddFace(i, i + 1, i + H);
                                    RhinoMesh.Faces.AddFace(i + 1, i + H + 1, i + H);
                                }
                            }
                        }


                    }

                }

                if (Net.isCircularRows && !Net.isCircularColumns)         // no circular columns but circular rows
                    for (int i = 0; i < pointCount; i++)
                    {
                        if (i % H != H - 1 && !(i > pointCount - H - 1))
                        {
                            RhinoMesh.Faces.AddFace(i, i + 1, i + H + 1, i + H);
                        }
                        else
                        {
                            if (i > (pointCount - H - 1) && !(i == pointCount - 1)) { RhinoMesh.Faces.AddFace(i, i + 1, (i + 1) - (H * (W - 1)), i - (H * (W - 1))); }
                        }
                    }


                if (Net.isCircularRows && Net.isCircularColumns)          // circular rows and circular columns
                    for (int i = 0; i < pointCount; i++)
                    {
                        if (i % H != H - 1 && !(i > pointCount - H - 1))  // if is not in upper or left side
                        {
                            RhinoMesh.Faces.AddFace(i, i + 1, i + H + 1, i + H);
                        }
                        else
                        {
                            if (!(i > pointCount - H - 1)) { RhinoMesh.Faces.AddFace(i, i - (H - 1), i + 1, i + H); } //if is in upper corner
                            if (i > (pointCount - H - 1) && !(i == pointCount - 1)) { RhinoMesh.Faces.AddFace(i, i + 1, (i + 1) - (H * (W - 1)), i - (H * (W - 1))); } // if is in left side
                            if (i == pointCount - 1) { RhinoMesh.Faces.AddFace(i, i - H + 1, 0, H - 1); }
                        }
                    }
                #endregion
            }



            CMesh.CastFrom(RhinoMesh);
            DA.SetData(0, CMesh);


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
                return Crow.Properties.Resources.crowmesh;
            }
        }
        # endregion

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("{CBB22576-08BC-44D8-9FB0-FBE24645DBD9}"); }
        }
    }
}