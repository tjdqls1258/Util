using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Collections;
using static TextLinkEventHandler;
using HVC.DataModel;

public class TermsClickableText : MonoBehaviour, IPointerDownHandler
{
    class ClickableWordInfo
    {
        private LinkTag wordTag;
        private int wordStartIndex;
        private int wordLength;

        public ClickableWordInfo(string clickWord, int wordIndex, string tag)
        {
            bool isParseTags = Enum.TryParse(typeof(LinkTag), tag, out var res);
            wordTag = isParseTags ? (LinkTag)res : LinkTag.None;

            wordStartIndex = wordIndex;
            wordLength = clickWord.Length - 1;
        }

        public void CheckClick(int ClickWordIndex)
        {
            if (wordStartIndex <= ClickWordIndex && (wordLength) + wordStartIndex >= ClickWordIndex)
            {
                TextLinkEventHandler.Instance.InvokeLink(wordTag);
            }
        }
    }

    [SerializeField] private Color32 _textColor;
    private const string TAG_OPEN_REGEX = @"\<popup=(.*?)\>";
    private const string TAG_CLOSE_REGEX = @"\</popup\>";
    private const string FIND_TAG_REGEX = @"(?<=\<popup=)(.*?)(?=\</popup\>)";
    [SerializeField] private Image[] _underLineImage;
    private int _underLineCount = 0;

    [SerializeField] private Text _text;
    private string _originelText;

    TextUnderLine _drawUnderLineInText = new();
    List<ClickableWordInfo> _clickableWordInfos = new();

    bool _drawTextDone = false;

    [SerializeField] private string desc_Key = "POPUP_TERMS_INFO";

    private IEnumerator Start()
    {
        InitText(Lang.GET_UI(desc_Key));

        yield return new WaitUntil(() => _drawTextDone);
        AddPopupTagInText();
    }

    //태그 추출 후 해당 단어 팝업 워드에 등록
    private void AddPopupTagInText()
    {
        Regex serchPopup = new Regex(FIND_TAG_REGEX);

        MatchCollection mat = serchPopup.Matches(_originelText);
        string[] keyValue;
        int targetIndex = 0;
        List<KeyValuePair<int,(int,float)>> linePos = new();
        
        foreach (Match ma in mat)
        {
            Group group = ma.Groups[0];
            targetIndex = 0;

            //match Group : <popup=tag>word</popup> => tag>word
            //'>'를 기준으로 나눠 줍니다.
            keyValue = group.Value.Split('>');

            targetIndex = group.Index + (keyValue[0].Length + 1);
            ClickableWordInfo ClickTargetWord = new ClickableWordInfo(keyValue[1], targetIndex, keyValue[0]);
            _clickableWordInfos.Add(ClickTargetWord);

            if (_underLineCount < _underLineImage.Length)
            {
                var Line = _drawUnderLineInText.SetLine(_underLineImage[_underLineCount], _text, targetIndex, keyValue[1], _textColor, _originelText);
                linePos.Add(new KeyValuePair<int, (int, float)>(Line.line,(_underLineCount ,Line.lineHeight)));

                _underLineCount++;
            }
        }

        //그려진 라인중 가장 아래 있는 선에 맞춤.
        float FixUnderLine = float.MaxValue;
        int currentline = 0;
        int startchangeLineIndex = 0;
        foreach (var line in linePos)
        {
            if (FixUnderLine >= _underLineImage[line.Value.Item1].rectTransform.localPosition.y)
                FixUnderLine = _underLineImage[line.Value.Item1].rectTransform.localPosition.y;

            if (currentline != line.Key)
            {
                for (int i = startchangeLineIndex; i <= line.Value.Item1; i++)
                {
                    _underLineImage[i].rectTransform.localPosition = new Vector2(
                        _underLineImage[i].rectTransform.localPosition.x, FixUnderLine);
                }
                FixUnderLine = float.MaxValue;
                startchangeLineIndex = line.Value.Item1 + 1;
            }

            currentline = line.Key;
        }

        //for (int i = startchangeLineIndex; i < _underLineImage.Length; i++)
        //{
        //    _underLineImage[i].rectTransform.localPosition = new Vector2(
        //        _underLineImage[i].rectTransform.localPosition.x, FixUnderLine);
        //}
    }

