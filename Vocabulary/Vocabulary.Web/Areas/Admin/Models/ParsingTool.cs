using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Speech.Synthesis;
using System.Threading.Tasks;
using Fizzler.Systems.HtmlAgilityPack;
using HtmlAgilityPack;
using Vocabulary.Domain.Entities;
//reference Nuget Package NAudio.Lame
using NAudio.Wave;
using NAudio.Lame;

namespace Vocabulary.Web.Areas.Admin.Models
{
    public class ParsingTool
    {
        private static readonly SpeechSynthesizer Synthesizer = new SpeechSynthesizer();
        public delegate void AddToDataBase(FullPhraseViewModel p, Glossary g);

        #region Parse wooordhunt.ru
        public static async void StartParse(string websites, AddToDataBase function, Glossary glossary = null)
        {
            if (function == null || string.IsNullOrEmpty(websites)) return;

            var html = new HtmlDocument();
            var addresses = websites.Split('\n');
            foreach (var address in addresses)
            {
                try
                {
                    if (string.IsNullOrEmpty(address)) continue;
                    using (var client = new WebClient())
                    {
                        var response = client.OpenRead(string.Format("http://www.wooordhunt.ru/word/{0}", address));
                        if (response == null) continue;
                        
                        var reader = new StreamReader(response);
                        var source = reader.ReadToEnd();
                        html.LoadHtml(source);
                    }
                    var document = html.DocumentNode;

                    var fullPhrase = await GetFullPhrase(document);
                    function(fullPhrase, glossary);
                }
                catch(Exception e)
                {
                }
            }
        }
        
        /// <summary>
        /// Parse web pages to simplify population of database
        /// </summary>
        /// <param name="document">words list separated by '\n' symbol</param>
        /// <returns></returns>
        public static async Task<FullPhraseViewModel> GetFullPhrase(HtmlNode document)
        {
            var fp = new FullPhraseViewModel();

            fp.Phrase.Phrase = document.QuerySelector("#wd_title h1").InnerText.Split('-')[0].Trim();
            fp.Phrase.Transcription = document.QuerySelector(".trans_sound .transcription").InnerText;
            var freq = document.QuerySelector(".trans_sound #rank_box a").InnerText.Replace(" ", "");
            if (!(string.IsNullOrEmpty(freq) || freq[0] == '>'))
                fp.Phrase.Frequency = int.Parse(freq);
            fp.Phrase.LanguageId = 1m;
            fp.Phrase.PhraseType = PhraseType.Word;

            fp.Translation.TranslationPhrase = document.QuerySelector("#wd_content span").InnerText;
            if (document.QuerySelector("#word_forms") != null)
            {
                fp.Translation.TranslationPhrase += "/n" + document.QuerySelector("#word_forms").InnerText;
            }
            fp.Translation.LanguageId = 3m;

            var collection = document.QuerySelectorAll("#wd_content .tr .ex").ToList();

            //collection.Add(document.QuerySelector("#wd_content .block.phrases")); 
            foreach (var htmlNode in collection)
            {
                var lines = htmlNode.InnerHtml.Split(new[] { "<br>" }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var line in lines)
                {
                    var mas = line.Split(new[] { "&ensp;—&ensp;" }, StringSplitOptions.RemoveEmptyEntries);
                    if (mas.Length < 2) continue;
                    var example = new GlobalExample
                    {
                        Phrase = mas[0].Replace("<i>", "").Trim(),
                        Translation = mas[1].Replace("</i>", "").Trim()
                    };

                    fp.Examples.Add(example);
                }
            }

            fp.Phrase.Audio = GenerateAudio(fp.Phrase.Phrase);
            foreach (var ex in fp.Examples)
            {
                ex.Audio = GenerateAudio(ex.Phrase);
            }

            return fp;
        }
        #endregion 

        public static byte[] GenerateAudio(string phrase) // generate wave audio and convert it to mp3(to decrease size)
        {
            var m = new MemoryStream();
            var r = new MemoryStream();

            Synthesizer.SetOutputToWaveStream(m);

            //I don't know why sync piece of code needs to be invoked in async action method
            Synthesizer.Speak(phrase);

            m.Seek(0, SeekOrigin.Begin);
            using (var rdr = new WaveFileReader(m))
            using (var wtr = new LameMP3FileWriter(r, rdr.WaveFormat, LAMEPreset.VBR_100))
            {
                rdr.CopyTo(wtr);
            }

            return r.GetBuffer();
        }
    }
}