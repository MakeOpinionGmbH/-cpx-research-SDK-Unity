using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using CPXResearch;

public class CPXResearchScript : MonoBehaviour, IPointerClickHandler, ICPXResearch
{
    [Header("Banner")]
    public Image imageForContent;

    [Header("Style")]
    public CPXStyleConfiguration.SurveyPosition position;
    public string bannerText = "Take a survey";
    public int bannerTextSize = 20;
    public Color bannerTextColor;
    public Color bannerBgColor;
    public bool roundedCorners = true;

    [Header("Configuration")]
    public string appId;
    public string extUserId;
    public string secureHash;

    private CPXResearch.CPXResearch cpx;

    // Start is called before the first frame update
    void Start()
    {
        CPXStyleConfiguration Style = new CPXStyleConfiguration(position,
            bannerText,
            bannerTextSize,
            ColorUtility.ToHtmlStringRGB(bannerTextColor),
            ColorUtility.ToHtmlStringRGB(bannerBgColor),
            true);
        CPXConfiguration Config = new CPXConfiguration(appId, extUserId, secureHash, Style);

        cpx = new CPXResearch.CPXResearch(Config)
        {
            Callback = this
        };
        cpx.SetSurveyVisible(true);
        StartCoroutine(cpx.RequestSurveyUpdate(true));
    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnApplicationPause(bool pauseStatus)
    {
        if (!pauseStatus && cpx != null)
        {
            StartCoroutine(cpx.RequestSurveyUpdate(true));
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Application.OpenURL(cpx.GetSurveysListUrl());
    }

    // ICPXResearch impl
    void ICPXResearch.OnSurveysUpdated()
    {
        Debug.Log($"Surveys received.");
        StartCoroutine(cpx.GetImage(texture =>
        {
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);

            if (sprite != null)
            {
                imageForContent.sprite = sprite;
            }
        }, error =>
        {
            Debug.LogError(error);
        }));
    }

    void ICPXResearch.OnTransactionsUpdated(CPXResearchLib.TransactionItem[] UnpaidTransactions)
    {
        if (UnpaidTransactions != null)
        {
            double amount = 0;
            foreach (CPXResearchLib.TransactionItem item in cpx.UnpaidTransactions)
            {
                Debug.Log($"Transaction: {item.TransactionId}");
                if (double.TryParse(item.VerdienstPublisher, out double earning))
                {
                    amount += earning;
                }
                Debug.Log($"Amount: {amount}");
                cpx.MarkTransactionAsPaid(item.MessageId);
            }
        }        
    } 

    void ICPXResearch.OnSurveysDidOpen()
    {
        Debug.Log("Did open Surveys.");
    }

    void ICPXResearch.OnSurveysDidClose()
    {
        Debug.Log("Did close Surveys.");
    }
}
