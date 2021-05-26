using Komodo.Runtime;
using UnityEngine;

namespace Komodo.IMPRESS
{
    public class TriggerUnlink : MonoBehaviour
    {
        public void OnTriggerEnter(Collider collider)
        {
            GroupManager.Instance.RemoveFromLinkedGroup(collider);
        }
    }
}
