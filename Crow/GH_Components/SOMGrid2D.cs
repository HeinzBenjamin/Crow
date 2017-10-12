using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;

namespace Crow
{
    public class SOMGrid2D : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the CrowGrid class.
        /// </summary>
        public SOMGrid2D()
            : base("2D SOM Grid Display", "SOM-2D Grid",
                "Displays your two-dimensional SOM as a grid of lines",
                "Crow", "SOM")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Crow Net", "Net", "Connect your processed SOM-2D to be shown as a grid", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.Register_LineParam("Grid Rows", "R", "The lines forming the grid's rows", GH_ParamAccess.list);
            pManager.Register_LineParam("Grid Columns", "C", "The lines forming the grid's columns", GH_ParamAccess.list);
            pManager.Register_LineParam("Hexagonal Rows", "H", "The lines turning the quads from rows and columns into triangles (optional)", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<GH_Line> rowLines = new List<GH_Line>();
            List<GH_Line> columnLines = new List<GH_Line>();
            List<GH_Line> hexLines = new List<GH_Line>();
            CrowNetP Net = new CrowNetP();


            if (!DA.GetData<CrowNetP>(0, ref Net)) return;

            if (Net.netType == "som")
            {
                for (int i = 0; i < Net.layerHeight; i++)
                {
                    Point3d iPointA;
                    Point3d iPointB;

                    for (int j = 0; j < Net.layerWidth; j++)
                    {
                        if (j != Net.layerWidth - 1)
                        {
                            iPointA = new Point3d(Net.rowX[i][j], Net.rowY[i][j], Net.rowZ[i][j]);
                            iPointB = new Point3d(Net.rowX[i][j + 1], Net.rowY[i][j + 1], Net.rowZ[i][j + 1]);
                            rowLines.Add(new GH_Line(new Line(iPointA, iPointB)));
                        }
                        else
                        {
                            if (Net.isCircularRows)
                            {
                                iPointA = new Point3d(Net.rowX[i][j], Net.rowY[i][j], Net.rowZ[i][j]);
                                iPointB = new Point3d(Net.rowX[i][0], Net.rowY[i][0], Net.rowZ[i][0]);
                                rowLines.Add(new GH_Line(new Line(iPointA, iPointB)));
                            }
                        }
                    }
                }



                for (int i = 0; i < Net.layerWidth; i++)
                {
                    Point3d iPointA;
                    Point3d iPointB;

                    for (int j = 0; j < Net.layerHeight; j++)
                    {
                        if (j != Net.layerHeight - 1)
                        {
                            iPointA = new Point3d(Net.columnX[i][j], Net.columnY[i][j], Net.columnZ[i][j]);
                            iPointB = new Point3d(Net.columnX[i][j + 1], Net.columnY[i][j + 1], Net.columnZ[i][j + 1]);
                            columnLines.Add(new GH_Line(new Line(iPointA, iPointB)));
                        }
                        else
                        {
                            if (Net.isCircularColumns)
                            {
                                iPointA = new Point3d(Net.columnX[i][j], Net.columnY[i][j], Net.columnZ[i][j]);
                                iPointB = new Point3d(Net.columnX[i][0], Net.columnY[i][0], Net.columnZ[i][0]);
                                columnLines.Add(new GH_Line(new Line(iPointA, iPointB)));
                            }
                        }

                    }


                }

                if (Net.latticeTopology)
                {
                    for (int i = 0; i < Net.layerWidth - 1; i++)
                    {
                        Point3d iPointA;
                        Point3d iPointB;
                        for (int j = 0; j < Net.layerHeight; j++)
                        {
                            if (j != Net.layerHeight - 1)
                            {
                                iPointA = new Point3d(Net.hexagonalX[i][j], Net.hexagonalY[i][j], Net.hexagonalZ[i][j]);
                                iPointB = new Point3d(Net.hexagonalX[i][j + 1], Net.hexagonalY[i][j + 1], Net.hexagonalZ[i][j + 1]);
                                columnLines.Add(new GH_Line(new Line(iPointA, iPointB)));
                            }

                        }
                        if (Net.isCircularColumns)
                        {
                            /*iPointA = new Point3d(hexagonalX[layerWidth-1][layerHeight], hexagonalY[layerWidth-1][layerHeight], 0);
                            iPointB = new Point3d(hexagonalX[0][layerHeight], hexagonalY[0][layerHeight], 0);
                            columnLines.Add(new GH_Line(new Line(iPointA, iPointB)));*/
                        }
                    }
                }
            }
            if (Net.netType == "tsp")
            {
                bool circle = Net.isCircularRows;

                for (int i = 0; i < Net.xVal.Length - 2; i++)
                {
                    Point3d iPointA = new Point3d(Net.xVal[i], Net.yVal[i], Net.zVal[i]);
                    Point3d iPointB = new Point3d(Net.xVal[i + 1], Net.yVal[i + 1], Net.zVal[i + 1]);
                    rowLines.Add(new GH_Line(new Line(iPointA, iPointB)));
                    if (i == Net.xVal.Length - 3 && circle)
                    {
                        Point3d A = new Point3d(Net.xVal[i], Net.yVal[i], Net.zVal[i]);
                        Point3d B = new Point3d(Net.xVal[0], Net.yVal[0], Net.zVal[0]);
                        rowLines.Add(new GH_Line(new Line(A, B)));
                    }
                }

            }

            DA.SetDataList(0, rowLines);
            DA.SetDataList(1, columnLines);
            DA.SetDataList(2, hexLines);

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
                return Crow.Properties.Resources.crowgrid;
            }
        }
        # endregion

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("{8B9B57E7-7941-4E30-9D60-63652EA96318}"); }
        }
    }
}