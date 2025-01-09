
using UnityEngine;


public class PlayerAtributes : MonoBehaviour
{
    private float speed = 5;
    [HideInInspector] public float atk = 0;
    [SerializeField] private int maxChopCount = 10;
    private int currentChopCount;

    public void TrackChopCount()
    {
        currentChopCount++;
    }

    private void Start()
    {
        currentChopCount = 0;
    }
}
