using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using My_Translation_App.Services;

namespace My_Translation_App
{
    public partial class MainPage : PhoneApplicationPage
    {
        TranslationService _translator;

        public MainPage()
        {
            InitializeComponent();
            _translator = new TranslationService();
            _translator.TranslationComplete += _translator_TranslationComplete;
            _translator.TranslationFailed += _translator_TranslationFailed;
        }
        
        void _translator_TranslationComplete(object sender, TranslationCompleteEventArgs e)
        {
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                targetTextBox.Text = e.Translation;
            });
        }

        void _translator_TranslationFailed(object sender, TranslationFailedEventArgs e)
        {
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                MessageBox.Show("Bummer, the translation failed. \n " + e.ErrorDescription);
            });
        }

        private void On_CheckClicked(object sender, System.EventArgs e)
        {
            _translator.GetTranslation(sourceTextBox.Text, "en", "es");
        }
    }
}