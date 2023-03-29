using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode; 

public class PlayerTagManager : NetworkBehaviour
{
    public NetworkVariable<int> m_TimesTagged = new NetworkVariable<int>(0); 

    public override void OnNetworkSpawn()
    {
        // Set a tagger to this object when no one is tagger
        if (IsServer)
        {
            Debug.Log("The current tagger is: " + TagSystem.m_CurrentHunterID.Value);
            if (TagSystem.m_CurrentHunterID.Value == -1)
            {
                SetHunter(OwnerClientId);
            }
        }
    }

    private void Update()
    {
        // Update the color based on who is tagger 
        if ((int)OwnerClientId == TagSystem.m_CurrentHunterID.Value)
        {
            gameObject.GetComponent<SpriteRenderer>().color = Color.red;
        }
        else
        {
            gameObject.GetComponent<SpriteRenderer>().color = Color.green;
        }
    }


    private void SetHunter(ulong playerID)
    {
        TagSystem.m_CurrentHunterID.Value = (int)playerID;
        m_TimesTagged.Value += 1; 
        Debug.Log("Player " + playerID + " is now it.");
    }

    void OnCollisionEnter(Collision other)
    {
        if (IsServer && other.gameObject.tag == "Player")
        {
            //SetHunter(); 
        }
    }
}
