using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Komodo.Runtime;

namespace Komodo.IMPRESS
{
    // TODO - replace DrawingInstanceManager in IMPRESSMain2.unity with this script.
    public class DrawingManager : DrawingInstanceManager
    {
        public PlayerReferences playerRefs;

        private UnityAction _drawToolEnabled;

        private UnityAction _drawToolDisabled;

        void OnValidate ()
        {
            if (playerRefs == null)
            {
                throw new UnassignedReferenceException("playerRefs");
            }
        }

        void Start ()
        {
            _drawToolEnabled += EnableDrawTool;

            ImpressEventManager.StartListening("drawToolEnabled", _drawToolEnabled);

            _drawToolDisabled += DisableDrawTool;

            ImpressEventManager.StartListening("drawToolDisabled", _drawToolDisabled);
        }

        public void EnableDrawTool ()
        {
            playerRefs.drawL.gameObject.SetActive(true); 

            playerRefs.drawR.gameObject.SetActive(true); 
        }

        public void DisableDrawTool ()
        {
            playerRefs.drawL.gameObject.SetActive(false); 
            
            playerRefs.drawR.gameObject.SetActive(false); 
        }    
    }
}
