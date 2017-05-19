// https://blogs.msdn.microsoft.com/appserviceteam/2017/03/16/publishing-a-net-class-library-as-a-function-app/

using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using System.Configuration;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;


namespace TheThingsNetworkGateway
{
    public class Gateway
    {
        static Telemetry telemetry = new Telemetry();
        static string iotHubApiVersion = "2016-11-14";

        static string host = ConfigurationManager.AppSettings["IoTHubHostname"];
        static string iotHubRegistryReadPolicyKeyName = ConfigurationManager.AppSettings["IotHubRegistryReadPolicyKeyName"];
        static string iotHubRegistryReadKey = ConfigurationManager.AppSettings["IotHubRegistryReadPolicyKey"];
        static string ttnAppIDs = ConfigurationManager.AppSettings["TTNAppIDsCommaSeperated"];


        public static async Task<HttpResponseMessage> Run(HttpRequestMessage req, TraceWriter log)
        {
            dynamic data = await req.Content.ReadAsAsync<object>(); // Get request body

            TtnEntity ttn = JsonConvert.DeserializeObject<TtnEntity>(data.ToString(), new JsonSerializerSettings() { ContractResolver = new CamelCasePropertyNamesContractResolver() });
            
            if (!ValidTtnApplicationId(ttn.app_id)) { return req.CreateResponse(HttpStatusCode.BadRequest); }

            var result = DecodeRawData(ttn.payload_raw);

            string key = await GetDeviceKeyFromRegistry(ttn.dev_id); // get device key from IoT Hub Registry
            if (key == null) { return req.CreateResponse(HttpStatusCode.BadRequest); }

            await PostDataToIoTHub(host, key, result, ttn);

            log.Info($"device '{ttn.dev_id}', raw payload '{ttn.payload_raw}', decoded value '{result}'");

            return req.CreateResponse(HttpStatusCode.OK);
        }

        public static async Task<bool> PostDataToIoTHub(string host, string key, uint data, TtnEntity ttn)
        {
            string restUri = $"https://{host}/devices/{ttn.dev_id}/messages/events?api-version={iotHubApiVersion}";
            var sasToken = GetDeviceSaSToken(host, ttn.dev_id, key);

            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", sasToken);
            client.DefaultRequestHeaders.Add("iothub-app-route-id", "ttn-" + ttn.app_id);

            var content = new StringContent(telemetry.ToJson(data));
            var response = await client.PostAsync(restUri, content);

            return true;
        }

        public static uint DecodeRawData(string base64EncodedData)
        {
            uint result = 0;
            var base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
            var d = System.Text.Encoding.UTF7.GetString(base64EncodedBytes);

            for (int i = 0; i < d.Length; i++)
            {
                result = result << (8);
                result += (byte)d[i];
            }
            return result;
        }

        public static async Task<string> GetDeviceKeyFromRegistry(string deviceId)
        {
            string sasToken = GetRegistrySaSToken(host, iotHubRegistryReadKey, iotHubRegistryReadPolicyKeyName);

            var url = $"https://{host}/devices/{deviceId}?api-version={iotHubApiVersion}"; //%s' % (self.iotHost, deviceId, self.API_VERSION)
            HttpClient client = new HttpClient();

            client.DefaultRequestHeaders.Add("Authorization", sasToken);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));//ACCEPT header

            var response = await client.GetAsync(url);

            if (response.StatusCode != HttpStatusCode.OK) { return null; }

            var responseBody = await response.Content.ReadAsStringAsync();

            return JObject.Parse(responseBody)["authentication"]["symmetricKey"]["primaryKey"].ToString();
        }

        public static string GetRegistrySaSToken(string host, string key, string keyName)
        {
            return $"{GenerateSasToken(host, key)}&skn={keyName}";
        }

        public static string GetDeviceSaSToken(string host, string device, string key)
        {
            return GenerateSasToken($"{host}/devices/{device}", key);
        }

        public static string GenerateSasToken(string url, string key, int expirySeconds = 3600)
        {
            TimeSpan fromEpochStart = DateTime.UtcNow - new DateTime(1970, 1, 1);
            string expiry = Convert.ToString((int)fromEpochStart.TotalSeconds + expirySeconds);

            string baseAddress = WebUtility.UrlEncode(url.ToLower());
            string stringToSign = $"{baseAddress}\n{expiry}";

            byte[] data = Convert.FromBase64String(key);
            HMACSHA256 hmac = new HMACSHA256(data);
            string signature = Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(stringToSign)));

            string token = String.Format(CultureInfo.InvariantCulture, "SharedAccessSignature sr={0}&sig={1}&se={2}",
                            baseAddress, WebUtility.UrlEncode(signature), expiry);
            return token;
        }

        public static bool ValidTtnApplicationId(string appId)
        {
            bool found = false;
            string[] appIds = ttnAppIDs.Split(',');
            foreach (string id in appIds)
            {
                if (appId == id)
                {
                    found = true;
                    break;
                }
            }
            return found;
        }
    }
}