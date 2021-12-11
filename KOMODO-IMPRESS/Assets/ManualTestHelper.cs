using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Komodo.Runtime;

public class ManualTestHelper : MonoBehaviour
{
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
}
