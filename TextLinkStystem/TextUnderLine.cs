using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TextUnderLine
{
    public struct FontLineInfo
    {
        public float startPosX;
        public float endPosX;
        public float lineHeight;
        public float lineTop;
        public int line;
    }

    float beforeLineTop;
    int beforeLine;
    bool _beforeSet = false;
    public FontLineInfo SetLine(Image underLine, Text textObject, int targetIndex, string target, Color32 TextColor, string OriginelText)
    {
        int targetWordLength = target.Replace(" ", string.Empty).Length;

        float deltaSizeRatioX = textObject.rectTransform.localScale.x * textObject.canvas.transform.localScale.x;

        FontLineInfo _lineInfo = SetFontLineInfo(textObject, targetIndex, targetWordLength, OriginelText);

        if ((((beforeLineTop + (textObject.fontSize * 0.5f)) < _lineInfo.lineTop) ||
                ((beforeLineTop - (textObject.fontSize * 0.5f)) < _lineInfo.lineTop)) && 
                _beforeSet)
        {
            _lineInfo.line = beforeLine + 1;
        }
        else
        {
            _lineInfo.line = beforeLine;
        }

        if ((_beforeSet == false) || (beforeLine != _lineInfo.line))
        {
            beforeLineTop = _lineInfo.lineTop;
            beforeLine = _lineInfo.line;
            _beforeSet = true;
        }
        else if (_beforeSet && beforeLine == _lineInfo.line)
        {
            _lineInfo.lineTop = beforeLineTop = beforeLineTop <= _lineInfo.lineTop ? beforeLineTop : _lineInfo.lineTop;
        }
        else if (beforeLine == _lineInfo.line)
        {
            beforeLine = _lineInfo.line;
        }

        underLine.transform.position = textObject.transform.position + Vector3.right * _lineInfo.startPosX;

        underLine.rectTransform.localPosition =
            new Vector2(underLine.rectTransform.localPosition.x, (_lineInfo.lineTop));

        underLine.rectTransform.sizeDelta = new Vector2((_lineInfo.endPosX - _lineInfo.startPosX) / deltaSizeRatioX, underLine.rectTransform.sizeDelta.y);

        underLine.color = TextColor;
        underLine.gameObject.SetActive(true);

        return _lineInfo;
    }

    private FontLineInfo SetFontLineInfo(Text TargetTextObject, int StartIndex, int WordLength, string OriginelText)
    {
        var textGen = TargetTextObject.cachedTextGenerator;
        int countChar = 0;
        int WordFirstIndex = StartIndex;
        int line = 0;
        int WordLastIndex = WordLength - 1;
        int countTag = 0;

        float _deltaSizeRatioY = TargetTextObject.rectTransform.localScale.y * TargetTextObject.canvas.transform.localScale.y;

        for (int i = 0; i <= StartIndex; ++i)
        {
            if (OriginelText[i] == '<')
            {
                int count = 0;
                while (OriginelText[i + count] != '>')
                {
                    count++;
                }
                if (OriginelText[i + count] == '>')
                {
                    count++;
                }
                countTag += count;
            }
            if (char.IsWhiteSpace(OriginelText[i]))
            {
                countChar++;
                if (OriginelText[i] == '\n') line++;
                if (StartIndex + countChar > OriginelText.Length)
                {
                    countChar--;
                    break;
                }
            }
        }

        WordFirstIndex += -(countChar + countTag);
        WordLastIndex += WordFirstIndex;

        Vector2 locUpperLeft = new Vector2(textGen.verts[WordFirstIndex * 4].position.x, textGen.verts[WordFirstIndex * 4].position.y);
        Vector2 locBottomRight = new Vector2(textGen.verts[WordLastIndex * 4 + 2].position.x, textGen.verts[WordLastIndex * 4 + 2].position.y);

        locUpperLeft *= TargetTextObject.transform.localScale;
        locBottomRight *= TargetTextObject.transform.localScale;

        float LinePosY = (locBottomRight.y / _deltaSizeRatioY) - (TargetTextObject.fontSize * 0.25f);

        return new FontLineInfo()
        {
            startPosX = locUpperLeft.x,
            endPosX = locBottomRight.x,
            line = line,
            lineTop = LinePosY
        };
    }
}
