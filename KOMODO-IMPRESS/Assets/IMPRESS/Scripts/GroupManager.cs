using Komodo.Utilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Komodo.Runtime;

namespace Komodo.IMPRESS
{
    public struct GroupProperties
    {
        public int groupID;
        public int entityID;
        public bool isAdding;
    }

    public enum GroupColor {
        RED,
        BLUE,

    }

    public class GroupManager : SingletonComponent<GroupManager>
    {
        public static GroupManager Instance
        {
            get { return (GroupManager)_Instance; }
            set { _Instance = value; }
        }

        private BoxCollider currentRootCollider;

        private UnityAction _showGroups;

        private UnityAction _hideGroups;

        private UnityAction _enableGrouping;

        private UnityAction _disableGrouping;

        private UnityAction _enableUngrouping;

        private UnityAction _disableUngrouping;

        private UnityAction _selectRed;

        private UnityAction _selectBlue;

        private Group currentGroup;

        private int _currentGroupType;

        public KomodoControllerInteraction leftControllerInteraction;

        public KomodoControllerInteraction rightControllerInteraction;

        public ImpressPlayer player;

        public GameObject groupBoundingBox;

        public Dictionary<int, Group> groups = new Dictionary<int, Group>();

        public void Awake()
        {
            //used to set our managers alive state to true to detect if it exist within scene
            var initManager = Instance;

            //register our message with the funcion that will be receiving updates from others
            GlobalMessageManager.Instance.Subscribe("group", (str) => ReceiveGroupUpdate(str));
        }

        public void Start ()
        {
            if (!leftControllerInteraction)
            {
                throw new UnassignedReferenceException("LeftControllerInteraction");
            }

            if (!rightControllerInteraction)
            {
                throw new UnassignedReferenceException("RightControllerInteraction");
            }

            if (!player)
            {
                throw new UnassignedReferenceException("player");
            }

            if (!groupBoundingBox)
            {
                throw new UnassignedReferenceException("groupBoundingBox");
            }

            _showGroups += ShowGroups;

            ImpressEventManager.StartListening("groupTool.showGroups", _showGroups);

            _hideGroups += HideGroups;

            ImpressEventManager.StartListening("groupTool.hideGroups", _hideGroups);

            _enableGrouping += _EnableGrouping;

            ImpressEventManager.StartListening("groupTool.enableGrouping", _enableGrouping);

            _disableGrouping += _DisableGrouping;

            ImpressEventManager.StartListening("groupTool.disableGrouping", _disableGrouping);

            _enableUngrouping += _EnableUngrouping;

            ImpressEventManager.StartListening("groupTool.enableUngrouping", _enableUngrouping);

            _disableUngrouping += _DisableUngrouping;

            ImpressEventManager.StartListening("groupTool.disableUngrouping", _disableUngrouping);

            _selectRed += _SelectRed;

            ImpressEventManager.StartListening("groupTool.selectRed", _selectRed);

            _selectBlue += _SelectBlue;

            ImpressEventManager.StartListening("groupTool.selectBlue", _selectBlue);
        }

        private void _EnableGrouping ()
        {
            player.triggerGroupLeft.gameObject.SetActive(true);

            player.triggerGroupRight.gameObject.SetActive(true);
        }

        private void _DisableGrouping ()
        {
            player.triggerGroupLeft.gameObject.SetActive(false);

            player.triggerGroupRight.gameObject.SetActive(false);
        }

        private void _EnableUngrouping ()
        {
            player.triggerUngroupLeft.gameObject.SetActive(true);

            player.triggerUngroupRight.gameObject.SetActive(true);
        }

        private void _DisableUngrouping ()
        {
            player.triggerUngroupLeft.gameObject.SetActive(false);

            player.triggerUngroupRight.gameObject.SetActive(false);
        }

        /// <summary>
        /// Show and hide bounding boxes, which represent groups.
        /// </summary>
        public void ShowGroups()
        {
            foreach (var item in groups.Values)
            {
                item.GetComponent<MeshRenderer>().enabled = true;
            }
        }

        public void HideGroups()
        {
            foreach (var item in groups.Values)
            {
                item.transform.GetComponent<BoxCollider>().enabled = true;

                foreach (Transform colItems in item.transform.GetChild(0))
                {
                    colItems.GetComponent<BoxCollider>().enabled = false;
                }

                item.GetComponent<MeshRenderer>().enabled = false;
            }
        }

        private void _SelectRed ()
        {
            this._currentGroupType = (int) GroupColor.RED;
        }

        private void _SelectBlue ()
        {
            this._currentGroupType = (int) GroupColor.BLUE;
        }

