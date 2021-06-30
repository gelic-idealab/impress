using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Komodo.Runtime;

namespace Komodo.IMPRESS
{
    public class ImpressMenu : MonoBehaviour
    {
        public Button eraseButton;

        private bool _eraserEnabled = false;

        public Button undoButton;

        public GameObject drawTab;

        public Toggle brushToggle;

        public Toggle capsuleToggle;

        public Toggle cylinderToggle;

        public Toggle cubeToggle;

        public Toggle planeToggle;

        public Toggle sphereToggle;

        void OnValidate ()
        {
            if (eraseButton == null)
            {
                throw new UnassignedReferenceException("eraseButton");
            }
            
            if (undoButton == null)
            {
                throw new UnassignedReferenceException("undoButton");
            }

            if (drawTab == null)
            {
                throw new UnassignedReferenceException("drawTab");
            }

            if (brushToggle == null)
            {
                throw new UnassignedReferenceException("brushToggle");
            }

            if (sphereToggle == null)
            {
                throw new UnassignedReferenceException("sphereToggle");
            }

            if (capsuleToggle == null)
            {
                throw new UnassignedReferenceException("capsuleToggle");
            }

            if (cylinderToggle == null)
            {
                throw new UnassignedReferenceException("cylinderToggle");
            }

            if (cubeToggle == null)
            {
                throw new UnassignedReferenceException("cubeToggle");
            }

            if (planeToggle == null)
            {
                throw new UnassignedReferenceException("planeToggle");
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

            undoButton.onClick.AddListener(() =>
            {
                UndoRedoManager.Instance.Undo();
            });

            TabButton tab = drawTab.GetComponent<TabButton>();

            if (tab)
            {
                tab.onTabSelected.AddListener(() =>
                {
                    ImpressEventManager.TriggerEvent("drawToolEnabled");
                });

                tab.onTabDeselected.AddListener(() =>
                {
                    ImpressEventManager.TriggerEvent("drawToolDisabled");
                });
            }

            brushToggle.onValueChanged.AddListener((isOn) =>
            {
                if (isOn)
                {
                    // TODO(Brandon) - send event to turn on brush

                    ImpressEventManager.TriggerEvent("primitiveDisableCreation");

                    // TODO(Brandon) - is this the best way to get out of the primitive creation mode?

                    return;
                }
            });

            sphereToggle.onValueChanged.AddListener((isOn) =>
            {
                if (isOn)
                {
                    ImpressEventManager.TriggerEvent("primitiveSelectSphere");

                    ImpressEventManager.TriggerEvent("primitiveEnableCreation");

                    return;
                }
                
                ImpressEventManager.TriggerEvent("primitiveDeselectSphere");
            });

            capsuleToggle.onValueChanged.AddListener((isOn) =>
            {
                if (isOn)
                {
                    ImpressEventManager.TriggerEvent("primitiveSelectCapsule");

                    ImpressEventManager.TriggerEvent("primitiveEnableCreation");

                    return;
                }
                
                ImpressEventManager.TriggerEvent("primitiveDeselectCapsule");
            });

            cylinderToggle.onValueChanged.AddListener((isOn) =>
            {
                if (isOn)
                {
                    ImpressEventManager.TriggerEvent("primitiveSelectCylinder");

                    ImpressEventManager.TriggerEvent("primitiveEnableCreation");

                    return;
                }
                
                ImpressEventManager.TriggerEvent("primitiveDeselectCylinder");
            });

            cubeToggle.onValueChanged.AddListener((isOn) =>
            {
                if (isOn)
                {
                    ImpressEventManager.TriggerEvent("primitiveSelectCube");

                    ImpressEventManager.TriggerEvent("primitiveEnableCreation");

                    return;
                }
                
                ImpressEventManager.TriggerEvent("primitiveDeselectCube");
            });

            planeToggle.onValueChanged.AddListener((isOn) =>
            {
                if (isOn)
                {
                    ImpressEventManager.TriggerEvent("primitiveSelectPlane");

                    ImpressEventManager.TriggerEvent("primitiveEnableCreation");

                    return;
                }
                
                ImpressEventManager.TriggerEvent("primitiveDeselectPlane");
            });
        }
    }
}
