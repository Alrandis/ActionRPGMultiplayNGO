//using UnityEngine;
//using Unity.Netcode;
//using System;

//public class PlayerShooting : NetworkBehaviour
//{
//    public GameObject projectilePrefab;
//    public Transform firePoint;
//    [SerializeField] private Transform spriteHolder; // нужен для направления спрайта


//    // Update is called once per frame
//    void Update()
//    {
//        if (!IsOwner) return;

//        if (Input.GetKeyDown(KeyCode.Space))
//        {
//            Vector2 dir = spriteHolder.right;
//            ShootServerRpc(firePoint.position, dir);
//        }
//    }

//    [ServerRpc]
//    private void ShootServerRpc(Vector2 position, Vector2 dir)
//    {
//        var proj = Instantiate(projectilePrefab, position, Quaternion.FromToRotation(Vector3.right, dir));
//        proj.GetComponent<Projectile>().Init(dir);
//        proj.GetComponent<NetworkObject>().Spawn();
//    }
//}
