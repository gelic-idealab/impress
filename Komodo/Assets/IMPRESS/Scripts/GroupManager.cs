using Komodo.Utilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Komodo.Runtime;

namespace Komodo.IMPRESS
{
    public class GroupManager : SingletonComponent<GroupManager>
    {
        public static GroupManager Instance
        {
            get { return ((GroupManager)_Instance); }
            set { _Instance = value; }
        }

        public void Awake()
        {
            //used to set our managers alive state to true to detect if it exist within scene
            var initManager = Instance;


            if (GameObject.FindGameObjectWithTag("MenuUI").TryGetComponent(out MainUIIMPRESSReferences uiRef))
            {
                uiRef.groupButton.onFirstClick.AddListener(() => SetAllGroupsToRender());
                uiRef.groupButton.onSecondClick.AddListener(() => SetAllGroupsToNOTRender());
            }
        }

        public List<LinkedGroup> linkedGroups;

        public void SetAllGroupsToRender()
        {
            foreach (var item in linkedGroups)
            {
                item.SetEnable();
            };
        }
        public void SetAllGroupsToNOTRender()
        {
            foreach (var item in linkedGroups)
            {
                item.SetOnDisable();
            };
        }

    }
}
