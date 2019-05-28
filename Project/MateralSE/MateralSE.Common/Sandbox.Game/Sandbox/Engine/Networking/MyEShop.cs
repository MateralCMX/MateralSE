namespace Sandbox.Engine.Networking
{
    using LitJson;
    using Microsoft.IdentityModel.Tokens;
    using Sandbox;
    using Sandbox.Engine.Platform;
    using Sandbox.Engine.Utils;
    using System;
    using System.Diagnostics;
    using System.IdentityModel.Tokens.Jwt;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Runtime.CompilerServices;
    using System.Security.Claims;
    using System.Text;
    using System.Threading.Tasks;
    using VRage.Utils;

    public class MyEShop
    {
        private const int SERVER_REQUEST_DELAY_MILLISEC = 100;
        private const int TOKEN_EXPIRATION_MINUTES = 5;
        private const string JWT_STEAM_TICKET_ATTRIBUTE = "ticketSteam";
        private const string JWT_SYMMETRIC_CIPHER = "N1F5Kn7yqWx3RQa9U29Iu1WpMOE04EKxyd6CHueSVb19Ot1C7us7cEt0D6yPLLAM";
        private const string HTTP_OP_GET = "rest/user/status/";
        private const string HTTP_OP_POST = "rest/user/";
        private static byte[] m_steamAuthTicketBuffer = new byte[0x400];
        private static HttpClient m_client = new HttpClient();
        private static Uri m_UriPOST = new Uri(MyPerGameSettings.EShopUrl + "rest/user/");
        private static Uri m_UriGET = new Uri(MyPerGameSettings.EShopUrl + "rest/user/status/");

        static MyEShop()
        {
            if (!Game.IsDedicated && ((MyFakes.FORCE_UPDATE_NEWSLETTER_STATUS || ((MySandboxGame.Config.NewsletterCurrentStatus == MyConfig.NewsletterStatus.NoFeedback) || (MySandboxGame.Config.NewsletterCurrentStatus == MyConfig.NewsletterStatus.Unknown))) || (MySandboxGame.Config.NewsletterCurrentStatus == MyConfig.NewsletterStatus.EmailNotConfirmed)))
            {
                CheckServerAndUpdateStatus();
            }
        }

        [AsyncStateMachine(typeof(<CheckServerAndUpdateStatus>d__15))]
        private static void CheckServerAndUpdateStatus()
        {
            <CheckServerAndUpdateStatus>d__15 d__;
            d__.<>t__builder = AsyncVoidMethodBuilder.Create();
            d__.<>1__state = -1;
            d__.<>t__builder.Start<<CheckServerAndUpdateStatus>d__15>(ref d__);
        }

        private static string GenerateToken(string steamTicket)
        {
            JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();
            DateTime utcNow = DateTime.UtcNow;
            SecurityTokenDescriptor tokenDescriptor = new SecurityTokenDescriptor();
            Claim[] claims = new Claim[] { new Claim("ticketSteam", steamTicket) };
            tokenDescriptor.Subject = new ClaimsIdentity(claims);
            tokenDescriptor.Expires = new DateTime?(utcNow.AddMinutes((double) Convert.ToInt32(5)));
            tokenDescriptor.SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(Convert.FromBase64String("N1F5Kn7yqWx3RQa9U29Iu1WpMOE04EKxyd6CHueSVb19Ot1C7us7cEt0D6yPLLAM")), "http://www.w3.org/2001/04/xmldsig-more#hmac-sha256");
            return handler.WriteToken(handler.CreateToken(tokenDescriptor));
        }

        private static string GetAuthenticatedTicket()
        {
            try
            {
                uint num;
                uint num2;
                if (MyGameService.GetAuthSessionTicket(out num2, m_steamAuthTicketBuffer, out num))
                {
                    return BitConverter.ToString(m_steamAuthTicketBuffer, 0, (int) num).Replace("-", "").ToLowerInvariant();
                }
            }
            catch
            {
            }
            return null;
        }

        private static HttpRequestMessage GetGETRequestMessage(string tokenTicket)
        {
            HttpRequestMessage message1 = new HttpRequestMessage();
            message1.RequestUri = m_UriGET;
            message1.Method = HttpMethod.Get;
            message1.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/plain"));
            message1.Headers.Add("Authorization", "Bearer " + tokenTicket);
            return message1;
        }

        private static HttpRequestMessage GetPOSTRequestMessage(string tokenTicket, string jsonString)
        {
            HttpRequestMessage message1 = new HttpRequestMessage();
            message1.RequestUri = m_UriPOST;
            message1.Method = HttpMethod.Post;
            message1.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/plain"));
            message1.Headers.Add("Authorization", "Bearer " + tokenTicket);
            message1.Content = new StringContent(jsonString, Encoding.UTF8, "application/json");
            return message1;
        }

        private static void ReadPlayerStatus(string jsonString)
        {
            if (!string.IsNullOrEmpty(jsonString))
            {
                JsonData data;
                try
                {
                    data = JsonMapper.ToObject(jsonString)["status"];
                }
                catch
                {
                    return;
                }
                if ((data != null) && data.IsString)
                {
                    string str = data.ToString();
                    if (str == "UNKNOWN")
                    {
                        MySandboxGame.Config.NewsletterCurrentStatus = MyConfig.NewsletterStatus.NoFeedback;
                    }
                    else if (str == "REFUSED")
                    {
                        MySandboxGame.Config.NewsletterCurrentStatus = MyConfig.NewsletterStatus.NotInterested;
                    }
                    else if (str == "UNCONFIRMED")
                    {
                        MySandboxGame.Config.NewsletterCurrentStatus = MyConfig.NewsletterStatus.EmailNotConfirmed;
                    }
                    else
                    {
                        if (str != "AGREED")
                        {
                            return;
                        }
                        MySandboxGame.Config.NewsletterCurrentStatus = MyConfig.NewsletterStatus.EmailConfirmed;
                    }
                    MySandboxGame.Config.Save();
                }
            }
        }

        [AsyncStateMachine(typeof(<SendInfo>d__14))]
        public static void SendInfo(string email)
        {
            <SendInfo>d__14 d__;
            d__.email = email;
            d__.<>t__builder = AsyncVoidMethodBuilder.Create();
            d__.<>1__state = -1;
            d__.<>t__builder.Start<<SendInfo>d__14>(ref d__);
        }

        public static bool ShowNewsletterScreenAtStartup =>
            (MySandboxGame.Config.NewsletterCurrentStatus == MyConfig.NewsletterStatus.NoFeedback);

        [CompilerGenerated]
        private struct <CheckServerAndUpdateStatus>d__15 : IAsyncStateMachine
        {
            public int <>1__state;
            public AsyncVoidMethodBuilder <>t__builder;
            private string <response>5__2;
            private HttpRequestMessage <request>5__3;
            private TaskAwaiter <>u__1;
            private TaskAwaiter<HttpResponseMessage> <>u__2;

            private void MoveNext()
            {
                int num = this.<>1__state;
                try
                {
                    if (num > 1)
                    {
                        string authenticatedTicket = MyEShop.GetAuthenticatedTicket();
                        if (!string.IsNullOrEmpty(authenticatedTicket))
                        {
                            string tokenTicket = MyEShop.GenerateToken(authenticatedTicket);
                            this.<response>5__2 = string.Empty;
                            this.<request>5__3 = MyEShop.GetGETRequestMessage(tokenTicket);
                        }
                        else
                        {
                            goto TR_0002;
                        }
                    }
                    try
                    {
                        int num2 = num;
                        try
                        {
                            HttpResponseMessage message;
                            TaskAwaiter awaiter;
                            TaskAwaiter<HttpResponseMessage> awaiter2;
                            if (num == 0)
                            {
                                awaiter = this.<>u__1;
                                this.<>u__1 = new TaskAwaiter();
                                this.<>1__state = num = -1;
                            }
                            else if (num == 1)
                            {
                                awaiter2 = this.<>u__2;
                                this.<>u__2 = new TaskAwaiter<HttpResponseMessage>();
                                this.<>1__state = num = -1;
                                goto TR_000A;
                            }
                            else
                            {
                                awaiter = Task.Delay(100).GetAwaiter();
                                if (!awaiter.IsCompleted)
                                {
                                    this.<>1__state = num = 0;
                                    this.<>u__1 = awaiter;
                                    this.<>t__builder.AwaitUnsafeOnCompleted<TaskAwaiter, MyEShop.<CheckServerAndUpdateStatus>d__15>(ref awaiter, ref this);
                                    return;
                                }
                            }
                            awaiter.GetResult();
                            awaiter2 = MyEShop.m_client.SendAsync(this.<request>5__3).GetAwaiter();
                            if (awaiter2.IsCompleted)
                            {
                                goto TR_000A;
                            }
                            else
                            {
                                this.<>1__state = num = 1;
                                this.<>u__2 = awaiter2;
                                this.<>t__builder.AwaitUnsafeOnCompleted<TaskAwaiter<HttpResponseMessage>, MyEShop.<CheckServerAndUpdateStatus>d__15>(ref awaiter2, ref this);
                            }
                            return;
                        TR_000A:
                            message = awaiter2.GetResult();
                            this.<response>5__2 = message.Content.ReadAsStringAsync().Result;
                        }
                        catch (Exception exception)
                        {
                            MyLog.Default.WriteLine(exception);
                        }
                    }
                    finally
                    {
                        if ((num < 0) && (this.<request>5__3 != null))
                        {
                            this.<request>5__3.Dispose();
                        }
                    }
                    this.<request>5__3 = null;
                    MyEShop.ReadPlayerStatus(this.<response>5__2);
                }
                catch (Exception exception2)
                {
                    this.<>1__state = -2;
                    this.<>t__builder.SetException(exception2);
                    return;
                }
            TR_0002:
                this.<>1__state = -2;
                this.<>t__builder.SetResult();
            }

            [DebuggerHidden]
            private void SetStateMachine(IAsyncStateMachine stateMachine)
            {
                this.<>t__builder.SetStateMachine(stateMachine);
            }
        }

        [CompilerGenerated]
        private struct <SendInfo>d__14 : IAsyncStateMachine
        {
            public int <>1__state;
            public AsyncVoidMethodBuilder <>t__builder;
            public string email;
            private HttpRequestMessage <request>5__2;
            private TaskAwaiter <>u__1;
            private TaskAwaiter<HttpResponseMessage> <>u__2;

            private void MoveNext()
            {
                int num = this.<>1__state;
                try
                {
                    if (num > 1)
                    {
                        string authenticatedTicket = MyEShop.GetAuthenticatedTicket();
                        if (!string.IsNullOrEmpty(authenticatedTicket))
                        {
                            string tokenTicket = MyEShop.GenerateToken(authenticatedTicket);
                            this.<request>5__2 = MyEShop.GetPOSTRequestMessage(tokenTicket, JsonMapper.ToJson(new MyEShop.NLFeedback(this.email, string.IsNullOrEmpty(this.email))));
                        }
                        else
                        {
                            goto TR_0002;
                        }
                    }
                    try
                    {
                        int num2 = num;
                        try
                        {
                            TaskAwaiter awaiter;
                            TaskAwaiter<HttpResponseMessage> awaiter2;
                            if (num == 0)
                            {
                                awaiter = this.<>u__1;
                                this.<>u__1 = new TaskAwaiter();
                                this.<>1__state = num = -1;
                            }
                            else if (num == 1)
                            {
                                awaiter2 = this.<>u__2;
                                this.<>u__2 = new TaskAwaiter<HttpResponseMessage>();
                                this.<>1__state = num = -1;
                                goto TR_000A;
                            }
                            else
                            {
                                awaiter = Task.Delay(100).GetAwaiter();
                                if (!awaiter.IsCompleted)
                                {
                                    this.<>1__state = num = 0;
                                    this.<>u__1 = awaiter;
                                    this.<>t__builder.AwaitUnsafeOnCompleted<TaskAwaiter, MyEShop.<SendInfo>d__14>(ref awaiter, ref this);
                                    return;
                                }
                            }
                            awaiter.GetResult();
                            awaiter2 = MyEShop.m_client.SendAsync(this.<request>5__2).GetAwaiter();
                            if (awaiter2.IsCompleted)
                            {
                                goto TR_000A;
                            }
                            else
                            {
                                this.<>1__state = num = 1;
                                this.<>u__2 = awaiter2;
                                this.<>t__builder.AwaitUnsafeOnCompleted<TaskAwaiter<HttpResponseMessage>, MyEShop.<SendInfo>d__14>(ref awaiter2, ref this);
                            }
                            return;
                        TR_000A:
                            awaiter2.GetResult();
                        }
                        catch (Exception exception)
                        {
                            MyLog.Default.WriteLine(exception);
                        }
                    }
                    finally
                    {
                        if ((num < 0) && (this.<request>5__2 != null))
                        {
                            this.<request>5__2.Dispose();
                        }
                    }
                    this.<request>5__2 = null;
                }
                catch (Exception exception2)
                {
                    this.<>1__state = -2;
                    this.<>t__builder.SetException(exception2);
                    return;
                }
            TR_0002:
                this.<>1__state = -2;
                this.<>t__builder.SetResult();
            }

            [DebuggerHidden]
            private void SetStateMachine(IAsyncStateMachine stateMachine)
            {
                this.<>t__builder.SetStateMachine(stateMachine);
            }
        }

        public class NLFeedback
        {
            public NLFeedback(string email, bool steamRefusalFlag)
            {
                this.Email = email;
                this.SteamRefusalFlag = steamRefusalFlag;
            }

            public string Email { get; private set; }

            public bool SteamRefusalFlag { get; private set; }
        }
    }
}