        private void _CreateNewGroup (int groupID)
        {
            currentGroup = Instantiate(groupBoundingBox).AddComponent<Group>();

            //make net component
            NetworkedObjectsManager.Instance.CreateNetworkedGameObject(currentGroup.gameObject);

            //make child of parent to contain our grouped objects
            currentGroup.groupsParent = new GameObject("Linker Parent").transform;

            var mat = currentGroup.GetComponent<MeshRenderer>().material;

            Color color = Color.black;

            if (groupID == (int) GroupColor.RED)
            {
                color = Color.red;
            }
            else if (groupID == (int) GroupColor.BLUE)
            {
                color = Color.blue;
            }
            else if(groupID == 2)
            {
                color = Color.green;
            }
            else if (groupID == 3)
            {
                color = Color.grey;
            }

            color.a = 0.39f;

            mat.SetColor("_Color", color);

            currentGroup.GetComponent<MeshRenderer>().material = mat;

            //add new item to our dictionary and collection
            groups.Add(groupID, currentGroup);

            currentGroup.groups = new List<Collider>();
        }

        private void _UpdateGroupBounds ()
        {
            //remove our children before we create a new bounding box for our parent containing the group
            currentGroup.transform.DetachChildren();

            currentGroup.groupsParent.transform.DetachChildren();

            //establish new bounding box
            var newBound = new Bounds(currentGroup.groups[0].transform.position, Vector3.one * 0.02f);

            for (int i = 0; i < currentGroup.groups.Count; i += 1)
            {
                var _collider = currentGroup.groups[i];

                //turn it on to get bounds info 
                _collider.enabled = true;

                //set new collider bounds
                newBound.Encapsulate(new Bounds(_collider.transform.position, _collider.bounds.size));

                _collider.enabled = false;
            }

            //set fields for our new created bounding box
            currentGroup.transform.position = newBound.center;

            currentGroup.transform.SetGlobalScale(newBound.size);

            currentGroup.transform.rotation = Quaternion.identity;

            currentGroup.groupsParent.rotation = Quaternion.identity;

            //recreate our collider to be consistent with the new render bounds
            if (currentGroup.TryGetComponent(out BoxCollider boxCollider))
            {
                Destroy(boxCollider);
            }

            currentRootCollider = currentGroup.gameObject.AddComponent<BoxCollider>();

            //add children again
            foreach (var item in currentGroup.groups)
            {
                item.transform.SetParent(currentGroup.groupsParent, true);
            }

            //add our collection parent to our bounding box parent
            currentGroup.groupsParent.SetParent(currentGroup.transform, true);
        }

        public int GroupIDFromClientIDOrGroupType (int otherClientID)
        {
            if (otherClientID == -1)
            {
                return _currentGroupType;
            }
            else
            {
                return otherClientID;
            }
        }

        /// <summary>
        /// Add an object to a specific group
        /// </summary>
        /// <param name="collider"> The collider of the adding elemen</param>
        /// <param name="otherClientID">customID should not be included or should be -1 to use the userID instead for identifying what group to add to</param>
        public void AddToGroup(Collider collider, int otherClientID = -1)
        {
            if (!collider.CompareTag("Interactable"))
            {
                return;
            }

            foreach (KeyValuePair<int, Group> candidateGroup in groups)
            {
                //we do not want to add the constructing boxes to itself
                if (candidateGroup.Value.gameObject.GetInstanceID() == collider.gameObject.GetInstanceID())
                {
                    return;
                }
            }

            // check if this is a call from the user or an external client
            int groupID = GroupIDFromClientIDOrGroupType(otherClientID);

            if (!groups.ContainsKey(groupID))
            {
                _CreateNewGroup(groupID);
            }

            currentGroup = groups[groupID];

            if (currentGroup.groups.Contains(collider))
            {
                return; // if the group already has this collider, stop.
            }

            currentGroup.groups.Add(collider);

            _UpdateGroupBounds();

            _DropGroupedObject(collider);

            // send call if it was a client call
            if (!collider.TryGetComponent<Group>(out Group group)
                && otherClientID == -1)
            {
                SendGroupUpdate(collider.GetComponent<NetworkedGameObject>().thisEntityID, groupID, true);
            }
        }

        // Tells the controller to stop holding the object represented by collider.
        private void _DropGroupedObject(Collider collider)
        {
            /* TODO FIX THIS TO WORK WITH KOMODOCORE v0.5.4

            //only set our grab object to the group object when it is a different object than its own to avoid the object reparenting itself
            if (leftControllerInteraction.thisHandTransform != null
                && leftControllerInteraction.currentTransform.GetInstanceID() == collider.transform.GetInstanceID())
            {
                leftControllerInteraction.Drop();
            }

            if (rightControllerInteraction.currentTransform != null
                && rightControllerInteraction.currentTransform.GetInstanceID() == collider.transform.GetInstanceID())
            {
                rightControllerInteraction.Drop();
            }
            */
        }

        public void SendGroupUpdate(int _entityID, int _groupID, bool isAdding)
        {
            KomodoMessage km = new KomodoMessage("group", JsonUtility.ToJson(
                new GroupProperties
                {
                    entityID = _entityID,
                    groupID = _groupID,
                    isAdding = isAdding
                })
            );

            km.Send();
        }

