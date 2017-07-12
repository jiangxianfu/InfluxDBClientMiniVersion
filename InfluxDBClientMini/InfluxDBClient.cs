using System;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;

namespace InfluxDBClientMiniVersion
{
    public class InfluxDBClient : IDisposable
    {
        private HttpClient client;
        private string influxurl;
        private string dbusername;
        private string dbpassword;
        private int retries;


        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (client != null)
            {
                client.Dispose();
                client = null;
            }
        }

        public InfluxDBClient(string InfluxUrl, int Retries = 3)
            : this(InfluxUrl, null, null, Retries)
        {

        }
        public InfluxDBClient(string InfluxUrl, string DBUserName, string DBPassowrd, int Retries = 3)
        {
            influxurl = InfluxUrl;
            dbusername = DBUserName;
            dbpassword = DBPassowrd;
            retries = Retries;

            client = new HttpClient();

            if (!(String.IsNullOrWhiteSpace(dbusername) && String.IsNullOrWhiteSpace(dbpassword)))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic",
                    Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes(string.Format("{0}:{1}", dbusername, dbpassword))));
            }
            client.DefaultRequestHeaders.ConnectionClose = false;
        }

        public bool Write(string urlparttern, string lines)
        {
            string url = string.Format("{0}/write?{1}", influxurl, urlparttern);
            ByteArrayContent bytesContent = new ByteArrayContent(Encoding.UTF8.GetBytes(lines));
            int retry = 0;
            while (retry < retries)
            {
                retry++;
                try
                {
                    HttpResponseMessage response = client.PostAsync(url, bytesContent).Result;
                    if (response.StatusCode == HttpStatusCode.NoContent)
                    {
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    Thread.Sleep(50);
                }
            }
            return false;
        }
        public HttpResponseMessage Query(string urlparttern, HttpCompletionOption completion = HttpCompletionOption.ResponseContentRead)
        {
            string url = string.Format("{0}/query?{1}", influxurl, urlparttern);
            try
            {
                HttpResponseMessage response = client.GetAsync(url, completion).Result;

                if (response != null && response.StatusCode == HttpStatusCode.OK)
                {
                    return response;
                }
            }
            catch (Exception ex)
            {
            }
            return null;
        }
    }
    public static class InfluxDBExtensions
    {
        static readonly DateTime Origin = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        public static DateTime FromEpoch(this string time, TimePrecision precision)
        {
            long duration = long.Parse(time);
            DateTime t = Origin;
            switch (precision)
            {
                case TimePrecision.Hours: return t.AddHours(duration);
                case TimePrecision.Minutes: return t.AddMinutes(duration);
                case TimePrecision.Seconds: return t.AddSeconds(duration);
                case TimePrecision.Milliseconds: return t.AddMilliseconds(duration);
                case TimePrecision.Microseconds: return t.AddTicks(duration * TimeSpan.TicksPerMillisecond * 1000);
                case TimePrecision.Nanoseconds: return t.AddTicks(duration / 100); //1 tick = 100 nano sec
            }
            return t;
        }

        public static long ToEpoch(this DateTime time, TimePrecision precision)
        {
            TimeSpan t = time - Origin;
            switch (precision)
            {
                case TimePrecision.Hours: return (long)t.TotalHours;
                case TimePrecision.Minutes: return (long)t.TotalMinutes;
                case TimePrecision.Seconds: return (long)t.TotalSeconds;
                case TimePrecision.Milliseconds: return (long)t.TotalMilliseconds;
                case TimePrecision.Microseconds: return (long)(t.TotalMilliseconds * 1000);
                case TimePrecision.Nanoseconds:
                default: return (long)t.Ticks * 100; //1 tick = 100 nano sec
            }
        }
        public static string EscapeTag(this string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                return value.Replace(' ', '_').Replace("\\", "\\\\");
            }
            return "";
        }
    }

    public enum TimePrecision
    {
        Hours = 1,
        Minutes = 2,
        Seconds = 3,
        Milliseconds = 4,
        Microseconds = 5,
        Nanoseconds = 6
    }
}
