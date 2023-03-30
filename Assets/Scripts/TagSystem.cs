using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode; 

public class TagSystem : NetworkBehaviour
{
    //public static NetworkVariable<int> m_OldHunterID = new NetworkVariable<int>(-1);
    public NetworkVariable<int> m_CurrentHunterID = new NetworkVariable<int>(-1);
}
