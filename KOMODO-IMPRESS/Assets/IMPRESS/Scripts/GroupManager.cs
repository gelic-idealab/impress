using Komodo.Utilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Komodo.Runtime;

public struct GroupProperties
{
   public int groupID;
    public int entityID;
    public bool isAdding;
}

namespace Komodo.IMPRESS
{
    public class GroupManager : SingletonComponent<GroupManager>
    {
        public static GroupManager Instance
        {
            get { return ((GroupManager)_Instance); }
            set { _Instance = value; }
        }

        public GameObject linkBoundingPrefab;
        public Dictionary<int, LinkedGroup> linkedGroupDictionary = new Dictionary<int, LinkedGroup>();

        BoxCollider currentRootCollider;

        public KomodoControllerInteraction LcontrollerInteraction;
        public KomodoControllerInteraction RcontrollerInteraction;

        public void Awake()
        {
            //used to set our managers alive state to true to detect if it exist within scene
            var initManager = Instance;


            if (GameObject.FindGameObjectWithTag("MenuUI").TryGetComponent(out ImpressEventManager uiRef))
            {
                uiRef.groupButton.onFirstClick.AddListener(() => SetAllGroupsToRender());
                uiRef.groupButton.onSecondClick.AddListener(() => SetAllGroupsToNOTRender());
            }

            //register our message with the funcion that will be receiving updates from others
            GlobalMessageManager.Instance.Subscribe("group", (str) => GroupRefresh(str));
        }


        /// <summary>
        /// Set all groups to render their bounding boxes.
        /// </summary>
        public void SetAllGroupsToRender()
        {
            foreach (var item in linkedGroupDictionary.Values)
            {
                item.GetComponent<MeshRenderer>().enabled = true; 
            };
        }
        public void SetAllGroupsToNOTRender()
        {
            foreach (var item in linkedGroupDictionary.Values)
            {
   

                item.transform.GetComponent<BoxCollider>().enabled = true;

                foreach (Transform colItems in item.transform.GetChild(0))
                    colItems.GetComponent<BoxCollider>().enabled = false;

                item.GetComponent<MeshRenderer>().enabled = false;

            };
        }

        public int userId;


      /// <summary>
      /// Current group id selected for the user
      /// </summary>
      /// <param name="id"></param>
        public void WorkingGroupID(int id)
        {
            this.userId = id;

        }

