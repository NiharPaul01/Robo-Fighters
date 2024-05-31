using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class QuitGame : NetworkBehaviour
{
    [SerializeField] Button QuitGameButton;
    // Start is called before the first frame update
    void Start()
    {
        QuitGameButton.onClick.AddListener(call:() =>
        {
            RequestServerToQuitGameServerRpc();
        });
    }

    [ServerRpc(RequireOwnership =false)]
    void RequestServerToQuitGameServerRpc()
    {
        NetworkManager.Singleton.SceneManager.LoadScene(sceneName: "LoadScene", LoadSceneMode.Single);
    }

    
}
