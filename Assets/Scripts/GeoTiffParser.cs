using System;
using System.IO;
using BitMiracle.LibTiff.Classic;
using UnityEngine;

namespace YabbaDataDoo
{
    public static class GeoTiffParser
    {
        private const float NO_DATA = -9999; // default is sometimes -9999 in tif (edo?). 

        public static GeoTiffData ReadGeoTiff(string path)
        {
            using (var stream = new FileStream(path, FileMode.Open))
            {
                return ReadGeoTiff(stream, path);
            }
        }

        public static GeoTiffData ReadGeoTiff(Stream stream, string tiffName)
        {
            using (var tiff = Tiff.ClientOpen(tiffName, "r", stream, new TiffStream()))
            {
                // Tiff width & height
                var arrayWidth = tiff.GetField(TiffTag.IMAGEWIDTH)[0].ToInt();
                var arrayHeight = tiff.GetField(TiffTag.IMAGELENGTH)[0].ToInt();

                // NO_DATA
                var noDataValue = NO_DATA;
                var nodataField = tiff.GetField((TiffTag)42113)?[1].ToString();
                if (nodataField != null)
                {
                    noDataValue = float.Parse(nodataField);
                }

                // CHeck orientation of tiff
                var orientation = tiff.GetField(TiffTag.ORIENTATION)?[0].ToShort();
                if (orientation != null && orientation != 1)
                    throw new NotSupportedException("Tiff orientation tag other than top left is unsupported");

                // Calc bytes per pixel
                var scanlineSize = tiff.ScanlineSize();
                if (scanlineSize % arrayWidth != 0)
                {
                    throw new Exception("GeoTiffParser -> ScanlineSize not an integer multiple of ArrayWidth");
                }
                var bytesPerPixel = scanlineSize / arrayWidth;

                var rawData = new float[arrayWidth, arrayHeight];
                float minValue = float.MaxValue;
                float maxValue = float.MinValue;
                try
                {
                    // Try tile interface.
                    Debug.Log("Parsing tiff using tile interface.");
                    ReadUsingTiles(tiff, arrayWidth, arrayHeight, bytesPerPixel, rawData, noDataValue,
                        out minValue,
                        out maxValue);
                }
                catch (Exception e)
                {
                    // Not all tiffs are tiled based. Some are scanline. IMplement the interface here.
                    Debug.LogError(e.Message + ". Try implementing scanline interface");
                }

                // Calc transformation matrices
                var tiffToGeoMatrix = ReadTiffToGeoMatrix(tiff);
                var geoToTiffMatrix = tiffToGeoMatrix.inverse;

                // Geo extent
                var geoXY1 = ApplyTransformation(new Vector2(0, arrayHeight), tiffToGeoMatrix);
                var geoXY2 = ApplyTransformation(new Vector2(arrayWidth, 0), tiffToGeoMatrix);

                // Initialize geotiff map
                var geoTiffData = new GeoTiffData
                {
                    RawData = rawData,
                    MinValue = minValue,
                    MaxValue = maxValue,
                    NoDataValue = noDataValue,
                    TiffToGeoTransform = tiffToGeoMatrix,
                    GeoToTiffTransform = geoToTiffMatrix,
                    GeoX1 = geoXY1.x,
                    GeoY1 = geoXY1.y,
                    GeoX2 = geoXY2.x,
                    GeoY2 = geoXY2.y
                };

                Debug.Log("Parsing tiff done.");
                return geoTiffData;
            }
        }


        private static void ReadUsingTiles(Tiff tiff, int arrayWidth, int arrayHeight, int bytesPerPixel, float[,] rawData, float noData, out float minValue, out float maxValue)
        {
            minValue = float.MaxValue;
            maxValue = float.MinValue;
            var tileWidthTag = tiff.GetField(TiffTag.TILEWIDTH);
            var tileHeightTag = tiff.GetField(TiffTag.TILELENGTH);
            if (tileWidthTag == null || tileHeightTag == null)
            {
                throw new Exception("Not a Tiled tiff. Missing TILEWIDTH or TILELENGTH tags.");
            }

            var tileWidth = tileWidthTag[0].ToInt();
            var tileHeight = tileHeightTag[0].ToInt();
            var buffer = new byte[tiff.TileSize()];

            // Iterate over each tile
            for (var y = 0; y < arrayHeight; y += tileHeight)
            {
                for (var x = 0; x < arrayWidth; x += tileWidth)
                {
                    // Try to read the tile, or throw an error if not possible
                    var success = tiff.ReadTile(buffer, 0, x, y, 0, 0);
                    if (success == -1)
                    {
                        throw new Exception("Tiff ReadTile failed");
                    }

                    // Copy the tile contents to the raw data array
                    for (var ty = 0; ty < tileHeight && y + ty < arrayHeight; ty++)
                    {
                        for (var tx = 0; tx < tileWidth && x + tx < arrayWidth; tx++)
                        {
                            float pixelValue;
                            if (bytesPerPixel == 1) // The functies_huidig.tif has a 8bit unassign int pixel 
                            {
                                pixelValue = buffer[(ty * tileWidth + tx)];
                            }
                            else // edo.tif has a 32bit float pixel
                            {
                                pixelValue = BitConverter.ToSingle(buffer, (ty * tileWidth + tx) * bytesPerPixel);
                            }
                            
                            if (!Mathf.Approximately(pixelValue, noData))
                            {
                                if (pixelValue < minValue) minValue = pixelValue;
                                if (pixelValue > maxValue) maxValue = pixelValue;
                            }

                            rawData[x + tx, y + ty] = pixelValue;
                        }
                    }
                }
            }
        }