        /// <summary>
        /// Add an object to a specific group
        /// </summary>
        /// <param name="collider"> The collider of the adding elemen</param>
        /// <param name="customID">customID should not be included or should be -1 to use the userID instead for identifying what group to add to</param>
        public void AddToLinkedGroup(Collider collider, int customID = -1)
        {
            if (!collider.CompareTag("Interactable"))
                return;

            //to check if this is a call from the client or external clients
            var id = 0;

            if (customID == -1)
                id = userId;
            else
                id = customID;

         
            foreach (KeyValuePair<int,LinkedGroup> linkGroup in linkedGroupDictionary)
            {
                //we do not want to add the constructing boxes to itself
                if (linkGroup.Value.gameObject.GetInstanceID() == collider.gameObject.GetInstanceID())
                    return;
            }


            //check if we do not have a group created
            if (!linkedGroupDictionary.ContainsKey(id))
            {
                currentLinkedGroup = GameObject.Instantiate(linkBoundingPrefab).AddComponent<LinkedGroup>();
                
                //make net component
              ClientSpawnManager.Instance.CreateNetworkedGameObject(currentLinkedGroup.gameObject);

                //make child of parent to contain our grouped objects
                currentLinkedGroup.parentOfCollection = new GameObject("Linker Parent").transform;

                //chose what group to make 
                if (id == 0)
                {
                    var mat = currentLinkedGroup.GetComponent<MeshRenderer>().material;
                    var color = Color.red;
                    color.a = 0.39f;

                    mat.SetColor("_Color", color);
                    currentLinkedGroup.GetComponent<MeshRenderer>().material = mat;
                }
                else if (id == 1)
                {
                  var mat = currentLinkedGroup.GetComponent<MeshRenderer>().material;
                    var color = Color.blue;
                    color.a = 0.39f;

                    mat.SetColor("_Color", color);
                    currentLinkedGroup.GetComponent<MeshRenderer>().material = mat;
                }
                else if(id == 2)
                {
                    var mat = currentLinkedGroup.GetComponent<MeshRenderer>().material;
                    var color = Color.green;
                    color.a = 0.39f;

                    mat.SetColor("_Color", color);
                    currentLinkedGroup.GetComponent<MeshRenderer>().material = mat;
                }
                else if (id == 3)
                {
                    var mat = currentLinkedGroup.GetComponent<MeshRenderer>().material;
                    var color = Color.grey;
                    color.a = 0.39f;

                    mat.SetColor("_Color", color);
                    currentLinkedGroup.GetComponent<MeshRenderer>().material = mat;
                }

                //add new item to our dictionary and collection
                linkedGroupDictionary.Add(id, currentLinkedGroup);
                currentLinkedGroup.groupListCollection = new List<Collider>();

            }

            //establish what group are we currently working on
                currentLinkedGroup = linkedGroupDictionary[id];

            //check if we are addng a new object to the group
            if (!currentLinkedGroup.groupListCollection.Contains(collider))
                currentLinkedGroup.groupListCollection.Add(collider);
            else
                return;

            //remove our children before we create a new bounding box for our parent containing the group
            currentLinkedGroup.transform.DetachChildren();
            currentLinkedGroup.parentOfCollection.transform.DetachChildren();

            //establish new bounding box
            var newBound = new Bounds(currentLinkedGroup.groupListCollection[0].transform.position, Vector3.one * 0.02f);
            for (int i = 0; i < currentLinkedGroup.groupListCollection.Count; i++)
            {
                var col = currentLinkedGroup.groupListCollection[i];

                //turn it on to get bounds info 
                col.enabled = true;

                //set new collider bounds
                newBound.Encapsulate(new Bounds(col.transform.position, col.bounds.size));

                col.enabled = false;
            }

            //set fields for our new created bounding box
            currentLinkedGroup.transform.position = newBound.center;
            currentLinkedGroup.transform.SetGlobalScale(newBound.size);

            currentLinkedGroup.transform.rotation = Quaternion.identity;
            currentLinkedGroup.parentOfCollection.rotation = Quaternion.identity;

            //recreate our collider to be consistent with the new render bounds
            if (currentLinkedGroup.TryGetComponent(out BoxCollider boxCollider))
                Destroy(boxCollider);

            currentRootCollider = currentLinkedGroup.gameObject.AddComponent<BoxCollider>();

            //add children again
            foreach (var item in currentLinkedGroup.groupListCollection)
                item.transform.SetParent(currentLinkedGroup.parentOfCollection, true);

            //add our collection parent to our bounding box parent
            currentLinkedGroup.parentOfCollection.SetParent(currentLinkedGroup.transform, true);

            //only set our grab object to the group object when it is a different object than its own to avoid the object reparanting itself
            if (LcontrollerInteraction.currentTransform != null)
                if (LcontrollerInteraction.currentTransform.GetInstanceID() == collider.transform.GetInstanceID()) //&& lcontrollerOBJID != currentLinkBoundingBox.transform.GetInstanceID())
                    LcontrollerInteraction.Drop();

            //IF WE ENVELOP THE OBJECT THAT WE ARE GRABBING DROP THAT OBJECT
            if (RcontrollerInteraction.currentTransform != null)
                if (RcontrollerInteraction.currentTransform.GetInstanceID() == collider.transform.GetInstanceID()) //&& RcontrollerInteraction.currentTransform.GetInstanceID() != currentLinkBoundingBox.transform.GetInstanceID())
                    RcontrollerInteraction.Drop();


            //send call if it was a client call
            if (!collider.TryGetComponent<LinkedGroup>(out LinkedGroup lg))
                if (customID == -1)
                    SendNetworkMessage(collider.GetComponent<NetworkedGameObject>().thisEntityID, id, true);

        }
        public void SendNetworkMessage(int eID, int gID, bool isAdding)
        {
            KomodoMessage km = new KomodoMessage("group", JsonUtility.ToJson(new GroupProperties { entityID = eID, groupID = gID, isAdding = isAdding }));
            km.Send();
        }

