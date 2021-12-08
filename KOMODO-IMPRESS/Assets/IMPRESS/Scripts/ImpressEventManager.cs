using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Komodo.IMPRESS
{
    public static class ImpressEventManager
    {
        // public void Start()
        // {
        //     TODO (Brandon): Restore ability to add spot light and sun light.
        //      Examples: 
        //      public Alternate_Button_Function sunPointLightButton;
        //      public Alternate_Button_Function spotLightModelButton;
        //      sunPointLightButton.onFirstClick.AddListener(() =>
        //      {
        //             LightsManager.Instance.sunPointLightModel.SetActive(true);
        //      });

        //      LightsManager.Instance.spotLightModel.SetActive(true);
        // }

        // From Unity Learn Tutorial: 
        // Create a Simple Messaging System with Events 
        // Tutorial Last Updated June 3rd, 2019
        // "As an introduction to UnityActions and UnityEvents, 
        // we will create a simple messaging system which will 
        // allow items in our projects to subscribe to events, 
        // and have events trigger actions in our games. This 
        // will reduce dependencies and allow easier maintenance 
        // of our projects."

        private static Dictionary <string, UnityEvent> eventDictionary;

        // Call this function to create a new dictionary.
        // 
        public static void StartListening (string eventName, UnityAction listener)
        {
            if (eventDictionary == null)
            {
                Debug.Log("ImpressEventManager: new dictionary created.");

                eventDictionary = new Dictionary<string, UnityEvent>();
            }

            if (eventDictionary.TryGetValue(eventName, out UnityEvent existingEvent))
            {
                existingEvent.AddListener(listener);
            }
            else
            {
                UnityEvent newEvent = new UnityEvent();

                newEvent.AddListener(listener);

                eventDictionary.Add(eventName, newEvent);
            }
        }

        public static void StopListening (string eventName, UnityAction listener)
        {
            if (eventDictionary == null)
            {
                Debug.LogWarning("Tried to remove event, but eventDictionary was null. Proceeding anyways.");

                return;
            }

            if (eventDictionary.TryGetValue(eventName, out UnityEvent existingEvent))
            {
                existingEvent.RemoveListener(listener);
            }
        }

        public static void TriggerEvent (string eventName)
        {
            if (eventDictionary.TryGetValue(eventName, out UnityEvent existingEvent))
            {
                existingEvent.Invoke();
            }
        }
    }
}
