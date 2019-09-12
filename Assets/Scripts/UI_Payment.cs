using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MGlobal;
using System.Runtime.Serialization;
using System;
using UnityEngine.Networking;
using Newtonsoft.Json.Linq;

public class UI_Payment : MonoBehaviour{
    public UI_Home home;
    public EasyTween viewPan;
    public EasyTween[] hidePan;
    [Header("------")]
    public Text tx_Balance;
    public Text tx_CoinSymbol;
    public Text tx_AssetValue;

    public Dropdown dd_Dest;
    public InputField in_Amount;

    public GameObject pan_PayConfirm;
    public Text tx_sendAmount;
    public Text tx_sendDest;
    public Text tx_sendDebug;
    public Button bt_Success;


    private void Awake() {
    }

    void Start() {
        tx_Balance.text = MG.Balance.ToString("###,##0.#######");
        tx_CoinSymbol.text = MG.TokenSymbol;
    }

    void Update() {
        decimal assetvalue = MG.Balance * MG.nowDionPrice;
        if (!assetvalue.Equals(MG.AssetValue)) {
            MG.AssetValue = assetvalue;

            tx_AssetValue.text = MG.AssetValue.ToString("###,##0.###");
        }
    }

    public void onClick_Payment() {
        if (!viewPan.IsObjectOpened()) {
            viewPan.OpenCloseObjectAnimation();
        }
        for (int i = 0; i < hidePan.Length; i++) {
            if (hidePan[i].IsObjectOpened()) {
                hidePan[i].OpenCloseObjectAnimation();
            }
        }
        send_Amount = 0m;
    }

    public void onClick_SendCoin() {
        send_Amount = 0m;
        if (decimal.TryParse(in_Amount.text, out send_Amount) ==false) {
            return;
        }
        if(send_Amount <= 0m) {
            return;

        }
        if (MG.Balance < send_Amount) {
            send_Amount = MG.Balance;
        }
        tx_sendAmount.text = send_Amount.ToString("###,##0.#######");

        tx_sendDest.text = dd_Dest.options[dd_Dest.value].text;

        if (!pan_PayConfirm.GetComponent<EasyTween>().IsObjectOpened()) {
            pan_PayConfirm.GetComponent<EasyTween>().OpenCloseObjectAnimation();
        }
    }


    decimal send_Amount = 0m;
    public void onClick_Confirm() {
        StartCoroutine(coWait());
        StartCoroutine(coSendCoin());
    }

    bool coWait_loop = true;
    IEnumerator coWait() {
        coWait_loop = true;
        while (coWait_loop) {
            tx_sendDebug.text += "-";
            yield return new WaitForSeconds(0.01f);
        }
        Debug.Log("coWait Finish");
    }


    [DataContract]
    public class body_sendDion {
        [DataMember]
        public string sender { get; set; }
        [DataMember]
        public decimal amount { get; set; }
        [DataMember]
        public string memo { get; set; }
    }
    IEnumerator coSendCoin() {
        string url = MG.WalletURL + "sendDion";

        var body = WebReq.ToJsonBinary(new body_sendDion() {
            sender = "user",
            amount = send_Amount,
            memo = "Payment at the " + tx_sendDest.text
        });
        var www = new UnityWebRequest(url);
        www.method = UnityWebRequest.kHttpVerbPOST;
        www.uploadHandler = new UploadHandlerRaw(body);
        www.uploadHandler.contentType = "application/json";
        www.downloadHandler = new DownloadHandlerBuffer();
        yield return www.SendWebRequest();
        coWait_loop = false;
        tx_sendDebug.text = "";

        if (www.isNetworkError || www.isHttpError) {
            Debug.LogError("인터넷에 연결되어있는지 확인하세요");
            tx_sendDebug.text = "<color=red>" + www.error + "</color>";
        }
        else {
            if(www.responseCode == 200) {//성공
                var jo = JObject.Parse(www.downloadHandler.text);
                string txID = jo["transaction_id"].Value<string>();
                tx_sendDebug.text = "Transfer completed \nTxID = " + txID;
                Debug.Log(tx_sendDebug.text);
                home.BalanceUpdate();
                bt_Success.gameObject.SetActive(true);
            }
            else {// Fail
                tx_sendDebug.text = "<color=red>" + www.error + "</color>";
            }
        }
    }
}