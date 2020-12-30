using agi = HtmlAgilityPack;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;


namespace ConfirmedToday_Server
{
    class Program
    {
        public static TodayConfirmed todayConfirmed = new TodayConfirmed();
        static void Main(string[] args)
        {
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint ep = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 7777);
            socket.Bind(ep);

            socket.Listen(10);

            Socket accpetClient = socket.Accept();
            if (accpetClient.Connected)
                Console.WriteLine("클라이언트 연결");
            while (!Console.KeyAvailable)
            {
                if (!accpetClient.Connected)
                {
                    accpetClient = socket.Accept();
                    continue;
                }
                byte[] buff = new byte[16384];
                string result;
                try
                {
                    int n = accpetClient.Receive(buff);
                    result = Encoding.UTF8.GetString(buff, 0, n);
                }
                catch(SocketException e)
                {
                    accpetClient.Close();
                    Console.WriteLine($"연결 종료 : " + e.Message);
                    continue;
                }
                
                Console.WriteLine($"Client Call : {result}");
                string convertedMessage = MessageProcessToCase(result.TrimStart());
                Console.WriteLine($"Loaded {Encoding.UTF8.GetBytes(result).Length} Bytes");
                accpetClient.Send(Encoding.UTF8.GetBytes(convertedMessage));

            }
            accpetClient.Close();
            socket.Close();
        }
        static string MessageProcessToCase(string content)
        {
            switch (content)
            {
                case "!확진자":
                case "!ConfirmedPerson":
                    return todayConfirmed.GetConfirmed();
                case "!스팀게임순위":
                case "!SteamGameRanking":
                    return todayConfirmed.GetRealTimeGameRanking(0, 99);
                default:
                    if (content.Contains("!스팀게임순위") || content.Contains("!SteamGameRanking"))
                    {
                        string[] splitstr = content.Split(":");
                        if (splitstr.Length > 3)
                            break;
                        int startTo = Convert.ToInt32(splitstr[1]);
                        int endTo = Convert.ToInt32(splitstr[2]);
                        return todayConfirmed.GetRealTimeGameRanking(startTo, endTo);
                    }
                    break;
            }

            return "입력된 커멘드가 존재하지 않습니다";
        }



        
    }
    public class TodayConfirmed
    {
        public string GetConfirmed()
        {
            WebClient webClient = new WebClient();
            webClient.Encoding = Encoding.UTF8;

            string html = webClient.DownloadString("http://ncov.mohw.go.kr/bdBoardList_Real.do");
            agi.HtmlDocument doc = new agi.HtmlDocument();
            doc.LoadHtml(html);

            agi.HtmlNodeCollection htmlNodes = doc.GetElementbyId("content").SelectNodes("//p[@class='inner_value']");
            StringBuilder sb = new StringBuilder();
            sb.Append("== 하루 확진자 현황 ==\n");

            for (int i = 0; i < 3; i++)
            {
                sb.Append(MessageProcessToCase(i));
                sb.Append(htmlNodes[i].InnerText);
                sb.Append("\n");
            }
            return sb.ToString();
        }

        /// <summary>
        /// Load 1 ~ 100 Realtime Game Ranking
        /// </summary>
        /// <returns></returns>
        public string GetRealTimeGameRanking(int startTo, int endTo)
        {
            if (startTo > endTo || startTo < 0 || endTo >= 100)
                return "잘못된 순위 집계 입니다. (0 ~ 99)까지 가능";
            WebClient webClient = new WebClient();
            webClient.Encoding = Encoding.UTF8;

            string html = webClient.DownloadString("https://store.steampowered.com/stats/?l=koreana");
            agi.HtmlDocument doc = new agi.HtmlDocument();
            doc.LoadHtml(html);
            agi.HtmlNodeCollection htmlNodes = doc.GetElementbyId("detailStats").SelectNodes("//tr[@class='player_count_row']");
            // 1 : 현재 플레이어, 3 : 오늘 최고 기록 ,7 : 게임
            StringBuilder sb = new StringBuilder();
            sb.Append("\t\t\t\t== 현재 가장 플레이어 수가 많은 게임 ==\n");

            for (int i = startTo; i <= endTo; i++)
            {
                sb.Append($"{i + 1}위 : ");
                sb.Append(MessageProcessToCase(3));
                sb.Append(htmlNodes[i].ChildNodes[1].InnerText.Trim());
                sb.Append("\t");
                sb.Append(MessageProcessToCase(4));
                sb.Append(htmlNodes[i].ChildNodes[3].InnerText.Trim());
                sb.Append("\t\t");
                sb.Append(MessageProcessToCase(5));
                sb.Append(htmlNodes[i].ChildNodes[7].InnerText.Trim());
                sb.Append("\n");
            }

            return sb.ToString();
        }
        static string MessageProcessToCase(int content)
        {
            switch (content)
            {
                case 0:
                    return "합계 : ";
                case 1:
                    return "국내발생 : ";
                case 2:
                    return "해외유입 : ";
                case 3:
                    return "현재 플레이어 :";
                case 4:
                    return "오늘 최고 기록 : ";
                case 5:
                    return "게임 : ";
                default:
                    break;
            }

            return string.Empty;
        }
    }
}
