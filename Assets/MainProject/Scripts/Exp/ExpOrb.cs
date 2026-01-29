using PlayerControll;
using UnityEngine;

public class ExpOrb : MonoBehaviour
{
    public float expValue = 1;

    public float detectRadius = 2.5f;
    public float pullSpeed = 10f;
    public float backOffset = 0.5f;

    private Transform player;
    private Vector2 offset;
    private bool isAttracting;

    Vector3 p0, p1, p2;
    float t;

    private void Start()
    {
        player = Player.Instance.transform;
        offset = Player.Instance.Get_Offset();
    }

    private void Update()
    {
        if (isAttracting)
        {
            t += Time.deltaTime * pullSpeed;
            transform.position = Bezier(p0, p1, p2, t);

            if (t >= 1f)
            {
                Collect();
            }
            return;
        }

        float dist = Vector2.Distance(transform.position, player.position+ new Vector3(0, offset.y, 0));
        if (dist <= detectRadius)
        {
            StartAttract();
        }
    }

    private void StartAttract()
    {
        isAttracting = true;
        t = 0;

        Vector3 dir = (transform.position - player.position + new Vector3(0, offset.y, 0)).normalized;

        p0 = transform.position;
        p1 = transform.position + dir * backOffset;
        p2 = player.position + new Vector3(0, offset.y, 0);
    }

    Vector3 Bezier(Vector3 a, Vector3 b, Vector3 c, float t)
    {
        Vector3 ab = Vector3.Lerp(a, b, t);
        Vector3 bc = Vector3.Lerp(b, c, t);
        return Vector3.Lerp(ab, bc, t);
    }

    private void Collect()
    {
        Player.Instance.AddExp(expValue);
        ExpOrbPooling.Instance.Return(this);
    }
}
