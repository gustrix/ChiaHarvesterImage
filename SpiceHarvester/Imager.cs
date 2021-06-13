using System;
using System.Drawing;
using System.Collections.Generic;

namespace SpiceHarvester
{

    struct Size2
    {
        public Size2(int Width, int Height) { width = Width; height = Height; }
        public int width, height;
        public int Length { get { return width * height; } }
    }

    class Imager
    {
        Rectangle imgSize;
        Size2 rSize;
        ChiaSession chs;

        public Imager(ChiaSession Set)
        {
            chs = Set;
        }

        byte[] harvestData;

        //
        // 640 x 360 @ 8 x 6 block size = 80 hours of data
        //

        const int blockWidth = 8, blockHeight = 6;
        Rectangle tp = new Rectangle(0, 0, blockWidth, blockHeight);

        /// <summary>
        /// Render the harvest data to an image.
        /// </summary>
        /// <param name="Target"></param>
        void RenderImage(Bitmap Target)
        {
            System.Drawing.Imaging.BitmapData bmpData =
            Target.LockBits(imgSize, System.Drawing.Imaging.ImageLockMode.ReadWrite,
            Target.PixelFormat);

            IntPtr ptr = bmpData.Scan0;
            int count = Math.Abs(bmpData.Stride) * Target.Height;
            byte[] rgbValues = new byte[count];
            System.Runtime.InteropServices.Marshal.Copy(ptr, rgbValues, 0, count);

            int col = 0;

            for (int y = 0; y < rSize.height; y++)
            {
                for (int x = 0; x < rSize.width; x++)
                {
                    int src = x + (y * rSize.width);

                    // Clamp at index 8 (8 challenges)
                    // Sometimes, we see more than 8+ challenges in a minute
                    // Is that a bug?
                    int idx = harvestData[src];

                    col = idx * 3;

                    byte r = Challenge.colors[col];
                    byte g = Challenge.colors[col + 1];
                    byte b = Challenge.colors[col + 2];

                    // Draw a block
                    RenderBlock(rgbValues, r, g, b, x, y);
                }
            }

            System.Runtime.InteropServices.Marshal.Copy(rgbValues, 0, ptr, count);
            Target.UnlockBits(bmpData);
        }

        void RenderBlock(byte[] ImageData, byte R, byte G, byte B, int X, int Y)
        {
            // Draw a block
            for (int bh = 0; bh < blockHeight; bh++)
            {
                for (int bw = 0; bw < blockWidth; bw++)
                {
                    int px = (((X * blockWidth) + (((Y * blockHeight) + bh) * imgSize.Width)) + bw) * 4;

                    ImageData[px + 0] = B;
                    ImageData[px + 1] = G;
                    ImageData[px + 2] = R;
                    ImageData[px + 3] = 255;
                }
            }
        }


        /// <summary>
        /// Read a text file and prepare the data
        /// for rendering.
        /// </summary>
        void PrepareData()
        {
            // Create a map of harvest data in a 2D grid
            // We render upwards and left, starting at the
            // lower-right corner.
            for (int x = rSize.width - 1; x >= 0; x--)
                for (int y = rSize.height - 1; y >= 0; y--)
                    // Use +1 for the color index.
                    harvestData[x + (y * rSize.width)] = chs.GetBlockValue();
        }

        public Bitmap RenderLog()
        {
            Bitmap bmp;

            bmp = new Bitmap(640, 360);
            imgSize = new Rectangle(0, 0, bmp.Width, bmp.Height);
            rSize = new Size2(imgSize.Width / blockWidth, imgSize.Height / blockHeight);
            harvestData = new byte[rSize.Length];

            // Prepare the image via the log
            PrepareData();

            // Draw the image
            RenderImage(bmp);

            return bmp;
        }
    }
}
