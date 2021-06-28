using Komodo.Runtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Komodo.IMPRESS
{
    public class IMPRESSEraseManager : EraseManager
    {
        public UnityAction _enableEraser;

        public UnityAction _disableEraser;

        public GameObject eraserObjectLeft;

        public GameObject eraserDisplayLeft; // TODO(Brandon) why do we need this? 

        public GameObject eraserObjectRight;

        public GameObject eraserDisplayRight; // TODO(Brandon) why do we need this? 

        public void OnValidate ()
        {
            if (eraserObjectLeft == null)
            {
                Debug.LogWarning("eraserObjectLeft is missing", gameObject);
            }

            if (eraserDisplayLeft == null)
            {
                Debug.LogWarning("eraserDisplayLeft is missing", gameObject);
            }

            if (eraserObjectRight == null)
            {
                Debug.LogWarning("eraserObjectRight is missing", gameObject);
            }

            if (eraserDisplayRight == null)
            {
                Debug.LogWarning("eraserDisplayRight is missing", gameObject);
            }
        }

        public override void Start ()
        {
            base.Start();

            _enableEraser += ShowEraserDisplays;

            ImpressEventManager.StartListening("eraserEnabled", _enableEraser);

            _disableEraser += HideEraserDisplays;

            ImpressEventManager.StartListening("eraserDisabled", _disableEraser);
        }

        public override void TryAndErase(NetworkedGameObject netReg)
        {
            // komodo stuff
            base.TryAndErase(netReg);

            var entityID = entityManager.GetComponentData<NetworkEntityIdentificationComponentData>(netReg.Entity).entityID;

            if (entityManager.HasComponent<PrimitiveTag>(netReg.Entity))
            {
                // hide object in our view instead ofdestroying it, so we can undo the erasure. 
                netReg.gameObject.SetActive(false);

                // tell other clients to do the same thing
                CreatePrimitiveManager.Instance.SendPrimitiveNetworkUpdate(entityID, -9);

                // save to actions stack
                if (UndoRedoManager.IsAlive)
                {
                    UndoRedoManager.Instance.savedStrokeActions.Push(() =>
                    {
                        netReg.gameObject.SetActive(true);

                        CreatePrimitiveManager.Instance.SendPrimitiveNetworkUpdate(entityID, 9);
                    });
                }
            }
        }

        public void ShowEraserDisplays ()
        {
            eraserObjectLeft.SetActive(true);

            eraserDisplayLeft.SetActive(true);

            eraserObjectRight.SetActive(true);

            eraserDisplayRight.SetActive(true);
        }

        public void HideEraserDisplays ()
        {
            eraserObjectLeft.SetActive(false);

            eraserDisplayLeft.SetActive(false);

            eraserObjectRight.SetActive(false);

            eraserDisplayRight.SetActive(false);
        }
    }
}