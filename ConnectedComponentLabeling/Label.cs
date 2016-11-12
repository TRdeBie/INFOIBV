using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace INFOIBV
{
    public class Label                                
    {
        public int id;
        public Label root;
        public int rank = 0;

        // Constructor
        public Label(int id)
        {
            this.id = id;
            root = this;
        }

        // Method to get the root of the pixel
        public Label GetRoot()
        {
            if(root != this)
            {
                root = root.GetRoot();
            }

            return root;
        }

        // Method to set the rank of a pixel equal to it's parent
        public void Join(Label secondRoot)
        {
            if (secondRoot.rank < rank) // If the rank of secondRoot is lower, root is the parent of secondRoot
                secondRoot.rank = rank;
            else // If the rank is equal to or higher
            {
                root = secondRoot;  // Set the root equal to secondRoot
                if (rank == secondRoot.rank) // If the ranks are equal increment the rank
                    secondRoot.rank++;
            }
        }

    }
}
