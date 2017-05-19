using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Ninject.Infrastructure.Language;
using Vocabulary.Domain.Abstract;
using Vocabulary.Domain.Entities;
using Vocabulary.Web.Areas.Admin.Models;
using Vocabulary.Web.Controllers;
using WebGrease.Css.Extensions;

// ReSharper disable SpecifyACultureInStringConversionExplicitly
// ReSharper disable CSharpWarnings::CS1998
namespace Vocabulary.Web.Areas.Admin.Controllers
{
    [Authorize(Roles="Admin")]
    public class PhrasesController : BaseController
    {
        #region Initialize

        private readonly IGlossaryRepository _glossaryRepository;
        private readonly IGlobalExampleRepository _globalExampleRepository;
        private readonly IGlobalPhraseRepository _globalPhraseRepository;
        private readonly IGlobalTranslationRepository _globalTranslationRepository;

        public PhrasesController(ILanguageRepository languageRepository, IGlossaryRepository glossaryRepository, 
            IGlobalExampleRepository globalExampleRepository,IGlobalPhraseRepository globalPhraseRepository, 
            IGlobalTranslationRepository globalTranslationRepository) : base(languageRepository)
        {
            _glossaryRepository = glossaryRepository;
            _globalExampleRepository = globalExampleRepository;
            _globalPhraseRepository = globalPhraseRepository;
            _globalTranslationRepository = globalTranslationRepository;
        }

        #endregion Initialize

        #region Main

        public ActionResult Index()
        {
            return RedirectToAction("MainPage");
        }
        
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
            var phraseModel = new PhraseViewModel
            {
                GlobalPhrase = new GlobalPhrase(),
                Languages = _languageRepository.Languages
                    .Select(
                        x => new SelectListItem { Text = x.FullName, Value = x.Id.ToString() })
                    .ToEnumerable()
            };
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
                        phraseModel.GlobalPhrase.Audio = ParsingTool.GenerateAudio(phraseModel.GlobalPhrase.Phrase);
                    }
                    _globalPhraseRepository.Add(phraseModel.GlobalPhrase);

