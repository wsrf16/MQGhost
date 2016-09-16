using Library.Serializer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Vector
{
    public class RMQAPI
    {
        HttpClient client = new HttpClient();
        public string IP;
        public string User;
        public string PWD;
        public string ApiUrl
        {
            get {
                return string.Format("http://{0}:15672/api/", IP);
            }
        }
        public string AuthBase64
        {
            get
            {
                return "Basic " + Library.Ciphering.Base64.EncodingString(string.Format("{0}:{1}", User, PWD));
            }
        }

        public RMQAPI(string ip, string user, string pwd)
        {
            this.IP = ip;
            this.User = user;
            this.PWD = pwd;

            client.DefaultRequestHeaders.Add("Authorization", AuthBase64);
        }

        public object Op(string operation)
        {
            string url = ApiUrl + operation;
            string _json = client.GetAsync(url).Result.Content.ReadAsStringAsync().Result;
            object ret;
            switch (operation)
            {
                case "overview":
                    {
                        InfoOverView infoOverView = JsonSerializerHelper.Json2Obj<InfoOverView>(_json);
                        ret = infoOverView;
                    }
                    break;
                case "ObjectTotals":
                    {
                        InfoOverView infoOverView = JsonSerializerHelper.Json2Obj<InfoOverView>(_json);
                        ObjectTotals objectTotals = infoOverView.ObjectTotals;
                        ret = objectTotals;
                    }
                    break;
                default:
                    ret = null;
                    break;
            }
            return ret;
        }

        public InfoOverView APIOverview()
        {
            return Op("overview") as InfoOverView;
        }
    }
}