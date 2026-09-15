using PlayerControll;
using UnityEngine;

public class ExpOrb : MonoBehaviour
{
    public float expValue = 1;

    [Header("Collect")]
    public float detectRadius = 2.5f;
    public float pullSpeed = 10f;
    public float backOffset = 0.5f;

    [Header("Lifetime")]
    [Min(5f)] public float lifeTime = 45f;

    private Transform player;
    private Vector2 offset;
    private bool isAttracting;
    private bool returnedToPool;
    private float lifeTimer;

    private Vector3 p0, p1, p2;
    private float t;

    private void OnEnable()
    {
        ResetRuntimeState();
        CachePlayer();
    }

    public void Prepare(float value)
    {
        expValue = value;
        ResetRuntimeState();
        CachePlayer();
    }

    private void ResetRuntimeState()
    {
        isAttracting = false;
        returnedToPool = false;
        t = 0f;
        lifeTimer = Mathf.Max(5f, lifeTime);
        p0 = transform.position;
        p1 = transform.position;
        p2 = transform.position;
    }

    private bool CachePlayer()
    {
        if (player != null)
            return true;

        Player instance = Player.Instance;
        if (instance == null)
            return false;

        player = instance.transform;
        offset = instance.Get_Offset();
        return true;
    }

    private void Update()
    {
        if (returnedToPool)
            return;

        if (!CachePlayer())
            return;

        if (isAttracting)
        {
            t += Time.deltaTime * pullSpeed;
            transform.position = Bezier(p0, p1, p2, Mathf.Clamp01(t));

            if (t >= 1f)
                Collect();

            return;
        }

        lifeTimer -= Time.deltaTime;
        if (lifeTimer <= 0f)
        {
            ReturnToPool();
            return;
        }

        Vector3 playerTarget = player.position + new Vector3(0f, offset.y, 0f);
        float dist = Vector2.Distance(transform.position, playerTarget);
        if (dist <= detectRadius)
            StartAttract();
    }

    private void StartAttract()
    {
        if (player == null)
            return;

        isAttracting = true;
        t = 0f;

        offset = Player.Instance != null ? Player.Instance.Get_Offset() : offset;
        Vector3 playerTarget = player.position + new Vector3(0f, offset.y, 0f);
        Vector3 dir = (transform.position - playerTarget).normalized;

        p0 = transform.position;
        p1 = transform.position + dir * backOffset;
        p2 = playerTarget;
    }

    private static Vector3 Bezier(Vector3 a, Vector3 b, Vector3 c, float value)
    {
        Vector3 ab = Vector3.Lerp(a, b, value);
        Vector3 bc = Vector3.Lerp(b, c, value);
        return Vector3.Lerp(ab, bc, value);
    }

    private void Collect()
    {
        if (returnedToPool)
            return;

        Player instance = Player.Instance;
        if (instance != null)
            instance.AddExp(expValue);

        ReturnToPool();
    }

    private void ReturnToPool()
    {
        if (returnedToPool)
            return;

        returnedToPool = true;

        if (ExpOrbPooling.Instance != null)
            ExpOrbPooling.Instance.Return(this);
        else
            gameObject.SetActive(false);
    }
}
