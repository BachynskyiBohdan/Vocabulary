using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Speech.Synthesis;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Fizzler.Systems.HtmlAgilityPack;
using HtmlAgilityPack;
using Ninject.Infrastructure.Language;
using Vocabulary.Domain.Abstract;
using Vocabulary.Domain.Entities;
using Vocabulary.Web.Models.Admin;
using WebGrease.Css.Extensions;

namespace Vocabulary.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : AsyncController
    {
        #region Initialize

        private readonly ILanguageRepository _languageRepository;
        private readonly IGlossaryRepository _glossaryRepository;
        private readonly IGlobalExampleRepository _globalExampleRepository;
        private readonly IGlobalPhraseRepository _globalPhraseRepository;
        private readonly IGlobalTranslationRepository _globalTranslationRepository;
        private readonly SpeechSynthesizer _synthesizer;

        public delegate byte[] TestDelegate(string str);

        public AdminController(ILanguageRepository languageRepository, IGlossaryRepository glossaryRepository, 
            IGlobalExampleRepository globalExampleRepository,IGlobalPhraseRepository globalPhraseRepository, 
            IGlobalTranslationRepository globalTranslationRepository)
        {
            _synthesizer = new SpeechSynthesizer();

            _languageRepository = languageRepository;
            _glossaryRepository = glossaryRepository;
            _globalExampleRepository = globalExampleRepository;
            _globalPhraseRepository = globalPhraseRepository;
            _globalTranslationRepository = globalTranslationRepository;
        }

        #endregion Initialize


        #region Add|Edit|Delete Global Phrase
        public ActionResult MainPage(int count = 50, int page = 1)
        {
            var list = new GlobalListViewModel
            {
                GlobalPhrases = _globalPhraseRepository.GlobalPhrases
                    .OrderBy(p => p.Id)
                    .Skip(count * (page - 1)).Take(count).ToList()
            };

            var lang = _languageRepository.GetAll();
            foreach (var language in lang)
            {
                list.Languages[language.Id] = language.FullName;
            }
            Session["ElementsPerPage"] = count;

            return View(list);
        }

        public ActionResult AddNewGlobalPhrase()
        {
            var phraseModel = new PhraseViewModel { GlobalPhrase = new GlobalPhrase() };
            phraseModel.Languages = _languageRepository.Languages
                .Select(x => new SelectListItem() { Text = x.FullName, Value = x.Id.ToString() }).ToEnumerable();
            return View(phraseModel);
        }
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> AddNewGlobalPhrase(PhraseViewModel phraseModel, HttpPostedFileBase audio)
        {
            if (ModelState.IsValid)
            {
                phraseModel.GlobalPhrase.PhraseType = GlobalPhrase.ParseType(phraseModel.SelectedPhraseType);
                phraseModel.GlobalPhrase.LanguageId = decimal.Parse(phraseModel.SelectedLanguage);
                return Task.Factory.StartNew(() =>
                {
                    if (audio != null)
                    {
                        phraseModel.GlobalPhrase.Audio = new byte[audio.ContentLength];
                        audio.InputStream.Read(phraseModel.GlobalPhrase.Audio, 0, audio.ContentLength);
                    }
                    else //auto generation 
                    {
                        phraseModel.GlobalPhrase.Audio = GenerateAudio(phraseModel.GlobalPhrase.Phrase);
                    }
                    _globalPhraseRepository.Add(phraseModel.GlobalPhrase);

                    TempData["Success"] = string.Format("Phrase with Id = {0} was successfully added.", phraseModel.GlobalPhrase.Id);
                }).ContinueWith(t => RedirectToAction("MainPage")).Result;
            }
            
            phraseModel.Languages = _languageRepository.Languages
                .Select(x => new SelectListItem() { Text = x.FullName, Value = x.Code }).ToEnumerable();
            return View(phraseModel);
        }

        public ActionResult AddWordsByParse()
        {
            return View((object)"");
        }
        [HttpPost]
        public async Task<RedirectToRouteResult> AddWordsByParse(string model)
        {
            return Task.Factory.StartNew(() =>
            {
                StartParse(model);
                TempData["Success"] = string.Format("Words was successfully added by parsing.");
            })
                .ContinueWith(t => RedirectToAction("MainPage")).Result;
        }

        public ActionResult AddPhrasesByParse()
        {
            var model = new ParsePhraseViewModel();
            _languageRepository.Languages.ForEach(
                l => model.Languages.Add(new SelectListItem() {Text = l.FullName, Value = l.Id.ToString()}));
            return View(model);
        }
        [HttpPost]
        public async Task<ActionResult> AddPhrasesByParse(ParsePhraseViewModel model)
        {
            if (string.IsNullOrEmpty(model.ParseString))
            {
                TempData["Error"] = string.Format("Posted parsing string is empty.");
            }
            return Task.Factory.StartNew(async () =>
            {
                var lines = model.ParseString.Split('\n');
                var wordsList = "";
                var phLangId = decimal.Parse(model.PhraseLanguage);
                var trLangId = decimal.Parse(model.TranslationLanguage);

                foreach (var line in lines)
                {
                    var mas = line.Split('—');
                    if (mas.Length < 2) continue;
                    if (mas[0].Trim().Split(' ').Length == 1)
                    {
                        wordsList += mas[0] + "\n";
                        continue;
                    }
                    _globalPhraseRepository.Add(new GlobalPhrase()
                    {
                        Audio = GenerateAudio(mas[0]),
                        Phrase = mas[0],
                        PhraseType = PhraseType.Phrase,
                        LanguageId = phLangId
                    });
                    var text = mas[0];
                    var phrase = _globalPhraseRepository.Get(p => p.Phrase == text);
                    _globalTranslationRepository.Add(new GlobalTranslation()
                    {
                        GlobalPhraseId = phrase.Id,
                        LanguageId = trLangId,
                        TranslationPhrase = mas[1]
                    });
                }
                if (!string.IsNullOrEmpty(wordsList))
                {
                    await AddWordsByParse(wordsList);
                }
                TempData["Success"] = string.Format("Phrases was successfully added by parsing.");
            }).ContinueWith(t => RedirectToAction("MainPage")).Result;
        }

        public ActionResult AddSentencesByParse()
        {
            var model = new ParsePhraseViewModel();
            _languageRepository.Languages.ForEach(
                l => model.Languages.Add(new SelectListItem() { Text = l.FullName, Value = l.Id.ToString() }));
            return View(model);
        }
        [HttpPost]
        public ActionResult AddSentencesByParse(ParsePhraseViewModel model)
        {
            if (string.IsNullOrEmpty(model.ParseString))
            {
                TempData["Error"] = string.Format("Posted parsing string is empty.");
            }

            var lines = model.ParseString.Split('\n');
            var phLangId = decimal.Parse(model.PhraseLanguage);
            var trLangId = decimal.Parse(model.TranslationLanguage);

            foreach (var line in lines)
            {
                var mas = line.Split('—');
                if (mas.Length < 2) continue;

                _globalPhraseRepository.Add(new GlobalPhrase()
                {
                    Audio = GenerateAudio(mas[0]),
                    Phrase = mas[0],
                    PhraseType = PhraseType.Phrase,
                    LanguageId = phLangId
                });

                var text = mas[0];
                var phrase = _globalPhraseRepository.Get(p => p.Phrase == text);
                _globalTranslationRepository.Add(new GlobalTranslation()
                {
                    GlobalPhraseId = phrase.Id,
                    LanguageId = trLangId,
                    TranslationPhrase = mas[1]
                });
            }
            TempData["Success"] = string.Format("Phrases was successfully added by parsing.");
            return RedirectToAction("MainPage");
        }

        public ActionResult EditGlobalPhrase(decimal id)
        {
            var globalPhrase = _globalPhraseRepository.GlobalPhrases.FirstOrDefault(p => p.Id == id);
            if (globalPhrase == null)
            {
                TempData["Error"] = string.Format("Phrase with Id = {0} not found.", id);
                return RedirectToAction("MainPage");
            }
            var phraseModel = new PhraseViewModel() { GlobalPhrase = globalPhrase };

            var lang = _languageRepository.Languages.FirstOrDefault(l => l.Id == globalPhrase.LanguageId);
            phraseModel.Languages = _languageRepository.Languages
                .Select(x => new SelectListItem() { Text = x.FullName, Value = x.Id.ToString() }).ToEnumerable();
            phraseModel.Languages.FirstOrDefault(x => x.Value == lang.Id.ToString()).Selected = true;

            return View(phraseModel);
        }
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> EditGlobalPhrase(PhraseViewModel phraseModel, HttpPostedFileBase audio)
        {
            if (ModelState.IsValid)
            {
                phraseModel.GlobalPhrase.PhraseType = GlobalPhrase.ParseType(phraseModel.SelectedPhraseType);
                phraseModel.GlobalPhrase.LanguageId = decimal.Parse(phraseModel.SelectedLanguage);
                var p = phraseModel.GlobalPhrase.Transcription;
                return Task.Factory.StartNew(() =>
                {
                    if (audio != null)
                    {
                        phraseModel.GlobalPhrase.Audio = new byte[audio.ContentLength];
                        audio.InputStream.Read(phraseModel.GlobalPhrase.Audio, 0, audio.ContentLength);
                    }
                    else
                    {
                        phraseModel.GlobalPhrase.Audio = GenerateAudio(phraseModel.GlobalPhrase.Phrase);
                    }
                    _globalPhraseRepository.Update(phraseModel.GlobalPhrase);
                    TempData["Success"] = string.Format("Phrase with Id = {0} was successfully updated.",
                        phraseModel.GlobalPhrase.Id);

                }).ContinueWith(t => RedirectToAction("MainPage")).Result;
            }

            phraseModel.Languages = _languageRepository.Languages
                .Select(x => new SelectListItem() { Text = x.FullName, Value = x.Code }).ToEnumerable();
            return View(phraseModel);
        }

        public ActionResult DeleteGlobalPhrase(decimal id)
        {
            var globalPhrase = _globalPhraseRepository.GlobalPhrases.FirstOrDefault(p => p.Id == id);
            if (globalPhrase == null)
            {
                TempData["Error"] = string.Format("Phrase with Id = {0} not found.", id);
                return RedirectToAction("MainPage");
            }

            _globalPhraseRepository.Delete(globalPhrase);
            TempData["Success"] = string.Format("Phrase with Id = {0} was successfully deleted.", id);

            return RedirectToAction("MainPage");
        }

        #endregion Add|Edit|Delete Global Phrase


        #region Add|Edit|Delete Global Translation

        public ActionResult TranslationMainPage(decimal phraseId, int count = 20, int page = 1)
        {
            var phrase = _globalPhraseRepository.GlobalPhrases.First(p => p.Id == phraseId);
            if (phrase == null)
            {
                TempData["Message"] = string.Format("Translation of phrase with id = {0} not found", phraseId);
                return RedirectToAction("MainPage");
            }
            Session["GlobalPhrase"] = phrase.Phrase;
            Session["GlobalPhraseId"] = phrase.Id;
            Session["ElementsPerPage"] = count;

            var list = new GlobalListViewModel();
            list.GlobalTranslations = _globalTranslationRepository.GlobalTranslations
                .Where(t => t.GlobalPhraseId == phraseId)
                .OrderBy(t => t.Id)
                .Skip(count * (page - 1))
                .Take(count).ToList(); 

            var lang = _languageRepository.GetAll();
            foreach (var language in lang)
            {
                list.Languages[language.Id] = language.FullName;
            }
            return View(list);
        }

        public ActionResult AddTranslation(decimal phraseId)
        {
            var model = new TranslationViewModel();
            //var phrase = _globalPhraseRepository.Get(p => p.Id == phraseId);
            //using (var context = new EFDbContext())
            //{
            //    var trans = context.GlobalTranslations.Where(t => t.GlobalPhraseId == phraseId).ToList();
            //    var lang = trans.Select(l => new Language() {Id = l.LanguageId}).ToList();
            //    lang.Add(new Language() {Id = phrase.LanguageId});
            //    var p = context.Languages.Where(l => lang.Contains(new Language(){Id = l.Id}));

            //    model.Languages = p.Select(x => new SelectListItem() {Text = x.FullName, Value = x.Code}).ToEnumerable();
            //    model.Languages.Count();
            //}

            model.Languages = _languageRepository.Languages.Select(x => new SelectListItem() { Text = x.FullName, Value = x.Id.ToString() }).ToEnumerable();
            model.GlobalTranslation.GlobalPhraseId = phraseId;

            return View(model);
        }
        [HttpPost]
        public ActionResult AddTranslation(TranslationViewModel model)
        {
            var phrase = _globalPhraseRepository.Get(p => p.Id == model.GlobalTranslation.GlobalPhraseId);
            var trans = _globalTranslationRepository.GetAll(t => t.GlobalPhraseId == model.GlobalTranslation.GlobalPhraseId).ToList();

            var lang = trans.Select(l => new Language() {Id = l.LanguageId}).ToList();            
            lang.Add(new Language() {Id = phrase.LanguageId});

            model.GlobalTranslation.LanguageId = decimal.Parse(model.SelectedLanguage);

            if (lang.Any(l => l.Id == model.GlobalTranslation.LanguageId))
            {
                ModelState.AddModelError("SelectedLanguage", "Such Language already used.");
            }
            if (ModelState.IsValid)
            {
                _globalTranslationRepository.Add(model.GlobalTranslation);
                TempData["Success"] = string.Format("Translation with Id = {0} was successfully added.", model.GlobalTranslation.Id);

                return RedirectToAction("TranslationMainPage", new { phraseId = Session["GlobalPhraseId"] });
            }

            model.Languages = _languageRepository.Languages.Select(x => new SelectListItem() { Text = x.FullName, Value = x.Code }).ToEnumerable();
            return View(model);
        }

        public ActionResult EditTranslation(decimal Id)
        {
            var translation = _globalTranslationRepository.Get(t => t.Id == Id);
            if (translation == null)
            {
                TempData["Error"] = string.Format("Translation with id = {0} not found", translation.Id);
                return RedirectToAction("TranslationMainPage", new { phraseId = Session["GlobalPhraseId"] });
            }
            return View(translation);
        }
        [HttpPost]
        public ActionResult EditTranslation(GlobalTranslation translation)
        {
            if (ModelState.IsValid)
            {
                _globalTranslationRepository.Update(translation);
                TempData["Success"] = string.Format("Translation with Id = {0} was successfully updated.", translation.Id);

                return RedirectToAction("TranslationMainPage", new {phraseId = translation.GlobalPhraseId});
            }
            return View(translation);
        }

        public ActionResult DeleteTranslation(decimal id)
        {
            var tr = _globalTranslationRepository.Get(t => t.Id == id);
            if (tr == null)
            {
                TempData["Error"] = string.Format("Translation with id = {0} not found", tr.Id);
                return RedirectToAction("TranslationMainPage", new { phraseId = Session["GlobalPhraseId"] });
            }
            if (_globalTranslationRepository.Delete(tr))
            {
                TempData["Success"] = string.Format("Translation with Id = {0} was successfully deleted.", tr.Id);
            }
            else
            {
                TempData["Error"] = string.Format("An Error occupied. Translation with id = {0} wasn't deleted", tr.Id);
            }
            return RedirectToAction("TranslationMainPage", new { phraseId = Session["GlobalPhraseId"] });
        }

        #endregion Add|Edit|Delete Global Translation


        #region Add|Edit|Delete Global Examples

        public ActionResult ExampleMainPage(decimal phraseId, int count = 50, int page = 1)
        {
            var phrase = _globalPhraseRepository.GlobalPhrases.First(p => p.Id == phraseId);
            Session["GlobalPhrase"] = phrase.Phrase;
            Session["GlobalPhraseId"] = phrase.Id;
            Session["ElementsPerPage"] = count;

            var list = new GlobalListViewModel();
            list.GlobalExamples = _globalExampleRepository.GlobalExamples
                .Where(t => t.GlobalPhraseId == phraseId)
                .OrderBy(e => e.Id)
                .Skip(count * (page - 1))
                .Take(count).ToList();

            var lang = _languageRepository.GetAll();
            foreach (var language in lang)
            {
                list.Languages[language.Id] = language.FullName;
            }
            return View(list);
        }

        public ActionResult AddExample(decimal phraseId)
        {
            var model = new ExampleViewModel() {GlobalExample = new GlobalExample()};
            
            model.Translations = _globalTranslationRepository.GlobalTranslations
                .Where(t => t.GlobalPhraseId == phraseId)
                .Select(x => new SelectListItem() { Text = x.TranslationPhrase, Value = x.Id.ToString() }).ToEnumerable();
            model.GlobalExample.GlobalPhraseId = phraseId;

            return View(model);
        }
        [HttpPost]
        public async Task<ActionResult> AddExample(ExampleViewModel example, HttpPostedFileBase audio)
        {
            if (ModelState.IsValid)
            {
                var id = decimal.Parse(example.SelectedTranslation);
                example.GlobalExample.GlobalTranslationId = id;
                var translations =
                    _globalTranslationRepository.GetAll(t => t.GlobalPhraseId == example.GlobalExample.GlobalPhraseId).Where(t => t.Id == id);
                return Task.Factory.StartNew(() =>
                {
                    if (audio != null)
                    {
                        example.GlobalExample.Audio = new byte[audio.ContentLength];
                        audio.InputStream.Read(example.GlobalExample.Audio, 0, audio.ContentLength);
                    }
                    else
                    {
                        example.GlobalExample.Audio = GenerateAudio(example.GlobalExample.Phrase);
                    }

                    _globalExampleRepository.Add(example.GlobalExample);
                    TempData["Success"] = string.Format("Exapmle with id = {0} was succesfully added", example.GlobalExample.Id);
                }).ContinueWith(t => RedirectToAction("ExampleMainPage", new {phraseId = Session["GlobalPhraseId"]})).Result;
            }

            return View(example);
        }

        public ActionResult AddExamplesByParse(decimal phraseId)
        {
            var model = new ParseExamplesViewModel();
            _globalTranslationRepository.GlobalTranslations
                .Where(t => t.GlobalPhraseId == phraseId)
                .ForEach(
                t => model.Translations.Add(new SelectListItem() { Text = t.TranslationPhrase, Value = t.Id.ToString() }));
            model.PhraseId = phraseId;
            return View(model);
        }
        [HttpPost]
        public async Task<ActionResult> AddExamplesByParse(ParseExamplesViewModel model)
        {
            if (string.IsNullOrEmpty(model.ParseString))
            {
                TempData["Error"] = string.Format("Posted parsing string is empty.");
            }
            return Task.Factory.StartNew(() =>
            {
                var lines = model.ParseString.Split('\n');
                var transId = decimal.Parse(model.SelectedTranslation);

                foreach (var line in lines)
                {
                    var mas = line.Split('—');
                    if (mas.Length < 2) continue;

                    _globalExampleRepository.Add(new GlobalExample()
                    {
                        Audio = GenerateAudio(mas[0]),
                        Phrase = mas[0],
                        Translation = mas[1],
                        GlobalPhraseId = model.PhraseId,
                        GlobalTranslationId = transId
                    });
                }
                TempData["Success"] = string.Format("Phrases was successfully added by parsing.");
            }).ContinueWith(x => RedirectToAction("ExampleMainPage")).Result;
        }

        public ActionResult EditExample(decimal id)
        {
            var example = _globalExampleRepository.Get(e => e.Id == id);
            if (example == null)
            {
                TempData["Error"] = string.Format("Example with Id = {0} not found.", id);
                return RedirectToAction("ExampleMainPage", new { phraseId = Session["GlobalPhraseId"] });
            }
            return View(example);
        }
        [HttpPost]
        public async Task<ActionResult> EditExample(GlobalExample example, HttpPostedFileBase audio)
        {
            if (ModelState.IsValid)
            {
                return Task.Factory.StartNew(() =>
                {
                    if (audio != null)
                    {
                        example.Audio = new byte[audio.ContentLength];
                        audio.InputStream.Read(example.Audio, 0, audio.ContentLength);
                    }
                    else // auto generation
                    {
                        example.Audio = GenerateAudio(example.Phrase);
                    }

                    _globalExampleRepository.Update(example);
                    TempData["Success"] = string.Format("Example with id = {0} was succesfully updated", example.Id);

                }).ContinueWith(t => RedirectToAction("ExampleMainPage", new { phraseId = example.GlobalPhraseId })).Result;
            }
            return View(example);
        }

        public ActionResult DeleteExample(decimal id)
        {
            var example = _globalExampleRepository.Get(t => t.Id == id);
            if (example == null)
            {
                TempData["Error"] = string.Format("Example with Id = {0} not found.", id);
                return RedirectToAction("ExampleMainPage", new { phraseId = Session["GlobalPhraseId"] });
            }
            if (_globalExampleRepository.Delete(example))
            {
                TempData["Success"] = string.Format("Example with id = {0} was succesfully deleted", example.Id);
            }
            else
            {
                TempData["Error"] = string.Format("An Error occupied. Example with id = {0} wasn't deleted",
                    example.Id);
            }
            if (Request.IsAjaxRequest())
            {
                return Json(new {result = true}, JsonRequestBehavior.AllowGet);
            }
            return RedirectToAction("ExampleMainPage", new { phraseId = Session["GlobalPhraseId"] });
        }


        #endregion Add|Edit|Delete Global Examples


        #region Add|Edit|Delete Glossaries

        public ActionResult GlossariesMainPage(int count = 20, int page = 1)
        {
            var gl = new GlobalListViewModel();
            gl.Glossaries = _glossaryRepository.Glossaries
                .OrderBy(e => e.Id)
                .Skip(count * (page - 1))
                .Take(count).ToList();

            gl.Languages = new Dictionary<decimal, string>();
            var l = _languageRepository.GetAll();
            foreach (var language in l)
            {
                gl.Languages.Add(language.Id, language.FullName);
            }
            Session["ElementsPerPage"] = count;

            return View(gl);
        }
        public ActionResult SelectedGlossary(decimal id)
        {
            var glossary = _glossaryRepository.Get(g => g.Id == id);
            if (glossary == null)
            {
                TempData["Message"] = string.Format("Glossary with Id = {0} not found.", id);
                return RedirectToAction("GlossariesMainPage");
            }
            Session["Glossary"] = glossary.Name;
            Session["GlossaryId"] = glossary.Id;
            Session["LanguageId"] = glossary.LanguageId;

            return View(glossary);
        }

        public ActionResult AddPhrasesToGlossary(int count = 20, int page = 1)
        {
            var id = decimal.Parse(Session["GlossaryId"].ToString());
            var glossary = _glossaryRepository.Get(g => g.Id == id);
            if (glossary == null)
            {
                TempData["Message"] = string.Format("Glossary with Id = {0} not found.", id);
                return RedirectToAction("GlossariesMainPage");
            }
            Session["ElementsPerPage"] = count;

            var list = _globalPhraseRepository.GlobalPhrases
                .Where(p => p.LanguageId == glossary.LanguageId)
                .OrderBy(p => p.Id)
                .Skip(count*(page - 1))
                .Take(count).AsEnumerable();

            return View(list);
        }
        public ActionResult AddPhraseToGlossary(decimal phraseId)
        {
            var id = decimal.Parse(Session["GlossaryId"].ToString());
            bool b;
            using (var c = new EFDbContext())
            {
                var glossary = c.Glossaries.First(g => g.Id == id);
                if (glossary == null)
                {
                    TempData["Error"] = string.Format("Glossary with Id = {0} not found.", id);
                    return RedirectToAction("GlossariesMainPage");
                }

                var phrase = c.GlobalPhrases.First(p => p.Id == phraseId);
                if (phrase == null)
                {
                    TempData["Error"] = string.Format("Phrase with Id = {0} not found.", phraseId);
                    return RedirectToAction("GlossariesMainPage");
                }

                glossary.GlobalPhrases.Add(phrase);
                b = c.SaveChanges() > 0;
            }
            TempData["Success"] = string.Format("Phrase with Id = {0} was successfully added to glossary.", phraseId);
            if (Request.IsAjaxRequest())
            {
                return Json(new { success = b, phraseId }, JsonRequestBehavior.AllowGet);
            }
            return RedirectToAction("AddPhrasesToGlossary", new {id, langId = Session["LanguageId"]});
        }

        public ActionResult AddWordsToGlossaryByParse()
        {
            var model = "";
            return View((object)model);
        }
        [HttpPost]
        public async Task<ActionResult> AddWordsToGlossaryByParse(string model)
        {
            if (ModelState.IsValid)
            {
                var id = decimal.Parse(Session["GlossaryId"].ToString());
                var glossary = _glossaryRepository.Get(g => g.Id == id);

                TempData["Success"] = string.Format("Phrases was successfully added to glossary by parsing.");
                return Task.Factory.StartNew(() => StartParse(model, glossary))
                    .ContinueWith(t => RedirectToAction("SelectedGlossary", new {id})).Result;
            }
            return View(model);
        }


        public ActionResult AddGlossary()
        {
            var glossary = new GlossaryViewModel();

            glossary.Languages =
                _languageRepository.Languages.Select(
                    l => new SelectListItem() {Text = l.FullName, Value = l.Id.ToString()});

            return View(glossary);
        }
        [HttpPost]
        public ActionResult AddGlossary(GlossaryViewModel glossary, HttpPostedFileBase image)
        {
            if (image == null)
            {
                ModelState.AddModelError("", "Icon is empty. Please choose icon.");
            }
            if (ModelState.IsValid)
            {
                glossary.Glossary.LanguageId = decimal.Parse(glossary.SelectedLanguage);

                glossary.Glossary.Icon = new byte[image.ContentLength];
                image.InputStream.Read(glossary.Glossary.Icon, 0, image.ContentLength);

                _glossaryRepository.Add(glossary.Glossary);
                TempData["Success"] = string.Format("Glossary with Id = {0} was succesfully added", glossary.Glossary.Id);

                return RedirectToAction("GlossariesMainPage");
            }

            return View(glossary);
        }

        public ActionResult EditGlossary(decimal id)
        {
            var glossary = _glossaryRepository.Get(e => e.Id == id);
            if (glossary == null)
            {
                TempData["Error"] = string.Format("Glossary with Id = {0} not found.", id);
                return RedirectToAction("GlossariesMainPage");
            }
            return View(glossary);
        }
        [HttpPost]
        public ActionResult EditGlossary(Glossary glossary, HttpPostedFileBase image)
        {
            if (ModelState.IsValid)
            {
                if (image != null)
                {
                    glossary.Icon = new byte[image.ContentLength];
                    image.InputStream.Read(glossary.Icon, 0, image.ContentLength);
                }
                _glossaryRepository.Update(glossary);
                TempData["Success"] = string.Format("Glossary with id = {0} was succesfully updated", glossary.Id);

                return RedirectToAction("GlossariesMainPage");
            }
            return View(glossary);
        }

        public ActionResult DeleteGlossary(decimal id)
        {
            var glossary = _glossaryRepository.Get(t => t.Id == id);
            if (glossary == null)
            {
                TempData["Error"] = string.Format("Glossary with Id = {0} not found.", id);
                return RedirectToAction("GlossariesMainPage");
            }
            if (_glossaryRepository.Delete(glossary))
            {
                TempData["Success"] = string.Format("Glossary with id = {0} was succesfully deleted", glossary.Id);
            }
            else
            {
                TempData["Error"] = string.Format("An Error occupied. Glossary with id = {0} wasn't deleted",
                    glossary.Id);
            }
            return RedirectToAction("GlossariesMainPage");
        }


        #endregion Add|Edit|Delete Glossaries


        #region Navigation

        public ActionResult GetPhrasesNavigation()
        {
            var count = _globalPhraseRepository.GlobalPhrases.Count();
            return PartialView(count);
        }

        public ActionResult GetTranslationsNavigation()
        {
            var id = decimal.Parse(Session["GlobalPhraseId"].ToString());
            var count = _globalTranslationRepository.GlobalTranslations.Count(e => e.GlobalPhraseId == id);
            return PartialView(count);
        }
        
        public ActionResult GetExamplesNavigation()
        {
            var id = decimal.Parse(Session["GlobalPhraseId"].ToString());
            var count = _globalExampleRepository.GlobalExamples.Count(e => e.GlobalPhraseId == id);
            return PartialView(count);
        }

        public ActionResult GetGlossariesNavigation()
        {
            var count = _glossaryRepository.Glossaries.Count();
            return PartialView(count);
        }
        public ActionResult GetGlossaryPhrasesNavigation()
        {
            var id = decimal.Parse(Session["LanguageId"].ToString());
            var count = _globalPhraseRepository.GlobalPhrases.Count(p => p.LanguageId == id);
            return PartialView(count);
        }

        #endregion Navigation


        #region Helpers

        private async void StartParse(string websites, Glossary glossary = null)
        {
            var html = new HtmlDocument();
            var addresses = websites.Split('\n');
            var lang = _languageRepository.Languages.ToList();
            foreach (var address in addresses)
            {
                if (string.IsNullOrEmpty(address)) continue;
                using (var client = new WebClient())
                {
                    var response = client.OpenRead(string.Format("http://www.wooordhunt.ru/word/{0}", address));
                    var reader = new StreamReader(response);
                    var source = reader.ReadToEnd();
                    html.LoadHtml(source);
                }
                var document = html.DocumentNode;

                var fullPhrase = await GetFullPhrase(document);
                AddToDataBase(fullPhrase, glossary);
            }
        }
        /// <summary>
        /// Parse web pages to simplify population of database
        /// </summary>
        /// <param name="document">words list separated by '\n' symbol</param>
        /// <returns></returns>
        private async Task<FullPhraseViewModel> GetFullPhrase(HtmlNode document)
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
            fp.Translation.LanguageId = 3m;

            var collection = document.QuerySelectorAll("#wd_content .tr .ex").ToList();
            
            //collection.Add(document.QuerySelector("#wd_content .block.phrases")); //TODO: parse this section
            foreach (var htmlNode in collection)
            {
                var lines = htmlNode.InnerHtml.Split(new[] {"<br>"}, StringSplitOptions.RemoveEmptyEntries);
                foreach (var line in lines)
                {
                    var mas = line.Split(new[] {"&ensp;—&ensp;"}, StringSplitOptions.RemoveEmptyEntries);
                    if (mas.Length < 2) continue;
                    var example = new GlobalExample();

                    example.Phrase = mas[0].Replace("<i>", "").Trim();
                    example.Translation = mas[1].Replace("</i>", "").Trim();

                    fp.Examples.Add(example);
                }
            }

            var stream = new MemoryStream();
            {
                _synthesizer.SetOutputToWaveStream(stream);

                _synthesizer.Speak(fp.Phrase.Phrase);
                fp.Phrase.Audio = stream.GetBuffer();
                foreach (var ex in fp.Examples)
                {
                    stream = new MemoryStream();
                    _synthesizer.SetOutputToWaveStream(stream);
                    _synthesizer.Speak(ex.Phrase);
                    ex.Audio = stream.GetBuffer();
                }
            }

            return fp;
        }
        private void AddToDataBase(FullPhraseViewModel fp, Glossary glossary)
        {
            var b = _globalPhraseRepository.GlobalPhrases.Any(p => p.Phrase == fp.Phrase.Phrase);
            if (!b)
            {
                if (glossary != null)
                {
                    glossary.GlobalPhrases.Add(fp.Phrase);
                    _glossaryRepository.Update(glossary);
                }
                {
                    _globalPhraseRepository.Add(fp.Phrase);
                }

                var phrase = _globalPhraseRepository.Get(p => p.Phrase == fp.Phrase.Phrase);
                fp.Translation.GlobalPhraseId = phrase.Id;
                _globalTranslationRepository.Add(fp.Translation);
                var tr =
                    _globalTranslationRepository.Get(t => t.TranslationPhrase == fp.Translation.TranslationPhrase);

                foreach (var example in fp.Examples)
                {
                    example.GlobalPhraseId = phrase.Id;
                    example.GlobalTranslationId = tr.Id;
                    _globalExampleRepository.Add(example);
                }
            }
        }

        private byte[] GenerateAudio(string phrase)
        {
            var m = new MemoryStream();
            _synthesizer.SetOutputToWaveStream(m);
            //I don't know why with sync piece of code need to be invoked in async action method
            _synthesizer.Speak(phrase);
            return m.GetBuffer();
        }

        public ActionResult GetPhraseAudio(decimal id)
        {
            var phrase = _globalPhraseRepository.Get(p => p.Id == id);
            if (phrase.Audio == null)
            {
                return null;
            }
            return File(phrase.Audio, "audio/mpeg");
        }
        public ActionResult GetExampleAudio(decimal id)
        {
            var example = _globalExampleRepository.Get(p => p.Id == id);
            if (example.Audio == null)
            {
                return null;
            }
            return File(example.Audio, "audio/mpeg");
        }

        public ActionResult GetGlossaryIcon(decimal id)
        {
            var example = _glossaryRepository.Get(p => p.Id == id);
            if (example.Icon == null)
            {
                return null;
            }
            return File(example.Icon, "image/png");
        }

        #endregion Helpers
    }
}
    