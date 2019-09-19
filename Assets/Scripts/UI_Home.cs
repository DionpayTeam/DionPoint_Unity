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
    [Header("Unity Dev Server URL")]
    public string UnityDevURL;

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
    [Header("------")]
    public EasyTween introPan;
    public EasyTween DionLogo;
    public EasyTween txPoint;
    public GameObject pan_internetErr;


    [Header("Scripts")]
    public Stock_Item[] stockItems;
    public UI_Payment scUI_Payment;
    public UI_Accumulate scUI_Accumulate;

    private void Awake() {
        introPan.gameObject.SetActive(true);






    }

    void Start()  {
        DionLogo.OpenCloseObjectAnimation();
        MG.WalletURL = WalletURL;
        MG.EosMainNet = EosMainNet;
        MG.CoinName = CoinName;
        MG.sendAccountName = sendAccountName;
        MG.recvAccountName = recvAccountName;
        MG.TokenSymbol = TokenSymbol;

        tx_CoinSymbol.text = MG.TokenSymbol;

        StartCoroutine(coDionPrice());
        TotalUpdate();
        BalanceUpdate();

        StartCoroutine(coGetWalletURL());
    }

    IEnumerator coGetWalletURL() {
        UnityWebRequest www = new UnityWebRequest(UnityDevURL);
        www.downloadHandler = new DownloadHandlerBuffer();
        yield return www.SendWebRequest();

        bool err = false;
        if (www.isNetworkError || www.isHttpError) {
            Debug.Log(www.error);
            err = true;
        }
        else {
            MG.WalletURL = www.downloadHandler.text;
            Debug.Log(MG.WalletURL);
        }

        yield return new WaitUntil(() => txPoint.IsObjectOpened());
        if (err) {
            pan_internetErr.SetActive(true);
        }
        else {
            yield return new WaitForSeconds(2f);
            introPan.OpenCloseObjectAnimation();
        }
    }

    public void onClick_ExitApp() {
        Application.Quit();
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


    public void TotalUpdate() {
        StartCoroutine(coTotalUpdate());
    }
    IEnumerator coTotalUpdate() {
        tx_StockTotal.text = "<size=40>Total</size> <b>----- <size=40>DION</size></b>";
        yield return new WaitForSeconds(0.2f);

        for (int i = 0; i < stockItems.Length; i++) {
            stockItems[i].AmountUpdate();
        }
        int total = 0;
        for(int i = 0; i < MG.dion_Stock.Length; i++) {
            total += MG.dion_Stock[i];
        }
        tx_StockTotal.text = "<size=40>Total</size> <b>"+ total.ToString("###,##0") +" <size=40>DION</size></b>";
    }


    public void BalanceUpdate() {
        StartCoroutine(coGet_currency_balance());
    }

    public class body_get_currency_balance {
        public string code { get; set; }
        public string account { get; set; }
        public string symbol { get; set; }
    }

    IEnumerator coGet_currency_balance() {
        body_get_currency_balance body = new body_get_currency_balance { 
            code = MG.CoinName,
            account = MG.recvAccountName,
            symbol = MG.TokenSymbol
        };
        string json = JsonConvert.SerializeObject(body);

        byte[] bytes = System.Text.Encoding.ASCII.GetBytes(json);

        using (UnityWebRequest www = new UnityWebRequest(MG.EosMainNet + "get_currency_balance", UnityWebRequest.kHttpVerbPOST)) {
            UploadHandlerRaw uH = new UploadHandlerRaw(bytes);
            DownloadHandlerBuffer dH = new DownloadHandlerBuffer();

            www.uploadHandler = uH;
            www.downloadHandler = dH;
            www.SetRequestHeader("Content-Type", "application/json");
            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError) {
                Debug.Log(www.error);
                tx_Balance.text = www.error.ToString();
            }
            else {
                List<string> bal = JsonConvert.DeserializeObject<List<string>>(www.downloadHandler.text);
                string stbal = bal[0].Replace(MG.TokenSymbol, "");
                MG.Balance = System.Convert.ToDecimal(stbal);

                tx_Balance.text = MG.Balance.ToString("###,##0.#######");
                scUI_Payment.tx_Balance.text = tx_Balance.text;
            }
            scUI_Payment.enabled = true;
            scUI_Accumulate.enabled = true;
        }

    }


}
