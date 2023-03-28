using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class PlayerSpawnManager : NetworkBehaviour
{
    private Dictionary<ulong, Vector3Int> m_PlayerSpawnPositions = new Dictionary<ulong, Vector3Int>();

    Vector3Int m_SpawnPos = new Vector3Int(0, 0, 0); 

    public override void OnNetworkSpawn()
    {
        if (IsClient)
        {
            StartCoroutine(DelayedActionOnClientSpawn()); 
        }
    }

    private IEnumerator DelayedActionOnClientSpawn()
    {
        yield return new WaitForSeconds(0.1f);
        RequestSpawnPosServerRpc();
        Debug.Log(m_SpawnPos);
        transform.position = m_SpawnPos;
    }

    [ServerRpc(RequireOwnership = false)]
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
        foreach (Vector3Int space in randomSpaces) {
            int maxManhattan = 0;
            foreach (Vector3Int spawnPos in m_PlayerSpawnPositions.Values)
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
