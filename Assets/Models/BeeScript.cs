using UnityEngine;

public class BeeScript : MonoBehaviour
{
    Animator animator;
    Vector3 homePos;
    Vector3 targetPos;
    int specialAction;
    bool isMoving = false;

    [Header("Probabilities (per second)")]
    public float moveChance = 0.3f;
    public float specialChance = 0.05f;

    [Header("Movement Settings")]
    public float roamRadius = 5f;
    public float moveSpeed = 1.5f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        animator = GetComponent<Animator>();
        homePos = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        if (isMoving)
        {
            MoveToTarget();
        }
        else
        {
            if (Random.value < specialChance * Time.deltaTime)
            {
                specialAction = Random.Range(0, 2);
                animator.SetInteger("specialAction", specialAction);
                animator.SetTrigger("triggerSpecial");
            }
            else if (Random.value < moveChance * Time.deltaTime)
            {
                PickNextTarget();
            }
        }
        animator.SetBool("isMoving", isMoving);
    }
    void PickNextTarget()
    {
        Vector2 targetCircle = Random.insideUnitCircle * roamRadius;
        targetPos = homePos + new Vector3(targetCircle.x, 0, targetCircle.y);
        isMoving = true;
    }

    void MoveToTarget()
    {
        Vector3 direction = targetPos - transform.position;
        direction.y = 0;

        if (direction.magnitude > 0.2f)
        {
            transform.forward = Vector3.Lerp(transform.forward, direction.normalized, Time.deltaTime * 2f);
            transform.position += direction.normalized * moveSpeed * Time.deltaTime;
        }
        else
        {
            isMoving = false;
        }

        //float y = Terrain.activeTerrain.SampleHeight(transform.position);
        transform.position = new Vector3(transform.position.x, homePos.y, transform.position.z);
    }
}
