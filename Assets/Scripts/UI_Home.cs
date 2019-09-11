using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MGlobal;
using UnityEngine.Networking;
using System.Runtime.Serialization;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Globalization;
using System.Text.RegularExpressions;

public class UI_Home : MonoBehaviour{
    [Header("Keosd Server URL")]
    public string WalletURL;

    [Header("EOS MainNet URL")]
    public string EosMainNet;
    [Header("EOS Token Account Name")]
    public string CoinName;
    [Header("Send EOS Account Name")]
    public string sendAccountName;
    [Header("Recv EOS Account Name")]
    public string recvAccountName;
    [Header("EOS Token Symbol")]
    public string TokenSymbol;


    [Header("------")]
    public EasyTween[] hidePan;
    public Text tx_DionPrice;
    public Text tx_StockTotal;
    public Text tx_Balance;
    public Text tx_CoinSymbol;

    [Header("Scripts")]
    public UI_Payment scUI_Payment;
    public UI_Accumulate scUI_Accumulate;

    void Start()  {
        MG.WalletURL = WalletURL;
        MG.EosMainNet = EosMainNet;
        MG.CoinName = CoinName;
        MG.sendAccountName = sendAccountName;
        MG.recvAccountName = recvAccountName;
        MG.TokenSymbol = TokenSymbol;

        tx_CoinSymbol.text = MG.TokenSymbol;

        StartCoroutine(coDionPrice());
        StartCoroutine(coTotalUpdate());
        StartCoroutine(coGet_currency_balance());
    }

    void Update()  {
        
    }

    public void onClick_Home() {
        for(int i=0;i< hidePan.Length; i++) {
            if (hidePan[i].IsObjectOpened()) {
                hidePan[i].OpenCloseObjectAnimation();
            }
        }
    }

    IEnumerator coDionPrice() {
        while (true) {
            float val = Random.Range(-0.03f, 0.03f);
            MG.nowDionPrice += (decimal)val;
            if (MG.nowDionPrice < 0.01m)
                MG.nowDionPrice = 0.01m;
            tx_DionPrice.text = MG.nowDionPrice.ToString("###,##0.###");
            yield return new WaitForSeconds(15.3f);
        }
    }
    IEnumerator coTotalUpdate() {
        tx_StockTotal.text = "<size=40>Total</size> <b>----- <size=40>DION</size></b>";
        yield return new WaitForSeconds(0.2f);
        int total = 0;
        for(int i = 0; i < MG.dion_Stock.Length; i++) {
            total += MG.dion_Stock[i];
        }
        tx_StockTotal.text = "<size=40>Total</size> <b>"+ total.ToString("###,##0") +"<size=40>DION</size></b>";
    }

    
	[DataContract]
	public class body_get_currency_balance {
		[DataMember]
		public string code { get; set; }
		[DataMember]
		public string account { get; set; }
		[DataMember]
		public string symbol { get; set; }
	}
	IEnumerator coGet_currency_balance() {
		string url = MG.EosMainNet + "get_currency_balance";

		var body = WebReq.ToJsonBinary(new body_get_currency_balance()	{
			code = MG.CoinName,
			account = MG.recvAccountName,
			symbol = MG.TokenSymbol
		});
		var www = new UnityWebRequest(url);
		www.method = UnityWebRequest.kHttpVerbPOST;
		www.uploadHandler = new UploadHandlerRaw(body);
		www.uploadHandler.contentType = "application/json";
		www.downloadHandler = new DownloadHandlerBuffer();
		yield return www.SendWebRequest();

		if (www.isNetworkError || www.isHttpError)	{
			Debug.LogError("인터넷에 연결되어있는지 확인하세요");
			Debug.Log(www.error);
		}
        else {
            /*
            string jsonString = @"['116999999999.0000000 TSTDION']";
            Debug.Log(jsonString);

            List<string> bal = JsonConvert.DeserializeObject<List<string>>(jsonString);
            Debug.Log(bal[0]);
            string stbal = bal[0].Replace(MG.TokenSymbol, "");
            decimal balance = System.Convert.ToDecimal(stbal);
            Debug.Log(balance.ToString("###,##0.#######"));
            */

            List<string> bal = JsonConvert.DeserializeObject<List<string>>(www.downloadHandler.text);
            string stbal = bal[0].Replace(MG.TokenSymbol, "");
            MG.Balance = System.Convert.ToDecimal(stbal);

            tx_Balance.text = MG.Balance.ToString("###,##0.#######");
        }
        scUI_Payment.enabled = true;
        scUI_Accumulate.enabled=true;
    }
}
