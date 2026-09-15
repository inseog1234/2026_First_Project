using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CHAINLASER : ActiveSkill
{
    private readonly ChainLaserSkillData laserData;
    private readonly List<Enemy> chainTargets = new(16);

    private static Material sharedLineMaterial;

    public CHAINLASER(SkillData data, SkillController owner) : base(data, owner)
    {
        laserData = data as ChainLaserSkillData;
    }

    protected override void Cast()
    {
        if (laserData == null)
        {
            Debug.LogError("[CHAINLASER] SkillData must be ChainLaserSkillData.");
            return;
        }

        float range = GetLaserRange();
        Enemy firstTarget = FindNearestEnemy(owner.transform.position, range, null);
        if (firstTarget == null) return;

        int maxTargets = Mathf.Max(1, GetFinalProjectileCount() + owner.GlobalStats.projectileBonus);
        float jumpRange = Mathf.Max(1.5f, range * laserData.jumpRangeMultiplier);

        chainTargets.Clear();
        chainTargets.Add(firstTarget);

        Vector2 searchOrigin = GetTargetPosition(firstTarget);
        while (chainTargets.Count < maxTargets)
        {
            Enemy next = FindNearestEnemy(searchOrigin, jumpRange, chainTargets);
            if (next == null) break;

            chainTargets.Add(next);
            searchOrigin = GetTargetPosition(next);
        }

        owner.StartCoroutine(FireChain(new List<Enemy>(chainTargets)));
    }

    private IEnumerator FireChain(List<Enemy> targets)
    {
        Vector3 previousPosition = owner.transform.position + (Vector3)owner.offset;
        float baseDamage = GetFinalDamage();
        float delay = Mathf.Max(0.015f, GetFinalProjectileDelay());

        for (int i = 0; i < targets.Count; i++)
        {
            Enemy target = targets[i];
            if (target == null || !target.gameObject.activeInHierarchy || target.isDead)
                continue;

            Vector3 targetPosition = GetTargetPosition(target);
            float multiplier = Mathf.Max(
                laserData.minimumDamageMultiplier,
                1f - laserData.damageFalloffPerJump * i
            );
            float damage = baseDamage * multiplier;

            target.TakeDamage(damage);
            AddDamage(damage);

            float knockback = GetFinalKnockback();
            if (knockback > 0f && !target.isDead)
            {
                Vector2 knockbackDirection = ((Vector2)targetPosition - (Vector2)previousPosition).normalized;
                target.Knockback(knockbackDirection, knockback);
            }

            owner.StartCoroutine(LaserArcRoutine(previousPosition, targetPosition, i));
            for (int pulse = 0; pulse < laserData.impactPulseCount; pulse++)
                owner.StartCoroutine(ImpactPulseRoutine(targetPosition, i, pulse));

            previousPosition = targetPosition;

            if (i < targets.Count - 1)
                yield return new WaitForSeconds(delay);
        }
    }

    private Enemy FindNearestEnemy(Vector2 origin, float range, List<Enemy> excluded)
    {
        float bestDistanceSqr = range * range;
        Enemy best = null;
        List<Enemy> enemies = EnemyManager.ActiveEnemies;

        for (int i = 0; i < enemies.Count; i++)
        {
            Enemy enemy = enemies[i];
            if (enemy == null || enemy.isDead || !enemy.gameObject.activeInHierarchy)
                continue;
            if (excluded != null && excluded.Contains(enemy))
                continue;

            Vector2 targetPosition = GetTargetPosition(enemy);
            float distanceSqr = (targetPosition - origin).sqrMagnitude;
            if (distanceSqr > bestDistanceSqr)
                continue;

            bestDistanceSqr = distanceSqr;
            best = enemy;
        }

        return best;
    }

    private IEnumerator LaserArcRoutine(Vector3 start, Vector3 end, int linkIndex)
    {
        GameObject root = new GameObject($"ChainLaser_Arc_{linkIndex}");
        LineRenderer glow = root.AddComponent<LineRenderer>();

        GameObject coreObject = new GameObject("Core");
        coreObject.transform.SetParent(root.transform, false);
        LineRenderer core = coreObject.AddComponent<LineRenderer>();

        ConfigureLine(glow, 190 + linkIndex * 2);
        ConfigureLine(core, 191 + linkIndex * 2);

        int pointCount = Mathf.Max(3, laserData.jaggedPointCount);
        glow.positionCount = pointCount;
        core.positionCount = pointCount;

        float duration = Mathf.Max(0.08f, GetFinalLifetime());
        float elapsed = 0f;
        float seed = Time.time * 13.37f + linkIndex * 7.11f;
        float widthScale = Mathf.Max(0.25f, currentStat.scale);
        float baseWidth = laserData.lineWidth * widthScale;
        float visualSpeed = Mathf.Max(1f, GetFinalSpeed());

        while (elapsed < duration)
        {
            float normalized = Mathf.Clamp01(elapsed / duration);
            float fade = 1f - normalized;
            float pulse = 0.86f + 0.14f * Mathf.Sin((elapsed * visualSpeed + seed) * 18f);

            UpdateArcPoints(glow, core, start, end, seed + elapsed * visualSpeed * 2.5f);

            glow.startWidth = glow.endWidth = baseWidth * laserData.glowWidthMultiplier * pulse;
            core.startWidth = core.endWidth = baseWidth * pulse;

            Color glowColor = laserData.glowColor;
            glowColor.a *= fade;
            Color coreColor = laserData.coreColor;
            coreColor.a *= Mathf.Clamp01(fade * 1.25f);

            glow.startColor = glow.endColor = glowColor;
            core.startColor = core.endColor = coreColor;

            elapsed += Time.deltaTime;
            yield return null;
        }

        Object.Destroy(root);
    }

    private IEnumerator ImpactPulseRoutine(Vector3 position, int linkIndex, int pulseIndex)
    {
        if (pulseIndex > 0)
            yield return new WaitForSeconds(0.025f * pulseIndex);

        GameObject root = new GameObject($"ChainLaser_Impact_{linkIndex}_{pulseIndex}");
        LineRenderer ring = root.AddComponent<LineRenderer>();
        ConfigureLine(ring, 210 + linkIndex);
        ring.loop = true;

        const int segments = 20;
        ring.positionCount = segments;

        float duration = Mathf.Max(0.09f, GetFinalLifetime() * 0.75f);
        float elapsed = 0f;
        float widthScale = Mathf.Max(0.25f, currentStat.scale);
        float maxRadius = 0.28f + 0.1f * widthScale + pulseIndex * 0.07f;

        while (elapsed < duration)
        {
            float normalized = Mathf.Clamp01(elapsed / duration);
            float radius = Mathf.Lerp(0.08f, maxRadius, normalized);
            float alpha = 1f - normalized;

            for (int i = 0; i < segments; i++)
            {
                float angle = i / (float)segments * Mathf.PI * 2f;
                ring.SetPosition(i, position + new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * radius);
            }

            ring.startWidth = ring.endWidth = laserData.lineWidth * widthScale * Mathf.Lerp(1.8f, 0.35f, normalized);
            Color color = Color.Lerp(laserData.coreColor, laserData.glowColor, normalized);
            color.a *= alpha;
            ring.startColor = ring.endColor = color;

            elapsed += Time.deltaTime;
            yield return null;
        }

        Object.Destroy(root);
    }

    private void UpdateArcPoints(LineRenderer glow, LineRenderer core, Vector3 start, Vector3 end, float phase)
    {
        int count = glow.positionCount;
        Vector2 direction = (Vector2)(end - start);
        Vector2 perpendicular = direction.sqrMagnitude > 0.0001f
            ? new Vector2(-direction.y, direction.x).normalized
            : Vector2.up;

        float distance = direction.magnitude;
        float jitter = laserData.jitterAmount * Mathf.Clamp(distance / 3f, 0.55f, 1.35f);

        for (int i = 0; i < count; i++)
        {
            float t = i / (float)(count - 1);
            Vector3 point = Vector3.Lerp(start, end, t);

            if (i != 0 && i != count - 1)
            {
                float envelope = Mathf.Sin(Mathf.PI * t);
                float noise = Mathf.Sin(phase * 7.13f + i * 12.9898f)
                            + Mathf.Sin(phase * 11.71f + i * 4.1414f);
                point += (Vector3)(perpendicular * (noise * 0.5f * jitter * envelope));
            }

            glow.SetPosition(i, point);
            core.SetPosition(i, point);
        }
    }

    private static void ConfigureLine(LineRenderer line, int sortingOrder)
    {
        line.useWorldSpace = true;
        line.alignment = LineAlignment.View;
        line.textureMode = LineTextureMode.Stretch;
        line.numCapVertices = 4;
        line.numCornerVertices = 2;
        line.sortingOrder = sortingOrder;

        Material material = GetSharedLineMaterial();
        if (material != null)
            line.sharedMaterial = material;
    }

    private static Material GetSharedLineMaterial()
    {
        if (sharedLineMaterial != null)
            return sharedLineMaterial;

        Shader shader = Shader.Find("Sprites/Default");
        if (shader == null)
            shader = Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default");
        if (shader == null)
            shader = Shader.Find("Universal Render Pipeline/Unlit");
        if (shader == null)
            shader = Shader.Find("Unlit/Color");
        if (shader == null)
            return null;

        sharedLineMaterial = new Material(shader)
        {
            name = "ChainLaser_RuntimeMaterial",
            hideFlags = HideFlags.HideAndDontSave
        };
        sharedLineMaterial.renderQueue = 3100;
        return sharedLineMaterial;
    }

    private float GetLaserRange()
    {
        return GetFinalRange() * Mathf.Max(0.1f, owner.GlobalStats.areaMultiplier);
    }

    private static Vector3 GetTargetPosition(Enemy enemy)
    {
        return enemy.transform.position + (Vector3)enemy.Get_Offset();
    }
}
