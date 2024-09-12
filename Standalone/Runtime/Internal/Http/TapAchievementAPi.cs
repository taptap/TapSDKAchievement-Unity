using System.Threading.Tasks;
using TapSDK.Login;
using System;
using TapSDK.Achievement.Internal.Model;
using TapSDK.Achievement.Internal.Util;
using System.Collections.Generic;
using System.Reflection;
using Newtonsoft.Json;
using TapSDK.Core;
using TapSDK.Login.Internal;
using UnityEngine;
using System.Security.Cryptography;
using System.Text;
using System.Linq;
using TapSDK.Core.Standalone.Internal;
using TapSDK.Core.Standalone;

namespace TapSDK.Achievement.Internal.Http
{
    public static class TapAchievementAPi
    {
        static readonly string ChinaHost = "https://tapsdk.tapapis.cn";
        static readonly string ChinaRndHost = "https://tapsdk.api.xdrnd.cn";

        private static TapAchievementHttpClient HttpClient = new TapAchievementHttpClient(GetHost());

        public static async Task<TapAchievementResponseData> Unlock(string achievementId)
        {
            TapAchievementUnlockRequest body = new TapAchievementUnlockRequest(achievementId: achievementId);
            TapAchievementLog.Log("Increment achievementId = " + achievementId);
            string path = "achievement/v1/unlock?client_id=" + TapTapSDK.taptapSdkOptions.clientId;
            var headers = GetAuthHeaders(path, "POST", 0, body);
            TapTapAccount tapAccount = await TapTapLogin.Instance.GetCurrentTapAccount();
            AccessToken accessToken = tapAccount?.accessToken;
            var uri = new Uri(GetHost() + "/" + path);
            var sign = GetMacToken(accessToken, uri);
            headers.Add("Authorization", sign);
            TapAchievementResponse response = await HttpClient.Post<TapAchievementResponse>(
                path: path,
                data: body,
                headers: headers
            );
            TapAchievementLog.Log("Increment response = " + response);
            return response.Result;
        }

        public static async Task<TapAchievementResponseData> Increment(string achievementId, int steps)
        {
            TapAchievementIncrementRequest body = new TapAchievementIncrementRequest(achievementId: achievementId, steps: steps);
            string path = "achievement/v1/increment?client_id=" + TapTapSDK.taptapSdkOptions.clientId;
            var headers = GetAuthHeaders(path, "POST", 0, body);
            TapTapAccount tapAccount = await TapTapLogin.Instance.GetCurrentTapAccount();
            AccessToken accessToken = tapAccount?.accessToken;
            var uri = new Uri(GetHost() + "/" + path);
            var sign = GetMacToken(accessToken, uri);
            headers.Add("Authorization", sign);
            TapAchievementResponse response = await HttpClient.Post<TapAchievementResponse>(
                path: path,
                data: body,
                headers: headers
            );
            TapAchievementLog.Log("Increment response = " + response);
            return response.Result;
        }


        public static Dictionary<string, object> GetAuthHeaders(string path, string httpMethod, int timestamp, object body)
        {
            var httpClientType = typeof(TapAchievementHttpClient);
            var hostFieldInfo = httpClientType.GetField("serverUrl", BindingFlags.NonPublic | BindingFlags.Instance);
            string host = hostFieldInfo?.GetValue(HttpClient) as string;
            var uri = "/" + path;
            int ts = timestamp;
            if (ts == 0)
            {
                var dt = DateTime.UtcNow - new DateTime(1970, 1, 1);
                ts = (int)dt.TotalSeconds;
            }
            var nonce = new System.Random().Next().ToString();
            var headers = new Dictionary<string, object>
            {
                { "X-Tap-PN", "TapSDK" },
                { "X-Tap-Lang", Tracker.getServerLanguage()},
                { "X-Tap-Device-Id", SystemInfo.deviceUniqueIdentifier},
                { "X-Tap-Platform", "Android"},
                { "X-Tap-SDK-Module","TapCompliance"},
                { "X-Tap-SDK-Module-Version", TapTapSDK.Version},
                { "X-Tap-SDK-Artifact", "Unity"},
                { "User-Agent","TapSDK-Unity/" + TapTapSDK.Version},
                { "X-Tap-Nonce", nonce},
                { "X-Tap-Ts",ts}
            };
            TapAchievementLog.Log("GetAuthHeaders headers = " + headers);
            headers = headers.OrderBy(x => x.Key).ToDictionary(x => x.Key, y => y.Value);
            List<string> headerList = new List<string>();
            foreach (KeyValuePair<string, object> kv in headers)
            {
                if (kv.Key.ToLower().StartsWith("x-tap-"))
                {
                    headerList.Add(kv.Key.ToLower() + ":" + kv.Value);
                }
            }
            string headerString = string.Join("\n", headerList);
            var normalizedString = $"{httpMethod}\n{uri}\n{headerString}\n";
            if (body != null)
            {
                normalizedString += $"{JsonConvert.SerializeObject(body)}\n";
            }
            else
            {
                normalizedString += "\n";
            }

            HashAlgorithm hashGenerator = new HMACSHA256(Encoding.UTF8.GetBytes(TapTapSDK.taptapSdkOptions.clientToken));

            var hash = Convert.ToBase64String(hashGenerator.ComputeHash(Encoding.UTF8.GetBytes(normalizedString)));
            headers.Add("X-Tap-Sign", hash);
            return headers;
        }

        public static string GetMacToken(AccessToken token, Uri uri, long timestamp = 0)
        {
            int ts = (int)timestamp;
            if (ts == 0)
            {
                var dt = DateTime.UtcNow - new DateTime(1970, 1, 1);
                ts = (int)dt.TotalSeconds;
            }
            var sign = "MAC " + LoginService.GetAuthorizationHeader(token.kid,
                token.macKey,
                token.macAlgorithm,
                "POST",
                uri.PathAndQuery,
                uri.Host,
                "443", ts);
            return sign;
        }

        internal static string GetHost()
        {
            if (TapCoreStandalone.isRnd)
            {
                return ChinaRndHost;
            }
            else
            {
                return ChinaHost;
            }
        }
    }
}