        public void RemoveFromGroup(Collider collider, int otherClientID = -1)
        {
            if (!collider.CompareTag("Interactable"))
                return;

            //to check if this is a call from the client or external clients
            int groupID;

            if (otherClientID == -1)
            {
                groupID = _currentGroupType;
            }
            else
            {
                groupID = otherClientID;
            }

            //disable main collider of the linkedgroup to access its child group item colliders
            if (collider.TryGetComponent(out Group lg))
            {
                currentGroup = lg;

                //disable main collider and enable the child elements to remove from main
                lg.transform.GetComponent<BoxCollider>().enabled = false;

                lg.meshRenderer = lg.GetComponent<MeshRenderer>();

                StartCoroutine(ReenableParentColliderOutsideRenderBounds(lg.meshRenderer));

                foreach (Transform colItems in lg.transform.GetChild(0))
                {
                    colItems.GetComponent<Collider>().enabled = true;
                }

                return;
            }

            if (!groups.ContainsKey(groupID))
            {
                return;
            }

            currentGroup = groups[groupID];

            //handle the items that we touch
            if (collider.transform.IsChildOf(currentGroup.groupsParent))
            {
                collider.transform.parent = null;
                currentGroup.groups.Remove(collider);

                UpdateGroup(currentGroup);

            /* TODO FIX THIS TO WORK WITH KOMODOCORE v0.5.4
                if (currentGroup.groups.Count == 0)
                {
                    ////RE-enable our grabing funcionality by droping destroyed object
                    if (leftControllerInteraction.currentTransform != null)
                    {
                        if (leftControllerInteraction.currentTransform.GetInstanceID() == currentGroup.transform.GetInstanceID())
                        {
                            leftControllerInteraction.Drop();
                        }
                    }

                    if (rightControllerInteraction.currentTransform != null)
                    {
                        if (rightControllerInteraction.currentTransform.GetInstanceID() == currentGroup.transform.GetInstanceID())
                        {
                            rightControllerInteraction.Drop();
                        }
                    }

                    currentGroup.groupsParent.DetachChildren();
                    groups.Remove(groupID);

                    Destroy(currentGroup.gameObject);
                }
            */
            }

            //send call if it was a client call
            if (!collider.TryGetComponent<Group>(out Group lgnew))
            {
                if (otherClientID == -1)
                {
                    SendGroupUpdate(collider.GetComponent<NetworkedGameObject>().thisEntityID, groupID, false);
                }
            }
        }
        public void ExitGroup(Group lg)
        {
            if (lg)
            {
                lg.transform.GetComponent<BoxCollider>().enabled = true;

                foreach (Transform colItems in lg.transform.GetChild(0))
                {
                    colItems.GetComponent<Collider>().enabled = false;
                }
            }
        }

        public IEnumerator ReenableParentColliderOutsideRenderBounds(MeshRenderer lgRend)
        {
            yield return new WaitUntil(() =>
            {
                if (lgRend == null)
                {
                    return true;
                }

                if (!lgRend.bounds.Contains(leftControllerInteraction.transform.position) && 
                    !lgRend.bounds.Contains(rightControllerInteraction.transform.position))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            });

            if (lgRend == null)
            {
                yield break;
            }

            lgRend.transform.GetComponent<BoxCollider>().enabled = true;

            foreach (Transform colItems in lgRend.transform.GetChild(0))
            {
                colItems.GetComponent<Collider>().enabled = false;
            }
        }

        public void ReceiveGroupUpdate(string message)
        {
            GroupProperties data = JsonUtility.FromJson<GroupProperties>(message);

            /* TODO FIX THIS TO WORK WITH KOMODOCORE v0.5.4
            if (data.isAdding)
            {
                AddToGroup(ClientSpawnManager.Instance.networkedObjectFromEntityId[data.entityID].GetComponent<Collider>(), data.groupID);
            }
            else
            {
                RemoveFromGroup(ClientSpawnManager.Instance.networkedObjectFromEntityId[data.entityID].GetComponent<Collider>(), data.groupID);
            }
            */
        }

        public void UpdateGroup(Group linkParent)
        {
            List<Transform> childList = new List<Transform>();

            var rootParent = linkParent.transform;

            var parentOfCollection = linkParent.groupsParent;

            Bounds newBound = default;

            if (linkParent.groups.Count != 0)
            {
                newBound = new Bounds(linkParent.groups[0].transform.position, Vector3.one * 0.02f);// new Bounds();
            }
            else
            {
                return;
            }

            for (int i = 0; i < rootParent.GetChild(0).childCount; i++)
            {
                childList.Add(parentOfCollection.GetChild(i));

                var col = parentOfCollection.GetChild(i).GetComponent<Collider>();

                //set new collider bounds
                newBound.Encapsulate(new Bounds(col.transform.position, col.bounds.size));
            }

            rootParent.transform.DetachChildren();

            parentOfCollection.transform.DetachChildren();

            rootParent.position = newBound.center;

            rootParent.SetGlobalScale(newBound.size);

            rootParent.rotation = Quaternion.identity;

            parentOfCollection.rotation = Quaternion.identity;

            if (rootParent.TryGetComponent(out Collider boxCollider))
            {
                Destroy(boxCollider);
            }

            rootParent.gameObject.AddComponent<BoxCollider>();

            foreach (var item in childList)
            {
                item.transform.SetParent(parentOfCollection.transform, true);
            }

            parentOfCollection.transform.SetParent(rootParent.transform, true);
        }
    }
}
