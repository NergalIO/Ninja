using System;
using UnityEngine;

namespace Ninja.Core
{
    public static class TimeUtils
    {
        /// <summary>
        /// Форматирует время в секундах в красивую строку
        /// </summary>
        /// <param name="seconds">Время в секундах</param>
        /// <param name="showMilliseconds">Показывать ли миллисекунды (по умолчанию false)</param>
        /// <returns>Отформатированная строка времени (например: "1:23", "2:45:30", "0:05")</returns>
        public static string FormatTime(float seconds, bool showMilliseconds = false)
        {
            if (seconds < 0)
                seconds = 0;

            int totalSeconds = Mathf.FloorToInt(seconds);
            int hours = totalSeconds / 3600;
            int minutes = (totalSeconds % 3600) / 60;
            int secs = totalSeconds % 60;

            if (showMilliseconds)
            {
                int milliseconds = Mathf.FloorToInt((seconds - totalSeconds) * 100);
                
                if (hours > 0)
                {
                    return $"{hours}:{minutes:D2}:{secs:D2}.{milliseconds:D2}";
                }
                else if (minutes > 0)
                {
                    return $"{minutes}:{secs:D2}.{milliseconds:D2}";
                }
                else
                {
                    return $"{secs}.{milliseconds:D2}";
                }
            }
            else
            {
                if (hours > 0)
                {
                    return $"{hours}:{minutes:D2}:{secs:D2}";
                }
                else if (minutes > 0)
                {
                    return $"{minutes}:{secs:D2}";
                }
                else
                {
                    return $"0:{secs:D2}";
                }
            }
        }

        /// <summary>
        /// Форматирует время в секундах в краткую строку (без ведущих нулей для минут)
        /// </summary>
        /// <param name="seconds">Время в секундах</param>
        /// <returns>Отформатированная строка времени (например: "1:23", "45:30", "5")</returns>
        public static string FormatTimeShort(float seconds)
        {
            if (seconds < 0)
                seconds = 0;

            int totalSeconds = Mathf.FloorToInt(seconds);
            int hours = totalSeconds / 3600;
            int minutes = (totalSeconds % 3600) / 60;
            int secs = totalSeconds % 60;

            if (hours > 0)
            {
                return $"{hours}:{minutes:D2}:{secs:D2}";
            }
            else if (minutes > 0)
            {
                return $"{minutes}:{secs:D2}";
            }
            else
            {
                return secs.ToString();
            }
        }

        /// <summary>
        /// Форматирует время в секундах в строку с текстовыми единицами
        /// </summary>
        /// <param name="seconds">Время в секундах</param>
        /// <returns>Отформатированная строка времени (например: "1 мин 23 сек", "2 ч 45 мин")</returns>
        public static string FormatTimeText(float seconds)
        {
            if (seconds < 0)
                seconds = 0;

            int totalSeconds = Mathf.FloorToInt(seconds);
            int hours = totalSeconds / 3600;
            int minutes = (totalSeconds % 3600) / 60;
            int secs = totalSeconds % 60;

            if (hours > 0)
            {
                if (minutes > 0)
                {
                    return $"{hours} ч {minutes} мин";
                }
                else
                {
                    return $"{hours} ч";
                }
            }
            else if (minutes > 0)
            {
                if (secs > 0)
                {
                    return $"{minutes} мин {secs} сек";
                }
                else
                {
                    return $"{minutes} мин";
                }
            }
            else
            {
                return $"{secs} сек";
            }
        }
    }
}

