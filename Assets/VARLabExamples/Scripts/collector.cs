using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class collector : MonoBehaviour
{
    private int collectableCount;
    public TextMeshProUGUI BoltCountText;

    private void Start()
    {
        collectableCount = 0;
        BoltCountText.text = "Collected parts: 0/10";

        SetBoltCountText();
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.CompareTag("boltCollectable"))
        {
            other.gameObject.SetActive(false);
            collectableCount += 1;

            SetBoltCountText();

            if(collectableCount >= 10)
            {
                setObjective();
            }
        }
        else if(other.gameObject.CompareTag("winGate") && collectableCount >= 10)
        {
            setWinText();
        }
    }


    void SetBoltCountText()
    {
        BoltCountText.text = "Collected Parts: " + collectableCount.ToString() + "/10";
    }

    void setWinText()
    {
        BoltCountText.text = "You Win!";
    }

    void setObjective()
    {
        BoltCountText.text = "Objective: Return to ship";
    }
}
