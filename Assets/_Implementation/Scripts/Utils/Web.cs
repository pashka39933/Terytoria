using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System;

namespace Utils
{

    /// <summary>
    /// Web.
    /// <para>description</para>
    /// </summary>
    public class Web : MonoBehaviour
    {

        #region instance_management
        /// <summary>
        /// The instance.
        /// </summary>
        private static Web instance;

        /// <summary>
        /// Initializes a new instance of the <see cref="Utils.Web"/> class.
        /// </summary>
        Web()
        {
            Debug.Log("[Utils - Web] Initializing...");
            instance = this;
        }
        #endregion

        #region public_methods
        /// <summary>
        /// Gets the text.
        /// </summary>
        /// <param name="url">URL.</param>
        /// <param name="callback">Callback.</param>
        /// <param name="headers">Headers - optional.</param>
        public static void GetText(string url, System.Action<bool, string> callback, List<KeyValuePair<string, string>> headers = null)
        {

            if (instance == null)
            {
                Debug.LogError("[Utils - Web] Utils not initialized! Place Utils prefab in first scene!");
                return;
            }

            instance.MakeWebRequestMethod(url, response =>
            {

                // Analytics - reporting failed request
                if (response.responseCode != 200)
                {
                    string requestHeaders = "";
                    if (headers != null)
                    {
                        foreach (KeyValuePair<string, string> header in headers)
                            requestHeaders = requestHeaders + header.Key + " => " + header.Value + "; ";
                    }
                    AnalyticsController.instance.Report("WebRequestFail", new Dictionary<string, object>() {
                        { "method", "PostValues" },
                        { "url", url },
                        { "requestParams", "" },
                        { "requestHeaders", requestHeaders },
                        { "responseCode", response.responseCode },
                        { "responseText", System.Text.Encoding.Default.GetString(response.downloadedData) },
                    });
                }

                if (response.success && response.responseCode < 400)
                {
                    try
                    {
                        Debug.Log("[Utils - Web] GetText response: " + System.Text.Encoding.Default.GetString(response.downloadedData));
                        callback(true, System.Text.Encoding.Default.GetString(response.downloadedData));
                    }
                    catch (Exception e)
                    {
                        Debug.LogError("[Utils - Web] GetText error response: " + System.Text.Encoding.Default.GetString(response.downloadedData));
                        Debug.LogError("[Utils - Web] GetText error: " + e.StackTrace);
                        callback(false, "");
                    }
                }
                else
                {
                    callback(false, "");
                }

            }, headers);

        }

        /// <summary>
        /// Gets the sprite.
        /// </summary>
        /// <param name="url">URL.</param>
        /// <param name="callback">Callback.</param>
        /// <param name="headers">Headers - optional.</param>
        public static void GetSprite(string url, System.Action<bool, Sprite> callback, List<KeyValuePair<string, string>> headers = null)
        {

            if (instance == null)
            {
                Debug.LogError("[Utils - Web] Utils not initialized! Place Utils prefab in first scene!");
                return;
            }

            instance.MakeWebRequestMethod(url, response =>
            {

                // Analytics - reporting failed request
                if (response.responseCode != 200)
                {
                    string requestHeaders = "";
                    if (headers != null)
                    {
                        foreach (KeyValuePair<string, string> header in headers)
                            requestHeaders = requestHeaders + header.Key + " => " + header.Value + "; ";
                    }
                    AnalyticsController.instance.Report("WebRequestFail", new Dictionary<string, object>() {
                        { "method", "PostValues" },
                        { "url", url },
                        { "requestParams", "" },
                        { "requestHeaders", requestHeaders },
                        { "responseCode", response.responseCode },
                        { "responseText", System.Text.Encoding.Default.GetString(response.downloadedData) },
                    });
                }

                if (response.success && response.responseCode < 400)
                {
                    try
                    {
                        Texture2D texture2D = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                        texture2D.LoadImage(response.downloadedData);
                        Sprite sprite = Sprite.Create(texture2D, new Rect(0, 0, texture2D.width, texture2D.height), new Vector2(0.5f, 0.5f));
                        callback(true, sprite);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError("[Utils - Web] GetSprite error response: " + System.Text.Encoding.Default.GetString(response.downloadedData));
                        Debug.LogError("[Utils - Web] GetSprite error: " + e.StackTrace);
                        callback(false, null);
                    }
                }
                else
                {
                    callback(false, null);
                }

            }, headers);

        }

