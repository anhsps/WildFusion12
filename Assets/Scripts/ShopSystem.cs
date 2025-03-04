using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopSystem : MonoBehaviour
{
    [SerializeField] private GameObject[] buyObjects;
    [SerializeField] private TextMeshProUGUI notiText;
    [SerializeField] private int basePrice = 1000;

    private string key;
    private List<int> prices;

    // Start is called before the first frame update
    void Start()
    {
        key = GameMain.Instance.key;
        prices = new List<int>();

        for (int i = 0; i < buyObjects.Length; i++)
        {
            prices.Add((int)(basePrice * Mathf.Pow(2.5f, i)));// initial price

            // find child gameobject
            Button buyButton = buyObjects[i].transform.Find("BuyButton").GetComponent<Button>();
            var itemImg = buyObjects[i].transform.Find("BuyButton/BlockImg").GetComponent<Image>();
            var lvText = buyObjects[i].transform.Find("BuyButton/LVText (TMP)").GetComponent<TextMeshProUGUI>();
            var priceText = buyObjects[i].transform.Find("BuyButton/PriceText (TMP)").GetComponent<TextMeshProUGUI>();

            // update UI
            itemImg.sprite = GameMain.Instance.states[i].sprite;
            lvText.text = "LEVEL" + (i + 1);
            priceText.text = GameMain.Instance.FormatPrice(prices[i]);

            // add onClick
            int index = i;// luu gt tranh loi delegate
            buyButton.onClick.AddListener(() => BuyItem(index));
        }
        UpdateShop();

        LoadPrices();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void UpdateShop()
    {
        for (int i = 0; i < buyObjects.Length; i++)
        {
            GameObject shop2Image = buyObjects[i].transform.Find("Shop2Image").gameObject;
            shop2Image.SetActive(i > GameMain.Instance.levelExp);
        }
    }

    public void BuyItem(int index)
    {
        SoundManager12.Instance.PlaySound(4);
        if (GameMain.Instance.IsGridFull())
        {
            ShowNoti("Full slot");
            return;
        }

        if (GameMain.Instance.coin < prices[index])
        {
            ShowNoti("Not enough coins");
            return;
        }

        GameMain.Instance.UpdateCoin(-prices[index]);
        prices[index] = (int)(prices[index] * 1.2f);
        var priceText = buyObjects[index].transform.Find("BuyButton/PriceText (TMP)").GetComponent<TextMeshProUGUI>();
        priceText.text = GameMain.Instance.FormatPrice(prices[index]);
        GameMain.Instance.SpawnBlock(GameMain.Instance.GetFreeNode(), index + 1);
        GameMain.Instance.IncreaseExp(index);

        SavePrices();
    }

    private void ShowNoti(string msg)
    {
        notiText.text = msg;
        CancelInvoke(nameof(ClearNoti));
        Invoke(nameof(ClearNoti), 1.5f);
    }

    private void ClearNoti() => notiText.text = "";

    // save prices
    public void SavePrices()
    {
        for (int i = 0; i < prices.Count; i++)
        {
            PlayerPrefs.SetInt(key + $"ItemPrice_{i}", prices[i]);
        }
        PlayerPrefs.Save();
    }

    public void LoadPrices()
    {
        for (int i = 0; i < buyObjects.Length; i++)
        {
            prices[i] = PlayerPrefs.GetInt(key + $"ItemPrice_{i}", prices[i]);
            var priceText = buyObjects[i].transform.Find("BuyButton/PriceText (TMP)").GetComponent<TextMeshProUGUI>();
            priceText.text = GameMain.Instance.FormatPrice(prices[i]);
        }
    }
}
