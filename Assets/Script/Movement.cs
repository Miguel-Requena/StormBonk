using UnityEngine;

public class Movement : MonoBehaviour
{
    public float speed = 3f;
    private Rigidbody2D rb;
    private Vector2 input;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        input.x = Input.GetAxisRaw("Horizontal");
        input.y = Input.GetAxisRaw("Vertical");
        input.Normalize();
    }
    private void FixedUpdate()
    {
        rb.linearVelocity = input * speed;
    }
}
