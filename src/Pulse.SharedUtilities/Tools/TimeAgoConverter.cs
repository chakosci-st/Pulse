using System;

public class TimeAgoConverter
{
    public static string ConvertToTimeAgo(DateTime dateTime)
    {
        TimeSpan timeSpan = DateTime.Now - dateTime;

        if (timeSpan.TotalSeconds < 60)
        {
            return $"{(int)timeSpan.TotalSeconds} seconds ago";
        }
        else if (timeSpan.TotalMinutes < 60)
        {
            return $"{(int)timeSpan.TotalMinutes} mins ago";
        }
        else if (timeSpan.TotalHours < 24)
        {
            return $"{(int)timeSpan.TotalHours} hrs ago";
        }
        else if (timeSpan.TotalDays < 30)
        {
            return $"{(int)timeSpan.TotalDays} days ago";
        }
        else if (timeSpan.TotalDays < 365)
        {
            return $"{(int)(timeSpan.TotalDays / 30)} mons ago";
        }
        else
        {
            return $"{(int)(timeSpan.TotalDays / 365)} yrs ago";
        }
    }


}