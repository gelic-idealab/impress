using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Komodo.Runtime;

namespace Komodo.IMPRESS
{
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
}
