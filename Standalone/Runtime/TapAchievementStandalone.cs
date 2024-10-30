
using System;
using TapSDK.Achievement.Internal.Util;
using TapSDK.Achievement.Internal.Model;
using TapTap.Achievement.Standalone.Internal;
using TapSDK.Achievement.Internal.Http;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using System.Threading.Tasks;
using TapSDK.Core.Standalone;
using TapSDK.Core;
using TapSDK.Achievement.Standalone.Internal;
using TapSDK.Core.Standalone.Internal.Http;

namespace TapSDK.Achievement.Standalone
{
    public class TapAchievementStandalone : ITapTapAchievement
    {

        private static readonly List<ITapAchievementCallback> callbacks = new List<ITapAchievementCallback>();

        public static bool toastEnable = true;
        private static TapTapRegionType currentRegionType;

        public async void Init(string clientId, TapTapRegionType regionType, TapTapAchievementOptions achievementOptions)
        {
            currentRegionType = regionType;
            toastEnable = achievementOptions.enableToast;
            TapAchievementLog.Log("TapAchievementStandalone -- Init currentRegionType : " + currentRegionType + " , toastEnable : " + toastEnable);
            // 
            TapAchievementTracker.Instance.TrackInit();
            await PublicAchievement();
        }

        public async void Increment(string achievementId, int step)
        {
            TapAchievementLog.Log("TapAchievementStandalone -- Increment achievementId: " + achievementId + " step: " + step);
            // check init
            if (!TapAchievementUtil.CheckInit())
            {
                Debug.LogError("TapAchievement Increment achievementId: " + achievementId + " failed, not init");
                NotifyCallbackFailure(
                                    achievementId: achievementId,
                                    errorCode: TapTapAchievementConstants.NOT_INITIALIZED,
                                    "Currently not initialized, please initialize first."
                                );
                return;
            }
            if (currentRegionType == TapTapRegionType.Overseas)
            {
                TapAchievementLog.Log("TapAchievement Increment achievementId: " + achievementId + " failed, not support region");
                NotifyCallbackFailure(
                                    achievementId: achievementId,
                                    errorCode: TapTapAchievementConstants.REGION_NOT_SUPPORTED,
                                    "Current RegionType not supported, only support TapTapRegionType.CN"
                                );
                return;
            }
            // check login
            if (!await TapAchievementUtil.CheckAccount())
            {
                TapAchievementLog.Log("TapAchievement Increment achievementId: " + achievementId + " failed, not login");
                NotifyCallbackFailure(
                                     achievementId: achievementId,
                                     errorCode: TapTapAchievementConstants.NOT_LOGGED,
                                     "Currently not logged in, please login first."
                                 );
                return;
            }
            TapAchievementStoreBean bean = new TapAchievementStoreBean(type: 0, achievementId: achievementId, steps: step);
            await TapAchievementStore.Save(bean);
            await PublicAchievement();
        }

        public async void Unlock(string achievementId)
        {
            // check init
            if (!TapAchievementUtil.CheckInit())
            {
                Debug.LogError("TapAchievement Increment achievementId: " + achievementId + " failed, not init");
                NotifyCallbackFailure(
                                    achievementId: achievementId,
                                    errorCode: TapTapAchievementConstants.NOT_INITIALIZED,
                                    "Currently not initialized, please initialize first."
                                );
                return;
            }
            if (currentRegionType == TapTapRegionType.Overseas)
            {
                TapAchievementLog.Log("TapAchievement Increment achievementId: " + achievementId + " failed, not support region");
                NotifyCallbackFailure(
                                    achievementId: achievementId,
                                    errorCode: TapTapAchievementConstants.REGION_NOT_SUPPORTED,
                                    "Current RegionType not supported, only support TapTapRegionType.CN"
                                );
                return;
            }
            // check login
            if (!await TapAchievementUtil.CheckAccount())
            {
                Debug.LogError("TapAchievement Increment achievementId: " + achievementId + " failed, not login");
                NotifyCallbackFailure(
                                     achievementId: achievementId,
                                     errorCode: TapTapAchievementConstants.NOT_LOGGED,
                                     "Currently not logged in, please login first."
                                 );
                return;
            }
            TapAchievementLog.Log("Unlock achievementId");

            TapAchievementStoreBean bean = new TapAchievementStoreBean(type: 1, achievementId: achievementId);
            await TapAchievementStore.Save(bean);
            await PublicAchievement();
        }

