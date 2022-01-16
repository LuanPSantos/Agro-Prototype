using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System;

public class PlayerController : NetworkBehaviour
{
    private NetworkVariable<bool> canPlay = new NetworkVariable<bool>(false);
    private BowController bowController;
    private Collider2D ownCcollider;

    void Start()
    {
        bowController = GetComponent<BowController>();
        ownCcollider = GetComponent<Collider2D>();
    }

    private void Update()
    {
        if (IsLocalPlayer)
        {
            if (canPlay.Value)
            {
                bowController.enabled = true;
                ownCcollider.enabled = false;
            }
            else
            {
                bowController.enabled = false;
                ownCcollider.enabled = true;
            }
        }
    }

    public void SetCanPlay(bool canPlay)
    {
        if (IsServer)
        {
            this.canPlay.Value = canPlay;
        }
    }
}
