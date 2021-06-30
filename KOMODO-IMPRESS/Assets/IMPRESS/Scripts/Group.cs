using System.Collections.Generic;
using UnityEngine;
using Komodo.Utilities;

namespace Komodo.IMPRESS
{
    public class Group : MonoBehaviour
    {
        public List<Collider> groups;

        public Transform groupsParent;

        public GameObject currentGroupBoundingBox;

        public MeshRenderer meshRenderer;
    }
}