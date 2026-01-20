using UnityEngine;

public class GarlicAuraVisual : MonoBehaviour
{
    public SpriteRenderer sr;
    private SkillController owner;
    private float radius;

    public void Init(SkillController owner, float radius)
    {
        this.owner = owner;
        SetRadius(radius);
    }

    public void SetRadius(float r)
    {
        radius = r;
        transform.localScale = Vector3.one * radius * 2f;
    }

    private void LateUpdate()
    {
        if (owner != null)
            transform.position = owner.transform.position;
    }
}
