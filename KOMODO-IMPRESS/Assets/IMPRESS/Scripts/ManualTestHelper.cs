using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Komodo.Runtime;
using Komodo.Utilities;

public class ManualTestHelper : MonoBehaviour
{
    private Keyboard keyboard;

    [ContextMenu("EventSystemManager: Set to Desktop")]
    public void EventSystemManagerSetToDesktop ()
    {
        if (!EventSystemManager.IsAlive)
        {
            return;
        }

        EventSystemManager.Instance.SetToDesktop();
    }

    [ContextMenu("EventSystemManager: Set to XR")]
    public void EventSystemManagerSetToXR ()
    {
        if (!EventSystemManager.IsAlive)
        {
            return;
        }

        EventSystemManager.Instance.SetToXR();
    }

    [ContextMenu("UIManager: Toggle Left-Handed Menu")]
    public void UIManagerToggleLeftHandedMenu ()
    {
        if (!UIManager.IsAlive)
        {
            return;
        }

        UIManager.Instance.ToggleLeftHandedMenu();
    }

    [ContextMenu("UIManager: Toggle Right-Handed Menu")]
    public void UIManagerToggleRightHandedMenu ()
    {
        if (!UIManager.IsAlive)
        {
            return;
        }

        UIManager.Instance.ToggleRightHandedMenu();
    }

    public void Awake ()
    {
        keyboard = Keyboard.current;
    }

    [Header("Press T and Y for Left/Right Triggers")]
    public bool hint;

    public void Update ()
    {
        if (keyboard != null)
        {
            if (keyboard.tKey.wasPressedThisFrame)
            {
                KomodoEventManager.TriggerEvent("controllers.left.triggerDown");
            }

            if (keyboard.tKey.wasReleasedThisFrame)
            {
                KomodoEventManager.TriggerEvent("controllers.left.triggerUp");
            }

            if (keyboard.yKey.wasPressedThisFrame)
            {
                KomodoEventManager.TriggerEvent("controllers.right.triggerDown");
            }

            if (keyboard.yKey.wasReleasedThisFrame)
            {
                KomodoEventManager.TriggerEvent("controllers.right.triggerUp");
            }
        }
    }
}
