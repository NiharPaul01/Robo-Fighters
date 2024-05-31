using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

public class KillPlayer : MonoBehaviour
{
    [SerializeField] private Button killPlayerButton;

    public static event Action<ulong> OnKillPlayer;

    
    void Start()
    {
        killPlayerButton.onClick.AddListener(call: () =>
        {
            OnKillPlayer?.Invoke(NetworkManager.Singleton.LocalClientId);
        });
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
