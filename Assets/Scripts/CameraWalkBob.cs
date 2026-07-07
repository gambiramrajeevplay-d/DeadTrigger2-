using UnityEngine;

public class CameraWalkBob : MonoBehaviour
{
    [Header("References")]
    public PlayerAutoMove player;

    [Header("Walk Bob")]
    public float bobSpeed = 8f;
    public float verticalBob = 0.05f;
    public float horizontalBob = 0.03f;

    [Header("Smooth")]
    public float smoothSpeed = 12f;

    float timer;
    Vector3 startPos;

    void Start()
    {
        startPos = transform.localPosition;

        if (player == null)
            player = FindObjectOfType<PlayerAutoMove>();
    }

    void LateUpdate()
    {
        if (player == null)
            return;

        Vector3 targetPos = startPos;

        if (player.IsWalking)
        {
            timer += Time.deltaTime * bobSpeed;

            targetPos.y += Mathf.Sin(timer) * verticalBob;
            targetPos.x += Mathf.Cos(timer * 0.5f) * horizontalBob;
        }
        else
        {
            timer = Mathf.Lerp(timer, 0f, Time.deltaTime * 8f);
        }

        transform.localPosition = Vector3.Lerp(
            transform.localPosition,
            targetPos,
            smoothSpeed * Time.deltaTime);
    }
}