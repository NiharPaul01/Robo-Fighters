using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;

public class PlaceCharacter : NetworkBehaviour
{
    // Start is called before the first frame update
     
    [SerializeField] private GameObject placementObject;


    private Camera mainCam;



    private void Start()
    {
         mainCam = GameObject.FindObjectOfType<Camera>();
    }
    // Update is called once per frame
    void Update()
    {
        if (AllPlayerDataManager.Instance != default &&
            AllPlayerDataManager.Instance.GetHasPLayerPlaced(NetworkManager.Singleton.LocalClientId)) return;


#if UNITY_EDITOR
        if(Input.GetMouseButtonDown(0))
    {
            if (EventSystem.current.IsPointerOverGameObject())
            {
                Debug.Log(message: "UI Hit was recognized");
                return;
            }
            TouchToRay(Input.mousePosition);
        }

#endif
#if UNITY_IOS || UNITY_ANDROID
        if(Input.touchCount > 0 && Input.touchCount < 2 && 
            Input.GetTouch(0).phase == TouchPhase.Began)
        {
            Touch touch = Input.GetTouch(0);
            PointerEventData pointerData = new PointerEventData(EventSystem.current);
            pointerData.position = touch.position;

            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerData, results);

            if(results.Count > 0)
            {
                Debug.Log(message: "We hit an UI Element");
                return;
            }

            Debug.Log(message: "Touch detected, fingerId : " + touch.fingerId);

            if(EventSystem.current.IsPointerOverGameObject(touch.fingerId))
            {
                Debug.Log(message: "Is Pointer Over GOJ, No Placement");
                return;
            }

            TouchToRay(touch.position);


        }

#endif
    }

    void TouchToRay(Vector3 touch)
    {
        Ray ray = mainCam.ScreenPointToRay(touch);
        RaycastHit hit;

        if(Physics.Raycast(ray, out hit))
        {

            Quaternion rotation = Quaternion.FromToRotation(Vector3.up, hit.normal);
            SpawnPlayerServerRpc(hit.point, rotation, NetworkManager.Singleton.LocalClientId);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void SpawnPlayerServerRpc(Vector3 positon, Quaternion rotation, ulong callerID)
    {
        ulong localClientId = NetworkManager.Singleton.LocalClientId;
        GameObject character = Instantiate(placementObject, positon, rotation);

        NetworkObject characterNetworkObject = character.GetComponent<NetworkObject>();

        characterNetworkObject.SpawnWithOwnership(callerID);

        AllPlayerDataManager.Instance.AddPlacedPlayer(callerID);

    }
}
