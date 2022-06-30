using UnityEngine;

namespace pow.aidkit
{
    public class AdvertisementIdController : Singleton<AdvertisementIdController>
    {
        public string GetAdvertisingId()
        {
            string advertisingId = "DEFAULT";

            AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject jo = jc.GetStatic<AndroidJavaObject>("currentActivity");

            AndroidJavaClass jc2 = new AndroidJavaClass("com.google.android.gms.ads.identifier.AdvertisingIdClient");
            AndroidJavaObject jo2 = jc2.CallStatic<AndroidJavaObject>("getAdvertisingIdInfo", jo);
            if (jo2 != null)
            {
                // Get advertising id：
                advertisingId = jo2.Call<string>("getId");
                if (string.IsNullOrEmpty(advertisingId))
                    advertisingId = "none";

                // Get ad tracking status ： When it comes to false when , You can't push ads according to user behavior , But the number of advertisements seen will not decrease 
                //adTrackLimited = jo2.Call<bool>("isLimitAdTrackingEnabled");
            }

            return advertisingId;
        }
    }
}