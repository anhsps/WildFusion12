using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Block : MonoBehaviour
{
    public BlockState state { get; private set; }
    public int id { get; private set; }
    public Node node;

    private SpriteRenderer sr;
    private Node startNode;
    private bool dragging;
    private static Block draggedBlock;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();// sr phai Awake ms k error, prefab sinh o Start
        startNode = node;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Collider2D[] hits = Physics2D.OverlapPointAll(GetMousePos());

            foreach (var hit in hits)
            {
                Block block = hit.GetComponent<Block>();
                if (block)
                {
                    draggedBlock = block;
                    draggedBlock.StartDrag();
                    break;
                }
            }
        }

        if (Input.GetMouseButton(0) && draggedBlock)
            draggedBlock.OnDrag();

        if (Input.GetMouseButtonUp(0) && draggedBlock)
        {
            draggedBlock.EndDrag();
            draggedBlock = null;
        }
    }

    public void SetState(BlockState state)
    {
        this.state = state;
        id = state.id;
        sr.sprite = state.sprite;
    }

    public void SetBlock(Node node)
    {
        if (this.node != null) this.node.occupiedBlock = null;
        this.node = node;

        if (node != null) node.occupiedBlock = this;

        transform.position = node != null ? node.Pos : startNode.Pos;
    }

    private Vector2 GetMousePos() => Camera.main.ScreenToWorldPoint(Input.mousePosition);

    private void StartDrag()
    {
        if (id == -1)
        {
            SoundManager12.Instance.PlaySound(5);
            id = 1;
            int index = GameMain.Instance.GetIndex(id);
            SetState(GameMain.Instance.states[index]);
            StartCoroutine(OpenBox());
            Input.ResetInputAxes();// huy nhan dien giu chuot
            return;
        }

        dragging = true;
        startNode = node;
    }

    private void OnDrag()
    {
        if (dragging) transform.position = GetMousePos();
    }

    private void EndDrag()
    {
        dragging = false;

        Collider2D[] hits = Physics2D.OverlapPointAll(transform.position);
        var targetBlock = hits.Select(h => h.GetComponent<Block>()).FirstOrDefault(b => b && b != this);
        var targetNode = hits.Select(h => h.GetComponent<Node>()).FirstOrDefault(n => n);

        if (targetBlock) GameMain.Instance.HandleBlock(this, targetBlock);
        else SetBlock(targetNode ?? startNode);
    }

    private IEnumerator OpenBox()
    {
        float duration = 0.5f;
        Vector3 scale = transform.localScale;
        yield return ChangeScale(scale * 1.3f, duration / 2);
        yield return ChangeScale(scale, duration / 2);
    }

    private IEnumerator ChangeScale(Vector3 to, float t)
    {
        Vector3 from = transform.localScale;
        float elapsed = 0;
        while (elapsed < t)
        {
            transform.localScale = Vector3.Lerp(from, to, elapsed / t);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.localScale = to;
    }
}

// A ?? B (A != null -> A , A == null -> B