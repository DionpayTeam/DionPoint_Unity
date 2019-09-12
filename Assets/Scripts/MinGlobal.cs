using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.IO;
using System.Text;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;



namespace MGlobal {

    public static class MG {
        public static string WalletURL = "";
        public static string EosMainNet = "";
        public static string CoinName = "";
        public static string sendAccountName = "";
        public static string recvAccountName = "";
        public static string TokenSymbol = "";

        public static decimal nowDionPrice = 0.5m;
        public static decimal[] dion1_Price = new decimal[4];
        public static int[] dion_Stock = new int[4];
        public static decimal Balance = 0m;

        public static decimal AssetValue = 0m;
        public static decimal AccumulateAmount = 0m;

    }

    public static class WebReq {

        //-----  class 구조를 byte[] 변환 -------
        /* 사용방법
        -- class 선언 ---
        [DataContract]
        internal class SimpleReq {
            [DataMember]
            public string block_num_or_id;
        }
        -- 사용
        var body = WebReq.ToJsonBinary(new SimpleReq() {
             block_num_or_id = "03c66c7918840c84d64a667aaa2395f1578c7ee78f93dcd0480a7781cce718d1" 
             });
         */
        public static byte[] ToJsonBinary<T>(T data) {

            var stream1 = new MemoryStream();
            var ser = new DataContractJsonSerializer(typeof(T));
            ser.WriteObject(stream1, data);

            stream1.Position = 0;
            StreamReader sr = new StreamReader(stream1);
            var jsonBody = sr.ReadToEnd();

            byte[] byteArray = Encoding.UTF8.GetBytes(jsonBody);
            return byteArray;
        }

        public static byte[] ToIListBinary<T>(T data) {
            string strData = JsonConvert.SerializeObject(data);
            byte[] byteArray = Encoding.UTF8.GetBytes(strData);
            return byteArray;
        }

        public static byte[] ToStingBinary(string data) {
            byte[] byteArray = Encoding.UTF8.GetBytes(data);
            return byteArray;
        }
    }


}

