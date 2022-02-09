using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using mmisharp;
using Microsoft.Speech.Recognition;
using System.Xml.Linq;
using Newtonsoft.Json;

namespace speechModality
{
    public class SpeechMod
    {
        // changed 16 april 2020
        private static SpeechRecognitionEngine sre= new SpeechRecognitionEngine(new System.Globalization.CultureInfo("pt-PT"));
        private Grammar gr;


        public event EventHandler<SpeechEventArg> Recognized;
        protected virtual void onRecognized(SpeechEventArg msg)
        {
            EventHandler<SpeechEventArg> handler = Recognized;
            if (handler != null)
            {
                handler(this, msg);
            }
        }

        private LifeCycleEvents lce;
        private MmiCommunication mmic;

        //  NEW 16 april
        private static Tts tts = new Tts(sre);
        private MmiCommunication mmiReceiver;

        public SpeechMod()
        {
            //init LifeCycleEvents..
            lce = new LifeCycleEvents("ASR", "FUSION", "speech-1", "acoustic", "command"); // LifeCycleEvents(string source, string target, string id, string medium, string mode)
            mmic = new MmiCommunication("localhost",9876,"User1", "ASR");  //PORT TO FUSION - uncomment this line to work with fusion later
            //mmic = new MmiCommunication("localhost", 8000, "User1", "ASR"); // MmiCommunication(string IMhost, int portIM, string UserOD, string thisModalityName)

            mmic.Send(lce.NewContextRequest());

            //load pt recognizer
            //sre = new SpeechRecognitionEngine(new System.Globalization.CultureInfo("pt-PT"));
            gr = new Grammar(Environment.CurrentDirectory + "\\ptG.grxml", "rootRule");
            sre.LoadGrammar(gr);


            sre.SetInputToDefaultAudioDevice();
            sre.RecognizeAsync(RecognizeMode.Multiple);
            sre.SpeechRecognized += Sre_SpeechRecognized;
            sre.SpeechHypothesized += Sre_SpeechHypothesized;

            // NEW - TTS support 16 April
            tts.Speak("Olá. Bem vindo ao assistente de xadrez. Como posso ser útil?");


            //  o TTS  no final indica que se recebe mensagens enviadas para TTS
            mmiReceiver = new MmiCommunication("localhost",8000, "User1", "TTS");
            mmiReceiver.Message += MmiReceived_Message;
            mmiReceiver.Start();


        }


    private void Sre_SpeechHypothesized(object sender, SpeechHypothesizedEventArgs e)
        {
            onRecognized(new SpeechEventArg() { Text = e.Result.Text, Confidence = e.Result.Confidence, Final = false });
        }

        //
        private void Sre_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            onRecognized(new SpeechEventArg() { Text = e.Result.Text, Confidence = e.Result.Confidence, Final = true });

            //SEND
            
            if(e.Result.Confidence < 0.5)
            {
                tts.Speak("Não tenho a certeza da instrução. Pode repetir?");
            }
            else
            {
                string json = "{\"recognized\": [";
                foreach (var resultSemantic in e.Result.Semantics)
                {
                    foreach (var key in resultSemantic.Value)
                    {
                        json += "\"" + key.Key + "\", " + "\"" + key.Value.Value + "\",\n ";

                    }
                }
                json = json.Substring(0, json.Length - 3);
                json += "]}";
                var exNot = lce.ExtensionNotification(e.Result.Audio.StartTime + "", e.Result.Audio.StartTime.Add(e.Result.Audio.Duration) + "", e.Result.Confidence, json);
                mmic.Send(exNot);
            }
        }


        //  NEW 16 April 2020   - create receiver, answer to messages received
        //  Adapted from AppGUI code


        //MmiReceived_Message;
        // TTS of the information received about the actions performed

        private void MmiReceived_Message(object sender, MmiEventArgs e)
        {
            var doc = XDocument.Parse(e.Message);
            var com = doc.Descendants("command").FirstOrDefault().Value;
            dynamic tojson2 = JsonConvert.DeserializeObject(com);
            if (tojson2 != null) {
                if (tojson2.ContainsKey("error"))
                {
                    if ("Not your turn, or game already over".Equals(tojson2.error.ToString()))
                    {
                        tts.Speak("Não é a tua vez de jogar");
                    }
                    else if (tojson2.error.ToString().Contains("Piece on"))
                    {
                        tts.Speak("Jogada inválida");
                    }
                    else if ("No such game".Equals(tojson2.error.ToString()))
                    {
                        tts.Speak("Jogo inexistente");
                    }
                    else if (tojson2.error.ToString().Contains("No piece"))
                    {
                        tts.Speak("Não há peça nessa posição");
                    }
                    else if (tojson2.error.ToString().Contains("Not my piece"))
                    {
                        tts.Speak("Essa peça não é tua");
                    }
                }
                else if (tojson2.ContainsKey("ok"))
                {
                    tts.Speak(tojson2.ok.ToString());
                }
                else if (tojson2.ContainsKey("challenge"))
                {
                    tts.Speak("Convite enviado!");
                }

            }
        }
    }
}
