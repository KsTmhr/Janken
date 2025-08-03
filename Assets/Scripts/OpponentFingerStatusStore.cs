using UnityEngine;

public class OpponentFingerStatusStore : MonoBehaviour
{
    public static OpponentFingerStatusStore Instance { get; private set; }

    public FingerStatus[] fingerStatuses;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void Initialize(int size)
    {
        if (fingerStatuses == null || fingerStatuses.Length != size)
        {
            fingerStatuses = new FingerStatus[size];
            for (int i = 0; i < size; i++)
            {
                if (i < 10)
                    fingerStatuses[i] = FingerStatus.Bent;
                else
                    fingerStatuses[i] = FingerStatus.Disabled;
            }
        }
    }
}