    //<popup=tag> </popup> => <color=TEXTCOLOR> </color>
    public void InitText(string OrginalText)
    {
        string colorTag = $"<color=#{ColorUtility.ToHtmlStringRGBA(_textColor)}>";
        Regex regexOpen = new Regex(TAG_OPEN_REGEX);
        Regex regexClose = new Regex(TAG_CLOSE_REGEX);

        _drawTextDone = false;
        _originelText = OrginalText;
        _text.text = OrginalText;

        MatchCollection mat = regexOpen.Matches(_text.text);
        foreach (Match ma in mat)
        {
            Group group = ma.Groups[0];

            _text.text = _text.text.Replace(group.Value, colorTag);
        }

        _text.text = regexClose.Replace(_text.text, "</color>");
        StartCoroutine(WaitOneFrame());
    }

    private IEnumerator WaitOneFrame()
    {
        yield return new WaitForEndOfFrame();
        _drawTextDone = true;
    }

    private int GetIndexOfClick(Vector3 Position)
    {
        var textGen = _text.cachedTextGenerator;
        var serchTargetText = _originelText;
        Vector2 ClickPos = Position;
        int countChar = 0;

        for (int i = 0; i < textGen.characterCount; ++i)
        {
            if (i * 4 + 2 >= textGen.verts.Count)
                break;

            Vector2 locUpperLeft;
            Vector2 locBottomRight;
            GetVertexPos(textGen, i, i, out locBottomRight, out locUpperLeft);

            Vector2 drawLineLeft = new Vector2(locUpperLeft.x + _text.transform.position.x, locUpperLeft.y + _text.transform.position.y);
            Vector2 drawLineRight = new Vector2(locBottomRight.x + _text.transform.position.x, locBottomRight.y + _text.transform.position.y);

            if (ClickPos.x >= drawLineLeft.x &&
             ClickPos.x <= drawLineRight.x &&
             ClickPos.y <= drawLineLeft.y &&
             ClickPos.y >= drawLineRight.y)
            {
                for(int k = 0; k <= i; k++)
                {
                    //태그 앞 공백이 있는지? 혹은 현재 글자가 공백인지?
                    while (char.IsWhiteSpace(serchTargetText[k + countChar]))
                    {
                        countChar++;
                        if (k + countChar > serchTargetText.Length)
                        {
                            countChar--;
                            break;
                        }
                    }
                    //태그 체크
                    if (serchTargetText[k + countChar] == '<')
                    {
                        int count = 0;
                        while ((serchTargetText[k + countChar + count] != '>') && (k + countChar + count <= serchTargetText.Length))
                        {
                            count++;
                        }
                        if (serchTargetText[k + countChar + count] == '>')
                        {
                            count++;
                        }
                        countChar += count;
                    }
                    while (char.IsWhiteSpace(serchTargetText[k + countChar]))
                    {
                        countChar++;
                        if (k + countChar > serchTargetText.Length)
                        {
                            countChar--;
                            break;
                        }
                    }
                }

                return i + countChar;
            }
        }

        return -1;
    }

    private void GetVertexPos(TextGenerator textGen,
        int UpperIndex, int bottomIndex,
        out Vector2 bottomPos, out Vector2 upperPos)
    {
        Vector2 locUpperLeft = new Vector2(textGen.verts[UpperIndex * 4].position.x, textGen.verts[UpperIndex * 4].position.y);
        Vector2 locBottomRight = new Vector2(textGen.verts[bottomIndex * 4 + 2].position.x, textGen.verts[bottomIndex * 4 + 2].position.y);

        locUpperLeft *= _text.transform.localScale;
        locBottomRight *= _text.transform.localScale;

        bottomPos = locBottomRight;
        upperPos = locUpperLeft;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        eventData.Use();
        int index = GetIndexOfClick(eventData.position);
        
        if (index != -1)
        {
            _clickableWordInfos.ForEach(info => info.CheckClick(index));
        }
    }

    private void OnDestroy()
    {
        _drawUnderLineInText = null;
        _clickableWordInfos.Clear();
    }
}