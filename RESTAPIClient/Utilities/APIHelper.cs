using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace RESTAPIClient
{
    public class APIHelper
    {
        #region private members
        private const string HTTP_POST = "POST";
        #endregion private members

        #region private methods
        private string GetFormatedHTTPPostParameters(Dictionary<string, string> parameters)
        {
            StringBuilder parameterBuilder = new StringBuilder();
            parameterBuilder.Append("{");
            foreach (var parameter in parameters)
            {
                parameterBuilder.Append(string.Format("\"{0}\":\"{1}\"", parameter.Key, parameter.Value)).Append(",");
            }
            parameterBuilder.Append("}");
            return parameterBuilder.ToString().Replace(",}", "}");
        }

        public string GetFormattedHTTPGetUrl(string Url, Dictionary<string, string> parameters)
        {
            StringBuilder parameterBuilder = new StringBuilder();
            parameterBuilder.Append(Url);
            if (parameters != null && parameters.Count != 0)
            {
                parameterBuilder.Append("?");
                foreach (var parameter in parameters)
                {
                    parameterBuilder.Append(string.Format("{0}={1}", parameter.Key, parameter.Value)).Append("&");
                }
            }

            return parameterBuilder.ToString().TrimEnd('&');
        }

        private void AddHeaders(WebClient client, Dictionary<string, string> headers)
        {
            if (headers != null)
            {
                foreach (var header in headers)
                {
                    client.Headers.Add(header.Key, header.Value);
                }
            }
        }
        #endregion private methods

        #region public methods
        public string Get(string URL, Dictionary<string, string> parameters, Dictionary<string, string> headers)
        {
            string Products = null;
            using (WebClient client = new WebClient())
            {
                AddHeaders(client, headers);
                string FormattedUrl = GetFormattedHTTPGetUrl(URL, parameters);
                Logger.LogMessage(string.Format("Get Request Url: {0}", FormattedUrl));
                byte[] responseData = client.DownloadData(FormattedUrl);
                Products = System.Text.Encoding.Default.GetString(responseData);
            }
            return Products;
        }

        public string Post(string Url, Dictionary<string, string> parameters, Dictionary<string, string> headers)
        {
            string Result = null;
            using (WebClient client = new WebClient())
            {
                AddHeaders(client, headers);
                string FormattedPostParameters = GetFormatedHTTPPostParameters(parameters);
                Logger.LogMessage(string.Format("Post Request Url {0}, Formatted Post Parameters {1}", Url, FormattedPostParameters));
                byte[] requestData = Encoding.ASCII.GetBytes(FormattedPostParameters);
                byte[] responseData = client.UploadData(Url, HTTP_POST, requestData);
                Result = Encoding.Default.GetString(responseData);
            }
            return Result;
        }

        public async Task<string> GetAsync(string URL, Dictionary<string, string> parameters, Dictionary<string, string> headers)
        {
            string response = null;
            using (HttpClient oHttpClient = new HttpClient())
            {
                oHttpClient.DefaultRequestHeaders.Clear();
                oHttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                foreach (var header in headers)
                {
                    oHttpClient.DefaultRequestHeaders.Add(header.Key, header.Value);
                }
                HttpResponseMessage res = await oHttpClient.GetAsync(GetFormattedHTTPGetUrl(URL, parameters));
                if (res.IsSuccessStatusCode)
                {
                    response = await res.Content.ReadAsStringAsync();
                }
            }
            return response;
        }

        public bool DownloadRemoteImageFile(string uri, string fileName)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
            HttpWebResponse response;
            try
            {
                response = (HttpWebResponse)request.GetResponse();
            }
            catch (Exception)
            {
                return false;
            }

            // Check that the remote file was found. The ContentType
            // check is performed since a request for a non-existent
            // image file might be redirected to a 404-page, which would
            // yield the StatusCode "OK", even though the image was not
            // found.
            if ((response.StatusCode == HttpStatusCode.OK ||
                response.StatusCode == HttpStatusCode.Moved ||
                response.StatusCode == HttpStatusCode.Redirect) &&
                response.ContentType.StartsWith("image", StringComparison.OrdinalIgnoreCase))
            {

                // if the remote file was found, download it
                using (Stream inputStream = response.GetResponseStream())
                using (Stream outputStream = File.OpenWrite(fileName))
                {
                    byte[] buffer = new byte[4096];
                    int bytesRead;
                    do
                    {
                        bytesRead = inputStream.Read(buffer, 0, buffer.Length);
                        outputStream.Write(buffer, 0, bytesRead);
                    } while (bytesRead != 0);
                }
                return true;
            }
            else
                return false;
        }
        #endregion public methods
    }
}
