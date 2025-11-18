using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class BannerItem_CPKR<T> : CacheObject
{
    public enum BannerPosition
    {
        None = -1,
        Left,
        Center,
        Right,
        Reset
    }
    private Vector2 left;
    private Vector2 right;
    private Vector2 center;
    private BannerPosition m_currentPosition;
    protected int m_pageIndex = 0;

    private float m_moveSpeed = 0.5f;

    private UnityAction<int> m_clickAction;
    private Func<int, T> getDataAction;
    [SerializeField] private Button m_btn;
    protected T m_currentData;

    public void Awake()
    {
        m_btn.onClick.AddListener(OnClickAction);
    }

    public virtual void InitBannerItem(int centerPageIndex, int Max, T data)
    {
        if (m_currentPosition == BannerPosition.Center)
            m_pageIndex = centerPageIndex;
        else if (m_currentPosition == BannerPosition.Left)
            m_pageIndex = Max < centerPageIndex + 1 ? 0 : centerPageIndex + 1;
        else if (m_currentPosition == BannerPosition.Right)
            m_pageIndex = 1 > centerPageIndex ? Max : centerPageIndex - 1;

        if(getDataAction != null) 
            m_currentData = getDataAction.Invoke(m_pageIndex);
        UpdateBanner();
    }

    public void SetPosition(int index, Vector2 l, Vector2 r, Vector2 c, float moveSpeed)
    {
        m_pageIndex = index;
        m_currentPosition = index == 0 ? BannerPosition.Right : index == 1 ? BannerPosition.Center : BannerPosition.Left;
        left = l;
        right = r; 
        center = c;
        m_moveSpeed = moveSpeed;
    }
    public void SetSpeed(float slideSpeed)
    {
        m_moveSpeed = slideSpeed;
    }
    private void MoveBanner(BannerPosition position, float moveSpeed)
    {
        switch (position)
        {
            case BannerPosition.Left:
                MoveLeft(moveSpeed);
                break;
            case BannerPosition.Right:
                MoveRight(moveSpeed);
                break;
            case BannerPosition.Center:
                MoveCenter(moveSpeed);
                break;
        }
    }

    private void MoveRight(float moveSpeed) => DoMove(right, moveSpeed);
    private void MoveCenter(float moveSpeed) => DoMove(center, moveSpeed);

    private void MoveLeft(float moveSpeed) => DoMove(left, moveSpeed);

    private void DoMove(Vector2 Pos, float moveSpeed)
    {
        gameObject.SetActive(true);
        MyRT.DOKill();
        MyRT.DOAnchorPos(Pos, moveSpeed).OnComplete((ScrollEndEvent));
    }

    public void MoveNext()
    {
        if(CheckNext())
            MoveBanner(m_currentPosition, m_moveSpeed);
    }

    public void MoveBeforePage()
    {
        if(CheckBefore())
            MoveBanner(m_currentPosition, m_moveSpeed);
    }

    public virtual void UpdateIndex(int centerPageIndex, int Max, T data)
    {
        if (m_currentPosition == BannerPosition.Center)
            m_pageIndex = centerPageIndex;
        else if (m_currentPosition == BannerPosition.Left)
            m_pageIndex = Max < centerPageIndex + 1 ? 0 : centerPageIndex + 1;
        else if (m_currentPosition == BannerPosition.Right)
            m_pageIndex = 1 > centerPageIndex ? Max : centerPageIndex-1;

        if (getDataAction != null)
            m_currentData = getDataAction.Invoke(m_pageIndex);
        UpdateBanner();
    }

    public void SetAction(UnityAction<int> action, Func<int, T> data)
    {
        m_clickAction = action;
        getDataAction = data;
    }

    public void OnClickAction()
    {
        if(m_clickAction != null)
        {
            m_clickAction.Invoke(m_pageIndex);
        }
    }

    private void OnDestroy()
    {
        if(MyRT != null)
            MyRT.DOKill();
    }

    public void SlideNext(float speed)
    {
        if(CheckNext())
            MoveBanner(m_currentPosition, speed);
    }

    public void SlidBeforePage(float speed)
    {
        if(CheckBefore())
            MoveBanner(m_currentPosition, speed);
    }

    public void OnSlide()
    {
        MyRT.DOKill();

        if (getDataAction != null)
            m_currentData = getDataAction.Invoke(m_pageIndex);
        UpdateBanner();
    }

    public void MoveCurrent()
    {
        gameObject.SetActive(m_currentPosition == BannerPosition.Center);
        MoveBanner(m_currentPosition, 0.1f);
    }

    protected virtual void ScrollEndEvent()
    {
        gameObject.SetActive(BannerPosition.Center == m_currentPosition);
    }

    protected virtual void UpdateBanner() { }

    private bool CheckNext()
    {
        m_currentPosition = (BannerPosition)((int)m_currentPosition + 1);
        if (m_currentPosition == BannerPosition.Reset)
        {
            gameObject.SetActive(false);
            m_currentPosition = BannerPosition.Left;
            MyRT.anchoredPosition = left;
            return false;
        }
        if (m_currentPosition != BannerPosition.Right)
        {
            if (getDataAction != null)
                m_currentData = getDataAction.Invoke(m_pageIndex);
            UpdateBanner();
        }
        return true;
    }

    private bool CheckBefore()
    {
        m_currentPosition = (BannerPosition)((int)m_currentPosition - 1);
        if (m_currentPosition == BannerPosition.None)
        {
            gameObject.SetActive(false);
            m_currentPosition = BannerPosition.Right;
            MyRT.anchoredPosition = right;

            return false;
        }

        if (m_currentPosition != BannerPosition.Left)
        {
            if (getDataAction != null)
                m_currentData = getDataAction.Invoke(m_pageIndex);
            UpdateBanner();
        }
        return true;
    }
}