        public async void ShowAchievements()
        {
            TapAchievementLog.Log("TapAchievementStandalone -- ShowAchievements");
            // check init
            if (!TapAchievementUtil.CheckInit())
            {
                Debug.LogError("TapAchievement ShowAchievements failed, not init");
                NotifyCallbackFailure(
                                    achievementId: "",
                                    errorCode: TapTapAchievementConstants.NOT_INITIALIZED,
                                    "Currently not initialized, please initialize first."
                                );
                return;
            }
            if (currentRegionType == TapTapRegionType.Overseas)
            {
                TapAchievementLog.Log("TapAchievement ShowAchievements failed, not support region");
                NotifyCallbackFailure(
                                    achievementId: "",
                                    errorCode: TapTapAchievementConstants.REGION_NOT_SUPPORTED,
                                    "Current RegionType not supported, only support TapTapRegionType.CN"
                                );
                return;
            }
            // check login
            if (!await TapAchievementUtil.CheckAccount())
            {
                Debug.LogError("TapAchievement ShowAchievements failed, not login");
                NotifyCallbackFailure(
                                     achievementId: "",
                                     errorCode: TapTapAchievementConstants.NOT_LOGGED,
                                     "Currently not logged in, please login first."
                                 );
                return;
            }

            string seesionId = Guid.NewGuid().ToString();
            TapAchievementTracker.Instance.TrackStart("showAchievements", seesionId);
            // 打开 web
            string url = TapCoreStandalone.getGatekeeperConfigUrl("achievement_my_list_url");
            TapAchievementLog.Log("TapAchievementStandalone -- ShowAchievements url: " + url);
            Application.OpenURL(url);
            TapAchievementTracker.Instance.TrackSuccess("showAchievements", seesionId);
        }

        public void SetToastEnable(bool enable)
        {
            TapAchievementLog.Log("TapAchievementStandalone -- SetToastEnable enable: " + enable);
            toastEnable = enable;
        }

        public void RegisterCallBack(ITapAchievementCallback callback)
        {
            TapAchievementLog.Log("TapAchievementStandalone -- RegisterCallBack");
            if (!callbacks.Contains(callback))
            {
                callbacks.Add(callback);
            }
        }

        public void UnRegisterCallBack(ITapAchievementCallback callback)
        {
            TapAchievementLog.Log("TapAchievementStandalone -- UnRegisterCallBack");
            callbacks.Remove(callback);
        }

        private bool UrlExistsUsingSockets(string url)
        {
            if (url.StartsWith("https://")) url = url.Remove(0, "https://".Length);
            try
            {
                System.Net.IPHostEntry ipHost = System.Net.Dns.GetHostEntry(url);// System.Net.Dns.Resolve(url);
                TapAchievementLog.Log($"TapAchievementStandalone -- UrlExistsUsingSockets = true");
                return true;
            }
            catch (System.Net.Sockets.SocketException)
            {
                TapAchievementLog.Log($"TapAchievementStandalone -- UrlExistsUsingSockets = false");
                return false;
            }
        }

