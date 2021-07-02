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
        public TabButton eraseTab;

        public Button undoButton;

        public TabButton drawTab;

        public Toggle brushToggle;

        public Toggle capsuleToggle;

        public Toggle cylinderToggle;

        public Toggle cubeToggle;

        public Toggle planeToggle;

        public Toggle sphereToggle;

        public TabButton groupTab;

        public Toggle redToggle;

        public Toggle blueToggle;

        public Toggle groupToggle;

        public Toggle ungroupToggle;

        void OnValidate ()
        {
            if (eraseTab == null)
            {
                throw new UnassignedReferenceException("eraseButton");
            }

            if (!drawTab)
            {
                throw new UnassignedReferenceException("drawTab");
            }

            if (!groupTab)
            {
                throw new UnassignedReferenceException("groupTab");
            }

            if (undoButton == null)
            {
                throw new UnassignedReferenceException("undoButton");
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

            if (redToggle == null)
            {
                throw new UnassignedReferenceException("redToggle");
            }

            if (blueToggle == null)
            {
                throw new UnassignedReferenceException("blueToggle");
            }

            if (groupToggle == null)
            {
                throw new UnassignedReferenceException("groupToggle");
            }

            if (ungroupToggle == null)
            {
                throw new UnassignedReferenceException("ungroupToggle");
            }
        }

        public void Start ()
        {
            eraseTab.onTabSelected.AddListener(() => 
            {
                ImpressEventManager.TriggerEvent("eraser.enable");
            });

            eraseTab.onTabDeselected.AddListener(() => 
            {
                ImpressEventManager.TriggerEvent("eraser.disable");
            });

            undoButton.onClick.AddListener(() =>
            {
                UndoRedoManager.Instance.Undo();
            });

            drawTab.onTabSelected.AddListener(() =>
            {
                // do nothing.
            });

            drawTab.onTabDeselected.AddListener(() =>
            {
                ImpressEventManager.TriggerEvent("drawTool.disable");

                ImpressEventManager.TriggerEvent("primitiveTool.disable");
            });

            brushToggle.onValueChanged.AddListener((isOn) =>
            {
                if (isOn)
                {
                    ImpressEventManager.TriggerEvent("drawTool.enable");

                    ImpressEventManager.TriggerEvent("primitiveTool.disable");

                    // TODO(Brandon) - is this the best way to get out of the primitive creation mode?

                    return;
                }

                ImpressEventManager.TriggerEvent("drawTool.disable");
            });

            sphereToggle.onValueChanged.AddListener((isOn) =>
            {
                if (isOn)
                {
                    ImpressEventManager.TriggerEvent("primitiveTool.selectSphere");

                    ImpressEventManager.TriggerEvent("primitiveTool.enable");

                    return;
                }

                ImpressEventManager.TriggerEvent("primitiveTool.deselectSphere");
            });

            capsuleToggle.onValueChanged.AddListener((isOn) =>
            {
                if (isOn)
                {
                    ImpressEventManager.TriggerEvent("primitiveTool.selectCapsule");

                    ImpressEventManager.TriggerEvent("primitiveTool.enable");

                    return;
                }

                ImpressEventManager.TriggerEvent("primitiveTool.deselectCapsule");
            });

            cylinderToggle.onValueChanged.AddListener((isOn) =>
            {
                if (isOn)
                {
                    ImpressEventManager.TriggerEvent("primitiveTool.selectCylinder");

                    ImpressEventManager.TriggerEvent("primitiveTool.enable");

                    return;
                }

                ImpressEventManager.TriggerEvent("primitiveTool.deselectCylinder");
            });

            cubeToggle.onValueChanged.AddListener((isOn) =>
            {
                if (isOn)
                {
                    ImpressEventManager.TriggerEvent("primitiveTool.selectCube");

                    ImpressEventManager.TriggerEvent("primitiveTool.enable");

                    return;
                }

                ImpressEventManager.TriggerEvent("primitiveTool.deselectCube");
            });

            planeToggle.onValueChanged.AddListener((isOn) =>
            {
                if (isOn)
                {
                    ImpressEventManager.TriggerEvent("primitiveTool.selectPlane");

                    ImpressEventManager.TriggerEvent("primitiveTool.enable");

                    return;
                }

                ImpressEventManager.TriggerEvent("primitiveTool.deselectPlane");
            });

            ImpressEventManager.StartListening("menu.setRightHanded", () =>
            {
            });

            ImpressEventManager.StartListening("menu.setLeftHanded", () =>
            {
            });

            groupTab.onTabSelected.AddListener(() =>
            {
                ImpressEventManager.TriggerEvent("groupTool.showGroups");
            });

            groupTab.onTabDeselected.AddListener(() =>
            {
                ImpressEventManager.TriggerEvent("groupTool.hideGroups");

                ImpressEventManager.TriggerEvent("groupTool.disableGrouping");
                
                ImpressEventManager.TriggerEvent("groupTool.disableUngrouping");
            });

            redToggle.onValueChanged.AddListener((isOn) =>
            {
                if (isOn)
                {
                    ImpressEventManager.TriggerEvent("groupTool.selectRed");

                    return;
                }

                ImpressEventManager.TriggerEvent("groupTool.deselectRed");
            });

            blueToggle.onValueChanged.AddListener((isOn) =>
            {
                if (isOn)
                {
                    ImpressEventManager.TriggerEvent("groupTool.selectBlue");

                    return;
                }

                ImpressEventManager.TriggerEvent("groupTool.deselectBlue");
            });

            groupToggle.onValueChanged.AddListener((isOn) =>
            {
                if (isOn)
                {
                    ImpressEventManager.TriggerEvent("groupTool.enableGrouping");

                    return;
                }

                ImpressEventManager.TriggerEvent("groupTool.disableGrouping");
            });

            ungroupToggle.onValueChanged.AddListener((isOn) =>
            {
                if (isOn)
                {
                    ImpressEventManager.TriggerEvent("groupTool.enableUngrouping");

                    return;
                }

                ImpressEventManager.TriggerEvent("groupTool.disableUngrouping");
            });
        }

        // As of Komodo v0.3.2, UIManager does not have a public IsRightHanded function, so we must make do with this workaround. Returns a MenuAnchor.Location value, including UNKNOWN if the parent is not a MenuAnchor.
        public MenuAnchor.Kind GetMenuLocation ()
        {
            if (transform.parent.TryGetComponent(out MenuAnchor anchor))
            {
                return anchor.kind;
            }

            return MenuAnchor.Kind.UNKNOWN;
        }
    }
}
