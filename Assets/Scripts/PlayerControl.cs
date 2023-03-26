using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class PlayerControl : NetworkBehaviour
{
    Vector2 m_MoveDir = new Vector2(0, 0);
    Vector2 m_OldMoveDir = new Vector2(0, 0);
    float m_MoveSpeed = 3f;

    Rigidbody2D m_Rigidbody2D;

    void Awake()
    {
        m_Rigidbody2D = GetComponent<Rigidbody2D>(); 
    }

    void Update()
    {
        if (IsServer)
        {
            UpdateServer();
        }

        if (IsClient)
        {
            UpdateClient();
        }
    }

    void UpdateServer()
    {
        m_Rigidbody2D.position += m_MoveDir * m_MoveSpeed * Time.deltaTime;
    }

    void UpdateClient()
    {
        if (!IsLocalPlayer)
        {
            return; 
        }

        //UpdateLocalPlayer
        Vector2 moveDir = new Vector3(0, 0);

        if (Input.GetKey(KeyCode.W)) moveDir.y = +1f;
        if (Input.GetKey(KeyCode.S)) moveDir.y = -1f;
        if (Input.GetKey(KeyCode.A)) moveDir.x = -1f;
        if (Input.GetKey(KeyCode.D)) moveDir.x = +1f;

        if (m_OldMoveDir != moveDir)
        {
            MoveServerRpc(moveDir);
            m_OldMoveDir = moveDir; 
        }
    }
    
    [ServerRpc]
    private void MoveServerRpc(Vector3 moveDir)
    {
        m_MoveDir = moveDir; 
    }
}
