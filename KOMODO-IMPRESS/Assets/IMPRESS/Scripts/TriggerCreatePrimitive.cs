using Unity.Entities;
using UnityEngine;
using UnityEngine.UI;
using Komodo.Utilities;
using Komodo.Runtime;

namespace Komodo.IMPRESS
{
    public class TriggerCreatePrimitive : MonoBehaviour
    {
        // Start is called before the first frame update
        public void OnEnable()
        {
            //only create when our cursor is Off
            if (UIManager.IsAlive)
                if (UIManager.Instance.GetCursorActiveState())
                    return;

            CreatePrimitiveManager.Instance.CreatePrimitive();
        }
       
    }
}
