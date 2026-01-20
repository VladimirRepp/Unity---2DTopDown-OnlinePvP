using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;

public class FloorSystem : MonoBehaviour
{
    [Header("General Settings")]
    [SerializeField] private float floorChangeDelay = 0.1f; // задержка при смене этажа

    [Header("Floor Collider Settings")]
    // note: может быть не удобно, если будут подвалы (то есть этажи ниже 1)
    [SerializeField] private TilemapCollider2D[] floorColliders; // коллайдеры этажей

    [Header("Player Settings")]
    [SerializeField] private SpriteRenderer playerSprite; // для сортировки спрайта игрока

    [Header("Debug Settings")]
    [SerializeField] private bool isViewDebug = false;

    private int _currentFloor = 1;
    private const string STAIR_TAG = "Stair";
    private Coroutine _setDelayedFloorCoroutine;

    private Transform _stairTopPoint;
    private Transform _stairDownPoint;

    private Rigidbody2D _rb;

    public int CurrentFloor => _currentFloor;

    private void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        InitializeFloors();
    }

    private void InitializeFloors()
    {
        // По умолчанию игрок на 1 этаже
        SetFloorColliders(2);

        if (playerSprite != null)
        {
            playerSprite.sortingLayerName = "Player";
            playerSprite.sortingOrder = 0; // Выше пола, но ниже некоторых объектов
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent(out StairGameObject stair))
        {
            _stairTopPoint = stair.StairTop;
            _stairDownPoint = stair.StairDown;


            if (_currentFloor == 1)
            {
                ClimbToFloor2();
            }
            else if (_currentFloor == 2)
            {
                DescendToFloor1();
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag(STAIR_TAG))
        {
            _stairTopPoint = null;
            _stairDownPoint = null;
        }
    }

    private void ClimbToFloor2()
    {
        _currentFloor = 2;

        if (_setDelayedFloorCoroutine != null)
            StopCoroutine(_setDelayedFloorCoroutine);
        _setDelayedFloorCoroutine = StartCoroutine(SetDelayedFloorCollidersCoroutine(2, floorChangeDelay));

        // Меняем порядок отрисовки спрайта игрока
        if (playerSprite != null)
        {
            playerSprite.sortingOrder = 10; // Игрок выше объектов 2 этажа
        }

        _rb.MovePosition(_stairTopPoint.position);

        if (isViewDebug)
            Debug.Log("Поднялись на 2 этаж");
    }

    private void DescendToFloor1()
    {
        _currentFloor = 1;
        if (_setDelayedFloorCoroutine != null)
            StopCoroutine(_setDelayedFloorCoroutine);
        _setDelayedFloorCoroutine = StartCoroutine(SetDelayedFloorCollidersCoroutine(1, floorChangeDelay));

        // Возвращаем порядок отрисовки
        if (playerSprite != null)
        {
            playerSprite.sortingOrder = 0; // Игрок ниже объектов 2 этажа
        }

        _rb.MovePosition(_stairDownPoint.position);

        if (isViewDebug)
            Debug.Log("Спустились на 1 этаж");
    }

    private IEnumerator SetDelayedFloorCollidersCoroutine(int floorNumber, float delay)
    {
        yield return new WaitForSeconds(delay);
        SetFloorColliders(floorNumber);
    }

    private void SetFloorColliders(int floorNumber)
    {
        for (int i = 0; i < floorColliders.Length; i++)
        {
            if (floorColliders[i] != null)
            {
                floorColliders[i].enabled = (i + 1) == floorNumber;
            }
        }
    }
}
