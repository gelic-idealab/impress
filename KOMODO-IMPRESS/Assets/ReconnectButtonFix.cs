using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Komodo.Runtime;

namespace Komodo.IMPRESS
{
    public class ReconnectButtonFix : MonoBehaviour
    {
        void Start()
        {
            NetworkUpdateHandler netHandler = NetworkUpdateHandler.Instance;

            ImpressEventManager.StartListening("network.reconnect", () =>
            {
                netHandler.Reconnect();
            });
        }
    }
}