        private LinkedGroup currentLinkedGroup;
        public void RemoveFromLinkedGroup(Collider collider, int customID = -1)
        {
            if (!collider.CompareTag("Interactable"))
                return;

            //to check if this is a call from the client or external clients
            var id = 0;

            if (customID == -1)
                id = userId;
            else
                id = customID;


            //disable main collider of the linkedgroup to access its child group item colliders
            if (collider.TryGetComponent(out LinkedGroup lg))
            {
                currentLinkedGroup = lg;

                //disable main collider and enable the child elements to remove from main
                lg.transform.GetComponent<BoxCollider>().enabled = false;

                lg.meshRend = lg.GetComponent<MeshRenderer>();

                StartCoroutine(RenableParentColliderOutsideRenderBounds(lg.meshRend));

                foreach (Transform colItems in lg.transform.GetChild(0))
                    colItems.GetComponent<Collider>().enabled = true;

                return;
            }

            if (!linkedGroupDictionary.ContainsKey(id))
                return;

            currentLinkedGroup = linkedGroupDictionary[id];

            //handle the items that we touch
            if (collider.transform.IsChildOf(currentLinkedGroup.parentOfCollection))
            {
                collider.transform.parent = null;
                currentLinkedGroup.groupListCollection.Remove(collider);

                UpdateLinkedGroup(currentLinkedGroup);


                if (currentLinkedGroup.groupListCollection.Count == 0)
                {
                    ////RE-enable our grabing funcionality by droping destroyed object
                    if (LcontrollerInteraction.currentTransform != null)
                        if (LcontrollerInteraction.currentTransform.GetInstanceID() == currentLinkedGroup.transform.GetInstanceID())
                        {
                            LcontrollerInteraction.Drop();

                        }
                    if (RcontrollerInteraction.currentTransform != null)
                        if (RcontrollerInteraction.currentTransform.GetInstanceID() == currentLinkedGroup.transform.GetInstanceID())
                        {
                            RcontrollerInteraction.Drop();

                        }

                    currentLinkedGroup.parentOfCollection.DetachChildren();
                    linkedGroupDictionary.Remove(id);

                    Destroy(currentLinkedGroup.gameObject);
                }
            }

            //send call if it was a client call
            if (!collider.TryGetComponent<LinkedGroup>(out LinkedGroup lgnew))
                if (customID == -1)
                {
                    SendNetworkMessage(collider.GetComponent<NetworkedGameObject>().thisEntityID, id, false);

                }

        }

        //public void RemoveFromLinkedGroup1(Collider collider, int customID = -1)
        //{
        //    if (!collider.CompareTag("Interactable"))
        //        return;

        //    //to check if this is a call from the client or external clients
        //    var id = 0;

        //    if (customID == -1)
        //        id = userId;
        //    else
        //        id = customID;


        //    //disable main collider of the linkedgroup to access its child group item colliders
        //    if (collider.TryGetComponent(out LinkedGroup lg))
        //    {
        //        currentLinkedGroup = lg;

        //        //disable main collider and enable the child elements to remove from main
        //        lg.transform.GetComponent<BoxCollider>().enabled = false;

        //        lg.meshRend = lg.GetComponent<MeshRenderer>();

        //        StartCoroutine(RenableParentColliderOutsideRenderBounds(lg.meshRend));
        //        //   if(lg.transform.childCount > 0)
        //        foreach (Transform colItems in lg.transform.GetChild(0))
        //            colItems.GetComponent<Collider>().enabled = true;

        //        return;
        //    }

        //    if (!linkedGroupDictionary.ContainsKey(id))
        //        return;

        //    currentLinkedGroup = linkedGroupDictionary[id];

        //    //handle the items that we touch
        //    if (currentLinkedGroup)
        //    {
        //        //if (lg.transform.childCount == 0 )
        //        //{
        //        //    Destroy(lg.gameObject);
        //        //    linkedGroups.Remove(id);
        //        //    return;
        //        //}

        //        if (collider.transform.IsChildOf(currentLinkedGroup.transform.GetChild(0)))
        //        {
        //            collider.transform.parent = null;
        //            currentLinkedGroup.groupListCollection.Remove(collider);

        //            currentLinkedGroup.RefreshLinkedGroup();

        //            if (currentLinkedGroup.transform.GetChild(0).childCount == 0)
        //            {
        //                ////RE-enable our grabing funcionality by droping destroyed object
        //                if (LcontrollerInteraction.currentTransform != null)
        //                    if (LcontrollerInteraction.currentTransform.GetInstanceID() == currentLinkedGroup.transform.GetInstanceID())
        //                    {
        //                        LcontrollerInteraction.Drop();

