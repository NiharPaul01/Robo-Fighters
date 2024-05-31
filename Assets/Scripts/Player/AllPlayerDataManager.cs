using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.Netcode;
using UnityEditor.VersionControl;
using static UnityEditor.PlayerSettings;

public class AllPlayerDataManager : NetworkBehaviour
{


    public static AllPlayerDataManager Instance;
    private NetworkList<PlayerData> allPlayerData;
    private const int LIFEPOINTS = 5;
    private const int LIFEPOINTS_TO_REDUCE = 1;


    public event Action<ulong> OnPlayerDead;
    public event Action<ulong> OnPlayerHealthChanged;

    private void Awake()
    {
        allPlayerData = new NetworkList<PlayerData>();
        if (Instance != null && Instance != this)
        {
            Destroy(Instance);
        }

        Instance = this;
    }

    public void AddPlacedPlayer(ulong id)
    {
        for(int i = 0; i< allPlayerData.Count; i++)
        {
            if (allPlayerData[i].clientID == id)
            {
                PlayerData newData = new PlayerData(
                    allPlayerData[i].clientID,
                    allPlayerData[i].score,
                    allPlayerData[i].lifePoints,
                    playerPlaced:true
                    );


                

                allPlayerData[i] = newData;
            }
        }
    }

    public bool GetHasPLayerPlaced(ulong id)
    {
        for (int i = 0; i < allPlayerData.Count; i++)
        {
            if (allPlayerData[i].clientID == id)
            {
                return allPlayerData[i].playerPlaced;
            }
        }
        return false;
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            AddNewClientToList(NetworkManager.LocalClientId);
        }
    }


    void Start()
    {
        NetworkManager.Singleton.OnClientConnectedCallback += AddNewClientToList;
        BulletData.OnHitPlayer += BulletDataOnOnHitPlayer;
        KillPlayer.OnKillPlayer += KillPlayerOnOnKillPlayer;
        RestartGame.OnRestartGame += RestartGameOnOnRestartGame;
    }


    private void RestartGameOnOnRestartGame()
    {
        if (!IsServer) return;

        List<NetworkObject> playerObjects = FindObjectsOfType<PlayerMovement>().Select(x => x.transform.GetComponent<NetworkObject>()).ToList();
    
        List<NetworkObject> bulletObjects = FindObjectsOfType<BulletData>().Select(x => x.transform.GetComponent<NetworkObject>()).ToList();

        foreach(var playerobj in playerObjects){
            playerobj.Despawn();
        }

        foreach(var bulletObject in bulletObjects)
        {
            bulletObject.Despawn();
        }

        ResetNetworkList();
    }

    void ResetNetworkList()
    {
        for(int i = 0; i < allPlayerData.Count; i++)
        {
            PlayerData resetPlayer = new PlayerData(
                allPlayerData[i].clientID,
                playerPlaced: false,
                lifePoints: LIFEPOINTS,
                score: 0
                );

            allPlayerData[i] = resetPlayer;
        }
    }

    private void KillPlayerOnOnKillPlayer(ulong id)
    {
        (ulong, ulong) fromTO = new(555, id);
        BulletDataOnOnHitPlayer(fromTO);
    }


    public float GetPlayerHealth(ulong id)
    {
        for(int i =0;  i < allPlayerData.Count; i++)
        {
            if (allPlayerData[i].clientID == id)
            {
                return allPlayerData[i].lifePoints;
            }
        }
        return default;
    }

    void BulletDataOnOnHitPlayer((ulong from, ulong to) ids)
    {
        if(IsServer)
        {
            if(ids.from != ids.to)
            {
                for(int i =0; i < allPlayerData.Count; i++)
                {
                    if (allPlayerData[i].clientID == ids.to)
                    {
                        int lifePointsToReduce = allPlayerData[i].lifePoints == 0 ? 0 : LIFEPOINTS_TO_REDUCE;

                        PlayerData newData = new PlayerData(
                            allPlayerData[i].clientID,
                            allPlayerData[i].score,
                            allPlayerData[i].lifePoints - lifePointsToReduce,
                            allPlayerData[i].playerPlaced
                            );

                        
                        if (newData.lifePoints <= 0)
                        {
                            OnPlayerDead?.Invoke(ids.to);
                        }

                       Debug.Log("Hit Player" + ids.from + "to" + ids.to);
                        allPlayerData[i] = newData;
                        break;


                    }
                }
            }
        }
        SyncReducePlayerHealthClientRpc(ids.to);
    }

    [ClientRpc]
    void SyncReducePlayerHealthClientRpc(ulong hitID)
    {
        OnPlayerHealthChanged?.Invoke(hitID);
    }


    void AddNewClientToList(ulong clientId)
    {
        if (!IsServer) return;


  
        foreach(var playerData in allPlayerData)
        {
            if (playerData.clientID == clientId) return;
        }

        PlayerData newPlayerData = new PlayerData();
        newPlayerData.clientID = clientId;
        newPlayerData.score = 0;
        newPlayerData.lifePoints = LIFEPOINTS;
        newPlayerData.playerPlaced = false;



        if(allPlayerData.Contains(newPlayerData)) return;
     

        allPlayerData.Add(newPlayerData);
        PrintAllPlayerPlayerList();

    }


    void PrintAllPlayerPlayerList()
    {
        foreach (var playerData in allPlayerData)
        {
            Debug.Log(message: "Player ID =>" + playerData.clientID + " hasPlaced " + playerData.playerPlaced + "Called by" + NetworkManager.Singleton.LocalClientId);
            

        }
    }

    public void OnDisable()
    {
        if (IsServer)
        {
            allPlayerData.Clear();
            NetworkManager.Singleton.OnClientConnectedCallback -= AddNewClientToList;
        }
        BulletData.OnHitPlayer -= BulletDataOnOnHitPlayer;
        KillPlayer.OnKillPlayer -= KillPlayerOnOnKillPlayer;
        RestartGame.OnRestartGame -= RestartGameOnOnRestartGame;
    }


}