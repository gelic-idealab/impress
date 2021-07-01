using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using Komodo.Runtime;

namespace Komodo.IMPRESS
{
    public class ColorPicker : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
    {
        public List<LineRenderer> lineRenderers;

        public List<TriggerDraw> triggers;


        [Tooltip("Requires a RectTransform and a RawImage component with a texture in it. Assumes its image completely fills its RectTransform.")]
        public GameObject colorImageObject;

        public Transform selectedColorCursor;

        public Transform previewColorCursor;

        public Image selectedColorDisplay;

        public Image previewColorDisplay;
        /*  

        /!\ Don't forget to make the texture readable. Select your texture in the Inspector. Choose [Texture Import Setting] > Texture Type > Advanced > Read/Write enabled > True, then Apply.

        */
        private Texture2D colorTexture;

        private RectTransform colorRectTransform;

        private Vector2 selectedColorCursorLocalPosition;

        private Vector2 previewColorCursorLocalPosition;

        private bool _isPreviewing;

        private bool _isSelectingWithMouse;

        private void Awake()
        {
            _isPreviewing = false;

            _isSelectingWithMouse = false;

            if (!colorImageObject)
            {
                throw new UnassignedReferenceException("colorImageObject");
            }

            colorRectTransform = colorImageObject.GetComponent<RectTransform>();

            if (colorRectTransform == null)
            {
                throw new MissingComponentException("RectTransform on colorImageObject");
            }

            RawImage colorImage = colorImageObject.GetComponent<RawImage>();

            if (colorImage == null)
            {
                throw new MissingComponentException("RawImage on colorImageObject");
            }

            colorTexture = (Texture2D) colorImage.texture;

            if (!colorTexture)
            {
                throw new MissingReferenceException("texture in colorImage");
            }

            foreach (var lineRenderer in lineRenderers)
            {
                var triggerDraw = lineRenderer.GetComponent<TriggerDraw>();

                if (triggerDraw == null)
                {
                    Debug.LogError("There is no TriggerDraw.cs in Color Tool LineRenderer ", gameObject);

                    continue;
                }

                triggers.Add(triggerDraw);
            }
        }

        public void Start()
        {
            TryGrabPlayerDrawTargets();
        }

        public void TryGrabPlayerDrawTargets()
        {
            var player = GameObject.FindGameObjectWithTag(TagList.player);

            if (player && player.TryGetComponent(out PlayerReferences playRef))
            {
                lineRenderers.Add(playRef.drawL.GetComponent<LineRenderer>());

                lineRenderers.Add(playRef.drawR.GetComponent<LineRenderer>());
            }
        }

        private Vector2 GetMouseLocalPositionInRectFromPointerEventData(RectTransform rectTransform, PointerEventData data)
        {
            if (data.pressEventCamera == null)
            {
                // pressEventCamera was null for PointerEventData. This is expected for Screen Space - Overlay canvases, but not otherwise.

                //TODO - catch errors for VR mode.
            }

            return GetMouseLocalPositionInRect(rectTransform, data.position.x, data.position.y, data.pressEventCamera);
        }

        // Should return x and y values between 0 and 1, measuring from the upper-left corner of the rect.
        private Vector2 GetMouseLocalPositionInRect (RectTransform rectTransform, float globalX, float globalY, Camera eventCamera)
        {
            Vector2 globalPoint = new Vector2(globalX, globalY);

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                rectTransform, globalPoint, eventCamera, out Vector2 localPoint))
            {
                float localX = localPoint.x / rectTransform.rect.width;

                float localY = Mathf.Abs(localPoint.y / rectTransform.rect.height);

                return new Vector2(localX, localY);
            }

            Debug.LogError("Error calling ScreenPointToLocalPointInRectangle");

            return new Vector2(0.12345f, 0.67890f);
        }

        private Color GetPixelFromLocalPosition (Texture2D texture, Vector2 localPosition)
        {
            float localYFromBottom = (localPosition.y * -1f) + 1f;

            int textureY = (int) (localYFromBottom * texture.height);

            int textureX = (int) (localPosition.x * texture.width);

            return texture.GetPixel(textureX, textureY);
        }

        private void SetLineRenderersColor (List<LineRenderer> _lineRenderers, Color color)
        {
            foreach (var lineRenderer in _lineRenderers)
            {
                lineRenderer.startColor = color;

                lineRenderer.endColor = color;
            }
        }

        private void DisplayColor (Image displayObject, Color color)
        {
            displayObject.color = color;
        }

        public void Update()
        {
            if (_isPreviewing)
            {
                previewColorCursorLocalPosition = GetMouseLocalPositionInRect(colorRectTransform, Input.mousePosition.x, Input.mousePosition.y, null);

                Color previewColor = GetPixelFromLocalPosition(colorTexture, previewColorCursorLocalPosition);

                DisplayColor(previewColorDisplay, previewColor);
            }

            if (_isSelectingWithMouse)
            {
                selectedColorCursorLocalPosition = GetMouseLocalPositionInRect(colorRectTransform, Input.mousePosition.x, Input.mousePosition.y, null);
            }

            Color selectedColor = GetPixelFromLocalPosition(colorTexture, selectedColorCursorLocalPosition);

            DisplayColor(selectedColorDisplay, selectedColor);

            SetLineRenderersColor(lineRenderers, selectedColor);
        }

        private void ShowPreviewColor ()
        {
            previewColorDisplay.enabled = true;
        }

        private void HidePreviewColor ()
        {
            previewColorDisplay.enabled = false;
        }

        // Change color marker to match image selection location
        public void OnPointerClick(PointerEventData eventData)
        {
            selectedColorCursor.position = Input.mousePosition;

            selectedColorCursorLocalPosition = GetMouseLocalPositionInRectFromPointerEventData(colorRectTransform, eventData);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _isPreviewing = true;

            ShowPreviewColor();

            foreach (var item in triggers)
            {
                item.isSelectingColorPicker = true;
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _isPreviewing = false;

            HidePreviewColor();

            foreach (var item in triggers)
            {
                item.isSelectingColorPicker = false;
            }
        }

        public void OnPointerDown (PointerEventData eventData)
        {
            _isSelectingWithMouse = true;
        }

        public void OnPointerUp (PointerEventData eventData)
        {
            _isSelectingWithMouse = false;
        }

        // Stop selecting color picker, because OnPointerExit will not fire if the GameObject is disabled.
        public void OnDisable()
        {
            foreach (var item in triggers)
            {
                item.isSelectingColorPicker = false;
            }
        }
    }
}