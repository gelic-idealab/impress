using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Komodo.Runtime;

namespace Komodo.IMPRESS
{
    public static class ControllersManager
    {
        private static UnityAction _onLeftTriggerDown;

        private static UnityAction _onLeftTriggerUp;

        private static UnityAction _onRightTriggerDown;

        private static UnityAction _onRightTriggerUp;

        public static void Initialize ()
        {
            var selectors = Object.FindObjectsOfType(typeof(HandSelector)); // TODO assign selectorL and selectorR as class member variables

            _onLeftTriggerDown += StartLeftSelect;

            KomodoEventManager.StartListening("controllers.left.triggerDown", _onLeftTriggerDown);

            _onLeftTriggerUp += EndLeftSelect;

            KomodoEventManager.StartListening("controllers.left.triggerDown", _onLeftTriggerUp);

            _onRightTriggerDown += StartRightSelect;

            KomodoEventManager.StartListening("controllers.left.triggerDown", _onRightTriggerDown);

            _onRightTriggerUp += EndRightSelect;

            KomodoEventManager.StartListening("controllers.left.triggerDown", _onRightTriggerUp);
        }

        public static void StartLeftSelect ()
        {
            // TODO SetActive the HandSelector's gameObject.
        }

        public static void EndLeftSelect ()
        {
            // TODO  the HandSelector's gameObject.
        }

        public static void StartRightSelect ()
        {

        }

        public static void EndRightSelect ()
        {

        }

        public static void Deinitialize ()
        {
        
        }

        public static void TryStartLeftWorldPulling ()
        {

        }

        public static void TryStartRightWorldPulling ()
        {

        }
    }
}