                    TempData["Success"] = string.Format("Phrase with Id = {0} was successfully added.", phraseModel.GlobalPhrase.Id);
                }).ContinueWith(t => RedirectToAction("MainPage")).Result;
            }

            phraseModel.Languages = _languageRepository.Languages
                .Select(x => new SelectListItem { Text = x.FullName, Value = x.Code }).ToEnumerable();
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
                ParsingTool.StartParse(model, AddToDataBase);
                TempData["Success"] = string.Format("Words was successfully added by parsing.");
            })
                .ContinueWith(t => RedirectToAction("MainPage")).Result;
        }

        public ActionResult AddPhrasesByParse(decimal? glossaryId = null)
        {
            var model = new ParsePhraseViewModel();
            _languageRepository.Languages.ForEach(
                l => model.Languages.Add(new SelectListItem { Text = l.FullName, Value = l.Id.ToString() }));
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
                Glossary glossary = null;
                if (model.GlossaryId != null)
                {
                    glossary = _glossaryRepository.Get(g => g.Id == model.GlossaryId);
                }

                foreach (var line in lines)
                {
                    var mas = line.Split(new[] { '—' }, 2);
                    if (mas.Length < 2) continue;
                    mas[0] = mas[0].Trim();
                    mas[1] = mas[1].Trim();
                    if (mas[0].Split(' ').Length == 1)
                    {
                        wordsList += mas[0] + "\n";
                        continue;
                    }
                    var pTemp = new GlobalPhrase
                    {
                        Audio = ParsingTool.GenerateAudio(mas[0]),
                        Phrase = mas[0].Trim(),
                        PhraseType = PhraseType.Phrase,
                        LanguageId = phLangId,


                    };
                    if (glossary != null)
                    {
                        glossary.GlobalPhrases.Add(pTemp);
                        _glossaryRepository.Update(glossary);
                    }
                    else
                    {
                        _globalPhraseRepository.Add(pTemp);
                    }
                    var text = mas[0];
                    var phrase = _globalPhraseRepository.Get(p => p.Phrase == text);
                    _globalTranslationRepository.Add(new GlobalTranslation
                    {
                        GlobalPhraseId = phrase.Id,
                        LanguageId = trLangId,
                        TranslationPhrase = mas[1]
                    });
                }
                if (!string.IsNullOrEmpty(wordsList))
                {
                    if (glossary != null)
                    {
                        Session["GlossaryId"] = glossary.Id;
                        AddWordsToGlossaryByParse(wordsList);
                    }
                    else
                    {
                        await AddWordsByParse(wordsList);
                    }
                }
                TempData["Success"] = string.Format("Phrases was successfully added by parsing.");
            }).ContinueWith(t =>
            {
                if (model.GlossaryId == null)
                    return RedirectToAction("MainPage");
                return RedirectToAction("SelectedGlossary", new {controller = "Glossaries", id = model.GlossaryId });
            }).Result;
        }

        private void AddWordsToGlossaryByParse(string model)
        {
            var id = decimal.Parse(Session["GlossaryId"].ToString());
            var glossary = _glossaryRepository.Get(g => g.Id == id);

            ParsingTool.StartParse(model, AddToDataBase, glossary);
        }

        public ActionResult AddSentencesByParse()
        {
            var model = new ParsePhraseViewModel();
            _languageRepository.Languages.ForEach(
                l => model.Languages.Add(new SelectListItem { Text = l.FullName, Value = l.Id.ToString() }));
            return View(model);
        }
        [HttpPost]
        public async Task<ActionResult> AddSentencesByParse(ParsePhraseViewModel model)
        {
            if (string.IsNullOrEmpty(model.ParseString))
            {
                TempData["Error"] = string.Format("Posted parsing string is empty.");
            }
            return Task.Factory.StartNew(() =>
            {
                var lines = model.ParseString.Split('\n');
                var phLangId = decimal.Parse(model.PhraseLanguage);
                var trLangId = decimal.Parse(model.TranslationLanguage);
                Glossary glossary = null;
                if (model.GlossaryId != null)
                {
                    glossary = _glossaryRepository.Get(g => g.Id == model.GlossaryId);
                }

                foreach (var line in lines)
                {
                    var mas = line.Split(new[] { '—' }, 2);
                    if (mas.Length < 2) continue;
                    mas[0] = mas[0].Trim();
                    mas[1] = mas[1].Trim();

                    var pTemp = new GlobalPhrase
                    {
                        Audio = ParsingTool.GenerateAudio(mas[0]),
                        Phrase = mas[0],
                        PhraseType = PhraseType.Phrase,
                        LanguageId = phLangId
                    };
                    if (glossary != null)
                    {
                        glossary.GlobalPhrases.Add(pTemp);
                        _glossaryRepository.Update(glossary);
                    }
                    else
                    {
                        _globalPhraseRepository.Add(pTemp);
                    }

                    var text = mas[0];
                    var phrase = _globalPhraseRepository.Get(p => p.Phrase == text);
                    _globalTranslationRepository.Add(new GlobalTranslation
                    {
                        GlobalPhraseId = phrase.Id,
                        LanguageId = trLangId,
                        TranslationPhrase = mas[1]
                    });
                }
                TempData["Success"] = string.Format("Phrases was successfully added by parsing.");

            }).ContinueWith(t =>
            {
                if (model.GlossaryId == null)
                    return RedirectToAction("MainPage");
                return RedirectToAction("SelectedGlossary", new { controller = "Glossaries", id = model.GlossaryId });
            }).Result;
        }

        public ActionResult EditGlobalPhrase(decimal id)
        {
            var globalPhrase = _globalPhraseRepository.GlobalPhrases.FirstOrDefault(p => p.Id == id);
            if (globalPhrase == null)
            {
                TempData["Error"] = string.Format("Phrase with Id = {0} not found.", id);
                return RedirectToAction("MainPage");
            }
            var phraseModel = new PhraseViewModel { GlobalPhrase = globalPhrase };

            var lang = _languageRepository.Languages.FirstOrDefault(l => l.Id == globalPhrase.LanguageId);
            
            phraseModel.Languages = _languageRepository.Languages
                .Select(x => new SelectListItem { Text = x.FullName, Value = x.Id.ToString() }).ToEnumerable();
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
                return Task.Factory.StartNew(() =>
                {
                    if (audio != null)
                    {
                        phraseModel.GlobalPhrase.Audio = new byte[audio.ContentLength];
                        audio.InputStream.Read(phraseModel.GlobalPhrase.Audio, 0, audio.ContentLength);
                    }
                    else
                    {
                        phraseModel.GlobalPhrase.Audio = ParsingTool.GenerateAudio(phraseModel.GlobalPhrase.Phrase);
                    }
                    _globalPhraseRepository.Update(phraseModel.GlobalPhrase);
                    TempData["Success"] = string.Format("Phrase with Id = {0} was successfully updated.",
                        phraseModel.GlobalPhrase.Id);

                }).ContinueWith(t => RedirectToAction("MainPage")).Result;
            }

            phraseModel.Languages = _languageRepository.Languages
                .Select(x => new SelectListItem { Text = x.FullName, Value = x.Code }).ToEnumerable();
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
            if (Request.IsAjaxRequest())
            {
                return Json(new { result = TempData["Success"] }, JsonRequestBehavior.AllowGet);
            }
            return RedirectToAction("MainPage");
        }

        #endregion Main

        #region Helpers

        public ActionResult GetPhrasesNavigation()
        {
            var count = _globalPhraseRepository.GlobalPhrases.Count();
            return PartialView(count);
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
                else
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
                    example.PhraseId = phrase.Id;
                    example.TranslationLanguageId = tr.LanguageId;
                    _globalExampleRepository.Add(example);
                }
            }
        }

        #endregion Helpers
    }
}
