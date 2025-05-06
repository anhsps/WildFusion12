using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;
using System.Threading.Tasks;

public class GameManager12 : Singleton<GameManager12>
{
    public static int level;
    [SerializeField] private GameObject pauseMenu, mapMenu, shopMenu;
    [SerializeField] private RectTransform pausePanel, mapPanel, shopPanel;
    [SerializeField] private float topPosY = 250f, middlePosY, tweenDuration = 0.3f;
    private int maxLV = 3;

    protected override void Awake()
    {
        base.Awake();
        level = PlayerPrefs.GetInt("CurrentLevel", 1);
    }

    async void Start()
    {
        await HidePanel(pauseMenu, pausePanel);
        await HidePanel(mapMenu, mapPanel);
        await HidePanel(shopMenu, shopPanel);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Delete))
        {
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
            Retry();
        }
    }

    public void Retry() => SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);

    public void UnlockNextLevel()
    {
        int unlockedLevel = PlayerPrefs.GetInt("UnlockedLevel", 1);
        if (level >= unlockedLevel && level < maxLV)
            PlayerPrefs.SetInt("UnlockedLevel", level + 1);
    }

    public void SetCurrentLV(int levelIndex)
    {
        PlayerPrefs.SetInt("CurrentLevel", levelIndex);
        GameMain.Instance.SaveGame();//
        SceneManager.LoadScene(levelIndex.ToString());
    }

    public void PauseGame() => OpenMenu(pauseMenu, pausePanel, 1);
    public void ResumeGame() => CloseMenu(pauseMenu, pausePanel, 1);

    private void OpenMenu(GameObject menu, RectTransform panel, int soundIndex)
    {
        SoundManager12.Instance.PlaySound(soundIndex);
        ShowPanel(menu, panel);
    }

    private async void CloseMenu(GameObject menu, RectTransform panel, int soundIndex)
    {
        SoundManager12.Instance.PlaySound(soundIndex);
        await HidePanel(menu, panel);
    }

    private void ShowPanel(GameObject menu, RectTransform panel)
    {
        //Time.timeScale = 0f;
        menu.SetActive(true);
        menu.GetComponent<CanvasGroup>().DOFade(1, tweenDuration).SetUpdate(true);
        panel.DOAnchorPosY(middlePosY, tweenDuration).SetUpdate(true);
    }

    private async Task HidePanel(GameObject menu, RectTransform panel)
    {
        if (menu == null || panel == null) return;

        menu.GetComponent<CanvasGroup>().DOFade(0, tweenDuration).SetUpdate(true);
        await panel.DOAnchorPosY(topPosY, tweenDuration).SetUpdate(true).AsyncWaitForCompletion();
        if (menu) menu.SetActive(false);
    }

    public void OpenMapMenu()
    {
        CloseMenu(pauseMenu, pausePanel, 1);
        ShowPanel(mapMenu, mapPanel);
        LevelButtons12.Instance.HandlerLevel();//
    }

    public void CloseMapMenu()
    {
        CloseMenu(mapMenu, mapPanel, 1);
        ShowPanel(pauseMenu, pausePanel);
    }

    public void OpenShop() => OpenMenu(shopMenu, shopPanel, 1);
    public void CloseShop() => CloseMenu(shopMenu, shopPanel, 1);
}
