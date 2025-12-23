using TMPro;
using UnityEngine;

public class ChaosDebugHUD : MonoBehaviour
{
    public static ChaosDebugHUD Instance;

    [SerializeField] private TMP_Text text;

    private void Awake()
    {
        Instance = this;
    }

    public void SetLine(string line)
    {
        if (text != null) text.text = line;
    }
}
