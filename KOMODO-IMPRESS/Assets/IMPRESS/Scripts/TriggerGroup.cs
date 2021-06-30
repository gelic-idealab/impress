using System.Collections.Generic;
using UnityEngine;
using Komodo.Utilities;
using Komodo.Runtime;

namespace Komodo.IMPRESS
{
    public class TriggerGroup : MonoBehaviour
    {
        public void OnTriggerEnter(Collider collider)
        {
            GroupManager.Instance.AddToGroup(collider);
        }
    }
}
