using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Komodo.IMPRESS
{
    public class MenuAnchor : MonoBehaviour
    {
        public enum Kind
        {
            LEFT_HANDED,
            RIGHT_HANDED,
            SCREEN,
            UNKNOWN
        }

        public Kind kind;
    }
}