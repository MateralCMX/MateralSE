namespace Sandbox.Game.GameSystems
{
    using Newtonsoft.Json;
    using RestSharp;
    using Sandbox;
    using Sandbox.Engine.Analytics;
    using Sandbox.Game.Localization;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Text.RegularExpressions;
    using VRage;
    using VRage.Game.Components;
    using VRage.Library.Utils;
    using VRage.Serialization;
    using VRage.Utils;

    [MySessionComponentDescriptor(MyUpdateOrder.AfterSimulation, 900)]
    public class MyChatBot : MySessionComponentBase
    {
        private const string CHATBOT_URL = "http://chatbot.keenswh.com:8010";
        private const string CHATBOT_DEV_URL = "http://chatbot2.keenswh.com:8010";
        private readonly RestClient m_restClient = new RestClient(MyFakes.USE_GOODBOT_DEV_SERVER ? "http://chatbot2.keenswh.com:8010" : "http://chatbot.keenswh.com:8010");
        private static readonly char[] m_separators = new char[] { ' ', '\r', '\n' };
        private static readonly string[] m_nicks = new string[] { "+bot", "/bot", "+?", "/?", "?" };
        private const string MISUNDERSTANDING_TEXTID = "ChatBotMisunderstanding";
        private const string UNAVAILABLE_TEXTID = "ChatBotUnavailable";
        private static readonly MyStringId[] m_smallTalk;
        private static readonly Regex[] m_smallTalkRegex;
        private const int MAX_MISUNDERSTANDING = 1;
        private readonly List<Substitute> m_substitutes = new List<Substitute>();
        private Regex m_stripSymbols;
        private const string OUPTUT_FILE = @"c:\x\stats_out.csv";
        private const string INPUT_FILE = @"c:\x\stats.csv";

        static MyChatBot()
        {
            MyStringId[] idArray1 = new MyStringId[10];
            idArray1[0] = MySpaceTexts.ChatBot_Rude;
            idArray1[1] = MySpaceTexts.ChatBot_ThankYou;
            idArray1[2] = MySpaceTexts.ChatBot_Generic;
            idArray1[3] = MySpaceTexts.ChatBot_HowAreYou;
            idArray1[4] = MySpaceTexts.Description_FAQ_Objective;
            idArray1[5] = MySpaceTexts.Description_FAQ_GoodBot;
            idArray1[6] = MySpaceTexts.Description_FAQ_Begin;
            idArray1[7] = MySpaceTexts.Description_FAQ_Bug;
            idArray1[8] = MySpaceTexts.Description_FAQ_Test;
            idArray1[9] = MySpaceTexts.Description_FAQ_Clang;
            m_smallTalk = idArray1;
            m_smallTalkRegex = new Regex[m_smallTalk.Length];
        }

        public MyChatBot()
        {
            int index = 0;
            while (true)
            {
                MyStringId orCompute = MyStringId.GetOrCompute("ChatBot_Substitute" + index + "_S");
                MyStringId id = MyStringId.GetOrCompute("ChatBot_Substitute" + index + "_D");
                if (!MyTexts.Exists(orCompute) || !MyTexts.Exists(id))
                {
                    index = 0;
                    while (index < m_smallTalk.Length)
                    {
                        int num2 = 0;
                        string str = "";
                        while (true)
                        {
                            MyStringId id3 = MyStringId.GetOrCompute(m_smallTalk[index] + "_Q" + num2);
                            if (!MyTexts.Exists(id3))
                            {
                                m_smallTalkRegex[index] = new Regex(str + @"(?:[ ,.?!;\-()*]|$)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
                                index++;
                                break;
                            }
                            if (num2 != 0)
                            {
                                str = str + @"(?:[ ,.?!;\-()*]|$)|";
                            }
                            str = str + MyTexts.GetString(id3);
                            num2++;
                        }
                    }
                    this.m_stripSymbols = new Regex("(?:[^a-z0-9 ])", RegexOptions.Compiled | RegexOptions.IgnoreCase);
                    return;
                }
                Substitute item = new Substitute {
                    Source = new Regex(MyTexts.GetString(orCompute) + @"(?:[ ,.?;\-()*]|$)", RegexOptions.Compiled | RegexOptions.IgnoreCase),
                    Dest = id
                };
                this.m_substitutes.Add(item);
                index++;
            }
        }

        private string ApplySubstitutions(string text)
        {
            foreach (Substitute substitute in this.m_substitutes)
            {
                string text1 = substitute.Source.Replace(text, MyTexts.GetString(substitute.Dest));
                text = text1;
            }
            return text;
        }

        private RestRequest CreateChatbotRequest(string preprocessedQuestion)
        {
            string str = DateTime.UtcNow.ToString("r", CultureInfo.InvariantCulture);
            RestRequest request1 = new RestRequest("intent", Method.POST);
            request1.AddHeader("Date", str);
            request1.AddHeader("Content-Type", "application/json");
            string str2 = $"{{"state": "DEFAULT", "utterance": "{preprocessedQuestion}"}}";
            request1.AddParameter("application/json", str2, ParameterType.RequestBody);
            return request1;
        }

        private string ExtractPhrases(string messageText, out string potentialResponseId)
        {
            potentialResponseId = null;
            for (int i = 0; i < m_smallTalkRegex.Length; i++)
            {
                string str = m_smallTalkRegex[i].Replace(messageText, "");
                if (str.Length != messageText.Length)
                {
                    potentialResponseId = m_smallTalk[i].ToString();
                    return ((str.Trim().Length >= 4) ? str : null);
                }
            }
            return messageText;
        }

        public bool FilterMessage(string message, Action<string> responseAction)
        {
            string[] strArray = message.Split(m_separators, StringSplitOptions.RemoveEmptyEntries);
            if (strArray.Length > 1)
            {
                string[] nicks = m_nicks;
                for (int i = 0; i < nicks.Length; i++)
                {
                    if (nicks[i] == strArray[0].ToLower())
                    {
                        string str2;
                        string str3;
                        string messageText = "";
                        for (int j = 1; j < strArray.Length; j++)
                        {
                            messageText = messageText + strArray[j] + " ";
                        }
                        messageText = messageText.Trim();
                        ResponseType responseType = this.Preprocess(messageText, out str2, out str3);
                        if (responseType == ResponseType.ChatBot)
                        {
                            this.SendMessage(messageText, str2, str3, responseAction);
                        }
                        else
                        {
                            this.Respond(messageText, str3, responseType, responseAction);
                        }
                        return true;
                    }
                }
            }
            return false;
        }

        private string GetMisunderstandingTextId() => 
            ("ChatBotMisunderstanding" + MyRandom.Instance.Next(0, 1));

        private void OnResponse(IRestResponse arg, Action<string> responseAction, string potentialResponseId, string question)
        {
            string str;
            this.Respond(question, str, this.Postprocess(arg, potentialResponseId, out str), responseAction);
        }

        private unsafe void PerformDebugTest()
        {
            File.Delete(@"c:\x\stats_out.csv");
            List<string>[] listArray = new List<string>[12];
            int[][] numArray = new int[6][];
            for (int i = 0; i < 6; i++)
            {
                listArray[i] = new List<string>();
                listArray[i + 6] = new List<string>();
                numArray[i] = new int[6];
            }
            using (StreamWriter writer = new StreamWriter(@"c:\x\stats_out.csv", false))
            {
                using (StreamReader reader = new StreamReader(@"c:\x\stats.csv"))
                {
                    writer.WriteLine("No change: ");
                    int num2 = 0;
                    foreach (IList<string> list in CsvParser.Parse(reader, ';', '"'))
                    {
                        int count = list.Count;
                        if (list[0] != "")
                        {
                            ResponseType misunderstanding;
                            string str3;
                            string str4;
                            if (!Enum.TryParse<ResponseType>(list[0], out misunderstanding))
                            {
                                misunderstanding = ResponseType.Misunderstanding;
                            }
                            string messageText = list[1];
                            string str2 = list[2];
                            ResponseType type2 = this.Preprocess(messageText, out str3, out str4);
                            if (type2 == ResponseType.ChatBot)
                            {
                                RestRequest request = this.CreateChatbotRequest(str3);
                                IRestResponse arg = this.m_restClient.Execute(request);
                                type2 = this.Postprocess(arg, str4, out str4);
                            }
                            int* numPtr1 = (int*) ref numArray[(int) misunderstanding][(int) type2];
                            numPtr1[0]++;
                            string item = $"{type2};"{messageText}";{str4};{str2}";
                            if ((misunderstanding != type2) || (str4 != str2))
                            {
                                listArray[((int) misunderstanding) + ((misunderstanding == type2) ? 6 : 0)].Add(item);
                            }
                            else
                            {
                                writer.WriteLine(item);
                            }
                        }
                        int num8 = (num2 + 1) % 100;
                    }
                }
                writer.WriteLine("---");
                int num3 = 0;
                while (true)
                {
                    if (num3 >= 6)
                    {
                        int index = 0;
                        while (index < 6)
                        {
                            string str8 = ((ResponseType) index) + ": ";
                            int num6 = 0;
                            while (true)
                            {
                                if (num6 >= 6)
                                {
                                    index++;
                                    break;
                                }
                                str8 = str8 + numArray[index][num6] + " ";
                                num6++;
                            }
                        }
                        break;
                    }
                    writer.WriteLine(((ResponseType) num3) + ": ");
                    int num4 = 0;
                    while (true)
                    {
                        if (num4 >= 2)
                        {
                            num3++;
                            break;
                        }
                        foreach (string str7 in listArray[num3 + (num4 * 6)])
                        {
                            writer.WriteLine(str7);
                        }
                        writer.WriteLine("---");
                        num4++;
                    }
                }
            }
        }

        private ResponseType Postprocess(IRestResponse arg, string potentialResponseId, out string responseId)
        {
            responseId = "ChatBotUnavailable";
            ResponseType chatBot = ResponseType.ChatBot;
            if (!arg.IsSuccessful)
            {
                chatBot = ResponseType.Unavailable;
            }
            else
            {
                ChatBotResponse response = JsonConvert.DeserializeObject<ChatBotResponse>(arg.Content);
                if (response.Error != null)
                {
                    chatBot = ResponseType.Error;
                }
                else if (response.Intent != null)
                {
                    responseId = response.Intent;
                }
                else if (potentialResponseId == null)
                {
                    responseId = this.GetMisunderstandingTextId();
                    chatBot = ResponseType.Misunderstanding;
                }
                else
                {
                    responseId = potentialResponseId;
                    chatBot = ResponseType.SmallTalk;
                }
            }
            return chatBot;
        }

        private ResponseType Preprocess(string messageText, out string preprocessedText, out string responseId)
        {
            preprocessedText = messageText;
            responseId = this.GetMisunderstandingTextId();
            ResponseType garbage = ResponseType.Garbage;
            string str = this.m_stripSymbols.Replace(messageText, "").Trim();
            if (str.Length != 0)
            {
                garbage = ResponseType.SmallTalk;
                string text = this.ExtractPhrases(str, out responseId);
                if (text != null)
                {
                    preprocessedText = this.ApplySubstitutions(text);
                    garbage = ResponseType.ChatBot;
                }
            }
            return garbage;
        }

        private void Respond(string question, string responseId, ResponseType responseType, Action<string> responseAction)
        {
            object[] objArray1 = new object[] { "GoodBot(", responseType, "): ", question, " / ", responseId };
            MyAnalyticsHelper.ReportBug(string.Concat(objArray1), null, false, string.Empty, 0x14b);
            string text = MyTexts.GetString(responseId);
            MySandboxGame.Static.Invoke(() => responseAction(text), "OnChatBotResponse");
        }

        private void SendMessage(string originalQuestion, string preprocessedQuestion, string potentialResponseId, Action<string> responseAction)
        {
            RestRequest request = this.CreateChatbotRequest(preprocessedQuestion);
            this.m_restClient.ExecuteAsync(request, x => this.OnResponse(x, responseAction, potentialResponseId, originalQuestion));
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct ChatBotResponse
        {
            public string Intent;
            public string Error;
        }

        private enum ResponseType
        {
            Garbage,
            Misunderstanding,
            SmallTalk,
            ChatBot,
            Unavailable,
            Error,
            Count
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct Substitute
        {
            public Regex Source;
            public MyStringId Dest;
        }
    }
}

