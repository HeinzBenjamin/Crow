using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;

namespace Crow.GH_Components
{
    public class SOMDataPicker : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the SOMDataPicker class.
        /// </summary>
        public SOMDataPicker()
            : base("Generic SOG Data Picker", "Data Picker",
                "Let's you choose which data to display by dimension.",
                "Crow", "Unsupervised")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Data Vectors", "data", "Data tree as is comes out of the SOG Data Component", GH_ParamAccess.tree);
            pManager.AddNumberParameter("Indices", "i", "Indices with respect to the dimension as list of number 0.0 to 1.0; If 1.0 is supplied, the full range of the respective dimension is shown.", GH_ParamAccess.list);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Data Vectors", "data", "Filtered data", GH_ParamAccess.tree);
            pManager.AddPathParameter("Paths", "paths", "Selected Paths", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            GH_Structure<IGH_Goo> dataTree = new GH_Structure<IGH_Goo>();
            List<double> _indices = new List<double>();
            DA.GetDataTree(0, out dataTree);
            DA.GetDataList(1, _indices);

            //dataTree.Simplify(GH_SimplificationMode.CollapseAllOverlaps);

            int[] size = new int[dataTree.get_Path(0).Length];
            int[][] adressBook = new int[dataTree.PathCount][];
            

            IList<GH_Path> allPaths = dataTree.Paths;

            //make adressBook from dataTree
            for (int i = 0; i < allPaths.Count; i++)
            {
                adressBook[i] = allPaths[i].Indices;
            }

            //get size of dataTree dimensions
            for (int i = 0; i < size.Length; i++)
            {
                for (int j = 0; j < adressBook.Length; j++)
                {
                    if (adressBook[j][i] >= size[i]) size[i] = adressBook[j][i] + 1;
                }
            }

            if (_indices.Count > size.Length) AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Too many i-values");
            if (_indices.Count < size.Length)
            {
                for (int i = 0; i < size.Length - _indices.Count; i++)
                {
                    _indices.Add(1.0);
                }
            }

            //scale index values
            for (int i = 0; i < _indices.Count; i++) _indices[i] *= size[i];

            List<int[]> newAdresses = new List<int[]>();

            for (int i = 0; i < adressBook.Length; i++)
            {
                bool add = true;
                for (int j = 0; j < _indices.Count; j++)
                {
                    if (_indices[j] != (double)size[j])
                    {
                        if(adressBook[i][j] != (int)_indices[j])
                            add = false;
                    }                        
                }
                if (add) newAdresses.Add(adressBook[i]);
            }


            List<GH_Path> paths = new List<GH_Path>();
            GH_Structure<IGH_Goo> newDataTree = new GH_Structure<IGH_Goo>();
            foreach (int[] a in newAdresses)
            {
                GH_Path path = new GH_Path(a);
                paths.Add(path);
                for(int i = 0; i < dataTree.get_Branch(path).Count; i++)
                    newDataTree.Append(dataTree.get_DataItem(path, i),path);
            }

            DA.SetDataTree(0, newDataTree);
            DA.SetDataList(1, paths);


            
            
        }


        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                // return Resources.IconForThisComponent;
                return Crow.Properties.Resources.crowdatapicker;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("{94a00f11-2aff-4602-8f66-6401a70eab53}"); }
        }
    }
}