        /// <summary>
        /// Gets the json.
        /// </summary>
        /// <param name="url">URL.</param>
        /// <param name="callback">Callback.</param>
        /// <param name="headers">Headers.</param>
        public static void GetJSON(string url, System.Action<bool, Dictionary<string, object>> callback, List<KeyValuePair<string, string>> headers = null)
        {

            if (instance == null)
            {
                Debug.LogError("[Utils - Web] Utils not initialized!");
                return;
            }

            instance.MakeWebRequestMethod(url, response =>
            {

                // Analytics - reporting failed request
                if (response.responseCode != 200)
                {
                    string requestHeaders = "";
                    if (headers != null)
                    {
                        foreach (KeyValuePair<string, string> header in headers)
                            requestHeaders = requestHeaders + header.Key + " => " + header.Value + "; ";
                    }
                    AnalyticsController.instance.Report("WebRequestFail", new Dictionary<string, object>() {
                        { "method", "PostValues" },
                        { "url", url },
                        { "requestParams", "" },
                        { "requestHeaders", requestHeaders },
                        { "responseCode", response.responseCode },
                        { "responseText", System.Text.Encoding.Default.GetString(response.downloadedData) },
                    });
                }

                if (response.success && response.responseCode < 400)
                {
                    try
                    {
                        string jsonString = System.Text.Encoding.Default.GetString(response.downloadedData);
                        Debug.Log("[Utils - Web] GetJSON response: " + jsonString);
                        callback(true, Utils.JSON.GetDictionary(jsonString));
                    }
                    catch (Exception e)
                    {
                        Debug.LogError("[Utils - Web] GetJSON error response: " + System.Text.Encoding.Default.GetString(response.downloadedData));
                        Debug.LogError("[Utils - Web] GetJSON error: " + e.StackTrace);
                        callback(false, null);
                    }
                }
                else
                {
                    callback(false, null);
                }

            }, headers);
        }

        /// <summary>
        /// Posts the values.
        /// </summary>
        /// <param name="url">URL.</param>
        /// <param name="callback">Callback.</param>
        /// <param name="parameters">Parameters.</param>
        /// <param name="headers">Headers - optional.</param>
        public static void PostValues(string url, List<KeyValuePair<string, string>> parameters, System.Action<long, string> callback, List<KeyValuePair<string, string>> headers = null)
        {

            if (instance == null)
            {
                Debug.LogError("[Utils - Web] Utils not initialized! Place Utils prefab in first scene!");
                return;
            }

            instance.MakeWebRequestMethod(url, parameters, response =>
            {

                // Analytics - reporting failed request
                if (response.responseCode != 200)
                {
                    string requestParams = "";
                    if (parameters != null)
                    {
                        foreach (KeyValuePair<string, string> parameter in parameters)
                            requestParams = requestParams + parameter.Key + " => " + parameter.Value + "; ";
                    }
                    string requestHeaders = "";
                    if (headers != null)
                    {
                        foreach (KeyValuePair<string, string> header in headers)
                            requestHeaders = requestHeaders + header.Key + " => " + header.Value + "; ";
                    }
                    AnalyticsController.instance.Report("WebRequestFail", new Dictionary<string, object>() {
                        { "method", "PostValues" },
                        { "url", url },
                        { "requestParams", requestParams },
                        { "requestHeaders", requestHeaders },
                        { "responseCode", response.responseCode },
                        { "responseText", System.Text.Encoding.Default.GetString(response.downloadedData) },
                    });
                }

                Debug.Log("[Utils - Web] PostValues response: [" + response.responseCode + "] " + System.Text.Encoding.Default.GetString(response.downloadedData));
                callback(response.responseCode, System.Text.Encoding.Default.GetString(response.downloadedData));

            }, headers);

        }
        #endregion

