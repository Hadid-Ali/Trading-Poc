using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class PortfolioBuilderController : MonoBehaviour
{
    public static UnityAction<AssetType, float> OnInvestedAmountChanged;

    [SerializeField] private PBEventsDataSO eventsDataSO;

    [SerializeField] UiManager uiManager;

    [Header("Ui references")]
    [SerializeField] TextMeshProUGUI discTxt;
    [SerializeField] TextMeshProUGUI resultTxt;
    [SerializeField] TextMeshProUGUI learningPointTxt;
    [SerializeField] GameObject resultPopup;

    [SerializeField] List<InvestedAmountMeta> investedAmounts;
    [SerializeField] List<InvestedAmountMeta> lastInvestedAmounts;

    private List<EventData> shuffledEvents;
    [SerializeField] private EventData currentEvent;
    private bool continuePressed;

    // Debug
    [SerializeField] List<InvestedAmountResultMeta> investedAmountResultMetas;

    private void OnEnable()
    {
        OnInvestedAmountChanged += UpdateInvestedAmount;
    }
    private void OnDisable()
    {
        OnInvestedAmountChanged -= UpdateInvestedAmount;
    }

    private void Start()
    {
        lastInvestedAmounts = new();

        foreach (var item in investedAmounts)
        {
            lastInvestedAmounts.Add(new InvestedAmountMeta
            {
                assetType = item.assetType,
                amount = item.amount
            });
        }
        StartCoroutine(nameof(StartEvents));
    }

    public IEnumerator StartEvents()
    {
        // Create a shuffled copy of the events
        shuffledEvents = new List<EventData>(eventsDataSO.eventsData);

        for (int i = 0; i < shuffledEvents.Count; i++)
        {
            int randomIndex = UnityEngine.Random.Range(i, shuffledEvents.Count);
            (shuffledEvents[i], shuffledEvents[randomIndex]) =
                (shuffledEvents[randomIndex], shuffledEvents[i]);
        }

        // Loop through each event
        foreach (EventData evt in shuffledEvents)
        {
            currentEvent = evt;

            // Update UI
            discTxt.text = currentEvent.statement;

            // Wait until Continue is pressed
            continuePressed = false;
            yield return new WaitUntil(() => continuePressed);
        }

        EventsCompleted();
    }
    private void UpdateInvestedAmount(AssetType assetType, float amount)
    {
        InvestedAmountMeta investedAmountMeta = investedAmounts.Find(x => x.assetType == assetType);
        if (investedAmountMeta != null)
        {
            investedAmountMeta.amount = amount;
        }
    }

    //OnSubmitClick: store last values of investments in lastInvestedAmounts (in the prev round) and check agaist the new/latest investedAmountDict values. and store results in investedAmountResultMetas. like if the current val is > last val == it goes up, etc.
    public void OnSubmitClick()
    {
        // Clear previous results
        investedAmountResultMetas.Clear();

        // Compare current invested amounts with last invested amounts
        foreach (InvestedAmountMeta current in investedAmounts)
        {
            InvestedAmountMeta last = lastInvestedAmounts.Find(x => x.assetType == current.assetType);

            Direction direction = Direction.None;

            if (last != null)
            {
                if (current.amount > last.amount)
                    direction = Direction.Up;
                else if (current.amount < last.amount)
                    direction = Direction.Down;
            }

            investedAmountResultMetas.Add(new InvestedAmountResultMeta
            {
                assetType = current.assetType,
                dir = direction
            });
        }

        // Store current values as the new "last" values for the next submit
        lastInvestedAmounts = new List<InvestedAmountMeta>();

        foreach (InvestedAmountMeta current in investedAmounts)
        {
            lastInvestedAmounts.Add(new InvestedAmountMeta
            {
                assetType = current.assetType,
                amount = current.amount
            });
        }
        CalculateResults();
    }

    private void CalculateResults()
    {
        bool isCorrect = false;

        foreach (AssetDirection correctDirection in currentEvent.directions)
        {
            InvestedAmountResultMeta playerResult = investedAmountResultMetas.Find(x => x.assetType == correctDirection.assetType);

            if (playerResult != null && playerResult.dir == correctDirection.direction)
            {
                isCorrect = true;
                break; // One match is enough
            }
        }

        resultTxt.text = isCorrect ? "<b>Correct! </b>" + currentEvent.correctAns : "<b>Not quite. </b>" + currentEvent.wrongAns;
        learningPointTxt.text = currentEvent.learningPoint;

        ShowResult();
    }

    public void ShowResult()
    {
        resultPopup.SetActive(true);
    }

    public void OnContinueClick()
    {
        resultPopup.SetActive(false);
        continuePressed = true;
    }

    private void EventsCompleted()
    {
        uiManager.ToggleAllPanels(false);
        uiManager.ToggleHomePanel(true);
    }
}

[Serializable]
public class InvestedAmountMeta
{
    public AssetType assetType;
    public float amount;
}

[Serializable]
public class InvestedAmountResultMeta
{
    public AssetType assetType;
    public Direction dir;
}


public enum Direction
{
    None,
    Up,
    Down
}

public enum AssetType
{
    Apple,
    Tesla,
    Bitcoin,
    Gold,
    Oil,
    Bonds,
    etf
}