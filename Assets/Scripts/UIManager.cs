using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    [SerializeField] private InputActionAsset InputActions;

    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private GameObject gameOverMenu;
    [SerializeField] private TextMeshProUGUI gameOverMenuText;

    private GameManager gameManager;

    private InputAction pauseMenuAction;

    private UnityEngine.UI.Slider player1Health;
    private UnityEngine.UI.Slider player2Health;

    // TEMP //
    [SerializeField] private TextMeshProUGUI speedUITemp;
    [SerializeField] private GameObject player1Temp;
    // TEMP //



    // Enables the inputs whenever the player object is active
    private void OnEnable()
    {
        InputActions.FindActionMap("UI").Enable();
    }
    private void OnDisable()
    {
        InputActions.FindActionMap("UI").Disable();
    }

    // Sets all the neccessary variables
    private void Start()
    {
        player1Health = GameObject.Find("Player1Health").GetComponent<UnityEngine.UI.Slider>();
        player2Health = GameObject.Find("Player2Health").GetComponent<UnityEngine.UI.Slider>();

        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();

        pauseMenuAction = InputSystem.actions.FindAction("PauseMenu");
    }

    private void Update()
    {
        if (pauseMenuAction.WasPressedThisFrame())
        {
            if (Time.timeScale == 1)
            {
                PauseButton();
            }
            else if (Time.timeScale == 0)
            {
                ResumeButton();
            }
        }

        if (Time.timeScale == 1)
        {
            // TEMP //
            speedUITemp.text = "P1 Speed: " + string.Format("{0:N2}", MathF.Round(player1Temp.GetComponent<Rigidbody>().linearVelocity.x, 2));
            // TEMP //

            if (player2Health.value == 0)
            {
                GameOver("Player1");
            }
            else if (player1Health.value == 0)
            {
                GameOver("Player2");
            }
        }
    }

    // FUNCTIONS //
    public void TakeDamage(int oppNum, float health)
    {
        UnityEngine.UI.Slider opponnentHealth = GameObject.Find("Player" + oppNum + "Health").GetComponent<UnityEngine.UI.Slider>();
        opponnentHealth.value -= health;
    }

    public void GameOver(string winner)
    {
        gameOverMenu.SetActive(true);
        gameOverMenuText.text = "Game Over! " + winner + " won!";
    }

    public void PauseButton()
    {
        Time.timeScale = 0;
        pauseMenu.SetActive(true);
    }
    public void ResumeButton()
    {
        Time.timeScale = 1;
        pauseMenu.SetActive(false);
    }
    public void ExitButton()
    {
        Application.Quit();
    }
    public void RestartButton()
    {
        SceneManager.LoadScene(1);
    }
}