//using UnityEngine;
//using Unity.Netcode;
//using System.Linq;
//using Unity.Collections;
//using System;
//using static UnityEngine.GraphicsBuffer;
//using UnityEngine.UIElements;

//public class NPCController : NetworkBehaviour
//{
//    [SerializeField] private float speed = 1f;
//    private Vector2 direction = Vector2.right;

//    private Transform _target;

//    [SerializeField] private GameObject projectilePrefab;
//    public Transform firePoint;

//    private float shootCooldown = 2f;
//    private float shootTimer = 0f;

//    private NPCLook _npcLook;

   
//    public override void OnNetworkSpawn()
//    {

//        _npcLook = GetComponentInChildren<NPCLook>();

//    }



//    // Update is called once per frame
//    void Update()
//    {
//        if (!IsServer) return;

//        FindClosestPlayer();
//        if (_target != null)
//        {
//            _npcLook.SetTarget(_target);
//        }
//        else
//        {
//            _npcLook.ClearTarget();
//            Patrol();
//        }
        
//        TryShootAtClosestPlayer();
//    }

//    void Patrol()
//    {
//        transform.Translate(direction * speed * Time.deltaTime);

//        if (transform.position.x > 3f)
//            direction = Vector2.left;
//        else if (transform.position.x < -3f)
//            direction = Vector2.right;
//    }
//    void FindClosestPlayer()
//    {
//        NetworkObject[] allObjects = NetworkManager.Singleton.SpawnManager.SpawnedObjectsList.ToArray();
//        Transform closest = null;
//        float closestDist = float.MaxValue;

//        foreach (var obj in allObjects)
//        {
//            if (obj.TryGetComponent(out NetworkBehaviour nb) && obj.CompareTag("Player"))
//            {
//                float dist = Vector2.Distance(transform.position, obj.transform.position);
//                if (dist < 5f && dist < closestDist)
//                {
//                    closestDist = dist;
//                    closest = obj.transform;
//                }
//            }
//        }

//        _target = closest;
//    }

//    void TryShootAtClosestPlayer()
//    {
//        if (_target == null) return;

//        shootTimer -= Time.deltaTime;
//        if (shootTimer > 0f) return;

//        Vector2 dir = _npcLook.GetLookDirection();
//        if (dir != Vector2.zero)
//        {
//            var proj = Instantiate(projectilePrefab, firePoint.position, Quaternion.FromToRotation(Vector3.right, dir));
           

//            proj.GetComponent<Projectile>().Init(dir);
//            proj.GetComponent<NetworkObject>().Spawn();
//            shootTimer = shootCooldown;
//        }
//    }
//}
