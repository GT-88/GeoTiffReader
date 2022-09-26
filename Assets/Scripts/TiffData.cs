using UnityEngine;

namespace YabbaDataDoo
{
    public class TiffData : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {
            /// <summary>
            /// edo.tif is in RD-coordinaten (EPSG:28992)
            /// pixels hebben 32 float 
            /// Alle stukken die geen land zijn hebben NaN
            /// Grote stukken -9999 (no data value?)
            /// paar hele hoge stukken tot 997 m??
            /// </summary>
            var edoData = GeoTiffParser.ReadGeoTiff("C:\\Users\\gijs_\\Documents\\Maps\\edo.tif");
            Debug.Log($"[edo] tiff coordinaten: {edoData.GeoX1}, {edoData.GeoX2}, {edoData.GeoY1}, {edoData.GeoY2}");
            Debug.Log($"[edo] Min: {edoData.MinValue},   Max: {edoData.MaxValue}");
            Debug.Log($"[edo] pixel value on (209000, 527000): {edoData.ReadGeoPosition(209000, 527000)}"); //No data?
            Debug.Log($"[edo] pixel value on (205600, 505000): {edoData.ReadGeoPosition(205000, 505000)}"); //No data?
            Debug.Log($"[edo] pixel value on (200000, 510000): {edoData.ReadGeoPosition(200000, 510000)}"); 


            /// <summary>
            /// functies_huidig.tif is in RD-coordinaten (EPSG:28992)
            /// Pixels are 8bit uInt
            /// NoDataValue = 255
            /// </summary>
            var functiesData = GeoTiffParser.ReadGeoTiff("C:\\Users\\gijs_\\Documents\\Maps\\functies_huidig.tif");
            Debug.Log($"[Huidige functies] tiff coordinaten: {functiesData.GeoX1}, {functiesData.GeoX2}, {functiesData.GeoY1}, {functiesData.GeoY2}");
            Debug.Log($"[Huidige functies] Min: {functiesData.MinValue},   Max: {functiesData.MaxValue}");
            Debug.Log($"[Huidige functies] pixel value on (209000, 527000): {functiesData.ReadGeoPosition(209000, 527000)}");
            Debug.Log($"[Huidige functies] pixel value on (205600, 505000): {functiesData.ReadGeoPosition(205000, 505000)}");
            Debug.Log($"[Huidige functies] pixel value on (200000, 510000): {functiesData.ReadGeoPosition(200000, 510000)}");
        }
    }
}

