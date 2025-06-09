using UnityEngine;
using System.Collections;

public class DoorController : MonoBehaviour
{
    public int nextWave = 1;
    public int waveToOpen = 1;
    public DoorController prerequisiteDoor;

    private bool isOpen = false;
    private bool triggered = false;
    private Vector3 doorPosition;
    private Transform player;
    private float activateDistance = 1.5f;

    void OnEnable() => EnemySpawner.OnWaveEnd += OnWaveEnd;
    void OnDisable() => EnemySpawner.OnWaveEnd -= OnWaveEnd;

    void Start()
    {
        doorPosition = transform.position;
        StartCoroutine(AssignPlayerWhenReady());
    }

    IEnumerator AssignPlayerWhenReady()
    {
        while (GameManager.Instance.player == null)
            yield return null;

        player = GameManager.Instance.player.transform;
    }

    void Update()
    {
        if (!isOpen || triggered || player == null) return;

        float dist = Vector3.Distance(player.position, doorPosition);
        if (dist < activateDistance)
        {
            triggered = true;
            Debug.Log($"[Door] Player walked through door to wave {nextWave}");
            EnemySpawner.Instance.ForceStartWave(nextWave);
        }
    }

    void OnWaveEnd(int wave)
    {
        if (wave >= waveToOpen && GameManager.Instance.enemy_count == 0 &&
            (prerequisiteDoor == null || prerequisiteDoor.isOpen))
        {
            OpenDoor();
        }
    }

    void OpenDoor()
    {
        isOpen = true;

        var sr = GetComponent<SpriteRenderer>();
        if (sr) sr.enabled = false;

        var col = GetComponent<Collider2D>();
        if (col) col.enabled = false;

        Debug.Log("[Door] Door opened. Waiting for player to walk through.");
    }

    public bool IsOpen => isOpen;
}
