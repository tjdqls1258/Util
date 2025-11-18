using HVC;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class BannerScroll<T> : MonoBehaviour
{
    protected Vector2 m_pageItemPos = new Vector2(30 , -10);
    protected Vector2 m_pageItemLayoutPos = new Vector2(30 , 0);

    protected virtual Vector2 GetPageItemPos => m_pageItemPos;
    protected virtual Vector2 GetPageItemPosLayout => m_pageItemLayoutPos;

    private Vector2 m_leftPos;
    private Vector2 m_centerPos;
    private Vector2 m_rightPos;
    private int m_currentIndex;
    private List<BannerItem_CPKR<T>> m_items = new();

    [InspectorName("Page Select Item"), SerializeField] 
    private RectTransform m_currentPageItem;

    [InspectorName("Page Off Item Content"), SerializeField] 
    private RectTransform m_pageItemContent;

    [InspectorName("Page Select Item"),SerializeField] 
    private GameObject m_pageItemPrefab;

    [InspectorName("Banner Item"),SerializeField] 
    protected GameObject m_BannerItemPrefab;

    private List<RectTransform> m_pageItem = new();
    protected List<T> m_dataList;
    [InspectorName("CustomScrollRect"), SerializeField] private CustomScrollRect m_scrollRect;
    private bool scrolling = false;

    [SerializeField, Header("Page Move Speed")] 
    private float m_moveSpeed = 0.5f;
    private float m_slideSpeed = 0.2f;

    [SerializeField, Header("Auto Scrolling")] 
    private bool m_autoScrolling;
    [SerializeField, Range(1, 5)] private float m_scrollingTime = 2f;
    Coroutine m_scrollCo;
    WaitForSeconds m_waitForSeconds;

    protected void Awake()
    {
        m_waitForSeconds = new WaitForSeconds(m_scrollingTime + m_moveSpeed);
        m_BannerItemPrefab.SetActive(false);
        float size = ((RectTransform)m_BannerItemPrefab.transform).sizeDelta.x;
        for (int i = 0; i < 3; i++)
        {
            var obj = Instantiate(m_BannerItemPrefab, m_scrollRect.content);
            obj.SetActive(true);
            var bannerItemCom = obj.GetComponent<BannerItem_CPKR<T>>();
            bannerItemCom.MyRT.anchoredPosition = new Vector2(-size + (size * i), 0);
            m_items.Add(bannerItemCom);
        }

        m_rightPos = m_items[0].MyRT.anchoredPosition;
        m_centerPos = m_items[1].MyRT.anchoredPosition;
        m_leftPos = m_items[2].MyRT.anchoredPosition;


        for (int i = 0; i < m_items.Count; i++)
        {
            m_items[i].SetPosition(i,m_leftPos, m_rightPos, m_centerPos, m_moveSpeed);
            m_items[i].SetAction(OnClickBanner, GetData);
        }

        m_scrollRect.AddOnDragAction(OnDrageBegin);

        m_scrollRect.AddEndDragAction(OnPointerUp);
    }

    protected void OnEnable()
    {
        SetAutoScrolling(true);
    }

    protected void OnDisable()
    {
        SetAutoScrolling(false);
    }

    public T GetData(int index)
    {
        return m_dataList[index];
    }

    public virtual void UpdateBannerItem(List<T> bannerItem)
    {
        m_dataList = bannerItem;

        foreach (var item in m_pageItem)
        {
            item.gameObject.SetActive(true);
        }
        if(m_pageItem.Count < m_dataList.Count)
        {
            for(int i = m_pageItem.Count; m_dataList.Count > i; i++) 
            {
                var ob = Instantiate(m_pageItemPrefab, m_pageItemContent);
                ob.SetActive(true);
                ((RectTransform)ob.transform).anchoredPosition = GetPageItemPos + (GetPageItemPosLayout * i);
                m_pageItem.Add((RectTransform)ob.transform);
            }
        }
        else
        {
            for(int i = m_dataList.Count; i < m_pageItem.Count; i++)
            {
                m_pageItem[i].gameObject.SetActive(false);
            }
        }

        foreach (var item in m_items)
        {
            item.InitBannerItem(0, m_dataList.Count - 1, m_dataList[m_currentIndex]);
        }
    }

    public virtual void OnClickBanner(int index)
    {

    }

    public void OnPointerUp()
    {
        if(!scrolling) return;
        if (m_dataList.Count <= 1) return;

        scrolling = false;
        SetAutoScrolling(true);
        float movePos = m_scrollRect.horizontalNormalizedPosition;
        if(movePos <= 0)
        {
            SlideBeforePage();
        }
        else if(movePos >= 1)
        {
            SlideNextPage();
        }
        else
        {
            foreach (var i in m_items)
            {
                i.MoveCurrent();
            }
        }
    }

    protected void OnDrageBegin()
    {
        scrolling = true;
        SetAutoScrolling(false);
        foreach(var i in m_items)
        {
            i.gameObject.SetActive(true);
            i.OnSlide();
        }
    }

    protected void SetAutoScrolling(bool active)
    {
        if(m_autoScrolling == false) return;

        if (m_scrollCo != null)
            StopCoroutine(m_scrollCo);

        if (active)
        {
            m_scrollCo = StartCoroutine(AutoScrolling());
        }
    }

    protected IEnumerator AutoScrolling()
    {
        while(scrolling==false) 
        {
            yield return m_waitForSeconds;
            MoveNextPage();
        }
    }

    public void MoveNextPage()
    {
        NextPage(false);
    }

    public void MoveBeforePage()
    {
        BeforePage(false);
    }

    public void SlideNextPage()
    {
        NextPage(true);
    }

    public void SlideBeforePage()
    {
        BeforePage(true);
    }

    private void NextPage(bool slide)
    {
        if (scrolling || m_dataList.Count <= 1) return;
        m_currentIndex++;
        if (m_currentIndex >= m_pageItem.Count)
        {
            m_currentIndex = 0;
        }
        m_currentPageItem.anchoredPosition = m_pageItem[m_currentIndex].anchoredPosition;
        foreach (var i in m_items)
        {
            if (slide)
                i.SlideNext(m_slideSpeed);
            else
                i.MoveNext();
            i.UpdateIndex(m_currentIndex, m_dataList.Count - 1, m_dataList[m_currentIndex]);
        }
    }

    private void BeforePage(bool slide)
    {
        if (scrolling || m_dataList.Count <= 1) return;
        m_currentIndex--;
        if (m_currentIndex < 0)
        {
            m_currentIndex = Mathf.Max(0, m_pageItem.Count - 1);
        }
        m_currentPageItem.anchoredPosition = m_pageItem[m_currentIndex].anchoredPosition;
        foreach (var i in m_items)
        {
            if (slide)
                i.SlidBeforePage(m_slideSpeed);
            else
                i.MoveBeforePage();
            i.UpdateIndex(m_currentIndex, m_dataList.Count - 1, m_dataList[m_currentIndex]);
        }
    }
}
