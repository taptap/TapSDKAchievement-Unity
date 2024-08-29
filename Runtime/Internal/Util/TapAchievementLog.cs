using System;
using UnityEngine;

namespace TapSDK.Achievement.Internal.Util
{
    public class TapAchievementLog
    {
        public static void Log(string message)
        {
            Debug.Log($"TapAchievement -->> {message}");
        }

        public static void Log(Exception e)
        {
            Debug.Log($"TapAchievement -->> {e}");
        }
    }
}