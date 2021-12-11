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

    private void ControllerStateLeftTriggerPressed ()
    {
        DoubleTapState.Instance.rightHandTriggerPressed = true;

        if (DoubleTapState.Instance.leftHandTriggerPressed && DoubleTapState.Instance.rightHandTriggerPressed)
        {
            DoubleTapState.Instance.OnDoubleTriggerStateOn?.Invoke();
        }
    }

    private void ControllerStateLeftTriggerReleased ()
    {
        DoubleTapState.Instance.leftHandTriggerPressed = false;

        DoubleTapState.Instance.OnDoubleTriggerStateOff?.Invoke();
    }

    private void ControllerStateRightTriggerPressed ()
    {
        DoubleTapState.Instance.rightHandTriggerPressed = true;

        if (DoubleTapState.Instance.leftHandTriggerPressed && DoubleTapState.Instance.rightHandTriggerPressed)
        {
            DoubleTapState.Instance.OnDoubleTriggerStateOn?.Invoke();
        }
    }

    private void ControllerStateRightTriggerReleased ()
    {
        DoubleTapState.Instance.rightHandTriggerPressed = false;

        DoubleTapState.Instance.OnDoubleTriggerStateOff?.Invoke();
    }

    public void Update ()
    {
        if (keyboard != null)
        {
            if (keyboard.tKey.wasPressedThisFrame)
            {
                KomodoEventManager.TriggerEvent("controllers.left.triggerDown");

                ControllerStateLeftTriggerPressed();
            }

            if (keyboard.tKey.wasReleasedThisFrame)
            {
                KomodoEventManager.TriggerEvent("controllers.left.triggerUp");

                ControllerStateLeftTriggerReleased();
            }

            if (keyboard.yKey.wasPressedThisFrame)
            {
                KomodoEventManager.TriggerEvent("controllers.right.triggerDown");

                ControllerStateRightTriggerPressed();
            }

            if (keyboard.yKey.wasReleasedThisFrame)
            {
                KomodoEventManager.TriggerEvent("controllers.right.triggerUp");

                ControllerStateRightTriggerReleased();
            }
        }
    }
}
