using Unity.Entities;
using UnityEngine;
using UnityEngine.UI;
using Komodo.Utilities;
using Komodo.Runtime;

namespace Komodo.IMPRESS
{
    public class TriggerCreatePrimitive : MonoBehaviour
    {
        public void OnEnable()
        {
            //only create when our cursor is Off
            if (UIManager.IsAlive && UIManager.Instance.GetCursorActiveState()) {
                return;
            }

            CreatePrimitiveManager.Instance.CreatePrimitive();
        }
    }
}
