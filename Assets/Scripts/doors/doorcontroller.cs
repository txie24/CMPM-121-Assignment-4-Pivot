using UnityEngine;

public class DoorController : MonoBehaviour
{
    [Tooltip("Door must be cleared before this one opens (optional)")]
    public DoorController prerequisiteDoor;

    [Tooltip("Wave after which this door opens (automatically set by manager)")]
    public int waveToOpen = 1;

    private bool isCleared = false;

    void OnEnable()
    {
        EnemySpawner.OnWaveEnd += OnWaveEnd;
    }

    void OnDisable()
    {
        EnemySpawner.OnWaveEnd -= OnWaveEnd;
    }

    private void OnWaveEnd(int wave)
    {
        if (wave == waveToOpen)
        {
            TryOpenDoor();
        }
    }

    public void TryOpenDoor()
    {
        if (GameManager.Instance.enemy_count == 0 &&
            (prerequisiteDoor == null || prerequisiteDoor.isCleared))
        {
            OpenDoor();
        }
    }

    private void OpenDoor()
    {
        isCleared = true;
        gameObject.SetActive(false);
    }
}
