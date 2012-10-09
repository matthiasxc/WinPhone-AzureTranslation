using My_Translation_App.Models;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization.Json;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows;
using System.IO.IsolatedStorage;

namespace My_Translation_App.Services
{
    public class TokenService
    {
private static readonly string CLIENT_ID = "My_Translation_App";
private static readonly string CLIENT_SECRET = "[Go get your own]";
private static readonly string OAUTH_URI = "https://datamarket.accesscontrol.windows.net/v2/OAuth2-13";

        public void GetToken()
        {
            if (IsolatedStorageSettings.ApplicationSettings.Contains("admAccessToken"))
            {
                AdmAccessToken savedToken = (AdmAccessToken)IsolatedStorageSettings.ApplicationSettings["admAccessToken"];
                if (!savedToken.IsExpired())
                {
                    RaiseAccessTokenComplete(true, savedToken);
                    return;
                }
            }

            // Create our HTTP request
            WebRequest request = WebRequest.Create(OAUTH_URI);
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";

            //IAsyncResult postCallback = (IAsyncResult)request.BeginGetRequestStream(new AsyncCallback(RequestStreamReady), request);
            request.BeginGetRequestStream(new AsyncCallback(RequestStreamReady), request);
        }

        private void RequestStreamReady(IAsyncResult asyncResult)
        {
            try
            {
                // Create the data we're going to write into the POST body
                string clientID = CLIENT_ID;
                string clientSecret = CLIENT_SECRET;
                string scope = "scope=" + HttpUtility.UrlEncode("http://api.microsofttranslator.com");
                string grant_type = "grant_type=" + HttpUtility.UrlEncode("client_credentials");
                String postBody = string.Format("{0}&client_id={1}&client_secret={2}&{3}", grant_type, HttpUtility.UrlEncode(clientID), HttpUtility.UrlEncode(clientSecret), scope);

                // Write the data to the POST body
                HttpWebRequest request = (HttpWebRequest)asyncResult.AsyncState;
                byte[] bytes = System.Text.Encoding.UTF8.GetBytes(postBody);
                Stream postStream = request.EndGetRequestStream(asyncResult);
                postStream.Write(bytes, 0, bytes.Length);
                postStream.Close();

                // Get the response (including the token)
                request.BeginGetResponse(new AsyncCallback(GetTokenResponseCallback), request);
            }
            catch (WebException webExc)
            {
                RaiseAccessTokenComplete(false, null);
            }
        }

        private void GetTokenResponseCallback(IAsyncResult asyncResult)
        {                    
            try
            {
                HttpWebRequest endRequest = (HttpWebRequest)asyncResult.AsyncState;
                HttpWebResponse response = (HttpWebResponse)endRequest.EndGetResponse(asyncResult);
                DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(AdmAccessToken));
                AdmAccessToken token = (AdmAccessToken)serializer.ReadObject(response.GetResponseStream());

                // Set the token so that it will begin the expiration process
                token.Initalize();
                // Let's set this token to be accessible on the app level
                App.SetTranslationToken(token);
                // And save the token to our app settings
                if (IsolatedStorageSettings.ApplicationSettings.Contains("admAccessToken"))
                    IsolatedStorageSettings.ApplicationSettings["admAccessToken"] = token;
                else
                    IsolatedStorageSettings.ApplicationSettings.Add("admAccessToken", token);
                IsolatedStorageSettings.ApplicationSettings.Save();
                // Finally, we're ready to send our token out into the wild
                RaiseAccessTokenComplete(true, token);
            }
            catch (WebException webExc)
            {
                RaiseAccessTokenComplete(false, null);
            }
        }        

        public event EventHandler<TokenServiceCompleteEventArgs> AccessTokenComplete;
        private void RaiseAccessTokenComplete(bool isSuccess, AdmAccessToken token)
        {
            if(AccessTokenComplete!=null)
                AccessTokenComplete(this, new TokenServiceCompleteEventArgs(isSuccess, token));
        }
            }

        public class TokenServiceCompleteEventArgs : EventArgs
        {
            public TokenServiceCompleteEventArgs(bool isSuccess, AdmAccessToken token)
            {
                IsSuccess = isSuccess;
                TranslationToken = token;
            }

            public bool IsSuccess { get; private set; }
            public AdmAccessToken TranslationToken {get; private set;}
        } 
}
