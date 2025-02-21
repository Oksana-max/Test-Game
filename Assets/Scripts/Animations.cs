using UnityEngine;
using System;
using UnityEngine.EventSystems;

public class Animations : MonoBehaviour
{
    public static event Action<GameObject> OnCellChecked; // Событие
    Animator animator;
    Vector3 targetPosition;
    bool isMoving = false; // Флаг движения
    bool movingInX = true;
    GameObject targetCell = null;
    public CreateLevel createLevel;
    Vector3 intermediateTarget;
    private AudioSource audioSource;
    [SerializeField] AudioClip stepSound;

    void Start()
    {
        animator = GetComponent<Animator>();
        // animator.applyRootMotion = false; // Отключаем Root Motion
        audioSource = GetComponent<AudioSource>();
    }


    void Update()
    {
        MovingCharacter();
    }

    private void PlaySoundStep()
    {
        audioSource.PlayOneShot(stepSound, 1f);
    }

    void RotateTowardsTarget(Vector3 target)
    {
        Vector3 direction = (target - transform.position).normalized;
        direction.y = 0;  // Убираем наклон по оси Y
        if (direction != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(direction);
        }
    }

    void MovingCharacter()
    {
        if (createLevel.isPaused)  // Если игра на паузе, блокируем ввод
        {
            return;  // Прекращаем выполнение метода, чтобы персонаж не двигался
        }
        if (!isMoving && EventSystem.current.IsPointerOverGameObject())  // Проверка, был ли клик по UI
        {
            return;  // Прекращаем выполнение метода, если персонаж двигается или клик был по UI
        }
        if (Input.GetMouseButtonDown(0) && !isMoving)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                if (hit.transform.CompareTag("Cell2"))
                {
                    targetCell = hit.transform.gameObject;
                    // Получаем высоту террейна на позиции клика
                    float terrainHeight = Terrain.activeTerrain.SampleHeight(hit.transform.position);
                    targetPosition = new Vector3(
                       Mathf.Round(hit.transform.position.x * 2) / 2, // Округление до 0.5
                      terrainHeight,
                       Mathf.Round(hit.transform.position.z * 2) / 2   // Округление до 0.5
                   );

                    isMoving = true;
                    animator.SetBool("isWalking", true);

                    // Определяем начальное направление движения
                    movingInX = true;
                    intermediateTarget = new Vector3(targetPosition.x, transform.position.y, transform.position.z);
                    // поворот
                    RotateTowardsTarget(intermediateTarget);
                }
            }
        }

        if (isMoving)
        {
            Vector3 currentTarget = movingInX ? intermediateTarget : targetPosition;
            float terrainHeight = Terrain.activeTerrain.SampleHeight(currentTarget);
            currentTarget.y = terrainHeight;
            transform.position = Vector3.MoveTowards(transform.position, currentTarget, 1f * Time.deltaTime);

            if (Vector3.Distance(transform.position, currentTarget) < 0.05f)
            {
                if (movingInX)
                {
                    movingInX = false;
                    intermediateTarget = targetPosition;
                    RotateTowardsTarget(intermediateTarget);  // Поворачиваем к следующей цели
                }
                else
                {
                    isMoving = false;
                    animator.SetBool("isWalking", false);
                    if (targetCell != null)
                    {
                        OnCellChecked?.Invoke(targetCell);
                        targetCell = null;
                       
                    }
                }
            }
        }
    }

}


