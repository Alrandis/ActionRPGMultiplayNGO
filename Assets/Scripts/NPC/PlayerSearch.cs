using UnityEngine;


public class PlayerSearch : MonoBehaviour
{
    private GameObject _closestPlayer = null;
    [SerializeField] private float detectionRadius = 8f; // ������ ���������� �����

    public Transform FindClosestPlayer()
    {
        _closestPlayer = null;
        float closestDist = float.MaxValue;

        foreach (var player in GameObject.FindGameObjectsWithTag("Player"))
        {
            float dist = Vector2.Distance(transform.position, player.transform.position);
            if (dist < closestDist)
            {
                closestDist = dist;
                _closestPlayer = player;
            }
        }

        // ���������, ���������� �� ������
        if (_closestPlayer != null && closestDist <= detectionRadius)
        {
            return _closestPlayer.transform;
        }
        else
        {
            return null; // ������ ����� ���
        }
    }

}
