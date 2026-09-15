using TMPro;
using UnityEngine;

public sealed class RemotePlayerView : MonoBehaviour
{
    private const float FollowSharpness = 15f;

    private SpriteRenderer bodyRenderer;
    private SpriteRenderer sourceRenderer;
    private TMP_Text nameText;
    private TMP_Text hpText;

    private Vector3 targetPosition;
    private bool hasTarget;

    public static RemotePlayerView Create(string playerName, Color color, float nameHeight, float hpBarHeight)
    {
        var go = new GameObject($"RemotePlayer_{playerName}");
        var view = go.AddComponent<RemotePlayerView>();
        view.Initialize(playerName, color, nameHeight, hpBarHeight);
        return view;
    }

    private void Initialize(string playerName, Color color, float nameHeight, float hpBarHeight)
    {
        bodyRenderer = gameObject.AddComponent<SpriteRenderer>();

        var localPlayer = PlayerControll.Player.Instance;
        if (localPlayer != null)
        {
            sourceRenderer = localPlayer.GetComponentInChildren<SpriteRenderer>();
            if (sourceRenderer != null)
            {
                bodyRenderer.sprite = sourceRenderer.sprite;
                bodyRenderer.sharedMaterial = sourceRenderer.sharedMaterial;
                bodyRenderer.sortingLayerID = sourceRenderer.sortingLayerID;
                bodyRenderer.sortingOrder = sourceRenderer.sortingOrder;
                bodyRenderer.flipX = sourceRenderer.flipX;
            }
        }

        bodyRenderer.color = color;

        nameText = CreateWorldText("Name", nameHeight, 2.3f);
        hpText = CreateWorldText("Hp", hpBarHeight, 1.7f);

        SetName(playerName);
        SetHp01(1f);
    }

    private TMP_Text CreateWorldText(string objectName, float localY, float fontSize)
    {
        var textObject = new GameObject(objectName);
        textObject.transform.SetParent(transform, false);
        textObject.transform.localPosition = new Vector3(0f, localY, -0.01f);

        var text = textObject.AddComponent<TextMeshPro>();
        text.alignment = TextAlignmentOptions.Center;
        text.fontSize = fontSize;
        text.enableWordWrapping = false;
        text.color = Color.white;
        return text;
    }

    private void Update()
    {
        if (sourceRenderer != null && bodyRenderer != null)
            bodyRenderer.sprite = sourceRenderer.sprite;

        if (!hasTarget)
            return;

        Vector3 before = transform.position;
        float t = 1f - Mathf.Exp(-FollowSharpness * Time.deltaTime);
        transform.position = Vector3.Lerp(before, targetPosition, t);

        float moveX = targetPosition.x - before.x;
        if (bodyRenderer != null && Mathf.Abs(moveX) > 0.001f)
            bodyRenderer.flipX = moveX < 0f;
    }

    public void SetName(string playerName)
    {
        if (nameText == null)
            return;

        nameText.text = string.IsNullOrWhiteSpace(playerName) ? "Player" : playerName;
    }

    public void SetHp01(float hp01)
    {
        if (hpText == null)
            return;

        hpText.text = $"{Mathf.RoundToInt(Mathf.Clamp01(hp01) * 100f)}%";
    }

    public void SetTarget(Vector3 position, float yaw)
    {
        targetPosition = position;

        if (hasTarget)
            return;

        transform.position = position;
        hasTarget = true;
    }
}
