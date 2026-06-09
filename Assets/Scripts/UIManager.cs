using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class UIManager : MonoBehaviour
{
    private GameObject player1Health;
    private GameObject player2Health;

    [SerializeField] private TextMeshProUGUI speedUI;
    [SerializeField] private GameObject player;


    private void Start()
    {
        player1Health = GameObject.Find("Player1Health");
        player2Health = GameObject.Find("Player2Health");
    }

    private void Update()
    {
        speedUI.text = "P1 Speed: " + string.Format("{0:N2}", MathF.Round(player.GetComponent<Rigidbody>().linearVelocity.x, 2));
    }

    // FUNCTIONS //
    public void TakeDamage(int oppNum, float health)
    {
        UnityEngine.UI.Slider opponnentHealth = GameObject.Find("Player" + oppNum + "Health").GetComponent<UnityEngine.UI.Slider>();
        opponnentHealth.value -= health;
    }
}
