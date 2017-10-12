/*using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace Crow.GH_Components
{
    public class Options : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the Options class.
        /// </summary>
        public Options()
            : base("Crow Solver Options", "Options",
                "Define custom options for the Crow Solver.",
                "Crow", "SOM")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddBooleanParameter("Random Training", "randTrain", "Decide whether the training samples are iterated through in a random fashion", GH_ParamAccess.item, true);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Options", "Opt", "Solver Options", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            bool useRandomTraining = true;
            DA.GetData(0, ref useRandomTraining);

            DA.SetData(0, new CrowOptions(useRandomTraining));
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
                return Crow.Properties.Resources.crowoptions;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("{40cb5212-897d-4224-9dfb-a5111217365b}"); }
        }
    }
}
*/