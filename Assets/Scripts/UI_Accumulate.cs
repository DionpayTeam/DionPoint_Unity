using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using QRCodeReaderAndGenerator;
using MGlobal;
using System.Runtime.Serialization;
using UnityEngine.Networking;
using Newtonsoft.Json.Linq;

public class UI_Accumulate : MonoBehaviour{

    public UI_Home home;

    public EasyTween viewPan;
    public EasyTween[] hidePan;

    [SerializeField]
    Text recvAmount;

    [SerializeField]
    Text tx_recvLog;

    [SerializeField]
    Button but_Recv;

    [SerializeField]
    Button but_X;


    [Header("Need QRCode -- ")]
    [SerializeField]
    RawImage RawQRimage;

    [SerializeField]
    Texture2D icon;


    void Start() {
        float ran = Random.Range(2f, 15f);
        MG.AccumulateAmount = (decimal)ran;

        QRCodeManager.onError += QRCodeManager_onError;

        // Set output texture size
        QRCodeManager.Instance.SetOutputTextureSize(512, 512); // default is 256x256

        // Set overlay icon
        QRCodeManager.Instance.SetOverlayIcon(icon);

        // set invert qr code
        QRCodeManager.Instance.Inverted = false;
    }

    void Update() {

    }

    public void onClick_Accumulate() {
        if (!viewPan.IsObjectOpened()) {
            viewPan.OpenCloseObjectAnimation();
        }
        for (int i = 0; i < hidePan.Length; i++) {
            if (hidePan[i].IsObjectOpened()) {
                hidePan[i].OpenCloseObjectAnimation();
            }
        }
    }


    Texture2D qrCode;
    ZXing.BarcodeFormat codeFormat = ZXing.BarcodeFormat.QR_CODE;

    void OnEnable() {

    }

    void QRCodeManager_onError(string err) {
        Debug.Log(err);
    }

    public void GenerateQRCode() {
        recvAmount.text ="Recv <b>"+ MG.AccumulateAmount.ToString("###,##0.######") + "</b> "+MG.TokenSymbol;
        string stringCode = "DionCoin_" + MG.AccumulateAmount.ToString();

        if (RawQRimage) {
            qrCode = QRCodeManager.Instance.GenerateQRCode(stringCode, codeFormat);
            RawQRimage.texture = qrCode;
        }
        else {
            Debug.Log("Assign Input Field or Image.");
        }

        if (0m < MG.AccumulateAmount) {
            but_Recv.gameObject.SetActive(true);
            but_X.gameObject.SetActive(false);
        }
        else {
            but_Recv.gameObject.SetActive(false);
            but_X.gameObject.SetActive(true);
        }
    }

    public void onClick_recv() {
        but_Recv.gameObject.SetActive(false);
        StartCoroutine(coWait());
        StartCoroutine(coSendCoin());
    }

    bool coWait_loop = true;
    IEnumerator coWait() {
        coWait_loop = true;
        while (coWait_loop) {
            tx_recvLog.text += "-";
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
            sender = "company",
            amount = MG.AccumulateAmount,
            memo = "Earn points"
        });
        var www = new UnityWebRequest(url);
        www.method = UnityWebRequest.kHttpVerbPOST;
        www.uploadHandler = new UploadHandlerRaw(body);
        www.uploadHandler.contentType = "application/json";
        www.downloadHandler = new DownloadHandlerBuffer();
        yield return www.SendWebRequest();
        coWait_loop = false;
        tx_recvLog.text = "";

        if (www.isNetworkError || www.isHttpError) {
            Debug.LogError("인터넷에 연결되어있는지 확인하세요");
            tx_recvLog.text = "<color=red>" + www.error + "</color>";
            but_X.gameObject.SetActive(true);
        }
        else {
            if (www.responseCode == 200) {//성공
                var jo = JObject.Parse(www.downloadHandler.text);
                string txID = jo["transaction_id"].Value<string>();
                tx_recvLog.text = "Transfer completed \nTxID = " + txID;
                Debug.Log(tx_recvLog.text);

                MG.dion_Stock[0] -= (int)MG.AccumulateAmount;
                home.TotalUpdate();
                MG.AccumulateAmount = 0m;

                yield return new WaitForSeconds(0.5f);
                home.BalanceUpdate();
                yield return new WaitForSeconds(0.2f);
                viewPan.OpenCloseObjectAnimation();
            }
            else {// Fail
                tx_recvLog.text = "<color=red>" + www.error + "</color>";
                but_X.gameObject.SetActive(true);
            }
        }
    }


}
