using System;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace InfluxDBClientMiniVersion
{
    public class InfluxDBClient : IDisposable
    {
        private HttpClient client;
        private string influxurl;
        private string dbusername;
        private string dbpassword;


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

        public InfluxDBClient(string InfluxUrl)
            : this(InfluxUrl, null, null)
        {

        }
        public InfluxDBClient(string InfluxUrl, string DBUserName, string DBPassowrd)
        {
            influxurl = InfluxUrl;
            dbusername = DBUserName;
            dbpassword = DBPassowrd;

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
            try
            {
                HttpResponseMessage response = client.PostAsync(url, bytesContent).Result;
                if (response.StatusCode == HttpStatusCode.NoContent)
                {
                    return true;
                }
                else
                {
                    Debug.Fail(response.StatusCode.ToString());
                }
            }
            catch (Exception ex)
            {
                Debug.Fail(ex.ToString());
            }
            return false;
        }
        public HttpResponseMessage Query(string urlparttern, HttpCompletionOption completion = HttpCompletionOption.ResponseContentRead)
        {
            string url = string.Format("{0}/query?{1}", influxurl, urlparttern);
            try
            {
                HttpResponseMessage response = client.GetAsync(url, completion).Result;
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    return response;
                }
                else
                {
                    Debug.Fail(response.StatusCode.ToString());
                }
            }
            catch (Exception ex)
            {
                Debug.Fail(ex.ToString());
            }
            return null;
        }
    }
}
