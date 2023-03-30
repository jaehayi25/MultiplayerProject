using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class PlayerSpawnManager : NetworkBehaviour
{
    private bool m_TeleportToSpawn = false;
    private Vector3Int m_SpawnPos; 

    public override void OnNetworkSpawn()
    {
        if (IsLocalPlayer)
        {
            m_SpawnPos = new Vector3Int(0, 0, 0);
            StartCoroutine(DelayedActionOnClientSpawn());
        }
    }

    private void Update()
    {
        if (IsServer)
        {
            UpdateServer(); 
        }
    }

    void UpdateServer()
    {
        if (m_TeleportToSpawn)
        {
            transform.position = m_SpawnPos;
            //Debug.Log("teleporting player to " + m_SpawnPos);
            m_TeleportToSpawn = false;
        }
    }

    private IEnumerator DelayedActionOnClientSpawn()
    {
        yield return new WaitForSeconds(0.1f);
        if (m_SpawnPos == new Vector3Int(0, 0, 0))
        {
            SpawnServerRpc();
        }
    }
    
    public void ResetPositionAtSpawn()
    {
        if (IsServer)
        {
            m_TeleportToSpawn = true; 
        }
    }

    [ServerRpc]
    private void SpawnServerRpc(ServerRpcParams serverRpcParams = default)
    {
        var clientId = serverRpcParams.Receive.SenderClientId;
        m_SpawnPos = GetFurthestRandomPointFromSpawn(10);
        m_TeleportToSpawn = true; 
        //Debug.Log("new spawn position " + m_SpawnPos + " added for client with ID: " + clientId);
    }

    private Vector3Int GetFurthestRandomPointFromSpawn(int n)
    {
        MazeGenerator mazeGen = FindObjectOfType<MazeGenerator>();
        System.Random rand = new System.Random();
        int randomIndex = rand.Next(mazeGen.m_EmptySpaces.Count);

        Vector3Int best = mazeGen.m_EmptySpaces[randomIndex];
        /*
        System.Random rand = new System.Random();
        MazeGenerator mazeGen = FindObjectOfType<MazeGenerator>();
        List<Vector3Int> randomSpaces = new List<Vector3Int>();

        // pick n points from empty spaces
        while (randomSpaces.Count < n)
        {
            // Get a random index from the vectorList
            int randomIndex = rand.Next(mazeGen.m_EmptySpaces.Count);

            // If the random element hasn't already been added to the list, add it
            if (!randomSpaces.Contains(mazeGen.m_EmptySpaces[randomIndex]))
            {
                randomSpaces.Add(mazeGen.m_EmptySpaces[randomIndex]);
            }
        }

        int minDist = -1;
        Vector3Int best = new Vector3Int(2, 2, 0);
        foreach (Vector3Int space in randomSpaces)
        {
            int maxManhattan = 0;
            foreach (Vector3Int spawnPos in m_PlayerSpawnPositions)
            {
                Debug.Log(spawnPos);
                int manhattan = Mathf.Abs(spawnPos.y - space.y) + Mathf.Abs(spawnPos.x - space.x);
                maxManhattan = Mathf.Max(maxManhattan, manhattan);
            }
            if (minDist == -1 || maxManhattan < minDist)
            {
                minDist = maxManhattan;
                best = space;
            }
        }
        */

        return best;
    }

    /*
    private Dictionary<ulong, Vector3Int> m_PlayerSpawnPositions = new Dictionary<ulong, Vector3Int>();

    Vector3Int m_SpawnPos = new Vector3Int(0, 0, 0);

    public override void OnNetworkSpawn()
    {
        if (IsLocalPlayer)
        {
            StartCoroutine(DelayedActionOnClientSpawn());
        }
    }

    private IEnumerator DelayedActionOnClientSpawn()
    {
        yield return new WaitForSeconds(0.1f);
        RequestSpawnPosServerRpc();
        transform.position = m_SpawnPos;
    }

    [ServerRpc]
    private void RequestSpawnPosServerRpc(ServerRpcParams serverRpcParams = default)
    {
        var clientId = serverRpcParams.Receive.SenderClientId;
        if (NetworkManager.ConnectedClients.ContainsKey(clientId))
        {
            var client = NetworkManager.ConnectedClients[clientId];
            if (!m_PlayerSpawnPositions.ContainsKey(clientId))
            {
                Vector3Int bestSpawnPoint = GetFurthestRandomPointFromSpawn(10);

                m_PlayerSpawnPositions[clientId] = bestSpawnPoint;
                //Get 10 empty spots from maze
                //Choose the spot furthest from all other spots
                Debug.Log("new spawn position " + m_PlayerSpawnPositions[clientId] + " added for client with ID: " + clientId);
            }
            m_SpawnPos = m_PlayerSpawnPositions[clientId];
        }
    }

    private Vector3Int GetFurthestRandomPointFromSpawn(int n)
    {
        System.Random rand = new System.Random();
        MazeGenerator mazeGen = FindObjectOfType<MazeGenerator>();
        List<Vector3Int> randomSpaces = new List<Vector3Int>();
        // pick n points from empty spaces
        while (randomSpaces.Count < n)
        {
            // Get a random index from the vectorList
            int randomIndex = rand.Next(mazeGen.m_EmptySpaces.Count);

            // If the random element hasn't already been added to the list, add it
            if (!randomSpaces.Contains(mazeGen.m_EmptySpaces[randomIndex]))
            {
                randomSpaces.Add(mazeGen.m_EmptySpaces[randomIndex]);
            }
        }

        int minDist = -1;
        Vector3Int best = new Vector3Int(2, 2, 0);
        foreach (Vector3Int space in randomSpaces)
        {
            int maxManhattan = 0;
            foreach (Vector3Int spawnPos in m_PlayerSpawnPositions.Values)
            {
                Debug.Log(spawnPos); 
                int manhattan = Mathf.Abs(spawnPos.y - space.y) + Mathf.Abs(spawnPos.x - space.x);
                maxManhattan = Mathf.Max(maxManhattan, manhattan);
            }
            if (minDist == -1 || maxManhattan < minDist)
            {
                minDist = maxManhattan;
                best = space;
            }
        }

        return best;
    }
    */

    /*
    NetworkVariable<Dictionary<ulong, Vector3Int>> m_PlayerSpawnPositions = new NetworkVariable<Dictionary<ulong, Vector3Int>>(new Dictionary<ulong, Vector3Int>());

    Vector3Int m_SpawnPos = new Vector3Int(0, 0, 0);

    public override void OnNetworkSpawn()
    {
        if (IsClient && IsOwner)
        {
            StartCoroutine(DelayedActionOnClientSpawn());
        }
    }
    private IEnumerator DelayedActionOnClientSpawn()
    {
        yield return new WaitForSeconds(0.1f);
        transform.position = GetSpawnPos(OwnerClientId);
    }

    private IEnumerator DelayedActionOnClientSpawn()
    {
        yield return new WaitForSeconds(0.1f);
        RequestSpawnPosServerRpc();
        transform.position = m_PlayerSpawnPositions[OwnerClientId];
        Debug.Log("Transform.position is set to: " + transform.position);
    }

    [ServerRpc]
    private void RequestSpawnPosServerRpc(ServerRpcParams serverRpcParams = default)
    {
        var clientId = serverRpcParams.Receive.SenderClientId;
        if (NetworkManager.ConnectedClients.ContainsKey(clientId))
        {
            var client = NetworkManager.ConnectedClients[clientId];
            if (!m_PlayerSpawnPositions.ContainsKey(clientId))
            {
                Vector3Int bestSpawnPoint = GetFurthestRandomPointFromSpawn(10);

                m_PlayerSpawnPositions[clientId] = bestSpawnPoint;
                //Get 10 empty spots from maze
                //Choose the spot furthest from all other spots
                Debug.Log("new spawn position added for client with ID: " + clientId);
            }
            m_SpawnPos = m_PlayerSpawnPositions[clientId];
        }
    }
    */


    /*
    NetworkVariable< Dictionary<ulong, Vector3Int> > m_PlayerSpawnPositions = new NetworkVariable< Dictionary<ulong, Vector3Int> >(new Dictionary<ulong, Vector3Int>());

    public override void OnNetworkSpawn()
    {
        if (IsClient && IsOwner)
        {
            StartCoroutine(DelayedActionOnClientSpawn()); 
        }
    }
    private IEnumerator DelayedActionOnClientSpawn()
    {
        yield return new WaitForSeconds(0.1f);
        transform.position = GetSpawnPos(OwnerClientId);
    }

    private Vector3Int GetSpawnPos(ulong clientID)
    {
        if (NetworkManager.ConnectedClients.ContainsKey(clientID))
        {
            if (!m_PlayerSpawnPositions.Value.ContainsKey(clientID))
            {
                Vector3Int bestSpawnPoint = GetFurthestRandomPointFromSpawn(10, m_PlayerSpawnPositions.Value);

                m_PlayerSpawnPositions.Value[clientID] = bestSpawnPoint;
                //Get 10 empty spots from maze
                //Choose the spot furthest from all other spots
                Debug.Log("new spawn position " + m_PlayerSpawnPositions.Value[clientID] + " added for client with ID: " + clientID);
            }
        }
        return m_PlayerSpawnPositions.Value[clientID]; 
    }
    */

    /*
    private IEnumerator DelayedActionOnClientSpawn()
    {
        yield return new WaitForSeconds(0.1f);
        RequestSpawnPosServerRpc();
        transform.position = m_PlayerSpawnPositions[OwnerClientId];
        Debug.Log("Transform.position is set to: " + transform.position);
    }

    [ServerRpc]
    private void RequestSpawnPosServerRpc(ServerRpcParams serverRpcParams = default)
    {
        var clientId = serverRpcParams.Receive.SenderClientId;
        if (NetworkManager.ConnectedClients.ContainsKey(clientId))
        {
            var client = NetworkManager.ConnectedClients[clientId];
            if (!m_PlayerSpawnPositions.ContainsKey(clientId))
            {
                Vector3Int bestSpawnPoint = GetFurthestRandomPointFromSpawn(10); 

                m_PlayerSpawnPositions[clientId] = bestSpawnPoint;
                //Get 10 empty spots from maze
                //Choose the spot furthest from all other spots
                Debug.Log("new spawn position added for client with ID: " + clientId);
            }
            m_SpawnPos = m_PlayerSpawnPositions[clientId];
        }
    }
    */

    private Vector3Int GetFurthestRandomPointFromSpawn(int n, Dictionary<ulong, Vector3Int> possibleSpawnPositions)
    {
        System.Random rand = new System.Random();
        MazeGenerator mazeGen = FindObjectOfType<MazeGenerator>();
        List<Vector3Int> randomSpaces = new List<Vector3Int>();
        // pick n points from empty spaces
        while (randomSpaces.Count < n)
        {
            // Get a random index from the vectorList
            int randomIndex = rand.Next(mazeGen.m_EmptySpaces.Count);

            // If the random element hasn't already been added to the list, add it
            if (!randomSpaces.Contains(mazeGen.m_EmptySpaces[randomIndex]))
            {
                randomSpaces.Add(mazeGen.m_EmptySpaces[randomIndex]);
            }
        }

        int minDist = -1; 
        Vector3Int best = new Vector3Int(2, 2, 0);
        foreach (Vector3Int space in randomSpaces) {
            int maxManhattan = 0;
            foreach (Vector3Int spawnPos in possibleSpawnPositions.Values)
            {
                int manhattan = Mathf.Abs(spawnPos.y - space.y) + Mathf.Abs(spawnPos.x - space.x);
                maxManhattan = Mathf.Max(maxManhattan, manhattan);
            }
            if (minDist == -1 || maxManhattan < minDist)
            {
                minDist = maxManhattan;
                best = space; 
            }
        }

        return best; 
    }
}