        /// <summary>
        ///     Two methods to get the matrix:
        ///     - Use ModelTransformationTag
        ///     - Use a combination of ModelPixelScaleTag and ModelTiepointTag
        ///     See: http://geotiff.maptools.org/spec/geotiff2.6.html
        /// </summary>
        private static Matrix4x4 ReadTiffToGeoMatrix(Tiff tiff)
        {
            float a, b, c, d, e, f, g, h;

            // ModelTransformationTag way
            var modelTransformationTag = tiff.GetField((TiffTag)34264);
            if (modelTransformationTag != null)
            {
                var modelTransformationTagBytes = modelTransformationTag[1].GetBytes();
                a = (float)BitConverter.ToDouble(modelTransformationTagBytes, 0);
                b = (float)BitConverter.ToDouble(modelTransformationTagBytes, 8);
                c = (float)BitConverter.ToDouble(modelTransformationTagBytes, 16);
                d = (float)BitConverter.ToDouble(modelTransformationTagBytes, 24);

                e = (float)BitConverter.ToDouble(modelTransformationTagBytes, 32);
                f = (float)BitConverter.ToDouble(modelTransformationTagBytes, 40);
                g = (float)BitConverter.ToDouble(modelTransformationTagBytes, 48);
                h = (float)BitConverter.ToDouble(modelTransformationTagBytes, 56);
            }
            // ModelPixelScaleTag and ModelTiepointTag way
            else
            {
                var modelPixelScaleTag = tiff.GetField((TiffTag)33550);
                var modelPixelScale = modelPixelScaleTag[1].GetBytes();
                var pixelSizeX = (float)BitConverter.ToDouble(modelPixelScale, 0);
                var pixelSizeY = (float)BitConverter.ToDouble(modelPixelScale, 8);

                var modelTiepointTag = tiff.GetField((TiffTag)33922);
                var modelTransformation = modelTiepointTag[1].GetBytes();
                var originX = (float)BitConverter.ToDouble(modelTransformation, 24);
                var originY = (float)BitConverter.ToDouble(modelTransformation, 32);

                a = pixelSizeX;
                b = 0;
                c = 0;
                d = originX;

                e = 0;
                f = -pixelSizeY;
                g = 0;
                h = originY;
            }

            var matrix = new Matrix4x4
            {
                m00 = a,
                m01 = b,
                m02 = c,
                m03 = d,
                m10 = e,
                m11 = f,
                m12 = g,
                m13 = h,
                m20 = 0,
                m21 = 0,
                m22 = 1,
                m23 = 0,
                m30 = 0,
                m31 = 0,
                m32 = 0,
                m33 = 1
            };

            // Third row hardcoded to 0, 0, 1, 0. This assumes only 2D geotiffs, and also allows the matrix to be inverted (instead of it being 0, 0, 0, 0).
            // Fourth row hardcoded to 0, 0, 0, 1. See the geotiff specification linked in this method summary.

            return matrix;
        }

        private static Vector2 ApplyTransformation(Vector2 source, Matrix4x4 transformation)
        {
            var sourceVec4 = new Vector4(source.x, source.y, 0, 1);
            var destVec4 = transformation * sourceVec4;
            return new Vector2(destVec4.x, destVec4.y);
        }
    }

    public class GeoTiffData
    {
        public float GeoX1, GeoY1;
        public float GeoX2, GeoY2;

        public float MaxValue;
        public float MinValue;
        public float NoDataValue;
        public float[,] RawData;

        public Matrix4x4 TiffToGeoTransform;
        public Matrix4x4 GeoToTiffTransform;

        public float ReadGeoPosition(float geoX, float geoY)
        {
            var geoPosition = new Vector4(geoX, geoY, 0, 1);
            var tiffPosition = GeoToTiffTransform * geoPosition;

            var tiffX = (int)tiffPosition.x;
            var tiffY = (int)tiffPosition.y;

            return RawData[tiffX, tiffY];
        }
    }
}
