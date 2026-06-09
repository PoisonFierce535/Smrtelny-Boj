using Unity.VisualScripting;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    //private UIManager uiManager;


    public string winnerPlayer;

    public bool gameRunning;

    

    private void Awake()
    {
        gameRunning = true;
    }
}
