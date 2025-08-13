using UnityEngine;

public class Roadkill_CarCollider : MonoBehaviour
{
    public enum CarColliderTag
    {
        Undefined,
        FrontBack,
        Sides,
    }
    public CarColliderTag carColliderTag = CarColliderTag.Undefined;
}