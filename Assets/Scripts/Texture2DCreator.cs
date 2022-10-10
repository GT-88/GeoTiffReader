using UnityEngine;
using UnityEngine.UI;

namespace YabbaDataDoo
{
    public class Texture2DCreator : MonoBehaviour
    {
        public void CreateTexture2D(TextureInfo textureInfo)
        {
            var texture = new Texture2D(textureInfo.PixelsWidth, textureInfo.PixelsHeight, TextureFormat.RGBA32, false);
            texture.LoadRawTextureData(textureInfo.RawTextureData);
            texture.Apply();
            GetComponent<RawImage>().texture = texture;

            SetRectTransform(textureInfo);
        }

        private void SetRectTransform(TextureInfo textureInfo)
        {
            var rectTransform = GetComponent<RectTransform>();
            var textureWidth = textureInfo.GeoWidth;
            var textureHeigth = textureInfo.GeoHeight;
            rectTransform.pivot = new Vector2(0, 1);
            rectTransform.sizeDelta = new Vector2(textureWidth, textureHeigth);
            Debug.Log($"width and height = {textureWidth},{textureHeigth}");
            rectTransform.position = new Vector3(textureInfo.X1, textureInfo.Y2, 0);
        }
    }
}
