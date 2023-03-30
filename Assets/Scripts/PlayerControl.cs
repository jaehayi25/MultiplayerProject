using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class PlayerControl : NetworkBehaviour
{
    Vector2 m_MoveDir = new Vector2(0, 0);
    Vector2 m_OldMoveDir = new Vector2(0, 0);
    float m_MoveSpeed = 20;

    Rigidbody2D m_Rigidbody2D;

    private NetworkVariable<float> m_InvisibilityBuffTimer = new NetworkVariable<float>(0f);
    private NetworkVariable<float> m_InvisibilityBuffCD = new NetworkVariable<float>(0f);

    private NetworkVariable<int> m_BlockBreakLimit = new NetworkVariable<int>(5);

    private Vector2 m_FacingDir = new Vector2(0, -1);
    private RaycastHit2D m_OneUnitHit;

    private MazeGenerator m_MazeGenerator; 

    void Awake()
    {
        m_Rigidbody2D = GetComponent<Rigidbody2D>();
        m_MazeGenerator = FindObjectOfType<MazeGenerator>();
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

        HandleHunterIndicator();
        HandleBuffs();
    }

    void UpdateServer()
    {
        m_Rigidbody2D.velocity = m_MoveDir * m_MoveSpeed;
    }

    void UpdateClient()
    {
        if (!IsLocalPlayer)
        {
            return; 
        }

        //UpdateLocalPlayer
        Vector2 moveDir = new Vector2(0, 0);

        if (Input.GetKey(KeyCode.W))
        {
            moveDir.y = +1f;
        }
        if (Input.GetKey(KeyCode.S))
        {
            moveDir.y = -1f;
        }
        if (Input.GetKey(KeyCode.A))
        {
            moveDir.x = -1f;
        }
        if (Input.GetKey(KeyCode.D))
        {
            moveDir.x = +1f;
        }

        //Set facing direction
        if (moveDir != new Vector2(0, 0))
        {
            m_FacingDir = moveDir;
        }

        if (Input.GetKeyDown(KeyCode.B))
        {
            HandleBreakingBlockServerRpc(m_FacingDir); 
        }

        if (Input.GetKeyDown(KeyCode.I) && NetworkManager.ServerTime.TimeAsFloat > m_InvisibilityBuffCD.Value)
        {
            ToggleInvisilibityServerRpc(); 
        }

        if (m_OldMoveDir != moveDir)
        {
            MoveServerRpc(moveDir);
            m_OldMoveDir = moveDir; 
        }
    }

    [ServerRpc]
    private void HandleBreakingBlockServerRpc(Vector2 facingDir)
    {
        Vector2 hitPos = (Vector2)transform.position + facingDir;
        if (m_MazeGenerator.IsWallAtWorldPos(hitPos) && m_BlockBreakLimit.Value > 0)
        {
            m_MazeGenerator.RemoveWallAtWordPos(hitPos);
            if (!m_MazeGenerator.IsWallAtWorldPos(hitPos)) m_BlockBreakLimit.Value -= 1;
        }
    }

    private void HandleHunterIndicator()
    {
        if (gameObject.GetComponent<PlayerTagManager>().IsHunter())
        {
            gameObject.GetComponent<SpriteRenderer>().color = Color.red;
        }
        else
        {
            gameObject.GetComponent<SpriteRenderer>().color = Color.green;
        }
    }

    private void HandleBuffs()
    {
        if (m_InvisibilityBuffTimer.Value > NetworkManager.ServerTime.Time)
        {
            if (IsLocalPlayer)
            {
                Color temp = gameObject.GetComponent<SpriteRenderer>().color;
                temp.a = 0.2f; 
                gameObject.GetComponent<SpriteRenderer>().color = temp;
            }
            else
            {
                Color temp = gameObject.GetComponent<SpriteRenderer>().color;
                temp.a = 0f;
                gameObject.GetComponent<SpriteRenderer>().color = temp; 
            }
        }
    }

    [ServerRpc]
    private void ToggleInvisilibityServerRpc()
    {
        m_InvisibilityBuffTimer.Value = NetworkManager.ServerTime.TimeAsFloat + 5f;
        m_InvisibilityBuffCD.Value = NetworkManager.ServerTime.TimeAsFloat + 20f;
        Debug.Log("Invisibility on.");
    }

    [ServerRpc]
    private void MoveServerRpc(Vector2 moveDir)
    {
        m_MoveDir = moveDir; 
    }
}
