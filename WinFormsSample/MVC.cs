using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;

namespace WinFormsSample
{
    // Generates tree graphs.
    interface Model
    {
        Point point;
    }

    // Rasterizes graphs into bitmaps.
    interface View
    {
        // Givet graf skapa bitmap.
        Bitmap rasterizeTree(Tree tree);
    }

    // Handles user input. Main program.
    interface Controller
    {
   
    }
}
