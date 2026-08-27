using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class InfoBar : MonoBehaviour
{
    public static InfoBar instance;

    public TextMeshProUGUI infoReadout;
    [SerializeField] TextMeshProUGUI timerText;

    private Coroutine readoutCo = null;
    private Coroutine timerCo = null;

    public static event Action timerFinished = () => { };

    // CONFIG
    // How long to type out the info bar text?
    float typeTime = 1f;

    void Start()
    {
        if (instance == null) instance = this;
        else Destroy(this);
    }

    public void setReadout(string toText)
    {
        if(readoutCo != null)
        {
            StopCoroutine(readoutCo);
        }
        readoutCo = StartCoroutine(readoutTypeText(toText));
    }

    public void setTimer(float maxTime)
    {
        if (timerCo != null)
        {
            StopCoroutine(timerCo);
        }
        timerCo = StartCoroutine(startTimer(maxTime));
    }

    IEnumerator readoutTypeText(string toText)
    {
        float characterLen = toText.ToCharArray().Length;
        float timeForOneChar = typeTime / characterLen;

        float currTime = 0;
        int currPosition = 0;
        string currString = "";

        currString = toText[0].ToString();

        while(currTime < typeTime)
        {
            if(Time.deltaTime / timeForOneChar > 1)
            {
                int charsToAdd = Mathf.FloorToInt(Time.deltaTime / timeForOneChar);
                charsToAdd = Mathf.Min(charsToAdd, (int)characterLen - currPosition);
                currString = currString + toText.Substring(currPosition, charsToAdd);
                currPosition += charsToAdd;
            } else if(currTime + Time.deltaTime > (currPosition + 1) * timeForOneChar)
            {
                if(currPosition < characterLen - 1)
                {
                    currPosition += 1;
                    currString = currString + toText[currPosition];
                }
            }
            
            currTime += Time.deltaTime;
            infoReadout.text = currString;
            yield return null;
        }
        infoReadout.text = toText;
    }

    IEnumerator startTimer(float maxTime)
    {
        float runningClock = maxTime;
        while(runningClock > 0)
        {
            runningClock -= Time.deltaTime;
            timerText.text = ((int)runningClock).ToString();
            yield return null;
        }
        timerFinished.Invoke();
    }
}
