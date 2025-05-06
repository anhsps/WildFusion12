using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class BG : MonoBehaviour
{
    private int level;
    private Image image;
    [SerializeField] private Sprite[] bgImages;

    // Start is called before the first frame update
    void Start()
    {
        level= int.Parse(SceneManager.GetActiveScene().name);
        image = GetComponent<Image>();

        image.sprite = bgImages[level - 1];
    }
}
