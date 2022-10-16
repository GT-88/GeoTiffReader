using UnityEngine;

namespace YabbaDataDoo
{
    public class GeoTiffController : MonoBehaviour
    {
        public Texture2DCreator TexBuilder;
        public MouseGeoClicker MouseLocation;
        public GeoTiffData EdoData;

        void Start()
        {
            /// <summary>
            /// edo.tif is in RD-coordinaten (EPSG:28992)
            /// pixels hebben 32 float 
            /// Alle stukken die geen land zijn hebben NaN
            /// Er is geen NoDataValue maar grote stukken -9999 (no data value?)
            /// paar hele hoge stukken tot 997 m??
            /// </summary>
            EdoData = GeoTiffParser.ReadGeoTiff("C:\\Users\\gijs_\\Documents\\Maps\\edo.tif");


            /// <summary>
            /// functies_huidig.tif is in RD-coordinaten (EPSG:28992)
            /// Pixels are 8bit uInt
            /// NoDataValue = 255
            /// </summary>
            var functiesData = GeoTiffParser.ReadGeoTiff("C:\\Users\\gijs_\\Documents\\Maps\\functies_huidig_klein.tif");

            var textureInfo = TiffTextureInfo.TiffToTexture(EdoData);
            TexBuilder.CreateTexture2D(textureInfo);

            var center = new Vector3((textureInfo.X1 + textureInfo.X2) / 2, (textureInfo.Y1 + textureInfo.Y2) / 2, -30);
            Camera.main.transform.position = center;
            MouseLocation.Init(EdoData);

        }
    }
}

