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
    [Authorize(Roles = "Admin")]
    public class ExamplesController : BaseController
    {
        #region Initialize

        private readonly IGlobalExampleRepository _globalExampleRepository;
        private readonly IGlobalPhraseRepository _globalPhraseRepository;
        private readonly IGlobalTranslationRepository _globalTranslationRepository;

        public ExamplesController(ILanguageRepository languageRepository, IGlobalExampleRepository globalExampleRepository,
            IGlobalPhraseRepository globalPhraseRepository, IGlobalTranslationRepository globalTranslationRepository)
            : base(languageRepository)
        {
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

        public ActionResult MainPage(decimal phraseId, int count = 50, int page = 1)
        {
            var phrase = _globalPhraseRepository.GlobalPhrases.First(p => p.Id == phraseId);
            Session["GlobalPhrase"] = phrase.Phrase;
            Session["GlobalPhraseId"] = phrase.Id;
            Session["ElementsPerPage"] = count;

            var list = new GlobalListViewModel
            {
                GlobalExamples = _globalExampleRepository.GlobalExamples
                    .Where(t => t.PhraseId == phraseId)
                    .OrderBy(e => e.Id)
                    .Skip(count * (page - 1))
                    .Take(count).ToList()
            };

            var lang = _languageRepository.GetAll();
            foreach (var language in lang)
            {
                list.Languages[language.Id] = language.FullName;
            }
            return View(list);
        }

        public ActionResult AddExample(decimal phraseId)
        {
            var model = new ExampleViewModel
            {
                GlobalExample = new GlobalExample(),
                Translations = _globalTranslationRepository.GlobalTranslations
                    .Where(t => t.GlobalPhraseId == phraseId)
                    .Select(
                        x =>
                            new SelectListItem
                            {
                                Text = x.TranslationPhrase,
                                Value = x.LanguageId.ToString()
                            }).ToEnumerable()
            };

            model.GlobalExample.PhraseId = phraseId;

            return View(model);
        }
        [HttpPost]
        public async Task<ActionResult> AddExample(ExampleViewModel example, HttpPostedFileBase audio)
        {
            if (ModelState.IsValid)
            {
                var id = decimal.Parse(example.SelectedTranslation);
                example.GlobalExample.TranslationLanguageId = id;

                return Task.Factory.StartNew(() =>
                {
                    if (audio != null)
                    {
                        example.GlobalExample.Audio = new byte[audio.ContentLength];
                        audio.InputStream.Read(example.GlobalExample.Audio, 0, audio.ContentLength);
                    }
                    else
                    {
                        example.GlobalExample.Audio = ParsingTool.GenerateAudio(example.GlobalExample.Phrase);
                    }

                    _globalExampleRepository.Add(example.GlobalExample);
                    TempData["Success"] = string.Format("Exapmle with id = {0} was succesfully added", example.GlobalExample.Id);
                }).ContinueWith(t => RedirectToAction("MainPage", new { phraseId = Session["GlobalPhraseId"] })).Result;
            }

            return View(example);
        }

        public ActionResult AddExamplesByParse(decimal phraseId)
        {
            var model = new ParseExamplesViewModel();
            _globalTranslationRepository.GlobalTranslations
                .Where(t => t.GlobalPhraseId == phraseId)
                .ForEach(
                t => model.Translations.Add(new SelectListItem { Text = t.TranslationPhrase, Value = t.LanguageId.ToString() }));
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

                    _globalExampleRepository.Add(new GlobalExample
                    {
                        Audio = ParsingTool.GenerateAudio(mas[0]),
                        Phrase = mas[0],
                        Translation = mas[1],
                        PhraseId = model.PhraseId,
                        TranslationLanguageId = transId
                    });
                }
                TempData["Success"] = string.Format("Phrases was successfully added by parsing.");
            }).ContinueWith(x => RedirectToAction("MainPage")).Result;
        }

        public ActionResult EditExample(decimal id)
        {
            var example = _globalExampleRepository.Get(e => e.Id == id);
            if (example == null)
            {
                TempData["Error"] = string.Format("Example with Id = {0} not found.", id);
                return RedirectToAction("MainPage", new { phraseId = Session["GlobalPhraseId"] });
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
                        example.Audio = ParsingTool.GenerateAudio(example.Phrase);
                    }

                    _globalExampleRepository.Update(example);
                    TempData["Success"] = string.Format("Example with id = {0} was succesfully updated", example.Id);

                }).ContinueWith(t => RedirectToAction("MainPage", new { phraseId = example.PhraseId })).Result;
            }
            return View(example);
        }

        public ActionResult DeleteExample(decimal id)
        {
            var example = _globalExampleRepository.Get(t => t.Id == id);
            if (example == null)
            {
                TempData["Error"] = string.Format("Example with Id = {0} not found.", id);
                return RedirectToAction("MainPage", new { phraseId = Session["GlobalPhraseId"] });
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
                return Json(new { result = true }, JsonRequestBehavior.AllowGet);
            }
            return RedirectToAction("MainPage", new { phraseId = Session["GlobalPhraseId"] });
        }


        #endregion Main


        #region Helpers

        public ActionResult GetExamplesNavigation()
        {
            var id = decimal.Parse(Session["GlobalPhraseId"].ToString());
            var count = _globalExampleRepository.GlobalExamples.Count(e => e.PhraseId == id);
            return PartialView(count);
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

        #endregion Helpers

    }
}
