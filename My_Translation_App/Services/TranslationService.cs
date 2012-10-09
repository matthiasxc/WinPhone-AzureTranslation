using My_Translation_App.Models;
using System;
using System.Collections.Generic;
using System.Xml.Linq;
using System.Linq;
using System.Net;
using System.Text;
using System.IO;

namespace My_Translation_App.Services
{
    public class TranslationService
    {
        private TokenService _tokenService;
        private string _originalText;
        private string _sourceLanguage;
        private string _targetLanguage;

        public TranslationService()
        {
            _tokenService = new TokenService();
            _tokenService.AccessTokenComplete += _tokenService_AccessTokenComplete;
        }

        public void GetTranslation(string originalText, string sourceLanguage, string targetLanguage)
        {
            _originalText = originalText;
            _sourceLanguage = sourceLanguage;
            _targetLanguage = targetLanguage;

            _tokenService.GetToken();
        }

        void _tokenService_AccessTokenComplete(object sender, TokenServiceCompleteEventArgs e)
        {
            if (e.IsSuccess)
                StartTranslationWithToken(e.TranslationToken);
            else
                RaiseTranslationFailed("There was a problem securing an access token");
        }

        private void StartTranslationWithToken(AdmAccessToken token)
        {
            string translateUri = string.Format("http://api.microsofttranslator.com/v2/Http.svc/Translate?text={0}&from={1}&to={2}",
                HttpUtility.UrlEncode(_originalText), 
                HttpUtility.UrlEncode(_sourceLanguage), 
                HttpUtility.UrlEncode(_targetLanguage));

            WebRequest translationRequest = HttpWebRequest.Create(translateUri);

            // We need to put our access token into the Authorization header 
            //    with "Bearer " preceeding it.
            string bearerHeader = "Bearer " + token.access_token;
            translationRequest.Headers["Authorization"] = bearerHeader;

            // Finally call our translation service

            translationRequest.BeginGetResponse(asyncResult =>
            {
                try
                {
                    HttpWebRequest request = (HttpWebRequest)asyncResult.AsyncState;

                    HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(asyncResult);

                    // Read the contents of the response into a string
                    Stream streamResponse = response.GetResponseStream();
                    StreamReader streamRead = new StreamReader(streamResponse);
                    string translationData = streamRead.ReadToEnd();

                    // Read the XML return from the translator
                    //  you can get a preview of this XML if you go to your Azure
                    //  account, click on My Data on the left and then on "Use" on
                    //  Microsoft Translator. run a trial query and then click the 
                    //  XML button at the top of the query tool.
                    
                    // You'll need to add "System.XML.Linq" to your project to use XDocument
                    XDocument translationXML = XDocument.Parse(translationData);
                    string translationText = translationXML.Root.FirstNode.ToString();

                    RaiseTranslationComplete(_originalText, _sourceLanguage, _targetLanguage, translationText);
                }
                catch (WebException webExc)
                {
                    RaiseTranslationFailed(webExc.Status.ToString());
                }
            }, translationRequest);                                        
        }

        public event EventHandler<TranslationCompleteEventArgs> TranslationComplete;
        private void RaiseTranslationComplete(string originalText, string fromLang,
                                                string toLang, string translation)
        {
            if (TranslationComplete != null)
                TranslationComplete(this, new TranslationCompleteEventArgs(originalText, fromLang, toLang, translation));
        }

        public event EventHandler<TranslationFailedEventArgs> TranslationFailed;
        private void RaiseTranslationFailed(string error)
        {
            if(TranslationFailed != null)
                TranslationFailed(this, new TranslationFailedEventArgs(error));
        }
            }

        public class TranslationCompleteEventArgs : EventArgs
        {
            public TranslationCompleteEventArgs(string originalText, string fromLang, string toLang, string translation)
            {
                OriginalText = originalText;
                FromLanguage = fromLang;
                ToLanguage = toLang;
                Translation = translation;
            }
        
            public string OriginalText { get; private set; }
            public string FromLanguage { get; private set; }
            public string ToLanguage { get; private set; }
            public string Translation { get; private set; }
        }

        public class TranslationFailedEventArgs :EventArgs
        {
            public TranslationFailedEventArgs(string error)
            {
                ErrorDescription = error;
            }

            public string ErrorDescription { get; private set; }
        }
}
