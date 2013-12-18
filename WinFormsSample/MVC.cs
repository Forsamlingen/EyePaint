using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using System.Diagnostics;

namespace WinFormsSample
{
    // Generates tree graphs.
    interface Model
    {
        Stack<Tree> components;
        void createTree(Point root, Color color);
        void expandTree();
    }

    // Rasterizes graphs into bitmaps.
    interface View
    {
        Bitmap rasterizeTrees(IEnumerable<Tree> components);
    }

    // Handles user input. Main program.
    interface Controller
    {
        Stopwatch stopwatch;
        //TODO Can the interface require the class constructor to register events?
        private void OnGreenButtonDown(object sender, EventArgs e);
        private void OnGreenButtonUp(object sender, EventArgs e);
        private void OnRedButtonDown(object sender, EventArgs e);
        private void OnRedButtonUp(object sender, EventArgs e);
    }
}
