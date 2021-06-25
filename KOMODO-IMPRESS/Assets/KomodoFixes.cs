using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Komodo.Runtime;

public class KomodoFixes : MonoBehaviour
{
    void Start ()
    {
        if (UIManager.IsAlive) 
        {
            UIManager.Instance.isSceneButtonListReady = true;
        }
    }
}
