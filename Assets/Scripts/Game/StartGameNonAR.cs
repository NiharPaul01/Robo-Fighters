using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;

public class StartGameNonAR : NetworkBehaviour
{
    [SerializeField] private Button startHost;
    [SerializeField] private Button startClient;

    // Start is called before the first frame update
    void Start()
    {
        startHost.onClick.AddListener(call: () =>
        {
            NetworkManager.Singleton.StartHost();
        });
        startClient.onClick.AddListener(call: () =>
        {
            NetworkManager.Singleton.StartClient();
        });
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
