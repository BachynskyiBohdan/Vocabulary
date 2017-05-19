using System.Linq;
using System.Web.Mvc;
using Ninject.Infrastructure.Language;
using Vocabulary.Domain.Abstract;
using Vocabulary.Domain.Entities;
using Vocabulary.Web.Areas.Admin.Models;
using Vocabulary.Web.Controllers;

namespace Vocabulary.Web.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    public class TranslationsController : BaseController
    {
        #region Initialize

        private readonly IGlobalPhraseRepository _globalPhraseRepository;
        private readonly IGlobalTranslationRepository _globalTranslationRepository;

        public TranslationsController(ILanguageRepository languageRepository, IGlobalPhraseRepository globalPhraseRepository, 
            IGlobalTranslationRepository globalTranslationRepository) : base(languageRepository)
        {
            _globalPhraseRepository = globalPhraseRepository;
            _globalTranslationRepository = globalTranslationRepository;
        }

        #endregion Initialize


        #region Main

        public ActionResult Index()
        {
            return RedirectToAction("MainPage");
        }

        public ActionResult MainPage(decimal phraseId, int count = 20, int page = 1)
        {
            var phrase = _globalPhraseRepository.GlobalPhrases.First(p => p.Id == phraseId);
            if (phrase == null)
            {
                TempData["Message"] = string.Format("Translation of phrase with id = {0} not found", phraseId);
                return RedirectToAction("MainPage", "Phrases");
            }
            Session["GlobalPhrase"] = phrase.Phrase;
            Session["GlobalPhraseId"] = phrase.Id;
            Session["ElementsPerPage"] = count;

            var list = new GlobalListViewModel
            {
                GlobalTranslations = _globalTranslationRepository.GlobalTranslations
                    .Where(t => t.GlobalPhraseId == phraseId)
                    .OrderBy(t => t.Id)
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

        public ActionResult AddTranslation(decimal phraseId)
        {
            var model = new TranslationViewModel
            {
                Languages =
                    _languageRepository.Languages.Select(
                        x => new SelectListItem { Text = x.FullName, Value = x.Id.ToString() }).ToEnumerable(),
                GlobalTranslation = { GlobalPhraseId = phraseId }
            };

            return View(model);
        }
        [HttpPost]
        public ActionResult AddTranslation(TranslationViewModel model)
        {
            var phrase = _globalPhraseRepository.Get(p => p.Id == model.GlobalTranslation.GlobalPhraseId);
            var trans = _globalTranslationRepository.GetAll(t => t.GlobalPhraseId == model.GlobalTranslation.GlobalPhraseId).ToList();

            var lang = trans.Select(l => new Language { Id = l.LanguageId }).ToList();
            lang.Add(new Language { Id = phrase.LanguageId });

            model.GlobalTranslation.LanguageId = decimal.Parse(model.SelectedLanguage);

            if (lang.Any(l => l.Id == model.GlobalTranslation.LanguageId))
            {
                ModelState.AddModelError("SelectedLanguage", "Such Language already used.");
            }
            if (ModelState.IsValid)
            {
                _globalTranslationRepository.Add(model.GlobalTranslation);
                TempData["Success"] = string.Format("Translation with Id = {0} was successfully added.", model.GlobalTranslation.Id);

                return RedirectToAction("MainPage", new { phraseId = Session["GlobalPhraseId"] });
            }

            model.Languages = _languageRepository.Languages.Select(x => new SelectListItem { Text = x.FullName, Value = x.Code }).ToEnumerable();
            return View(model);
        }

        public ActionResult EditTranslation(decimal id)
        {
            var translation = _globalTranslationRepository.Get(t => t.Id == id);
            if (translation == null)
            {
                TempData["Error"] = string.Format("Translation with id = {0} not found", id);
                return RedirectToAction("MainPage", new { phraseId = Session["GlobalPhraseId"] });
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

                return RedirectToAction("MainPage", new { phraseId = translation.GlobalPhraseId });
            }
            return View(translation);
        }

        public ActionResult DeleteTranslation(decimal id)
        {
            var tr = _globalTranslationRepository.Get(t => t.Id == id);
            if (tr == null)
            {
                TempData["Error"] = string.Format("Translation with id = {0} not found", id);
                return RedirectToAction("MainPage", new { phraseId = Session["GlobalPhraseId"] });
            }
            if (_globalTranslationRepository.Delete(tr))
            {
                TempData["Success"] = string.Format("Translation with Id = {0} was successfully deleted.", tr.Id);
            }
            else
            {
                TempData["Error"] = string.Format("An Error occupied. Translation with id = {0} wasn't deleted", tr.Id);
            }
            return RedirectToAction("MainPage", new { phraseId = Session["GlobalPhraseId"] });
        }

        #endregion Main


        #region Helpers

        public ActionResult GetTranslationsNavigation()
        {
            var id = decimal.Parse(Session["GlobalPhraseId"].ToString());
            var count = _globalTranslationRepository.GlobalTranslations.Count(e => e.GlobalPhraseId == id);
            return PartialView(count);
        }

        #endregion Helpers

    }
}
