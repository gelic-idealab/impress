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

        private static HandSelector selectorLeft;

        private static HandSelector selectorRight;

        public static void Initialize ()
        {
            FindSelectors();

            _onLeftTriggerDown += StartLeftSelect;

            _onLeftTriggerDown += TryStartLeftWorldPulling;

            KomodoEventManager.StartListening("controllers.left.triggerDown", _onLeftTriggerDown);

            _onLeftTriggerUp += EndLeftSelect;

            _onLeftTriggerUp += StopLeftWorldPulling;

            KomodoEventManager.StartListening("controllers.left.triggerUp", _onLeftTriggerUp);

            _onRightTriggerDown += StartRightSelect;

            _onRightTriggerDown += TryStartRightWorldPulling;

            KomodoEventManager.StartListening("controllers.right.triggerDown", _onRightTriggerDown);

            _onRightTriggerUp += EndRightSelect;

            _onRightTriggerUp += StopRightWorldPulling;

            KomodoEventManager.StartListening("controllers.right.triggerUp", _onRightTriggerUp);
        }

        private static void FindSelectors ()
        {
            HandSelector[] selectors = Object.FindObjectsOfType(typeof(HandSelector)) as HandSelector[];

            if (selectors.Length == 0)
            {
                Debug.LogWarning("Found 0 selectors. Expected 2. Not initializing any.");

                return;
            }

            if (selectors.Length == 1)
            {
                Debug.LogWarning("Found 1 selectors. Expected 2.");

                selectorLeft = selectors[0];

                return;
            }

            if (selectors.Length > 2)
            {
                Debug.LogWarning("Found more than 2 selectors. Expected 2.");
            }

            AvatarComponent avatarComponent0 = selectors[0].transform.GetComponentInParent(typeof (AvatarComponent)) as AvatarComponent;

            AvatarComponent avatarComponent1 = selectors[1].transform.GetComponentInParent(typeof (AvatarComponent)) as AvatarComponent;

            if (avatarComponent0.thisEntityType != Entity_Type.users_Lhand && avatarComponent1.thisEntityType != Entity_Type.users_Lhand)
            {
                Debug.LogWarning("Left hand not found.");

                return;
            }

            if (avatarComponent0.thisEntityType != Entity_Type.users_Rhand && avatarComponent1.thisEntityType != Entity_Type.users_Rhand)
            {
                Debug.LogWarning("Right hand not found.");

                return;
            }

            if (avatarComponent0.thisEntityType == Entity_Type.users_Rhand)
            {
                selectorLeft = selectors[1];

                selectorRight = selectors[0];

                return;
            }

            selectorLeft = selectors[0];

            selectorRight = selectors[1];
        }

        public static void StartLeftSelect ()
        {
            selectorLeft.gameObject.SetActive(true);
        }

        public static void EndLeftSelect ()
        {
            selectorLeft.gameObject.SetActive(false);
        }

        public static void StartRightSelect ()
        {
            selectorRight.gameObject.SetActive(true);
        }

        public static void EndRightSelect ()
        {
            selectorRight.gameObject.SetActive(false);
        }

        public static void TryStartLeftWorldPulling ()
        {
            DoubleTapState.Instance.leftHandTriggerPressed = true;

            if (DoubleTapState.Instance.leftHandTriggerPressed && DoubleTapState.Instance.rightHandTriggerPressed)
            {
                DoubleTapState.Instance.OnDoubleTriggerStateOn?.Invoke();
            }
        }

        public static void TryStartRightWorldPulling ()
        {
            DoubleTapState.Instance.rightHandTriggerPressed = true;

            if (DoubleTapState.Instance.leftHandTriggerPressed && DoubleTapState.Instance.rightHandTriggerPressed)
            {
                DoubleTapState.Instance.OnDoubleTriggerStateOn?.Invoke();
            }
        }

        public static void StopLeftWorldPulling ()
        {
            DoubleTapState.Instance.leftHandTriggerPressed = false;

            DoubleTapState.Instance.OnDoubleTriggerStateOff?.Invoke();
        }

        public static void StopRightWorldPulling ()
        {
            DoubleTapState.Instance.rightHandTriggerPressed = false;

            DoubleTapState.Instance.OnDoubleTriggerStateOff?.Invoke();
        }

        public static void Deinitialize ()
        {
            KomodoEventManager.StopListening("controllers.left.triggerDown", _onLeftTriggerDown);

            KomodoEventManager.StopListening("controllers.left.triggerUp", _onLeftTriggerUp);

            KomodoEventManager.StopListening("controllers.right.triggerDown", _onRightTriggerDown);

            KomodoEventManager.StopListening("controllers.right.triggerUp", _onRightTriggerUp);
        }
    }
}
