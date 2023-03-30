using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class ScoreDisplayManager : NetworkBehaviour
{
    [SerializeField] private Transform m_ScoreDisplayGeneratorPrefab;
    [SerializeField] private Transform m_ScoreLayoutGroupPrefab;
    [SerializeField] private Transform m_ScoreDisplayTextPrefab;

    Transform m_scoreLayoutGroup; 
    Dictionary<int, Transform> m_PlayerIdToScore;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            Transform m_scoreDisplayGenerator = Instantiate(m_ScoreDisplayGeneratorPrefab);
            m_scoreDisplayGenerator.GetComponent<NetworkObject>().Spawn(true);

            m_scoreLayoutGroup = Instantiate(m_ScoreLayoutGroupPrefab);
            m_scoreLayoutGroup.GetComponent<NetworkObject>().Spawn(true);
            m_scoreLayoutGroup.GetComponent<NetworkObject>().TrySetParent(m_scoreDisplayGenerator);

            m_PlayerIdToScore = new Dictionary<int, Transform>(); 
        }
        if (IsClient)
        {
            CreateNewPlayerScoreServerRpc();
        }
    }

    [ServerRpc]
    void CreateNewPlayerScoreServerRpc()
    {
        int newPlayerID = (int)OwnerClientId;
        if (!m_PlayerIdToScore.ContainsKey(newPlayerID))
        {
            Transform ScoreDisplayText = Instantiate(m_ScoreDisplayTextPrefab);
            ScoreDisplayText.GetComponent<NetworkObject>().Spawn(true);
            ScoreDisplayText.GetComponent<NetworkObject>().TrySetParent(m_scoreLayoutGroup);
            m_PlayerIdToScore[newPlayerID] = ScoreDisplayText;
        }
    }
}