using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class GameMain : Singleton<GameMain>
{
    [SerializeField] private Node nodePrefab;
    [SerializeField] private Block blockPrefab;
    [SerializeField] private int width = 4, height = 4;
    [SerializeField] private float timeSpawn = 5f;
    [SerializeField] private Image woodImage, lineImage;
    [SerializeField] private TextMeshProUGUI expText, lvText;
    [SerializeField] private TextMeshProUGUI coinText, coinSecText;

    private List<Node> nodes;
    private List<Block> blocks;
    private ShopSystem shopSystem;

    private int currentLV;
    private int exp, maxExp;
    private int coinSec, mergeCoinSec;
    [HideInInspector] public int levelExp, coin;
    [HideInInspector] public string key;

    public BlockState[] states;

    // Start is called before the first frame update
    void Start()
    {
        shopSystem = FindObjectOfType<ShopSystem>(true);// ShopSystem ban dau bi tat nen dung (true)
        currentLV = int.Parse(SceneManager.GetActiveScene().name);
        key = $"Level_{currentLV}";

        CreateGrid();
        CameraPos();
        StartCoroutine(SpawnBlockRoutine());
        StartCoroutine(IncreaseCoin());

        if (PlayerPrefs.HasKey(key + "Coin"))
            LoadGame();
        else
        {
            SpawnBlocks(2, 1);
            levelExp = 1;
            maxExp = 5;
        }

        lineImage.fillAmount = (float)exp / maxExp;
        DisplayUI();
    }

    // Update is called once per frame
    void Update()
    {
        UpdateCoinSec();
    }

    private void CreateGrid()
    {
        nodes = new List<Node>();
        blocks = new List<Block>();

        for (int y = height - 1; y >= 0; y--)
        {
            for (int x = 0; x < width; x++)
            {
                var node = Instantiate(nodePrefab, new Vector2(x, y), Quaternion.identity, transform);
                nodes.Add(node);
            }
        }
    }

    private void CameraPos()
    {
        Vector3 center = new Vector3(width / 2f - 0.5f, (height - 1) / 2f, -10);
        Camera.main.transform.position = center;
    }

    private void DisplayUI()
    {
        lvText.text = levelExp.ToString();
        expText.text = exp + "/" + maxExp;
        coinText.text = FormatPrice(coin);
        coinSecText.text = FormatPrice(coinSec);
    }

    private void SpawnBlocks(int amount, int id)
    {
        var freeNodes = nodes.Where(n => n.occupiedBlock == null).OrderBy(b => Random.value).ToList();
        foreach (var node in freeNodes.Take(amount))
            SpawnBlock(node, id);
    }

    public void SpawnBlock(Node node, int id)
    {
        var block = Instantiate(blockPrefab, node.Pos, Quaternion.identity);
        int index = GetIndex(id);
        block.SetState(states[index]);
        block.SetBlock(node);
        blocks.Add(block);
    }

    private int IndexOf(int id)
    {
        for (int i = 0; i < states.Length; i++)
        {
            if (states[i].id == id)
                return i;
        }
        return 0;
    }

    public int GetIndex(int id) => Mathf.Clamp(IndexOf(id), 0, states.Length - 1);

    private IEnumerator SpawnBlockRoutine()
    {
        while (true)
        {
            woodImage.fillAmount = 0;
            float elapsed = 0;
            float duration = timeSpawn;

            while (elapsed < duration)
            {
                woodImage.fillAmount = Mathf.Lerp(0, 1f, elapsed / duration);
                elapsed += Time.deltaTime;
                yield return null;
            }
            woodImage.fillAmount = 1;

            while (IsGridFull()) yield return null;

            SpawnBlock(GetFreeNode(), -1);
        }
    }

    public bool IsGridFull() => nodes.All(n => n.occupiedBlock != null);

    public Node GetFreeNode() => nodes.FirstOrDefault(n => n.occupiedBlock == null);

    public void HandleBlock(Block a, Block b)
    {
        if (CanMerge(a, b)) Merge(a, b);
        else Swap(a, b);
    }

    private bool CanMerge(Block a, Block b) => a.id == b.id && a.id < 10;

    public void Merge(Block a, Block b)
    {
        SoundManager12.Instance.PlaySound(6);
        blocks.Remove(a);
        a.SetBlock(null);
        Destroy(a.gameObject);

        IncreaseExp(b.id < 10 ? b.id : 0);
        mergeCoinSec += 2 * (int)Mathf.Pow(2, b.id);

        int index = GetIndex(b.id) + 1;
        b.SetState(states[index]);
        StartCoroutine(b.OpenBox());
    }

    private void Swap(Block a, Block b)
    {
        Node nodeA = a.node;
        a.SetBlock(b.node);
        b.SetBlock(nodeA);

        a.node.occupiedBlock = a;// fix b.SetBlock(nodeA) lam a.node.occupiedBlock = null
    }

    public void IncreaseExp(int point)
    {
        exp += point;
        if (exp >= maxExp)
        {
            exp = 0;
            maxExp = (int)(maxExp * (levelExp < 4 ? 2 : 1.5f));
            levelExp++;
            lvText.text = levelExp.ToString();

            shopSystem.UpdateShop();
            if (levelExp > 5) GameManager12.Instance.UnlockNextLevel();
        }
        expText.text = exp + "/" + maxExp;
        lineImage.fillAmount = (float)exp / maxExp;
    }

    private void UpdateCoinSec()
    {
        int baseCoinSec = blocks.Where(b => b.id > 0).Sum(b => (int)Mathf.Pow(2, b.id));
        coinSec = baseCoinSec + mergeCoinSec;
        coinSecText.text = FormatPrice(coinSec);
    }

    private IEnumerator IncreaseCoin()
    {
        while (true)
        {
            yield return new WaitForSeconds(1);
            UpdateCoin(coinSec);
        }
    }

    public void UpdateCoin(int point)
    {
        coin += point;
        coinText.text = FormatPrice(coin);
    }

    public void HackBitCoin() => UpdateCoin(1000000);

    public string FormatPrice(int price)
    {
        if (price >= 1000000)
            return (price / 1000000) + "m" + ((price % 1000000) / 100000 == 0 ? "" : (price % 1000000) / 100000);
        if (price >= 1000)
            return (price / 1000) + "k" + ((price % 1000) / 100 == 0 ? "" : (price % 1000) / 100);
        return price.ToString();
    }

    public void SaveGame()
    {
        PlayerPrefs.SetInt(key + "Coin", coin);
        PlayerPrefs.SetInt(key + "CoinSec", coinSec);
        PlayerPrefs.SetInt(key + "Exp", exp);
        PlayerPrefs.SetInt(key + "MaxExp", maxExp);
        PlayerPrefs.SetInt(key + "LevelExp", levelExp);

        // luu trang thai cua blocks
        for (int i = 0; i < blocks.Count; i++)
        {
            PlayerPrefs.SetInt(key + $"Block_{i}_ID", blocks[i].id);
            PlayerPrefs.SetFloat(key + $"Block_{i}_X", blocks[i].transform.position.x);
            PlayerPrefs.SetFloat(key + $"Block_{i}_Y", blocks[i].transform.position.y);
        }
        PlayerPrefs.SetInt(key + "BlockCount", blocks.Count);

        PlayerPrefs.Save();
    }

    public void LoadGame()
    {
        coin = PlayerPrefs.GetInt(key + "Coin", 0);
        coinSec = PlayerPrefs.GetInt(key + "CoinSec", 0);
        exp = PlayerPrefs.GetInt(key + "Exp", 0);
        maxExp = PlayerPrefs.GetInt(key + "MaxExp", 5);
        levelExp = PlayerPrefs.GetInt(key + "LevelExp", 1);

        // load blocks
        int blockCount = PlayerPrefs.GetInt(key + "BlockCount", 0);
        for (int i = 0; i < blockCount; i++)
        {
            int id = PlayerPrefs.GetInt(key + $"Block_{i}_ID", 1);
            float x = PlayerPrefs.GetFloat(key + $"Block_{i}_X", 0);
            float y = PlayerPrefs.GetFloat(key + $"Block_{i}_Y", 0);

            Node node = nodes.FirstOrDefault(n => n.Pos == new Vector2(x, y));
            if (node != null) SpawnBlock(node, id);
        }
    }

    private void OnApplicationQuit()
    {
        SaveGame();
    }
}
