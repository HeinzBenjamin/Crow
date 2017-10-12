using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;

using Rhino.Geometry;

namespace Crow.GH_Components
{
    public class AnalyseData : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the AnalyseData class.
        /// </summary>
        public AnalyseData()
          : base("Analyse data", "Analyse",
              "A few measures to give insight into your data.",
              "Crow", "Util")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddNumberParameter("Data", "D", "", GH_ParamAccess.tree);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddNumberParameter("Arithmetic Mean", "AM", "https://en.wikipedia.org/wiki/Arithmetic_mean", GH_ParamAccess.list);
            pManager.AddNumberParameter("Geometric Mean", "GM", "https://en.wikipedia.org/wiki/Geometric_mean", GH_ParamAccess.list);
            pManager.AddNumberParameter("Minimum", "Min", "Smallest value per branch", GH_ParamAccess.list);
            pManager.AddNumberParameter("Maximum", "Max", "Largest value per branch", GH_ParamAccess.list);
            pManager.AddNumberParameter("Range", "R", "Max - Min", GH_ParamAccess.list);
            pManager.AddNumberParameter("Variance", "V", "https://en.wikipedia.org/wiki/Variance", GH_ParamAccess.list);
            pManager.AddNumberParameter("Standard Deviation", "SD", "https://en.wikipedia.org/wiki/Standard_deviation", GH_ParamAccess.list);
            pManager.AddNumberParameter("Vector Length", "VL", "https://en.wikipedia.org/wiki/Euclidean_vector#Length", GH_ParamAccess.list);
            pManager.AddNumberParameter("Scaled", "S", "vector scaled to [0.0, 1.0]", GH_ParamAccess.tree);
            pManager.AddNumberParameter("Normalized", "N", "Scaled to vector length 1.0", GH_ParamAccess.tree);
            pManager.AddNumberParameter("Softmax", "SM", "https://en.wikipedia.org/wiki/Softmax_function", GH_ParamAccess.tree);
            pManager.AddNumberParameter("Scaled Softmax", "sSM", "Softmax is applied after vector is scaled to 0.0 to 1.0.", GH_ParamAccess.tree);
            pManager.AddNumberParameter("Normalized Softmax", "nSM", "Softmax is applied after vector is scaled to have length 1.0.", GH_ParamAccess.tree);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            GH_Structure<GH_Number> data = new GH_Structure<GH_Number>();

            DA.GetDataTree(0, out data);

            List<double> am = new List<double>();
            List<double> gm = new List<double>();
            List<double> min = new List<double>();
            List<double> max = new List<double>();
            List<double> R = new List<double>();
            List<double> V = new List<double>();
            List<double> sd = new List<double>();
            List<double> vl = new List<double>();
            GH_Structure<GH_Number> Sv = new GH_Structure<GH_Number>();
            GH_Structure<GH_Number> Nv = new GH_Structure<GH_Number>();


            for (int k = 0; k < data.Branches.Count; k++)
            {
                List<GH_Number> d = data.Branches[k];

                double a = 0.0;
                double v = 0.0;
                double g = 1.0;
                double mi = double.MaxValue;
                double ma = double.MinValue;
                double l = 0.0;
                List<GH_Number> sv = new List<GH_Number>();
                List<GH_Number> nv = new List<GH_Number>();

                foreach (GH_Number n in d)
                {
                    a += n.Value;
                    g *= n.Value;
                    l += n.Value * n.Value;
                    if (n.Value < mi)
                        mi = n.Value;
                    if (n.Value > ma)
                        ma = n.Value;
                }
                a /= d.Count;
                l = Math.Sqrt(l);

                for (int i = 0; i < d.Count; i++)
                {
                    v += (d[i].Value - a) * (d[i].Value - a);
                    sv.Add(new GH_Number(d[i].Value / ma));
                    nv.Add(new GH_Number(d[i].Value / l));
                }

                v /= d.Count;
                g = Math.Pow(g, 1.0 / d.Count);

                am.Add(a);
                gm.Add(g);
                min.Add(mi);
                max.Add(ma);
                R.Add(ma - mi);
                V.Add(v);
                vl.Add(l);
                sd.Add(Math.Sqrt(v));
                Sv.AppendRange(sv, new GH_Path(k));
                Nv.AppendRange(nv, new GH_Path(k));
            }

            DA.SetDataList(0, am);
            DA.SetDataList(1, gm);
            DA.SetDataList(2, min);
            DA.SetDataList(3, max);
            DA.SetDataList(4, R);
            DA.SetDataList(5, V);
            DA.SetDataList(6, sd);
            DA.SetDataList(7, vl);
            DA.SetDataTree(8, Sv);
            DA.SetDataTree(9, Nv);
            DA.SetDataTree(10, Utils.MultidimensionalArrayToGHTree(Utils.SoftMax(Utils.GHTreeToMultidimensionalArray(data))));
            DA.SetDataTree(11, Utils.MultidimensionalArrayToGHTree(Utils.SoftMax(Utils.GHTreeToMultidimensionalArray(Sv))));
            DA.SetDataTree(12, Utils.MultidimensionalArrayToGHTree(Utils.SoftMax(Utils.GHTreeToMultidimensionalArray(Nv))));

        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Crow.Properties.Resources.data_analysis;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("{d3a388ef-c17d-4426-9704-07d0826a4d8d}"); }
        }
    }
}