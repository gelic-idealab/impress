using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Komodo.IMPRESS
{
    public class ImpressMenu : MonoBehaviour
    {
        public Button eraseButton;

        private bool _eraserEnabled = false;

        void OnValidate ()
        {
            if (eraseButton == null)
            {
                throw new System.NullReferenceException("eraseButton");
            }
        }

        void Start ()
        {
            eraseButton.onClick.AddListener(() => 
            {
                if (_eraserEnabled)
                {
                    ImpressEventManager.TriggerEvent("eraserDisabled");

                    _eraserEnabled = false;

                    return;
                }

                ImpressEventManager.TriggerEvent("eraserEnabled");

                _eraserEnabled = true;
            });
        }
    }
}
