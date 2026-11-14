
using UnityEngine;

public class DateCatcherController : MonoBehaviour
{
    //[Header("Movement")]
    //public float moveSpeed = 5f;

    //[Header("Movement Limits")]
    //public float minX = -7f;
    //public float maxX = 7f;

    [Header("Animation")]
    public Animator animator;

    private float horizontal;

    void Update()
    {
        // Read A and D keys
        horizontal = 0f;

        if (Input.GetKey(KeyCode.A))
        {
            horizontal = -1f;
        }
        else if (Input.GetKey(KeyCode.D))
        {
            horizontal = 1f;
        }

        // Update Animator
        animator.SetFloat("MoveX", horizontal);

        //// Move character horizontally
        //Vector3 position = transform.position;

        //position.x += horizontal * moveSpeed * Time.deltaTime;

        //position.x = Mathf.Clamp(position.x, minX, maxX);

        //transform.position = position;
    }
}
