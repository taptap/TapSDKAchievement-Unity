using System.Collections.Generic;
using TapSDK.Core;
using UnityEngine;

namespace TapSDK.Achievement
{
    public class TapTapAchievementOptions : TapTapSdkBaseOptions
    {
        public static TapTapAchievementOptions Config { get; set; }

        public string moduleName => "TapTapAchievement";

        // todo 
        public AndroidJavaObject androidObject =>
             new AndroidJavaObject("com.taptap.sdk.achievement.options.TapTapAchievementOptions", enableToast);

        public bool enableToast = true;

        public TapTapAchievementOptions(bool enableToast)
        {
            this.enableToast = enableToast;
        }

        public TapTapAchievementOptions()
        {
            enableToast = true;
        }


        public Dictionary<string, object> ToDict()
        {
            return new Dictionary<string, object>
            {
                ["enableToast"] = enableToast
            };
        }
    }
}

