using TapSDK.Core;

namespace TapSDK.Achievement 
{
    public class TapAchievementException : TapException 
    {
        public string Error { get; internal set; }

        public string Description { get; internal set; }

        public long Now { get; internal set; }

        public long ErrorCode {get; internal set;}

        public string ErrorMessage  { get; internal set; }

        public TapAchievementException(int code, string message) : base(code, message) { }

    }
}
