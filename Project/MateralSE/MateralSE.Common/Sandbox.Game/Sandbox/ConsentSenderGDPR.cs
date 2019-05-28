namespace Sandbox
{
    using Sandbox.Engine.Networking;
    using System;
    using System.IO;
    using System.Net;
    using System.Text;
    using VRage.Utils;

    public static class ConsentSenderGDPR
    {
        private static void HandleConsentResponse(IAsyncResult asynchronousResult)
        {
            try
            {
                string str = string.Empty;
                using (StreamReader reader = new StreamReader(((WebRequest) asynchronousResult.AsyncState).EndGetResponse(asynchronousResult).GetResponseStream()))
                {
                    str = reader.ReadToEnd();
                }
                if (str.Replace("\r", "").Replace("\n", "") == "OK")
                {
                    MySandboxGame.Config.GDPRConsentSent = true;
                    MySandboxGame.Config.Save();
                }
            }
            catch
            {
                MySandboxGame.Config.GDPRConsentSent = false;
                MySandboxGame.Config.Save();
            }
        }

        internal static void TrySendConsent()
        {
            try
            {
                ServicePointManager.SecurityProtocol = (ServicePointManager.SecurityProtocol | SecurityProtocolType.Tls11) | SecurityProtocolType.Tls12;
                WebRequest state = WebRequest.Create("https://gdpr.keenswh.com/consent.php");
                ((HttpWebRequest) state).UserAgent = "Space Engineers Client";
                string s = "lcvbex=se" + "&qudfgh=" + MyGameService.UserId;
                s = !MySandboxGame.Config.GDPRConsent.Value ? (s + "&praqnf=disagree") : (s + "&praqnf=agree");
                byte[] bytes = Encoding.ASCII.GetBytes(s);
                state.Method = "POST";
                state.ContentType = "application/x-www-form-urlencoded";
                state.ContentLength = bytes.Length;
                state.Timeout = 0x1388;
                using (Stream stream = state.GetRequestStream())
                {
                    stream.Write(bytes, 0, bytes.Length);
                }
                state.BeginGetResponse(new AsyncCallback(ConsentSenderGDPR.HandleConsentResponse), state);
            }
            catch (Exception exception)
            {
                MyLog.Default.WriteLine("Cannot confirm GDPR consent: " + exception);
            }
        }
    }
}