        private async Task PublicAchievement()
        {
            List<TapAchievementStoreBean> all = await TapAchievementStore.getAll();
            NetworkReachability internetReachability = Application.internetReachability;
            TapAchievementLog.Log($"TapAchievementStandalone -- internetReachability = {internetReachability}");
            if (all == null || all.Count <= 0)
            {
                return;
            }
            if (!UrlExistsUsingSockets(TapHttp.HOST_CN))
            {
                NotifyCallbackFailure(
                            achievementId: "",
                            errorCode: TapTapAchievementConstants.NETWORK_ERROR,
                            "The network is currently unavailable"
                        );
                return;
            }
            all.ForEach(async (x) =>
            {
                string seesionId = Guid.NewGuid().ToString();
                try
                {
                    TapAchievementResponseData result = null;
                    switch (x.Type)
                    {
                        case 0:
                            TapAchievementTracker.Instance.TrackStart("increment", seesionId, x.AchievementId);
                            TapAchievementLog.Log($"TapAchievementStandalone -- API Increment Request : {x.UUID}");
                            result = await TapAchievementAPi.Increment(x.AchievementId, x.Steps);
                            break;
                        case 1:
                            TapAchievementTracker.Instance.TrackStart("unlock", seesionId, x.AchievementId);
                            TapAchievementLog.Log($"TapAchievementStandalone -- API Unlock Request : {x.UUID}");
                            result = await TapAchievementAPi.Unlock(x.AchievementId);
                            break;
                    }
                    TapAchievementLog.Log($"TapAchievementStandalone -- API Request : {x.UUID}\nresult : {JsonConvert.SerializeObject(result)}");
                    if (result != null)
                    {
                        await TapAchievementStore.Delete(x.UUID);

                        TapAchievementResponseBean normalAchievement = result.Achievement;
                        TapAchievementResponseBean platinumAchievement = result.PlatinumAchievement;
                        if (normalAchievement != null)
                        {
                            TapAchievementResult normalAchievementResult =
                                   new TapAchievementResult(
                                       achievementId: normalAchievement.Id ?? "",
                                       achievementName: normalAchievement.Name ?? "",
                                       achievementType: TapAchievementType.NORMAL,
                                       currentSteps: normalAchievement.CurrentSteps
                                   );
                            if (x.Type == 0)
                            {
                                NotifyCallbackSuccess(code: TapTapAchievementConstants.INCREMENT_SUCCESS, normalAchievementResult);
                                TapAchievementTracker.Instance.TrackSuccess("increment", seesionId, x.AchievementId);
                            }
                            else if (x.Type == 1)
                            {
                                TapAchievementTracker.Instance.TrackSuccess("unlock", seesionId, x.AchievementId);
                            }
                            if (normalAchievement.NewlyUnlocked == true)
                            {
                                NotifyCallbackSuccess(code: TapTapAchievementConstants.UNLOCK_SUCCESS, normalAchievementResult);
                                TapAchievementLog.Log("Unlock success1 toastEnable = " + toastEnable);
                                if (toastEnable)
                                {
                                    TapAchievementToastManager.ShowToast(normalAchievementResult);
                                }
                            }
                        }
                        if (platinumAchievement != null && platinumAchievement.NewlyUnlocked == true)
                        {
                            TapAchievementResult platinumAchievementResult =
                                new TapAchievementResult(
                                    achievementId: platinumAchievement.Id ?? "",
                                    achievementName: platinumAchievement.Name ?? "",
                                    achievementType: TapAchievementType.PLATINUM,
                                    currentSteps: platinumAchievement.CurrentSteps
                                );
                            NotifyCallbackSuccess(code: TapTapAchievementConstants.UNLOCK_SUCCESS, platinumAchievementResult);
                            TapAchievementLog.Log("Unlock success2 toastEnable = " + toastEnable);
                            if (toastEnable)
                            {
                                TapAchievementToastManager.ShowToast(platinumAchievementResult);
                            }
                        }
                    }
                    else
                    {
                        switch (x.Type)
                        {
                            case 0:
                                TapAchievementTracker.Instance.TrackFailure("increment", seesionId, x.AchievementId, errorCode: TapTapAchievementConstants.UNKNOWN_ERROR, errorMessage: "Request result is null");
                                break;
                            case 1:
                                TapAchievementTracker.Instance.TrackFailure("unlock", seesionId, x.AchievementId, errorCode: TapTapAchievementConstants.UNKNOWN_ERROR, errorMessage: "Request result is null");
                                break;
                        }
                        TapAchievementLog.Log("TapAchievementStandalone -- PublicAchievement result is null");
                        // do nothing
                        NotifyCallbackFailure(
                                    achievementId: x.AchievementId,
                                    errorCode: TapTapAchievementConstants.UNKNOWN_ERROR,
                                    "Request result is null"
                                );
                    }
                }
                catch (Exception e)
                {
                    if (e is TapHttpServerException exception)
                    {
                        switch (x.Type)
                        {
                            case 0:
                                TapAchievementTracker.Instance.TrackFailure("increment", seesionId, x.AchievementId, errorCode: (int)exception.ErrorData.Code, errorMessage: exception.Message);
                                break;
                            case 1:
                                TapAchievementTracker.Instance.TrackFailure("unlock", seesionId, x.AchievementId, errorCode: (int)exception.ErrorData.Code, errorMessage: exception.Message);
                                break;
                        }
                        switch (exception.ErrorData.Error)
                        {
                            case TapHttpErrorConstants.ERROR_NOT_FOUND:
                            case TapHttpErrorConstants.ERROR_FORBIDDEN:
                            case TapHttpErrorConstants.ERROR_INVALID_REQUEST:
                                await TapAchievementStore.Delete(x.UUID);
                                NotifyCallbackFailure(
                                    achievementId: x.AchievementId,
                                    errorCode: TapTapAchievementConstants.INVALID_REQUEST,
                                    exception.ErrorData.Msg
                                );
                                break;
                            case TapHttpErrorConstants.ERROR_ACCESS_DENIED:
                                await TapAchievementStore.Delete(x.UUID);
                                NotifyCallbackFailure(
                                     achievementId: x.AchievementId,
                                     errorCode: TapTapAchievementConstants.ACCESS_DENIED,
                                     exception.ErrorData.Msg
                                 );
                                break;
                            default:
                                TapAchievementLog.Log("TapAchievementStandalone -- PublicAchievement Exception1 : " + exception.ErrorData.Msg);
                                NotifyCallbackFailure(
                                    achievementId: x.AchievementId,
                                    errorCode: TapTapAchievementConstants.UNKNOWN_ERROR,
                                    exception.Message
                                );
                                // do nothing
                                break;
                        }
                    }
                    else
                    {
                        TapAchievementTracker.Instance.TrackFailure("increment", seesionId, x.AchievementId, errorCode: TapTapAchievementConstants.UNKNOWN_ERROR, errorMessage: e.Message);
                        TapAchievementLog.Log("TapAchievementStandalone -- PublicAchievement Exception2 : " + e.GetType() + " , Message = " + e.Message);
                        // 没到服务器 全部 80030
                        // do nothing
                        NotifyCallbackFailure(
                                     achievementId: x.AchievementId,
                                     errorCode: TapTapAchievementConstants.UNKNOWN_ERROR,
                                     e.Message
                                 );
                    }
                }
            });
        }

        private void NotifyCallbackSuccess(int code, TapAchievementResult result)
        {
            TapAchievementLog.Log("TapAchievementStandalone -- NotifyCallbackSuccess");
            callbacks.ForEach((x) =>
            {
                x.OnAchievementSuccess(code, result);
            });
        }

        private void NotifyCallbackFailure(string achievementId, int errorCode, string errorMsg)
        {
            TapAchievementLog.Log("TapAchievementStandalone -- NotifyCallbackFailure");
            callbacks.ForEach((x) =>
            {
                x.OnAchievementFailure(achievementId, errorCode, errorMsg ?? "");
            });
        }
    }
}