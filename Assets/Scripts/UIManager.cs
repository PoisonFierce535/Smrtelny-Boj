using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class UIManager : MonoBehaviour
{
    private GameObject sliderObject1;
    private GameObject fill1;
    private UnityEngine.UI.Slider healthBar1;

    private GameObject sliderObject2;
    private GameObject fill2;
    private UnityEngine.UI.Slider healthBar2;



    void Start()
    {
        sliderObject1 = GameObject.Find("Player1Health");
        fill1 = GameObject.Find("Fill1");
        healthBar1 = sliderObject1.GetComponent<UnityEngine.UI.Slider>();

        sliderObject2 = GameObject.Find("Player2Health");
        fill2 = GameObject.Find("Fill2");
        healthBar2 = sliderObject1.GetComponent<UnityEngine.UI.Slider>();
    }

    void Update()
    {
        if (healthBar1.value == 0)
        {
            fill1.SetActive(false);
        }
        else
        {
            fill1.SetActive(true);
        }
        if (healthBar2.value == 0)
        {
            fill2.SetActive(false);
        }
        else
        {
            fill2.SetActive(true);
        }
    }

}
