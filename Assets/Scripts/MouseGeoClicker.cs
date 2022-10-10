using UnityEngine;

namespace YabbaDataDoo
{
    public class MouseGeoClicker : MonoBehaviour
    {
        private GeoTiffData TiffData;
        public void Init(GeoTiffData tiffData)
        {
            TiffData = tiffData;
        }

        void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                Vector3 mousePos = Input.mousePosition;
                mousePos.z = Camera.main.nearClipPlane;
                var geoCoordinates = Camera.main.ScreenToWorldPoint(mousePos);
                var value = TiffData.ReadGeoPosition(geoCoordinates.x, geoCoordinates.y);
                Debug.Log($"On location {geoCoordinates.x}, {geoCoordinates.y} is value: {value}");
            }
        }
    }
}
