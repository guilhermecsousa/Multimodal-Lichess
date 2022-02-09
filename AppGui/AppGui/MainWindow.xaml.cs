using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Xml.Linq;
using mmisharp;
using Newtonsoft.Json;

namespace AppGui
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MmiCommunication mmiC;

        private MmiCommunication mmiSender;
        private LifeCycleEvents lce;
        private MmiCommunication mmic;
        private string game = "";

        // Lichess token - Multimodal
        static public string token = "lip_2YhO0w971EuRh4mBRo8W";

        // Lichess token - pako25
        //static public string token = "lip_I0iYfH1quLT2GUWlxrAq";

        // Lichess token - TestarCaderno
        //static public string token = "lip_I0iYfH1quLT2GUWlxrAq";

        public MainWindow()
        {
            InitializeComponent();

            mmiC = new MmiCommunication("localhost",8000, "User1", "GUI");
            mmiC.Message += MmiC_Message;
            mmiC.Start();

            // NEW 16 april 2020
            //init LifeCycleEvents..
            lce = new LifeCycleEvents("APP", "TTS", "User1", "na", "command"); // LifeCycleEvents(string source, string target, string id, string medium, string mode
            // MmiCommunication(string IMhost, int portIM, string UserOD, string thisModalityName)
            mmic = new MmiCommunication("localhost", 8000, "User1", "GUI");
        }

        private async void MmiC_Message(object sender, MmiEventArgs e)
        {
            var doc = XDocument.Parse(e.Message);
            var com = doc.Descendants("command").FirstOrDefault().Value;
            dynamic json = JsonConvert.DeserializeObject(com);

            Console.WriteLine(json);

            String action = null;
            switch (json.recognized[1].ToString())
            {
                case "joga":
                    action = "joga";
                    break;

                case "envia":
                    action = "envia";
                    break;

                case "desisto":
                    action = "desisto";
                    break;

                case "desafia":
                    action = "desafia";
                    break;

                case "engano":
                    action = "engano";
                    break;

                case "accengano":
                    action = "accengano";
                    break;

                case "rejengano":
                    action = "rejengano";
                    break;

                case "list":
                    action = "list";
                    break;

                case "accept":
                    action = "accept";
                    break;

                case "decline":
                    action = "decline";
                    break;

                case "cancel":
                    action = "cancel";
                    break;

                case "empate":
                    action = "empate";
                    break;

                case "accempate":
                    action = "accempate";
                    break;

                case "rejempate":
                    action = "rejempate";
                    break;
            }

            // If I pretend to play
            if (action == "joga")
            {
                string move = "";
                move += (string)json.recognized[3].ToString();
                move += (string)json.recognized[5].ToString();
                move += (string)json.recognized[7].ToString();
                move += (string)json.recognized[9].ToString();
                Task<String> move_piece = Task.Run(() => MovePiece(game, move));
                String result = move_piece.Result;
                string auxok = @"""ok""";
                string auxtrue = "true";
                string aux = "{" + auxok + ":" + auxtrue + "}";
                if (result.Equals(aux))
                {
                    result = "{'ok' : ''}";
                }
                await mmic.Send(lce.NewContextRequest());
                var exNot = lce.ExtensionNotification(0 + "", 0 + "", 1, result);
                await mmic.Send(exNot);
            }

            // If I pretend to send a chat message
            else if (action == "envia")
            {
                string message = "";
                message += (string)json.recognized[3].ToString();
                Task<String> send_msg = Task.Run(() => SendMessage(game, message));
                String result = send_msg.Result;
                string auxok = @"""ok""";
                string auxtrue = "true";
                string aux = "{" + auxok + ":" + auxtrue + "}";
                if (result.Equals(aux))
                {
                    result = "{'ok' : 'Mensagem enviada'}";
                }
                await mmic.Send(lce.NewContextRequest());
                var exNot = lce.ExtensionNotification(0 + "", 0 + "", 1, result);
                await mmic.Send(exNot);
            }

            // If I pretend to give up
            else if (action == "desisto")
            {
                Task<String> resign = Task.Run(() => Resign(game));
                String result = resign.Result;
                string auxok = @"""ok""";
                string auxtrue = "true";
                string aux = "{" + auxok + ":" + auxtrue + "}";
                if (result.Equals(aux))
                {
                    result = "{'ok' : 'Partida terminada'}";
                }
                await mmic.Send(lce.NewContextRequest());
                var exNot = lce.ExtensionNotification(0 + "", 0 + "", 1, result);
                await mmic.Send(exNot);
            }

            // If I pretend to challenge someone
            else if (action == "desafia")
            {
                string user = "";
                user += (string)json.recognized[3].ToString();
                Task<String> challenge = Task.Run(() => Challenge(user));
                string result = challenge.Result;
                string data = getBetween(result, "url", "status");
                char[] removeStuff = { '"', ':', ',' };
                string auxgame = getBetween(result, "id", "url");
                string newgame = auxgame.TrimEnd(removeStuff);
                game = newgame.TrimStart(removeStuff);
                string auxdata = data.TrimEnd(removeStuff);
                string newdata = auxdata.TrimStart(removeStuff);
                System.Diagnostics.Process.Start("firefox.exe", newdata);
                await mmic.Send(lce.NewContextRequest());
                var exNot = lce.ExtensionNotification(0 + "", 0 + "", 1, result);
                Console.WriteLine(result);
                await mmic.Send(exNot);
            }

            // If I pretend to takeback
            else if (action == "engano")
            {
                string message = "yes";
                Task<String> send_msg = Task.Run(() => TakeBack(game, message));
                String result = send_msg.Result;
                Console.WriteLine(result);
                string auxok = @"""ok""";
                string auxtrue = "true";
                string aux = "{" + auxok + ":" + auxtrue + "}";
                if (result.Equals(aux))
                {
                    result = "{'ok' : 'Pedido enviado'}";
                }
                await mmic.Send(lce.NewContextRequest());
                var exNot = lce.ExtensionNotification(0 + "", 0 + "", 1, result);
                await mmic.Send(exNot);
            }

            // If I pretend to accept a takeback proposed by the opponent
            else if (action == "accengano")
            {
                string message = "yes";
                Task<String> send_msg = Task.Run(() => TakeBack(game, message));
                String result = send_msg.Result;
                string auxok = @"""ok""";
                string auxtrue = "true";
                string aux = "{" + auxok + ":" + auxtrue + "}";
                if (result.Equals(aux))
                {
                    result = "{'ok' : 'Pedido aceite'}";
                }
                await mmic.Send(lce.NewContextRequest());
                var exNot = lce.ExtensionNotification(0 + "", 0 + "", 1, result);
                await mmic.Send(exNot);
            }

            // If I pretend to reject a takeback proposed by the opponent
            else if (action == "rejengano")
            {
                string message = "no";
                Task<String> send_msg = Task.Run(() => TakeBack(game, message));
                String result = send_msg.Result;
                string auxok = @"""ok""";
                string auxtrue = "true";
                string aux = "{" + auxok + ":" + auxtrue + "}";
                if (result.Equals(aux))
                {
                    result = "{'ok' : 'Pedido rejeitado'}";
                }
                await mmic.Send(lce.NewContextRequest());
                var exNot = lce.ExtensionNotification(0 + "", 0 + "", 1, result);
                await mmic.Send(exNot);
            }

            // If I pretend to know if I have game invitations
            else if (action == "list")
            {
                Task<String> list = Task.Run(() => ListChallenges());
                String result = list.Result;

                string data = "";
                if (result.Contains(@"""in"":[]"))
                {
                    data = "";
                }
                else
                {
                    data = getBetween(result, "id", "url");
                    char[] removeStuff = { '"', ':', ',' };
                    data = data.TrimStart(removeStuff);
                    data = data.TrimEnd(removeStuff);
                    game = data;
                }
                
                await mmic.Send(lce.NewContextRequest());

                if (String.IsNullOrEmpty(data))
                {
                    result = "{'ok' : 'Não tem nehum desafio'}";
                }
                else
                {
                    result = "{'ok' : 'Tem desafios pendentes'}";
                }
                var exNot = lce.ExtensionNotification(0 + "", 0 + "", 1, result);
                await mmic.Send(exNot);
            }

            // If I pretend to accept a game invitation
            else if (action == "accept")
            {
                Task<String> accept = Task.Run(() => Accept(game));
                string result = accept.Result;
                string auxok = @"""ok""";
                string auxtrue = "true";
                string aux = "{" + auxok + ":" + auxtrue + "}";
                if (result.Equals(aux))
                {
                    result = "{'ok' : 'Desafio aceite'}";
                }
                System.Diagnostics.Process.Start("firefox.exe", "https://lichess.org/" + game);
                await mmic.Send(lce.NewContextRequest());
                var exNot = lce.ExtensionNotification(0 + "", 0 + "", 1, result);
                await mmic.Send(exNot);
            }

            // If I pretend to reject a game invitation
            else if (action == "decline")
            {
                Task<String> decline = Task.Run(() => Decline(game));
                string result = decline.Result;
                string auxok = @"""ok""";
                string auxtrue = "true";
                string aux = "{" + auxok + ":" + auxtrue + "}";
                if (result.Equals(aux))
                {
                    result = "{'ok' : 'Desafio recusado'}";
                }
                await mmic.Send(lce.NewContextRequest());
                var exNot = lce.ExtensionNotification(0 + "", 0 + "", 1, result);
                await mmic.Send(exNot);
            }


            // If I pretend to cancel a invitation I sent
            else if (action == "cancel")
            {
                Task<String> list = Task.Run(() => ListChallenges());
                String resultaux = list.Result;

                string data = "";
                if (resultaux.Contains(@"""out"":[]"))
                {
                    data = "";
                }
                else
                {
                    data = getBetween(resultaux, "out", "url");
                    data = getBetween(data, "id", ",");
                    char[] removeStuff = { '"', ':', ',' };
                    data = data.TrimStart(removeStuff);
                    data = data.TrimEnd(removeStuff);
                    game = data;
                }

                Task<String> cancel = Task.Run(() => Cancel(game));
                string result = cancel.Result;
                string auxok = @"""ok""";
                string auxtrue = "true";
                string aux = "{" + auxok + ":" + auxtrue + "}";
                if (result.Equals(aux))
                {
                    result = "{'ok' : 'Desafio cancelado'}";
                }
                await mmic.Send(lce.NewContextRequest());
                var exNot = lce.ExtensionNotification(0 + "", 0 + "", 1, result);
                await mmic.Send(exNot);
            }

            // If I pretend to propose a draw
            else if (action == "empate")
            {
                string message = "yes";
                Task<String> send_msg = Task.Run(() => Draw(game, message));
                String result = send_msg.Result;
                string auxok = @"""ok""";
                string auxtrue = "true";
                string aux = "{" + auxok + ":" + auxtrue + "}";
                if (result.Equals(aux))
                {
                    result = "{'ok' : 'Empate proposto'}";
                }
                await mmic.Send(lce.NewContextRequest());
                var exNot = lce.ExtensionNotification(0 + "", 0 + "", 1, result);
                await mmic.Send(exNot);
            }

            // If I pretend to accept a draw proposed by the opponent
            else if (action == "accempate")
            {
                string message = "yes";
                Task<String> send_msg = Task.Run(() => Draw(game, message));
                String result = send_msg.Result;
                string auxok = @"""ok""";
                string auxtrue = "true";
                string aux = "{" + auxok + ":" + auxtrue + "}";
                if (result.Equals(aux))
                {
                    result = "{'ok' : 'Empate aceite'}";
                }
                await mmic.Send(lce.NewContextRequest());
                var exNot = lce.ExtensionNotification(0 + "", 0 + "", 1, result);
                await mmic.Send(exNot);
            }

            // If I pretend to reject a draw proposed by the opponent
            else if (action == "rejempate")
            {
                string message = "no";
                Task<String> send_msg = Task.Run(() => Draw(game, message));
                String result = send_msg.Result;
                string auxok = @"""ok""";
                string auxtrue = "true";
                string aux = "{" + auxok + ":" + auxtrue + "}";
                if (result.Equals(aux))
                {
                    result = "{'ok' : 'Empate rejeitado'}";
                }
                await mmic.Send(lce.NewContextRequest());
                var exNot = lce.ExtensionNotification(0 + "", 0 + "", 1, result);
                await mmic.Send(exNot);
            }

        }
        // Function that given a string, a starting point and a ending point, returns the charachters in-between
        public static string getBetween(string strSource, string strStart, string strEnd)
        {
            if (strSource.Contains(strStart) && strSource.Contains(strEnd))
            {
                int Start, End;
                Start = strSource.IndexOf(strStart, 0) + strStart.Length;
                End = strSource.IndexOf(strEnd, Start);
                return strSource.Substring(Start, End - Start);
            }

            return "";
        }

        // Series of Tasks that communicate with the Lichess API to...
        //    ...  move a piece from one to another of given positions
        static async Task<String> MovePiece(String Game, String Move)
        {
            var client = new HttpClient();
            var url = new Uri("https://lichess.org/api/board/game/" + Game + "/move/" + Move);
            var values = new Dictionary<string, string>()
            {
                {"", "" }
            };
            var content = new FormUrlEncodedContent(values);
            client.DefaultRequestHeaders.Authorization
                            = new AuthenticationHeaderValue("Bearer", token);
            var result = await client.PostAsync(url, content);
            string resultContent = await result.Content.ReadAsStringAsync();
            return resultContent;
        }

        //    ... send to the chat a given message
        static async Task<String> SendMessage(String Game, String Message)
        {
            var client = new HttpClient();
            var url = new Uri("https://lichess.org/api/board/game/" + Game + "/chat");
            var values = new Dictionary<string, string>()
            {
                {"text", Message },
                {"room", "player" }
            };
            var content = new FormUrlEncodedContent(values);
            client.DefaultRequestHeaders.Authorization
                            = new AuthenticationHeaderValue("Bearer", token);
            var result = await client.PostAsync(url, content);
            string resultContent = await result.Content.ReadAsStringAsync();
            return resultContent;
        }

        //      ... give up the game
        static async Task<String> Resign(String Game)
        {
            var client = new HttpClient();
            var url = new Uri("https://lichess.org/api/board/game/" + Game + "/resign");
            var values = new Dictionary<string, string>()
            {
                {"", "" }
            };
            var content = new FormUrlEncodedContent(values);
            client.DefaultRequestHeaders.Authorization
                            = new AuthenticationHeaderValue("Bearer", token);
            var result = await client.PostAsync(url, content);
            string resultContent = await result.Content.ReadAsStringAsync();
            return resultContent;
        }

        //      ... invite a given opponent
        static async Task<String> Challenge(String User)
        {
            var client = new HttpClient();
            var url = new Uri("https://lichess.org/api/challenge/" + User);
            var values = new Dictionary<string, string>()
            {
                {"", "" }
            };
            var content = new FormUrlEncodedContent(values);
            client.DefaultRequestHeaders.Authorization
                            = new AuthenticationHeaderValue("Bearer", token);
            var result = await client.PostAsync(url, content);
            string resultContent = await result.Content.ReadAsStringAsync();
            return resultContent;
        }

        //      ... propose a takeback
        static async Task<String> TakeBack(String Game, String Message)
        {
            var client = new HttpClient();
            var url = new Uri("https://lichess.org/api/board/game/" + Game + "/takeback/" + Message);
            var values = new Dictionary<string, string>()
            {
                {"", "" },
            };
            var content = new FormUrlEncodedContent(values);
            client.DefaultRequestHeaders.Authorization
                            = new AuthenticationHeaderValue("Bearer", token);
            var result = await client.PostAsync(url, content);
            string resultContent = await result.Content.ReadAsStringAsync();
            return resultContent;
        }

        //      ... know if there is an active invite to play
        static async Task<String> ListChallenges()
        {
            var client = new HttpClient();
            var url = new Uri("https://lichess.org/api/challenge");
            client.DefaultRequestHeaders.Authorization
                            = new AuthenticationHeaderValue("Bearer", token);
            var result = await client.GetAsync(url);
            string resultContent = await result.Content.ReadAsStringAsync();
            return resultContent;
        }

        //      ... accept an invite
        static async Task<String> Accept(String Accept)
        {
            var client = new HttpClient();
            var url = new Uri("https://lichess.org/api/challenge/" + Accept + "/accept");
            var values = new Dictionary<string, string>()
            {
                {"", "" }
            };
            var content = new FormUrlEncodedContent(values);
            client.DefaultRequestHeaders.Authorization
                            = new AuthenticationHeaderValue("Bearer", token);
            var result = await client.PostAsync(url, content);
            string resultContent = await result.Content.ReadAsStringAsync();
            return resultContent;
        }

        //      ... decline an invite
        static async Task<String> Decline(String Decline)
        {
            var client = new HttpClient();
            var url = new Uri("https://lichess.org/api/challenge/" + Decline + "/decline");
            var values = new Dictionary<string, string>()
            {
                {"", "" }
            };
            var content = new FormUrlEncodedContent(values);
            client.DefaultRequestHeaders.Authorization
                            = new AuthenticationHeaderValue("Bearer", token);
            var result = await client.PostAsync(url, content);
            string resultContent = await result.Content.ReadAsStringAsync();
            return resultContent;
        }

        //      ... cancel a sent invitation
        static async Task<String> Cancel(String Cancel)
        {
            var client = new HttpClient();
            var url = new Uri("https://lichess.org/api/challenge/" + Cancel + "/cancel");
            var values = new Dictionary<string, string>()
            {
                {"", "" }
            };
            var content = new FormUrlEncodedContent(values);
            client.DefaultRequestHeaders.Authorization
                            = new AuthenticationHeaderValue("Bearer", token);
            var result = await client.PostAsync(url, content);
            string resultContent = await result.Content.ReadAsStringAsync();
            return resultContent;
        }

        //      ... propose a draw
        static async Task<String> Draw(String Game, String Message)
        {
            var client = new HttpClient();
            var url = new Uri("https://lichess.org/api/board/game/" + Game + "/draw/" + Message);
            var values = new Dictionary<string, string>()
            {
                {"", "" },
            };
            var content = new FormUrlEncodedContent(values);
            client.DefaultRequestHeaders.Authorization
                            = new AuthenticationHeaderValue("Bearer", token);
            var result = await client.PostAsync(url, content);
            string resultContent = await result.Content.ReadAsStringAsync();
            return resultContent;
        }

    }
}