        //                    }
        //                if (RcontrollerInteraction.currentTransform != null)
        //                    if (RcontrollerInteraction.currentTransform.GetInstanceID() == currentLinkedGroup.transform.GetInstanceID())
        //                    {
        //                        RcontrollerInteraction.Drop();

        //                    }


        //                Destroy(currentLinkedGroup.gameObject);
        //                linkedGroupDictionary.Remove(id);


        //            }


        //        }
        //    }

        //    //send call if it was a client call
        //    if (!collider.TryGetComponent<LinkedGroup>(out LinkedGroup lgnew))
        //        if (customID == -1)
        //        {
        //            SendNetworkMessage(collider.GetComponent<NetworkedGameObject>().thisEntityID, id, false);

        //        }

        //    //currentLinkedGroup.transform.GetComponent<BoxCollider>().enabled = true;

        //    //foreach (Transform colItems in lg.transform.GetChild(0))
        //    //    colItems.GetComponent<Collider>().enabled = false;



        //}

        public void ExitedLinkGroup(LinkedGroup lg)
        {
            if (lg)
            {
                lg.transform.GetComponent<BoxCollider>().enabled = true;

                foreach (Transform colItems in lg.transform.GetChild(0))
                    colItems.GetComponent<Collider>().enabled = false;
            }
        }


        public IEnumerator RenableParentColliderOutsideRenderBounds(MeshRenderer lgRend)
        {

           // Debug.Log("i am in");

            yield return new WaitUntil(() => { if (lgRend == null) return true; if (!lgRend.bounds.Contains(LcontrollerInteraction.transform.position) && !lgRend.bounds.Contains(RcontrollerInteraction.transform.position)) return true; else return false; });

            if (lgRend == null)
                yield break;

         //       Debug.Log("i am out");
            lgRend.transform.GetComponent<BoxCollider>().enabled = true;

            foreach (Transform colItems in lgRend.transform.GetChild(0))
                colItems.GetComponent<Collider>().enabled = false;

        }

        public void GroupRefresh(string message)
        {
            GroupProperties data = JsonUtility.FromJson<GroupProperties>(message);

            if (data.isAdding)
                AddToLinkedGroup(ClientSpawnManager.Instance.networkedObjectFromEntityId[data.entityID].GetComponent<Collider>(), data.groupID);
            else
                RemoveFromLinkedGroup(ClientSpawnManager.Instance.networkedObjectFromEntityId[data.entityID].GetComponent<Collider>(), data.groupID);

        }
      
        public void UpdateLinkedGroup(LinkedGroup linkParent)
        {
            List<Transform> childList = new List<Transform>();
            var rootParent = linkParent.transform;
            var parentOfCollection = linkParent.parentOfCollection;


            Bounds newBound = default;
            if (linkParent.groupListCollection.Count != 0)
                newBound = new Bounds(linkParent.groupListCollection[0].transform.position, Vector3.one * 0.02f);// new Bounds();
            else
            {
              

                return;
            }


            for (int i = 0; i < rootParent.GetChild(0).childCount; i++)
            {
                childList.Add(parentOfCollection.GetChild(i));

                var col = parentOfCollection.GetChild(i).GetComponent<Collider>();//capturedObjects[i];//uniqueIdToParentofLinks[currentIDworkingWith].collectedColliders[i];

                //set new collider bounds
                newBound.Encapsulate(new Bounds(col.transform.position, col.bounds.size));

            }

            rootParent.transform.DetachChildren();
            parentOfCollection.transform.DetachChildren();

            rootParent.position = newBound.center;//newLinkParentCollider.transform.position;
            rootParent.SetGlobalScale(newBound.size);

            rootParent.rotation = Quaternion.identity;

            parentOfCollection.rotation = Quaternion.identity;

            if (rootParent.TryGetComponent(out Collider boxCollider))
                Destroy(boxCollider);

            rootParent.gameObject.AddComponent<BoxCollider>();

            foreach (var item in childList)
                item.transform.SetParent(parentOfCollection.transform, true);


            parentOfCollection.transform.SetParent(rootParent.transform, true);

        }
    }
}
