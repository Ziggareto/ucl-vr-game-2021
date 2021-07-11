using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Gun object is child of player object
public class NaniteShooter : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] KeyCode shootKey = KeyCode.N;
    [SerializeField] KeyCode switchProjectile = KeyCode.M;

    [Header("Aiming")]
    [SerializeField] float cooldown = 0.3f;

    [Header("Projectiles")]
    [SerializeField] int currentProjectile = 1;
    [SerializeField] Projectile[] projectilePrefabs = null;
    //[SerializeField] Projectile projectilePrefab = null;

    [Header("Misc")]
    [SerializeField] public int segmentBank;
    [SerializeField] public bool noActiveCloud;

    float cooldownCounter;

    private void Start()
    {
        cooldownCounter = 0;
        segmentBank = 999999999;
        noActiveCloud = false;
    }

    private void Update()
    {
        cooldownCounter += Time.deltaTime;

        Switch();

        if (cooldownCounter >= cooldown && Input.GetKeyDown(shootKey))
        {
            if (currentProjectile != 0 || segmentBank >= 49)
            {
                Shoot();
                if (currentProjectile == 0)
                {
                    cooldownCounter = -10f;
                }
                else
                {
                    cooldownCounter = 0;
                }
            }

            //cooldownCounter = 0;
        }
    }

    void Shoot()
    {
        GameObject projectileObj = Instantiate(projectilePrefabs[currentProjectile].gameObject,
            transform.position + 0.5f * transform.forward, Quaternion.identity) as GameObject;

        Projectile projectile = projectileObj.GetComponent<Projectile>();

        projectile.transform.forward = transform.forward;
        projectile._trajectory = transform.forward;
    }

    void Switch()
    {
        if (Input.GetKeyDown(switchProjectile))
        {
            currentProjectile++;
            currentProjectile = currentProjectile % projectilePrefabs.Length;
        }
    }

    void AddSegments(int segmentNumber)
    {
        segmentBank += segmentNumber;
    }
}
