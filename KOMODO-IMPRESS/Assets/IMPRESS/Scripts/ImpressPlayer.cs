using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Komodo.IMPRESS
{
    public class ImpressPlayer : MonoBehaviour
    {
        public TriggerCreatePrimitive triggerCreatePrimitiveLeft;

        public TriggerGroup triggerGroupLeft;

        public TriggerUngroup triggerUngroupRight;

        public TriggerGroup triggerGroupRight;

        public TriggerUngroup triggerUngroupLeft;

        public void Start ()
        {
            if (triggerCreatePrimitiveLeft == null)
            {
                throw new UnassignedReferenceException("triggerCreatePrimitiveLeft");
            }

            if (!triggerGroupLeft)
            {
                throw new UnassignedReferenceException("triggerGroupLeft");
            }

            if (!triggerUngroupRight)
            {
                throw new UnassignedReferenceException("triggerUngroupRight");
            }

            if (!triggerGroupRight)
            {
                throw new UnassignedReferenceException("triggerGroupRight");
            }

            if (!triggerUngroupLeft)
            {
                throw new UnassignedReferenceException("triggerUngroupLeft");
            }
        }
    }
}
