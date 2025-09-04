using UnityEngine;
using Unity.Netcode;

public abstract class PowerUp : NetworkBehaviour
{
    protected const string _playerTag = "Player";

    public abstract void UpplyPowerUp(GameObject player);

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!IsServer || !other.gameObject.CompareTag(_playerTag)) return;

        UpplyPowerUp(other.gameObject);

        CollectPowerUp();

    }

    protected void CollectPowerUp()
    {
        NetworkObject netObj = GetComponent<NetworkObject>();
        if (netObj != null && netObj.IsSpawned)
        {
            netObj.Despawn();
        }
    }

}
