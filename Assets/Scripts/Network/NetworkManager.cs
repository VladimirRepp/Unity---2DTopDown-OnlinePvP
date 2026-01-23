using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkManager : MonoBehaviourPunCallbacks // специальный базовый класс от Photon, дает доступ к callback-методам (сетевым событиям).
{
    public static NetworkManager Instance { get; private set; }

    [Header("Settings")]
    [SerializeField] private string gameVersion = "1.0";
    [SerializeField] private byte maxPlayersPerRoom = 4;
    [SerializeField] private string sceneName = "GameScene_NetworkTest"; // имя вашей игровой сцены 

    [Header("Prefabs")]
    [SerializeField] private GameObject playerPrefab;

    public byte MaxPlayersPerRoom => maxPlayersPerRoom;

    private void Awake()
    {
        // Реализация паттерна Singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject); // Менеджер сохраняется между сценами
    }

    public override void OnEnable()
    {
        base.OnEnable();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void Start()
    {
        // Автоматически загружаем уровень для всех игроков в комнате
        PhotonNetwork.AutomaticallySyncScene = true;

        // Подключаемся только если не подключены
        if (!PhotonNetwork.IsConnected)
        {
            ConnectToPhoton();
        }
    }

    public override void OnDisable()
    {
        base.OnDisable();
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Спавним игрока в игровой сцене
        if (scene.buildIndex == SceneManager.GetSceneByName(sceneName).buildIndex)
        {
            SpawnPlayer();
        }
    }

    private void SpawnPlayer()
    {
        if (playerPrefab == null)
        {
            Debug.LogError("Не назначен префаб игрока в NetworkManager!");
            return;
        }

        // Получаем случайную точку спавна
        Transform spawnPoint = SpawnManager.Instance?.GetRandomSpawnPoint();
        Vector3 position = spawnPoint != null ? spawnPoint.position : Vector3.zero;
        Quaternion rotation = spawnPoint != null ? spawnPoint.rotation : Quaternion.identity;

        // Создаем сетевого игрока
        GameObject myPlayer = PhotonNetwork.Instantiate(
            playerPrefab.name,
            Vector3.zero,
            Quaternion.identity
        );

        Transform controllerTransform = myPlayer.GetComponent<GetPlayerController>()?.GetPlayerControllerTransform();
        if (controllerTransform != null)
        {
            controllerTransform.position = position;
            controllerTransform.rotation = rotation;
        }

        Debug.Log($"Игрок создан: {myPlayer.name}. Владелец: {myPlayer.GetPhotonView().Owner.NickName}");
    }

    public void ConnectToPhoton()
    {
        Debug.Log("Подключились к Photon Cloud...");

        // Отключаем повторное подключение на время установки соединения
        // PhotonNetwork.ReconnectAndRejoin = false;

        // Настраиваем игрока
        PhotonNetwork.NickName = "Player_" + Random.Range(1000, 9999);

        // Устанавливаем версию игры
        PhotonNetwork.GameVersion = gameVersion;

        // Подключаемся к мастер-серверу Photon
        PhotonNetwork.ConnectUsingSettings();
    }

    private void LoadGameScene()
    {
        PhotonNetwork.LoadLevel(sceneName);
    }

    #region === PUN CALLBACKS ===

    // CALLBACK: Успешное подключение к мастер-серверу
    public override void OnConnectedToMaster()
    {
        Debug.Log($"Подключились к мастер-серверу. Игрок: {PhotonNetwork.LocalPlayer.NickName}");

        // Можно присоединиться к лобби здесь, но мы сделаем это по кнопке
    }

    // CALLBACK: Успешное создание комнаты
    public override void OnCreatedRoom()
    {
        Debug.Log($"Комната создана: {PhotonNetwork.CurrentRoom.Name}");
    }

    // CALLBACK: Успешное присоединение к комнате
    public override void OnJoinedRoom()
    {
        Debug.Log($"Присоединились к комнате: {PhotonNetwork.CurrentRoom.Name}");
        Debug.Log($"Игроков в комнате: {PhotonNetwork.CurrentRoom.PlayerCount}");

        // Загружаем игровую сцену, если мы мастер-клиент
        if (PhotonNetwork.IsMasterClient)
        {
            LoadGameScene();
        }

        // Спавним игрока ТОЛЬКО если мы в игровой сцене
        if (SceneManager.GetActiveScene().name == sceneName)
        {
            SpawnPlayer();
        }
    }

    // CALLBACK: Когда мастер-клиент загрузил сцену и она синхронизирована
    // Используем SceneManager.sceneLoaded вместо устаревшего OnLevelWasLoaded


    // CALLBACK: При присоединении другого игрока
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log($"Новый игрок присоединился: {newPlayer.NickName}");
        Debug.Log($"Теперь игроков: {PhotonNetwork.CurrentRoom.PlayerCount}");
    }

    // CALLBACK: При ошибке присоединения к случайной комнате
    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log($"Не удалось найти комнату: {message}. Создаем новую...");

        // Автоматически создаем комнату если нет доступных
        RoomOptions options = new RoomOptions();
        options.MaxPlayers = maxPlayersPerRoom;
        PhotonNetwork.CreateRoom(null, options); // null = случайное имя
    }

    #endregion
}