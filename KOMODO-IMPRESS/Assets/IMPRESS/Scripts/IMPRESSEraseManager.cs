using Komodo.Runtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Komodo.IMPRESS{

    public class IMPRESSEraseManager : EraseManager
    {
        public override void Start()
        {
            base.Start();
        }
        public override void TryAndErase(NetworkedGameObject netReg)
        {
            //komodo stuff
            base.TryAndErase(netReg);

            var entityID = entityManager.GetComponentData<NetworkEntityIdentificationComponentData>(netReg.Entity).entityID;

            if (entityManager.HasComponent<PrimitiveTag>(netReg.Entity))
            {
                /////turn it off for ourselves and others
                netReg.gameObject.SetActive(false);

                CreatePrimitiveManager.Instance.SendPrimitiveNetworkUpdate(entityID, -9);

                ////save our reverted action for undoing the process with the undo button
                if (UndoRedoManager.IsAlive)
                    UndoRedoManager.Instance.savedStrokeActions.Push(() =>
                    {
                        netReg.gameObject.SetActive(true);

                        CreatePrimitiveManager.Instance.SendPrimitiveNetworkUpdate(entityID, 9);
                }
                    );
            }
        }
    }
}