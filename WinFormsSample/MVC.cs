using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;

namespace WinFormsSample
{
    // Generates tree graphs.
    interface Model
    {
        Point currentPoint;
        List<Tree> components;
        Tree generateTree(Point root);
    }

    // Rasterizes graphs into bitmaps.
    interface View
    {
        Bitmap rasterizeTree(Tree tree);
    }

    // Handles user input. Main program.
    interface Controller
    {
   
    }
}
