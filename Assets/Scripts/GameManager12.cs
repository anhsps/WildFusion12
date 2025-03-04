using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using DG.Tweening;
using System.Threading.Tasks;

public class GameManager12 : Singleton<GameManager12>
{
    public static int level;
    //[SerializeField] private TextMeshProUGUI lvText;
    [SerializeField] private GameObject /*winMenu, loseMenu, */pauseMenu, mapMenu, shopMenu;
    [SerializeField] private RectTransform /*winPanel, losePanel, */pausePanel, mapPanel, shopPanel;
    [SerializeField] private float topPosY = 250f, middlePosY, tweenDuration = 0.3f;

    protected override void Awake()
    {
        base.Awake();
        level = PlayerPrefs.GetInt("CurrentLevel", 1);
    }

    async void Start()
    {
        //if (lvText) lvText.text = "LEVEL " + (level < 10 ? "0" + level : level);

        //await HidePanel(winMenu, winPanel);
        //await HidePanel(loseMenu, losePanel);
        await HidePanel(pauseMenu, pausePanel);
        await HidePanel(mapMenu, mapPanel);
        await HidePanel(shopMenu, shopPanel);
    }

    //public void StartGame() => SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    public void Retry() => SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);

    /*public void NextLV()
    {
        level++;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }*/

    public void PauseGame()
    {
        SoundManager12.Instance.SoundClick();
        ShowPanel(pauseMenu, pausePanel);
    }

    public async void ResumeGame()
    {
        SoundManager12.Instance.SoundClick();
        await HidePanel(pauseMenu, pausePanel);
        Time.timeScale = 1f;
    }

    /*public void GameWin()
    {
        UnlockNextLevel();
        EndGame(winMenu, winPanel, 2);
    }*/

    //public void GameLose() => EndGame(loseMenu, losePanel, 3);

    /*private void EndGame(GameObject menu, RectTransform panel, int soundIndex)
    {
        SoundManager12.Instance.PlaySound(soundIndex);
        ShowPanel(menu, panel);
    }*/

    public void UnlockNextLevel()
    {
        int unlockedLevel = PlayerPrefs.GetInt("UnlockedLevel", 1);
        if (level >= unlockedLevel)
        {
            PlayerPrefs.SetInt("UnlockedLevel", level + 1);
            PlayerPrefs.Save();
        }
    }

    //public void SetCurrentLV(int levelIndex) => SceneManager.LoadScene((level = levelIndex).ToString());
    public void SetCurrentLV(int levelIndex)
    {
        level = levelIndex;
        PlayerPrefs.SetInt("CurrentLevel", level);
        GameMain.Instance.SaveGame();//
        SceneManager.LoadScene(level.ToString());
    }

    private void ShowPanel(GameObject menu, RectTransform panel)
    {
        menu.SetActive(true);
        //Time.timeScale = 0f;
        menu.GetComponent<CanvasGroup>().DOFade(1, tweenDuration).SetUpdate(true);
        panel.DOAnchorPosY(middlePosY, tweenDuration).SetUpdate(true);
    }

    private async Task HidePanel(GameObject menu, RectTransform panel)
    {
        if (menu == null || panel == null) return;

        panel.DOKill();// huy tween dang chay
        menu.GetComponent<CanvasGroup>().DOKill();

        menu.GetComponent<CanvasGroup>().DOFade(0, tweenDuration).SetUpdate(true);
        await panel.DOAnchorPosY(topPosY, tweenDuration).SetUpdate(true).AsyncWaitForCompletion();
        menu.SetActive(false);
    }

    public async void OpenMapMenu()
    {
        SoundManager12.Instance.SoundClick();
        await HidePanel(pauseMenu, pausePanel);
        ShowPanel(mapMenu, mapPanel);
    }

    public async void CloseMapMenu()
    {
        SoundManager12.Instance.SoundClick();
        await HidePanel(mapMenu, mapPanel);
        ShowPanel(pauseMenu, pausePanel);
    }

    public void OpenShop()
    {
        SoundManager12.Instance.SoundClick();
        ShowPanel(shopMenu, shopPanel);
    }

    public async void CloseShop()
    {
        SoundManager12.Instance.SoundClick();
        await HidePanel(shopMenu, shopPanel);
    }
}
