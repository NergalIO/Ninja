using UnityEngine;

namespace Ninja.Core
{
    public static class TimeUtils
    {
        public static string FormatTime(float seconds, bool showMs = false)
        {
            seconds = Mathf.Max(0, seconds);
            int total = Mathf.FloorToInt(seconds);
            int h = total / 3600;
            int m = (total % 3600) / 60;
            int s = total % 60;

            if (showMs)
            {
                int ms = Mathf.FloorToInt((seconds - total) * 100);
                if (h > 0) return $"{h}:{m:D2}:{s:D2}.{ms:D2}";
                if (m > 0) return $"{m}:{s:D2}.{ms:D2}";
                return $"{s}.{ms:D2}";
            }

            if (h > 0) return $"{h}:{m:D2}:{s:D2}";
            return $"{m}:{s:D2}";
        }

        public static string FormatTimeText(float seconds)
        {
            seconds = Mathf.Max(0, seconds);
            int total = Mathf.FloorToInt(seconds);
            int h = total / 3600;
            int m = (total % 3600) / 60;
            int s = total % 60;

            if (h > 0) return m > 0 ? $"{h} ч {m} мин" : $"{h} ч";
            if (m > 0) return s > 0 ? $"{m} мин {s} сек" : $"{m} мин";
            return $"{s} сек";
        }
    }
}
