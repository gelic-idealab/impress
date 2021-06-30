using Komodo.Runtime;
using UnityEngine;

namespace Komodo.IMPRESS
{
    public class TriggerUngroup : MonoBehaviour
    {
        public void OnTriggerEnter(Collider collider)
        {
            GroupManager.Instance.RemoveFromGroup(collider);
        }
    }
}
