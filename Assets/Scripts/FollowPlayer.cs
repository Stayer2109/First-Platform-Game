using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowPlayer : MonoBehaviour
{
    public Vector3 offset = new Vector3(0f, 2.5f, -10f);
    public float smoothTime = 0.12f;
    private Vector3 velocity = Vector3.zero;

    [Header("Axis Limitations")]
    public Vector2 xLimit = new Vector2(0f, 0f);
    public Vector2 yLimit = new Vector2(0f, 0f);

    [SerializeField] private Transform player;

    // Update is called once per frame
    void Update()
    {
        Vector3 targetPosition = player.position + offset;
        targetPosition = new Vector3(
            Mathf.Clamp(targetPosition.x, xLimit.x, xLimit.y),
            Mathf.Clamp(targetPosition.y, yLimit.x, yLimit.y),
            -10
        );
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);
    }
}
