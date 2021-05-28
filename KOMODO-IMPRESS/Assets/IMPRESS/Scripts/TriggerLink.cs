using System.Collections.Generic;
using UnityEngine;
using Komodo.Utilities;
using Komodo.Runtime;

namespace Komodo.IMPRESS
{

    public class TriggerLink : MonoBehaviour
    {
        public void OnTriggerEnter(Collider collider)
        {
            GroupManager.Instance.AddToLinkedGroup(collider);
        }
    }
}
