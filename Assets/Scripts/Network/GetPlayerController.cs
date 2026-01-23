using UnityEngine;

public class GetPlayerController : MonoBehaviour
{
    [SerializeField] private Transform playerController;

    public Transform GetPlayerControllerTransform()
    {
        return playerController;
    }
}
