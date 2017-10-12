using System;
using System.Drawing;
using Grasshopper.Kernel;

namespace Crow
{
    public class CrowInfo : GH_AssemblyInfo
    {
        public override string Name
        {
            get
            {
                return "Crow";
            }
        }
        public override Bitmap Icon
        {
            get
            {
                //Return a 24x24 pixel bitmap to represent this GHA library.
                return null;
            }
        }
        public override string Description
        {
            get
            {
                //Return a short string describing the purpose of this GHA library.
                return "";
            }
        }
        public override Guid Id
        {
            get
            {
                return new Guid("ab5a49e4-9245-41da-80ef-2b9d600c5cb7");
            }
        }

        public override string AuthorName
        {
            get
            {
                //Return a string identifying you or your company.
                return "";
            }
        }
        public override string AuthorContact
        {
            get
            {
                //Return a string representing your preferred contact details.
                return "";
            }
        }
    }
}
