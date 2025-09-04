using UnityEngine;

public interface IPlayerInput
{
    Vector2 GetMoveDirection(); // Чтобы было легко менять источник ввода (клава, геймпад или, если игрок должен управляться ИИ)
}
