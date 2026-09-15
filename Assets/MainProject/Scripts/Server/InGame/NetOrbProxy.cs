using UnityEngine;

[DisallowMultipleComponent]
public sealed class NetOrbProxy : MonoBehaviour
{
    private string orbId;
    private bool pickupRequested;
    private ExpOrb orb;

    private void Awake()
    {
        orb = GetComponent<ExpOrb>();
    }

    private void OnEnable()
    {
        pickupRequested = false;
    }

    public void Init(string id)
    {
        orbId = id;
        pickupRequested = false;
    }

    private void Update()
    {
        if (pickupRequested || string.IsNullOrEmpty(orbId))
            return;

        var player = PlayerControll.Player.Instance;
        if (player == null)
            return;

        float pickupRadius = orb != null ? Mathf.Max(0.1f, orb.detectRadius) : 1f;
        Vector2 delta = (Vector2)transform.position - (Vector2)player.transform.position;

        if (delta.sqrMagnitude > pickupRadius * pickupRadius)
            return;

        var client = WezardUdpClient.Instance;
        if (client == null)
            return;

        client.SendPickup(orbId);
        pickupRequested = true;
    }
}
