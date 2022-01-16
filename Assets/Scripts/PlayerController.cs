using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System;

public class PlayerController : NetworkBehaviour
{
    private BowController bowController;
    private NetworkVariable<bool> canPlay = new NetworkVariable<bool>(false);
    private Collider2D ownCollider;

    void Start()
    {
        ownCollider = GetComponent<Collider2D>();
        bowController = GetComponent<BowController>();
    }

    private void Update()
    {
        if (IsOwner || IsServer)
        {
            if (canPlay.Value)
            {
                bowController.enabled = true;
                //ownCollider.enabled = false;
            }
            else
            {
                bowController.enabled = false;
                //ownCollider.enabled = true;
            }
        }
    }

    public void SetCanPlay(bool canPlay)
    {
        if (!IsServer) return;

        this.canPlay.Value = canPlay;
    }
}
