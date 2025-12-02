using UnityEngine;

public class MoverScript : MonoBehaviour
{
    Animator animator;
    Vector3 homePos;
    Vector3 targetPos;
    int specialAction;
    bool isMoving = false;
    AudioSource audioSource;

    // Settings to control special idle animation audio
    [Header("Special Audio Assets")]
    public AudioClip special;
    public float specialVolume = 1.0f;

    // Settings to control chances of random actions
    [Header("Probabilities (per second)")]
    public int numActions = 1;
    public float moveChance = 0.3f;
    public float specialChance = 0.05f;

    // Settings to control motion
    [Header("Movement Settings")]
    public float heightOffset = 0.0f;
    public float roamRadius = 5f;
    public float moveSpeed = 1.5f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
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
            // Play a special idle animation randomly
            if (Random.value < specialChance * Time.deltaTime)
            {
                specialAction = Random.Range(0, numActions);
                animator.SetInteger("specialAction", specialAction);
                animator.SetTrigger("triggerSpecial");
                audioSource.PlayOneShot(special, specialVolume);
            }
            // Move around the map randomly
            else if (Random.value < moveChance * Time.deltaTime)
            {
                PickNewTarget();
            }
        }
        KeepGrounded();
    }
    void PickNewTarget()
    {
        // Pick a random spot to move to
        Vector2 targetCircle = Random.insideUnitCircle * roamRadius;
        targetPos = homePos + new Vector3(targetCircle.x, 0, targetCircle.y);
        isMoving = true;
    }

    void MoveToTarget()
    {
        Vector3 direction = targetPos - transform.position;
        direction.y = 0;

        // Check for obstacles in front
        Ray forwardRay = new Ray(transform.position + Vector3.up * 0.5f, transform.forward);
        if (Physics.Raycast(forwardRay, 1.0f))
        {
            isMoving = false;
            animator.SetBool("isMoving", false);

            // Rotate to random new direction within 120 degrees of current direction
            float randomTurn = Random.Range(-60f, 60f);
            transform.rotation = Quaternion.Euler(0f, transform.eulerAngles.y + randomTurn, 0f);
            return;
        }

        // While far from target, move!
        if (direction.magnitude > 0.2f)
        {
            transform.forward = Vector3.Lerp(transform.forward, direction.normalized, Time.deltaTime * 4.0f);
            animator.SetBool("isMoving", true);
            transform.position += direction.normalized * moveSpeed * Time.deltaTime;
        }
        else
        {
            isMoving = false;
            animator.SetBool("isMoving", false);
        }
        transform.position = new Vector3(transform.position.x, homePos.y, transform.position.z);
    }

    // Helper function to keep animals on the ground
    void KeepGrounded()
    {
        Ray ray = new Ray(transform.position + Vector3.up * 2f, Vector3.down);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 10f))
        {
            Vector3 pos = transform.position;
            pos.y = hit.point.y + heightOffset;
            transform.position = pos;
        }
    }
}
