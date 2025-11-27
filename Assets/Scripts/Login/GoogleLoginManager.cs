using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class GoogleLoginManager : MonoBehaviour
{
    [Header("Google OAuth")]
    [SerializeField] private string clientId = "";
    [SerializeField] private string scopes = "openid email profile";

    
    [SerializeField] private string redirectUriTemplate = "";

    [Header("Debug")]
    [SerializeField] private bool logDebug = true;

    private bool loginInProgress;
    public bool IsLoginInProgress => loginInProgress;

    private Action<bool, string> callback;

    private HttpListener httpListener;
    private Thread listenerThread;

    private string receivedCode;
    private string receivedError;
    private bool authFinished;

    private void OnDestroy()
    {
        StopListener();
    }

    public void StartGoogleSignIn(Action<bool, string> onCompleted)
    {
        if (loginInProgress)
        {
            Log("Sign-in already in progress.");
            return;
        }

        if (string.IsNullOrEmpty(clientId))
        {
            onCompleted?.Invoke(false, "Missing Google Client ID.");
            return;
        }

        callback = onCompleted;
        loginInProgress = true;
        authFinished = false;
        receivedCode = null;
        receivedError = null;

        int port = GetFreePort();
        string redirectUri = string.Format(redirectUriTemplate, port);

        StartListener(redirectUri);

        string authUrl = BuildGoogleAuthUrl(redirectUri);
        Log("Opening Google auth URL:\n" + authUrl);

        Application.OpenURL(authUrl);

        StartCoroutine(PollForAuthResult());
    }

    private IEnumerator PollForAuthResult()
    {
        float timeout = 180f; // 3 minutes
        float elapsed = 0f;

        while (!authFinished && elapsed < timeout)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        StopListener();

        if (!authFinished)
        {
            loginInProgress = false;
            callback?.Invoke(false, "Login timed out or was cancelled.");
            yield break;
        }

        if (!string.IsNullOrEmpty(receivedError))
        {
            loginInProgress = false;
            callback?.Invoke(false, "Google error: " + receivedError);
            yield break;
        }

        if (!string.IsNullOrEmpty(receivedCode))
        {
            loginInProgress = false;
            callback?.Invoke(true, "Google login completed. Code received.");
        }
        else
        {
            loginInProgress = false;
            callback?.Invoke(false, "No authorization code received.");
        }
    }

    private void StartListener(string redirectUri)
    {
        try
        {
            Uri uri = new Uri(redirectUri);
            string prefix = $"{uri.Scheme}://{uri.Host}:{uri.Port}/";

            httpListener = new HttpListener();
            httpListener.Prefixes.Add(prefix);
            httpListener.Start();

            listenerThread = new Thread(HandleIncomingRequests);
            listenerThread.IsBackground = true;
            listenerThread.Start();

            Log("Started HTTP listener at " + prefix);
        }
        catch (Exception ex)
        {
            Log("Failed to start HTTP listener: " + ex);
        }
    }

    private void StopListener()
    {
        try
        {
            if (httpListener != null)
            {
                httpListener.Stop();
                httpListener.Close();
                httpListener = null;
            }

            if (listenerThread != null && listenerThread.IsAlive)
            {
                listenerThread.Abort();
                listenerThread = null;
            }
        }
        catch (Exception ex)
        {
            Log("Error stopping listener: " + ex);
        }
    }

    private void HandleIncomingRequests()
    {
        try
        {
            while (httpListener != null && httpListener.IsListening)
            {
                HttpListenerContext context = httpListener.GetContext(); 

                string query = context.Request.Url.Query;
                Dictionary<string, string> queryParams = ParseQueryString(query);

                if (queryParams.TryGetValue("code", out var code))
                {
                    receivedCode = code;
                    Log("Received auth code from Google.");
                }

                if (queryParams.TryGetValue("error", out var error))
                {
                    receivedError = error;
                    Log("Received error from Google: " + error);
                }

                string html =
                    "<html><body><h2>Login complete</h2>" +
                    "<p>You can close this window and return to the game.</p></body></html>";

                byte[] buffer = Encoding.UTF8.GetBytes(html);
                context.Response.ContentLength64 = buffer.Length;
                context.Response.ContentType = "text/html; charset=utf-8";
                context.Response.OutputStream.Write(buffer, 0, buffer.Length);
                context.Response.OutputStream.Close();

                authFinished = true;
                break;
            }
        }
        catch (Exception ex)
        {
            Log("Listener exception: " + ex);
        }
    }

    private Dictionary<string, string> ParseQueryString(string query)
{
    var dict = new Dictionary<string, string>();

    if (string.IsNullOrEmpty(query))
        return dict;

    if (query.StartsWith("?"))
        query = query.Substring(1);

    string[] pairs = query.Split('&');

    foreach (var pair in pairs)
    {
        if (string.IsNullOrEmpty(pair)) continue;

        string[] kv = pair.Split('=');
        string key = Uri.UnescapeDataString(kv[0]);

        string value = kv.Length > 1
            ? Uri.UnescapeDataString(kv[1])
            : string.Empty;

        dict[key] = value;
    }

    return dict;
}


    private string BuildGoogleAuthUrl(string redirectUri)
    {
        string baseUrl = "https://accounts.google.com/o/oauth2/v2/auth";

        string encodedRedirect = Uri.EscapeDataString(redirectUri);
        string encodedClientId = Uri.EscapeDataString(clientId);
        string encodedScopes   = Uri.EscapeDataString(scopes);

        string url = $"{baseUrl}" +
                     $"?client_id={encodedClientId}" +
                     $"&redirect_uri={encodedRedirect}" +
                     $"&response_type=code" +
                     $"&scope={encodedScopes}" +
                     $"&access_type=offline" +
                     $"&include_granted_scopes=true";

        return url;
    }

    private int GetFreePort()
    {
        TcpListener l = new TcpListener(IPAddress.Loopback, 0);
        l.Start();
        int port = ((IPEndPoint)l.LocalEndpoint).Port;
        l.Stop();
        return port;
    }

    private void Log(string msg)
    {
        if (logDebug)
            Debug.Log("[GoogleLoginManager] " + msg);
    }
}
