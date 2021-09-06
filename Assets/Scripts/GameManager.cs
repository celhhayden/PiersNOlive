using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public int currGold = 0;
    public Text goldText;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // add gold to count
    public void AddGold(int goldToAdd)
    {
        currGold += goldToAdd;
        goldText.text = "GOLD: " + currGold;
    }
}
