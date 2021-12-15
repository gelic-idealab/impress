using UnityEngine;
using Komodo.Runtime;

namespace Komodo.IMPRESS
{
    public class FixForPlayerReferences : MonoBehaviour
    {
        public void Start ()
        {
            ControllersManager.Initialize();
        }

        public void OnDestroy ()
        {
            ControllersManager.Deinitialize();
        }
    }
}