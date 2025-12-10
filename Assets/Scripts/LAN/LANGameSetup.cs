using UnityEngine;
using Mirror;
using UnityEngine.SceneManagement;

public class LANGameSetup : NetworkManager
{
    public static LANGameSetup Instance { get; private set; }
    private static bool _initialized = false;

    public override void Awake()
    {
        base.Awake();
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }
    }

    public override void OnStartClient()
    {
        gameObject.SetActive(true);   // THIS LINE IS THE HOLY GRAIL

        base.OnStartClient();

        if (_initialized) return;
        _initialized = true;

        if (MenuLANButtons.ShouldHost)
            StartHost();
        else
        {
            StartClient();
            Invoke(nameof(ClientTimeout), 5f);
        }
    }

    private void ClientTimeout()
    {
        if (mode == NetworkManagerMode.ClientOnly && !NetworkClient.isConnected)
        {
            StopClient();
            SceneManager.LoadScene("Menu");
        }
    }

    public override void OnClientConnect()
    {
        base.OnClientConnect();
        CancelInvoke(nameof(ClientTimeout));
    }

    public void StopAndReturnToMenu()
    {
        _initialized = false;
        if (mode == NetworkManagerMode.Host) StopHost();
        else if (mode == NetworkManagerMode.ClientOnly) StopClient();
        SceneManager.LoadScene("Menu");
    }

    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        base.OnServerAddPlayer(conn);

        bool isWhite = numPlayers == 1;
        GameObject playerObj = Instantiate(playerPrefab);
        var pc = playerObj.GetComponent<PlayerController>();
        pc.isWhiteSide = isWhite;
        pc.playerColorName = isWhite ? "white" : "black";

        NetworkServer.AddPlayerForConnection(conn, playerObj);
    }
}