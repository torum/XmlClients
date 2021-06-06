using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlogWrite.Models
{

    public class NodeCategory : NodeTree
    {
        // Constructor.
        public NodeCategory(string title) : base(title)
        {
            PathIcon = "M16,17H5V7H16L19.55,12M17.63,5.84C17.27,5.33 16.67,5 16,5H5A2,2 0 0,0 3,7V17A2,2 0 0,0 5,19H16C16.67,19 17.27,18.66 17.63,18.15L22,12L17.63,5.84Z";

            //ID = Guid.NewGuid().ToString();

        }

    }

    public class NodeAtomPubCategory : NodeCategory
    {
        public string Term { get; set; }

        // Constructor.
        public NodeAtomPubCategory(string title) : base(title)
        {
            //PathIcon = "M16,17H5V7H16L19.55,12M17.63,5.84C17.27,5.33 16.67,5 16,5H5A2,2 0 0,0 3,7V17A2,2 0 0,0 5,19H16C16.67,19 17.27,18.66 17.63,18.15L22,12L17.63,5.84Z";

            //ID = Guid.NewGuid().ToString();

            Term = title;

        }

    }

    public class NodeXmlRpcMTCategory : NodeCategory 
    {
        public string CategoryName { get; set; }

        public string CategoryId { get; set; }

        public string ParentId { get; set; }

        public string Description { get; set; }

        public string CategoryDescription { get; set; }

        public Uri HtmlUrl { get; set; }

        public Uri RssUrl { get; set; }

        // Constructor.
        public NodeXmlRpcMTCategory(string title) : base(title)
        {
            //PathIcon = "M16,17H5V7H16L19.55,12M17.63,5.84C17.27,5.33 16.67,5 16,5H5A2,2 0 0,0 3,7V17A2,2 0 0,0 5,19H16C16.67,19 17.27,18.66 17.63,18.15L22,12L17.63,5.84Z";

            //ID = Guid.NewGuid().ToString();

            CategoryName = title;

        }

    }

    public class NodeXmlRpcWPCategory : NodeCategory
    {

        //TODO:

        //public string Term { get; set; }

        // Constructor.
        public NodeXmlRpcWPCategory(string title) : base(title)
        {
            //PathIcon = "M16,17H5V7H16L19.55,12M17.63,5.84C17.27,5.33 16.67,5 16,5H5A2,2 0 0,0 3,7V17A2,2 0 0,0 5,19H16C16.67,19 17.27,18.66 17.63,18.15L22,12L17.63,5.84Z";

            //ID = Guid.NewGuid().ToString();

            //Term = title;

        }

    }


}
