using UnityEngine;

public class StairGameObject : MonoBehaviour
{
    [SerializeField] private Transform stairTop;
    [SerializeField] private Transform stairDown;

    public Transform StairTop => stairTop;
    public Transform StairDown => stairDown;
}
