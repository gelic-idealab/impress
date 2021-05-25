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
      //  public GameObject currentLinkBoundingBox;
        public Dictionary<int, LinkedGroup> linkedGroups = new Dictionary<int, LinkedGroup>();
    //    public int currentIDworkingWith;
     //   public Transform linkCollectionParent;

        //LinkedGroup linkedGroup;
        BoxCollider currentRootCollider;

        public KomodoControllerInteraction LcontrollerInteraction;
        public KomodoControllerInteraction RcontrollerInteraction;

        //public MeshRenderer currentMeshRenderer;
        public void Awake()
        {
            //used to set our managers alive state to true to detect if it exist within scene
            var initManager = Instance;


            if (GameObject.FindGameObjectWithTag("MenuUI").TryGetComponent(out MainUIIMPRESSReferences uiRef))
            {
                uiRef.groupButton.onFirstClick.AddListener(() => SetAllGroupsToRender());
                uiRef.groupButton.onSecondClick.AddListener(() => SetAllGroupsToNOTRender());
            }

            GlobalMessageManager.Instance.Subscribe("group", (str) => GroupRefresh(str));
        }
        //public IEnumerator Start()
        //{
        //    var go = CreatePrimitiveManager.Instance.CreatePrimitive().GetComponent<NetworkedGameObject>();

        //    var go2 = CreatePrimitiveManager.Instance.CreatePrimitive().GetComponent<NetworkedGameObject>();


        //    AddToLinkedGroup(go.GetComponent<Collider>(),1);
        //    AddToLinkedGroup(go2.GetComponent<Collider>(), 1);

        //}



        public void SetAllGroupsToRender()
        {
            foreach (var item in linkedGroups.Values)
            {
                item.GetComponent<MeshRenderer>().enabled = true; //SetEnable();
            };
        }
        public void SetAllGroupsToNOTRender()
        {
            foreach (var item in linkedGroups.Values)
            {
                //item.SetOnDisable();

                item.transform.GetComponent<BoxCollider>().enabled = true;

                foreach (Transform colItems in item.transform.GetChild(0))
                    colItems.GetComponent<BoxCollider>().enabled = false;

                item.GetComponent<MeshRenderer>().enabled = false;

              //  currentLinkedGroup = null;

            };
        }

        public int userId;
        
        public void WorkingGroupID(int id)
        {
            this.userId = id;

        }


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

         
            List<int> linkedGroupsToRemove = new List<int>();
            
            //we do not want to add the constructing boxes to itself
            foreach (KeyValuePair<int,LinkedGroup> linkGroup in linkedGroups)
            {
                if (linkGroup.Value == null)
                {
                    linkedGroupsToRemove.Add(linkGroup.Key);
                    continue;
                }

                if (linkGroup.Value.gameObject.GetInstanceID() == collider.gameObject.GetInstanceID())
                    return;
            }

            foreach (var item in linkedGroupsToRemove)
            {
                linkedGroups.Remove(item);
            }
            //two parents to avoid weird scaling visual isue with just parent children scalling, so we do parent scaling, on another parent that has children
            //if (linkCollectionParent == null)
            //    linkCollectionParent = new GameObject("Linker Parent").transform;


            if (!linkedGroups.ContainsKey(id))//currentLinkBoundingBox == null)
            {
                currentLinkedGroup = GameObject.Instantiate(linkBoundingPrefab).AddComponent<LinkedGroup>();
                
                //make net component
              ClientSpawnManager.Instance.CreateNetworkedGameObject(currentLinkedGroup.gameObject);


                currentLinkedGroup.parentOfCollection = new GameObject("Linker Parent").transform;

                //make different groups
                if (id == 1)
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


                linkedGroups.Add(id, currentLinkedGroup);

                currentLinkedGroup.groupListCollection = new List<Collider>();//Dictionary<Transform parent, List<Collider> collectedColliders>();


            }

                currentLinkedGroup = linkedGroups[id]; //currentLinkBoundingBox.GetComponent<LinkedGroup>();


            if (!currentLinkedGroup.groupListCollection.Contains(collider))
                currentLinkedGroup.groupListCollection.Add(collider);
        

            currentLinkedGroup.transform.DetachChildren();
            currentLinkedGroup.parentOfCollection.transform.DetachChildren();

            //if it is not our parent collider we shut of its own collider
            var newBound = new Bounds(currentLinkedGroup.groupListCollection[0].transform.position, Vector3.one * 0.02f);
            for (int i = 0; i < currentLinkedGroup.groupListCollection.Count; i++)
            {
                var col = currentLinkedGroup.groupListCollection[i];

                //turn it on to get bounds info 
                col.enabled = true;

                //set new collider bounds
                newBound.Encapsulate(new Bounds(col.transform.position, col.bounds.size));

                col.enabled = false;
                //   Debug.Log(col.gameObject.name + " " + newBound.size, col.gameObject);
            }


            currentLinkedGroup.transform.position = newBound.center;//newLinkParentCollider.transform.position;
            currentLinkedGroup.transform.SetGlobalScale(newBound.size);

            currentLinkedGroup.transform.rotation = Quaternion.identity;
            currentLinkedGroup.parentOfCollection.rotation = Quaternion.identity;

            if (currentLinkedGroup.TryGetComponent(out BoxCollider boxCollider))
                Destroy(boxCollider);

            currentRootCollider = currentLinkedGroup.gameObject.AddComponent<BoxCollider>();


            //////This changed
            //currentLinkedGroup.parentOfCollection.rotation = Quaternion.identity;
            //currentLinkedGroup.parentOfCollection.SetParent(currentLinkedGroup.transform, true);

            //foreach (var item in currentLinkedGroup.groupListCollection)
            //    item.transform.SetParent(currentLinkedGroup.parentOfCollection, true);


            foreach (var item in currentLinkedGroup.groupListCollection)
                item.transform.SetParent(currentLinkedGroup.parentOfCollection, true);


            currentLinkedGroup.parentOfCollection.SetParent(currentLinkedGroup.transform, true);

            /////

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
             //   if(lg.transform.childCount > 0)
                foreach (Transform colItems in lg.transform.GetChild(0))
                    colItems.GetComponent<Collider>().enabled = true;

                return;
            }

            if (!linkedGroups.ContainsKey(id))
                return;

            currentLinkedGroup = linkedGroups[id];

            //handle the items that we touch
            if (currentLinkedGroup)
            {
                //if (lg.transform.childCount == 0 )
                //{
                //    Destroy(lg.gameObject);
                //    linkedGroups.Remove(id);
                //    return;
                //}

                if (collider.transform.IsChildOf(currentLinkedGroup.transform.GetChild(0)))
                {
                    collider.transform.parent = null;
                    currentLinkedGroup.groupListCollection.Remove(collider);

                    currentLinkedGroup.RefreshLinkedGroup();

                    if (currentLinkedGroup.transform.GetChild(0).childCount == 0)
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


                        Destroy(currentLinkedGroup.gameObject);
                        linkedGroups.Remove(id);

                        
                    }


                }
            }

            //send call if it was a client call
            if (!collider.TryGetComponent<LinkedGroup>(out LinkedGroup lgnew))
                if (customID == -1)
                {
                    SendNetworkMessage(collider.GetComponent<NetworkedGameObject>().thisEntityID, id, false);

                }

            //currentLinkedGroup.transform.GetComponent<BoxCollider>().enabled = true;

            //foreach (Transform colItems in lg.transform.GetChild(0))
            //    colItems.GetComponent<Collider>().enabled = false;



        }

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

            Debug.Log("i am in");

            yield return new WaitUntil(() => { if (lgRend == null) return true; if (!lgRend.bounds.Contains(LcontrollerInteraction.transform.position) && !lgRend.bounds.Contains(RcontrollerInteraction.transform.position)) return true; else return false; });

            if (lgRend == null)
                yield break;

                Debug.Log("i am out");
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

    }
}
