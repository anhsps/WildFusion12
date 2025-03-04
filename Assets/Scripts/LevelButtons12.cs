using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
//using TMPro;

public class LevelButtons12 : MonoBehaviour
{
    [SerializeField] private Sprite lockedSprite, unlockedSprite;
    [SerializeField] private Button[] levelButtons;

    void Start()
    {
        int unlockedLevel = PlayerPrefs.GetInt("UnlockedLevel", 1);

        for (int i = 0; i < levelButtons.Length; i++)
        {
            int levelIndex = i + 1;
            Button button = levelButtons[i];
            Image buttonImage = button.GetComponent<Image>();
            //TextMeshProUGUI buttonText = button.GetComponentInChildren<TextMeshProUGUI>();

            if (levelIndex <= unlockedLevel)
            {
                // Level da mo khoa
                button.interactable = true;
                //buttonText.gameObject.SetActive(true);
                buttonImage.sprite = unlockedSprite;

                button.onClick.AddListener(() => LoadLevel(levelIndex));
            }
            else
            {
                // Level chua mo khoa
                button.interactable = false;
                //buttonText.gameObject.SetActive(false);
                buttonImage.sprite = lockedSprite;
            }

            //if (buttonText)
                //buttonText.text = levelIndex < 10 ? "0" + levelIndex : levelIndex.ToString();
        }
    }

    void Update()
    {

    }

    private void LoadLevel(int levelIndex) => GameManager12.Instance.SetCurrentLV(levelIndex);
}
