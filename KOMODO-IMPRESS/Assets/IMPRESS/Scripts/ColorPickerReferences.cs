using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System;
using Komodo.Runtime;

namespace Komodo.IMPRESS
{
    public class ColorPickerReferences : MonoBehaviour
    {
        public List<LineRenderer> lineRenderers;

        public List<TriggerDraw> triggers;

        public Camera handCamera;

        [Tooltip("Requires a RectTransform and a RawImage component with a texture in it. Assumes its image completely fills its RectTransform.")]
        public GameObject colorImageObject;

        public GameObject selectedColorCursor;

        public GameObject previewColorCursor;

        public Image selectedColorDisplay;

        public Image previewColorDisplay;

        public MenuPlacement menuPlacement;
        /*  

        /!\ Don't forget to make the texture readable. Select your texture in the Inspector. Choose [Texture Import Setting] > Texture Type > Advanced > Read/Write enabled > True, then Apply.

        */

        private void Awake()
        {
            ColorPickerManager.Init();
        }

        public void Start()
        {
            ColorPickerManager.AssignComponentReferences(lineRenderers, triggers, handCamera, colorImageObject, selectedColorCursor, previewColorCursor, selectedColorDisplay, previewColorDisplay, menuPlacement);

            TryGrabPlayerDrawTargets();

            ColorPickerManager.InitListeners();
        }

        public void TryGrabPlayerDrawTargets()
        {
            var player = GameObject.FindWithTag(TagList.player);

            if (player && player.TryGetComponent(out PlayerReferences playRef))
            {
                lineRenderers.Add(playRef.drawL.GetComponent<LineRenderer>());

                lineRenderers.Add(playRef.drawR.GetComponent<LineRenderer>());
            }
        }
    }
}