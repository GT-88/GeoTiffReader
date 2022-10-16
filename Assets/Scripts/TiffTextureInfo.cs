namespace YabbaDataDoo
{
    public static class TiffTextureInfo
    {
        public static TextureInfo TiffToTexture(GeoTiffData tiffData)
        {
            var rawData = tiffData.RawData;
            var width = rawData.GetLength(0);
            var length = rawData.GetLength(1);
            var bytesPerPixel = 4;

            var textureBytes = new byte[width * length * bytesPerPixel];
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < length; y++)
                {
                    var pixelvalue = rawData[x, y];
                    var color = GetColor(pixelvalue, tiffData); //r,g,b,a

                    // Tiffs are assumed to have their origin at top-left.
                    // Textures have their origin bottom-left. So we need to flip the y coordinate.
                    var pixelX = x;
                    var pixelY = length - y - 1;
                    textureBytes[pixelY * width * bytesPerPixel + pixelX * bytesPerPixel + 0] = color[0];
                    textureBytes[pixelY * width * bytesPerPixel + pixelX * bytesPerPixel + 1] = color[1];
                    textureBytes[pixelY * width * bytesPerPixel + pixelX * bytesPerPixel + 2] = color[2];
                    textureBytes[pixelY * width * bytesPerPixel + pixelX * bytesPerPixel + 3] = color[3];
                }
            }

            return new TextureInfo
            {
                PixelsWidth = width,
                PixelsHeight = length,
                RawTextureData = textureBytes,
                X1 = tiffData.GeoX1,
                X2 = tiffData.GeoX2,
                Y1 = tiffData.GeoY1,
                Y2 = tiffData.GeoY2
            };
        }

        private static byte[] GetColor(float pixelvalue, GeoTiffData tiffData)
        {
            if (pixelvalue > 20) pixelvalue = 20;
            byte[] color; //r,g,b,a
            if (pixelvalue == tiffData.NoDataValue || pixelvalue <0)
            {
                color = new byte[] { 0, 0, 0, 255 };
            }
            else if (float.IsNaN(pixelvalue))
            {
                color = new byte[] { 255, 255, 255, 255 };
            }
            //else if (pixelvalue == 1)
            //{
            //    color = new byte[] { 255, 0, 0, 255 };
            //}
            //else if (pixelvalue == 2)
            //{
            //    color = new byte[] { 0, 255, 0, 255 };
            //}
            //else if (pixelvalue == 3)
            //{
            //    color = new byte[] { 0, 0, 255, 255 };
            //}
            //else if (pixelvalue == 4)
            //{
            //    color = new byte[] { 100, 255, 0, 255 };
            //}
            //else if (pixelvalue == 5)
            //{
            //    color = new byte[] { 100, 0, 100, 255 };
            //}
            //else if (pixelvalue == 8)
            //{
            //    color = new byte[] { 200, 70, 10, 255 };
            //}
            //else if (pixelvalue == 10)
            //{
            //    color = new byte[] { 100, 55, 200, 255 };
            //}
            //else if (pixelvalue == 13)
            //{
            //    color = new byte[] { 40, 180, 255, 255 };
            //}
            else
            {
               // var value = (byte)((pixelvalue - tiffData.MinValue) / (tiffData.MaxValue - tiffData.MinValue) * 255);
                var value = (byte)((pixelvalue + 10) / (30) * 255);
                color = new byte[] { value, value, value, 255 };
            }
            return color;
        }
    }

    public class TextureInfo
    {
        public int PixelsWidth;
        public int PixelsHeight;
        public byte[] RawTextureData;

        public float X1, Y1;
        public float X2, Y2;
        public float GeoWidth => X2 - X1;
        public float GeoHeight => Y2 - Y1;
    }
}
