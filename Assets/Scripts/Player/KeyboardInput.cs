using UnityEngine;

public class KeyboardInput : IPlayerInput
{
    public Vector2 GetMoveDirection()
    {
        return new Vector2(
            Input.GetAxisRaw("Horizontal"),
            Input.GetAxisRaw("Vertical")
            ).normalized;
        // �� ������� � ����� ��� ����� �������� ��������� ���� � �����
    }

}
