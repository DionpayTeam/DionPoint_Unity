using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MGlobal;

public class Stock_Item : MonoBehaviour{
    public int iNum;
    public Text tx_Num;
    public Text tx_DionPrice;
    public Text tx_Amount;

    void Start()  {
        tx_Num.text = (iNum + 1).ToString();

        float min = -((float)MG.nowDionPrice - 0.01f);
        float max = (float)MG.nowDionPrice * 2f;
        // Debug.Log(min);
        float ran_price = Random.Range(min, max);
        MG.dion1_Price[iNum] = MG.nowDionPrice + (decimal)ran_price;
        tx_DionPrice.text = "$ "+MG.dion1_Price[iNum].ToString("###,##0.###");

        MG.dion_Stock[iNum] = Random.Range(3000, 100000000);
        tx_Amount.text = MG.dion_Stock[iNum].ToString("###,##0") + " <size=30>DION</size>";
    }

    void Update()  {
        
    }
}
