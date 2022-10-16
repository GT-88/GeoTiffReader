using UnityEngine;
using UnityEngine.UI;

namespace YabbaDataDoo
{
    public class Selection : MonoBehaviour
    {
        public Text ValueText;
        public GeoTiffController Controller;
        public Toggle SelectionToggle;

        // stuff for selection box
        public Color BorderColor;
        public Color FillingColor;
        public float ThicknessBorder;

        private bool _isDraggingMouseBox = false;
        private Vector3 _dragStartPosition;
        private Rect Rect;

        private void Update()
        {
            if (SelectionToggle.isOn)
            {
                // Get the start position when selecting
                if (Input.GetMouseButtonDown(0))
                {
                    _isDraggingMouseBox = true;
                    _dragStartPosition = Input.mousePosition;
                }
                //get the end position when done selecting
                if (Input.GetMouseButtonUp(0))
                {
                    _isDraggingMouseBox = false;

                    var startPos = Camera.main.ScreenToWorldPoint(_dragStartPosition);
                    var endPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

                    // rearange positions so selection works in all directions
                    RearangePositions(startPos, endPos, out Vector2 topLeft, out Vector2 bottomRight);

                    var totalValue = Controller.EdoData.GetGeoSelection(topLeft, bottomRight);
                    var totalWaterAmount = totalValue * Controller.EdoData.pixelSize * Controller.EdoData.pixelSize;
                    ValueText.text = $"{totalWaterAmount} m2";
                }
            }
        }

        private static void RearangePositions(Vector3 startPos, Vector3 endPos, out Vector2 topLeft, out Vector2 bottomRight)
        {
            topLeft = new Vector2();
            bottomRight = new Vector2();
            if (startPos.x > endPos.x)
            {
                topLeft.x = endPos.x;
                bottomRight.x = startPos.x;
            }
            else
            {
                topLeft.x = startPos.x;
                bottomRight.x = endPos.x;
            }
            if (startPos.y < endPos.y)
            {
                topLeft.y = endPos.y;
                bottomRight.y = startPos.y;
            }
            else
            {
                topLeft.y = startPos.y;
                bottomRight.y = endPos.y;
            }
        }

        void OnGUI()
        {
            if (SelectionToggle.isOn)
            {
                if (_isDraggingMouseBox)
                {
                    // Create a rect from both mouse positions
                    Rect = Utils.GetScreenRect(_dragStartPosition, Input.mousePosition);
                }
                Utils.DrawScreenRect(Rect, FillingColor);
                Utils.DrawScreenRectBorder(Rect, ThicknessBorder, BorderColor);
            }
        }
    }
}