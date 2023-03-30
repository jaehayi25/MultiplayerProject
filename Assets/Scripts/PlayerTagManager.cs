using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using TMPro;

public class PlayerTagManager : NetworkBehaviour
{
    public NetworkVariable<int> m_TimesTagged = new NetworkVariable<int>(0);

    private TagSystem m_TagSystem;

    public bool Taggable { get; private set; }

    public override void OnNetworkSpawn()
    {
        m_TagSystem = FindObjectOfType<TagSystem>();

        if (IsServer)
        {
            Taggable = true;
        } 
    }

    private void Update()
    {
        // Set a tagger to this object when no one is tagger
        if (IsServer && m_TagSystem.m_CurrentHunterID.Value == -1)
        {
            OnBeingTagged();
        }
        /*
        // Update the color based on who is tagger 
        if (IsHunter())
        {
            gameObject.GetComponent<SpriteRenderer>().color = Color.red;
        }
        else
        {
            gameObject.GetComponent<SpriteRenderer>().color = Color.green;
        }
        */
    }

    public bool IsHunter()
    {
        return (int)OwnerClientId == m_TagSystem.m_CurrentHunterID.Value; 
    }

    public void SetHunter(ulong playerID)
    {
        m_TagSystem.m_CurrentHunterID.Value = (int)playerID;
        m_TimesTagged.Value += 1; 
        Debug.Log("Player " + m_TagSystem.m_CurrentHunterID.Value + " is now it.");
    }

    public void OnBeingTagged()
    {
        SetHunter(OwnerClientId);
    }
    
    void OnCollisionEnter2D(Collision2D other)
    {
        if (IsServer)
        {
            if (IsHunter() && other.gameObject.tag == "Player")
            {
                PlayerTagManager otherPlayerTagManager = other.gameObject.GetComponent<PlayerTagManager>();
                if (otherPlayerTagManager.Taggable) {
                    otherPlayerTagManager.OnBeingTagged();

                    //Reset positions of collided players
                    StartCoroutine(EnableTagImmunity());
                    gameObject.GetComponent<PlayerSpawnManager>().ResetPositionAtSpawn();
                    other.gameObject.GetComponent<PlayerSpawnManager>().ResetPositionAtSpawn();
                }
            }
        }
    }

    private IEnumerator EnableTagImmunity()
    {
        if (IsServer)
        {
            Taggable = false;
            yield return new WaitForSeconds(0.1f);
            Taggable = true;
        }
    }
}
