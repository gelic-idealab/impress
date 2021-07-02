using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Komodo.Runtime;

namespace Komodo.IMPRESS
{
    // We need to manually fire an event whenever the handedness changes.
    // This could break if the implementation of PlayerReferences changes,
    // so this is in a "Komodo v0.3.2 fix" file.
    public class SetHandednessEvent : MonoBehaviour
    {
        public PlayerReferences playerRefs;

        public void Awake ()
        {
            if (!playerRefs)
            {
                throw new UnassignedReferenceException("playerRefs");
            }
        }

        public void Start ()
        {
            if (UIManager.IsAlive)
            {
                playerRefs.LeftHandSwitchMenuAction.onFirstClick.AddListener(() =>
                {
                    ImpressEventManager.TriggerEvent("menu.setLeftHanded");

                    ImpressEventManager.TriggerEvent("primitiveTool.setRightHanded");
                });

                playerRefs.RightHandSwitchMenuAction.onFirstClick.AddListener(() =>
                {
                    ImpressEventManager.TriggerEvent("menu.setRightHanded");

                    ImpressEventManager.TriggerEvent("primitiveTool.setLeftHanded");
                });
            }
        }
    }
}