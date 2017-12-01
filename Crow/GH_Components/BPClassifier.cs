using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;

using Crow.Core;
using Crow.Core.Backpropagation;

namespace Crow.GH_Components
{
    public class BPClassifier : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the BPClassifier class.
        /// </summary>
        public BPClassifier()
            : base("Backpropagation Vector Classifier", "BP VC",
                "Classify vectors of arbitrary dimensions through a trained backpropagation network.",
                "Crow", "Supervised")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("CrowNet", "Net", "Trained Crow backpropagation network", GH_ParamAccess.item);
            pManager.AddNumberParameter("Input Vector", "iV", "Input vectors to be classified by your trained network", GH_ParamAccess.tree);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddNumberParameter("Output Vector", "oV", "Classified output Vectors", GH_ParamAccess.tree);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            CrowNetBPP net = new CrowNetBPP();
            GH_Structure<GH_Number> v = new GH_Structure<GH_Number>();

            DA.GetData(0, ref net);
            DA.GetDataTree(1, out v);

            double[][] vectors = Utils.GHTreeToMultidimensionalArray(v);
            double[][] classifiedVectors = new double[vectors.Length][];

            for (int i = 0; i < vectors.Length; i++)
            {
                classifiedVectors[i] = net.network.Run(vectors[i]);

            }

            DA.SetDataTree(0, Utils.MultidimensionalArrayToGHTree(classifiedVectors));
            
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
                return Crow.Properties.Resources.classifier;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("{361d6557-60b4-4670-8144-20507e298e7e}"); }
        }
    }
}