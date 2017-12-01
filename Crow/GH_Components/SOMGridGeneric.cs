using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;

namespace Crow
{
    public class SOMGridGeneric : GH_Component
    {
        List<int> xyzD = new List<int> { 0, 1, 2 };
        List<int> topD = new List<int> { 0, 1, 2 };
        /// <summary>
        /// Initializes a new instance of the CrowGrid class.
        /// </summary>
        public SOMGridGeneric()
            : base("Generic SOG Line Display", "SOG Lines",
                "Displays your n-dimensional SOM as a grid of lines",
                "Crow", "Unsupervised")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Crow Net", "Net", "Connect your processed SOG to be shown as a grid", GH_ParamAccess.item);
            pManager.AddIntegerParameter("x,y,z dimension indices (Optional)", "(xyzD)", "Indices of the dimension in the training vectors that encode position data (x,y,z). By default the first three vector values are used", GH_ParamAccess.list, xyzD);
            pManager.AddIntegerParameter("Topology dimension indices (Optional)", "(TopD)", "Indices of the dimensions in the network topology to be displayed. Only relevant if you have more than three network dimensions.", GH_ParamAccess.list, topD);
            pManager.AddNumberParameter("Rest dimension indices (Optional)", "(rdi)", "If you supply TopD you can tell me here, which item to pick in the other dimensions.", GH_ParamAccess.item);
            pManager[1].Optional = true;
            pManager[2].Optional = true;
            pManager[3].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddCurveParameter("Lines", "1st", "The connection lines in grid's first dimension", GH_ParamAccess.list);
            pManager.AddCurveParameter("Lines", "2nd", "The connection lines in grid's second dimension", GH_ParamAccess.list);
            pManager.AddCurveParameter("Lines", "3rd", "The connection lines in grid's third dimension", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            CrowNetSOMNDP net = new CrowNetSOMNDP();
            xyzD = new List<int>();
            topD = new List<int>();
            double relativeIndex = 0.0;
            DA.GetData(0, ref net);
            if(!DA.GetDataList(1, xyzD)) xyzD = new List<int> { 0, 1, 2 };
            if(!DA.GetDataList(2, topD)) topD = new List<int> { 0, 1, 2 };
            if (!DA.GetData(3, ref relativeIndex)) relativeIndex = 0.0;
            List<Point3d> pts = new List<Point3d>();

            if (net.trainedVectors.GetLength(0) > 0)
            {
                int[][] newAdressBook = net.adressBook;
                // in case network topology is one-dimensional, add two zeros to the end of each adress book entry
                if (newAdressBook[0].Length == 1)
                {
                    for (int i = 0; i < newAdressBook.GetLength(0); i++)
                    {
                        List<int> curr = new List<int> { newAdressBook[i][0], 0, 0 };
                        newAdressBook[i] = curr.ToArray();
                    }
                }
                // in case network topology is two-dimensional, add a zero to the end of each adress book entry
                else if (newAdressBook[0].Length == 2)
                {
                    for (int i = 0; i < newAdressBook.GetLength(0); i++)
                    {
                        List<int> curr = new List<int> { newAdressBook[i][0], newAdressBook[i][1], 0 };
                        newAdressBook[i] = curr.ToArray();
                    }
                }
                
                // create point tree structure
                GH_Structure<GH_Point> pointTree = net.PointTree(xyzD);

                //draw polylines
                List<Polyline> _1st = new List<Polyline>();
                List<Polyline> _2nd = new List<Polyline>();
                List<Polyline> _3rd = new List<Polyline>();


                int[] newSize = net.size;
                if (net.size.Length == 1) newSize = new int[3] { net.size[0], 1, 1 };
                if (net.size.Length == 2) newSize = new int[3] { net.size[0], net.size[1], 1 };

                for (int i = 0; i < newSize[topD[1]]; i++)
                {
                    for (int j = 0; j < newSize[topD[2]]; j++)
                    {
                        List<Point3d> p1 = new List<Point3d>();
                        for (int k = 0; k < newSize[topD[0]]; k++)
                        {
                            GH_Path newPath = AddZerosToPath(new GH_Path(k, i, j), topD.ToArray(), newSize.Length, relativeIndex, newSize);
                            p1.Add(pointTree.get_DataItem(newPath, 0).Value);
                        }
                            
                        _1st.Add(new Polyline(p1));
                    }
                }


                for (int i = 0; i < newSize[topD[2]]; i++)
                {
                    for (int j = 0; j < newSize[topD[0]]; j++)
                    {
                        List<Point3d> p1 = new List<Point3d>();
                        for (int k = 0; k < newSize[topD[1]]; k++)
                        {
                            GH_Path newPath = AddZerosToPath(new GH_Path(j, k, i), topD.ToArray(), newSize.Length, relativeIndex, newSize);
                            p1.Add(pointTree.get_DataItem(newPath, 0).Value);
                        }
                        _2nd.Add(new Polyline(p1));
                    }
                }


                for (int i = 0; i < newSize[topD[0]]; i++)
                {
                    for (int j = 0; j < newSize[topD[1]]; j++)
                    {
                        List<Point3d> p1 = new List<Point3d>();
                        for (int k = 0; k < newSize[topD[2]]; k++)
                        {
                            GH_Path newPath = AddZerosToPath(new GH_Path(i, j, k), topD.ToArray(), newSize.Length, relativeIndex, newSize);
                            p1.Add(pointTree.get_DataItem(newPath, 0).Value);
                        }                            
                        _3rd.Add(new Polyline(p1));
                    }
                }

                DA.SetDataList(0, _1st);
                DA.SetDataList(1, _2nd);
                DA.SetDataList(2, _3rd);

            }
        }

        public static GH_Path AddZerosToPath(GH_Path path, int[] topD, int desiredLength, double relativeIndex, int[] size)
        {
            GH_Path newPath = path;
            List<int> currP = new List<int>();
            foreach (int ind in path.Indices) currP.Add(ind);
            for (int i = 0; i < desiredLength; i++)
            {
                bool addZero = true;
                for(int j = 0; j < topD.Length; j++)
                {
                    if (i == topD[j]) addZero = false;
                }
                if (addZero) currP.Insert(i, (int)(relativeIndex * (size[i]-1)));
            }
            return new GH_Path(currP.ToArray());
        }


        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Crow.Properties.Resources.crowgridnd;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("{1f4aa0ef-be74-4cfd-a775-e4ced329a40d}"); }
        }
    }
}