        #region private_methods
        /// <summary>
        /// Makes the web request method - GET.
        /// </summary>
        /// <param name="url">URL.</param>
        /// <param name="callback">Callback.</param>
        /// <param name="headers">Headers - optional.</param>
        private void MakeWebRequestMethod(string url, System.Action<RawWebResponse> callback, List<KeyValuePair<string, string>> headers = null)
        {

            StartCoroutine(MakeWebRequestCoroutine(url, response =>
            {
                callback(response);
            }, headers));

        }

        /// <summary>
        /// Makes the web request method - POST.
        /// </summary>
        /// <param name="url">URL.</param>
        /// <param name="callback">Callback.</param>
        /// <param name="parameters">Parameters.</param>
        /// <param name="headers">Headers - optional.</param>
        private void MakeWebRequestMethod(string url, List<KeyValuePair<string, string>> parameters, System.Action<RawWebResponse> callback, List<KeyValuePair<string, string>> headers = null)
        {

            StartCoroutine(MakeWebRequestCoroutine(url, parameters, response =>
            {
                callback(response);
            }, headers));

        }

        /// <summary>
        /// Makes the web request coroutine - GET.
        /// </summary>
        /// <returns>The web request coroutine.</returns>
        /// <param name="url">URL.</param>
        /// <param name="callback">Callback.</param>
        /// <param name="headers">Headers - optional.</param>
        private IEnumerator MakeWebRequestCoroutine(string url, System.Action<RawWebResponse> callback, List<KeyValuePair<string, string>> headers = null)
        {
            string log = "HTTP/GET\n" + url + "\n";
            using (UnityWebRequest www = UnityWebRequest.Get(url))
            {
                if (headers != null)
                {
                    log = log + "HEADERS\n";
                    foreach (KeyValuePair<string, string> header in headers)
                    {
                        log = log + header.Key + " => " + header.Value + "\n";
                        www.SetRequestHeader(header.Key, header.Value);
                    }
                }
                Debug.Log("[Utils - Web] " + log);
                www.SetRequestHeader("Authorization", AppConstants.ApiAuthHeader);
                yield return www.SendWebRequest();
                callback(new RawWebResponse(!www.isNetworkError, www.responseCode, www.downloadHandler.data));
            }

        }

        /// <summary>
        /// Makes the web request coroutine - POST.
        /// </summary>
        /// <returns>The web request coroutine.</returns>
        /// <param name="url">URL.</param>
        /// <param name="callback">Callback.</param>
        /// <param name="parameters">Parameters.</param>
        /// <param name="headers">Headers - optional.</param>
        private IEnumerator MakeWebRequestCoroutine(string url, List<KeyValuePair<string, string>> parameters, System.Action<RawWebResponse> callback, List<KeyValuePair<string, string>> headers = null)
        {

            string log = "HTTP/POST\n" + url + "\n";
            WWWForm form = new WWWForm();
            if (parameters != null)
            {
                log = log + "PARAMETERS\n";
                foreach (KeyValuePair<string, string> parameter in parameters)
                {
                    log = log + parameter.Key + " => " + parameter.Value + "\n";
                    form.AddField(parameter.Key, parameter.Value);
                }
            }

            using (UnityWebRequest www = UnityWebRequest.Post(url, form))
            {
                if (headers != null)
                {
                    log = log + "HEADERS\n";
                    foreach (KeyValuePair<string, string> header in headers)
                    {
                        log = log + header.Key + " => " + header.Value + "\n";
                        www.SetRequestHeader(header.Key, header.Value);
                    }
                }
                Debug.Log("[Utils - Web] " + log);
                www.SetRequestHeader("Authorization", AppConstants.ApiAuthHeader);
                yield return www.SendWebRequest();
                callback(new RawWebResponse(!www.isNetworkError, www.responseCode, www.downloadHandler.data));
            }

        }

        /// <summary>
        /// Raw web response.
        /// </summary>
        private struct RawWebResponse
        {

            public bool success;
            public long responseCode;
            public byte[] downloadedData;

            public RawWebResponse(bool success, long responseCode, byte[] downloadedData)
            {
                this.success = success;
                this.responseCode = responseCode;
                this.downloadedData = downloadedData;
            }
        }
        #endregion
    }

}