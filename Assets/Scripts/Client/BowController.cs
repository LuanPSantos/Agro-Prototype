using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System;

public class BowController : NetworkBehaviour
{
    public static event Action<float> LaunchForcePercentChanged;
    public float timeToFullLoadLauchForce = 0.1f;
    public float releaseThreshold = 0.01f;
    public int maxLaunchForce = 2000;
    public Transform bowTransform;
    public GameObject arrow;

    private Camera mainCamera;
    private float currentLaunchForcePercent;
    private float timeSpentPulling = 0f;

    void Start()
    {
        mainCamera = Camera.main;
    }

    void Update()
    {
        if (IsOwner)
        {
            Aim();
            Pull();
            Fire();
        }
    }

    // TODO Poderia disparar um evento e executar essa classe em outra, que só tenha coisas de network
    [ServerRpc]
    void FireServerRpc(float force, Vector3 positon, Vector3 direction, Quaternion rotation)
    {
        if (!IsServer || !IsHost) return;

        NetworkLog.LogInfoServer("FireServerRpc");
        GameObject spawnedArrow = Instantiate(arrow, positon, rotation);

        spawnedArrow.GetComponent<NetworkObject>().Spawn();
        spawnedArrow.GetComponent<Rigidbody2D>().AddForce(direction * force);
    }


    private void Aim()
    {
        bowTransform.rotation = GetAimRotation();
    }

    private void Fire()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            FireServerRpc(GetLaunchForce(), GetArrowSpawnPosition(), GetAimDirection(), GetAimRotation());
        }
    }

    private void Pull()
    {
        if (Input.GetKey(KeyCode.Mouse1))
        {
            PullArrow();
        }
        else
        {
            if (currentLaunchForcePercent > releaseThreshold)
            {
                ReleaseArrow();
            }
        }        
    }

    private Quaternion GetAimRotation()
    {
        Vector3 directionToLook = GetAimDirection();

        float angle = Mathf.Atan2(directionToLook.y, directionToLook.x) * Mathf.Rad2Deg;
        return Quaternion.AngleAxis(angle, Vector3.forward);
    }

    private Vector3 GetAimDirection()
    {
        Vector3 mousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        return (mousePosition - bowTransform.position).normalized;
    }

    private float GetLaunchForce()
    {
        return currentLaunchForcePercent * maxLaunchForce;
    }

    private Vector2 GetArrowSpawnPosition()
    {
        return bowTransform.TransformPoint(Vector3.right);
    }

    private void PullArrow()
    {
        NetworkLog.LogInfoServer("PullArrow " + currentLaunchForcePercent);
        timeSpentPulling += Time.deltaTime;

        currentLaunchForcePercent = Mathf.Clamp(timeSpentPulling / timeToFullLoadLauchForce, 0f, 1f);
        LaunchForcePercentChanged?.Invoke(currentLaunchForcePercent);
    }

    private void ReleaseArrow()
    {
        NetworkLog.LogInfoServer("ReleaseArrow " + currentLaunchForcePercent);
        timeSpentPulling -= Time.deltaTime;

        currentLaunchForcePercent = Mathf.Clamp(timeSpentPulling / timeToFullLoadLauchForce, 0f, 1f);
        LaunchForcePercentChanged?.Invoke(currentLaunchForcePercent);
    }
}
