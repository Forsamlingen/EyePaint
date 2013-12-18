using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace EyePaint
{
    class ImageFactory
    {
        private Image currentImage;

        public ImageFactory()
        {
            currentImage = new Bitmap(200, 200);
        }

        public Image rasterizeTrees(IEnumerable<Tree> components)
        {
            foreach (var component in components)
            {
            //TODO
            }


            return currentImage;
        }
    }
}
