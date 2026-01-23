using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    [SerializeField] private Button createRoomButton;
    [SerializeField] private Button joinRandomButton;
    [SerializeField] private TMP_Text statusText;

    private void Start()
    {
        createRoomButton.onClick.AddListener(CreateRoom);
        joinRandomButton.onClick.AddListener(JoinRandomRoom);

        UpdateStatus("Готов к подключению");
    }

    private void CreateRoom()
    {
        UpdateStatus("Создаем комнату...");

        // Опции комнаты
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = NetworkManager.Instance.MaxPlayersPerRoom;
        roomOptions.IsVisible = true;
        roomOptions.IsOpen = true;

        // Создаем комнату с уникальным именем
        string roomName = "Room_" + Random.Range(1000, 9999);
        PhotonNetwork.CreateRoom(roomName, roomOptions);
    }

    private void JoinRandomRoom()
    {
        UpdateStatus("Ищем случайную комнату...");
        PhotonNetwork.JoinRandomRoom();
    }

    private void UpdateStatus(string message)
    {
        statusText.text = $"[{System.DateTime.Now:HH:mm:ss}] {message}";
        Debug.Log(message);
    }
}
