using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Vocabulary.Domain.Abstract;
using Vocabulary.Domain.Entities;
using Vocabulary.Web.Areas.Admin.Models;
using Vocabulary.Web.Controllers;
using WebGrease.Css.Extensions;

namespace Vocabulary.Web.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    public class GlossariesController : BaseController
    {
        #region Initialize

        private readonly IGlossaryRepository _glossaryRepository;
        private readonly IGlobalExampleRepository _globalExampleRepository;
        private readonly IGlobalPhraseRepository _globalPhraseRepository;
        private readonly IGlobalTranslationRepository _globalTranslationRepository;
        
        public GlossariesController(ILanguageRepository languageRepository, IGlossaryRepository glossaryRepository, 
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

        public ActionResult MainPage(int count = 20, int page = 1)
        {
            var gl = new GlobalListViewModel
            {
                Glossaries = _glossaryRepository.Glossaries
                    .OrderBy(e => e.Id)
                    .Skip(count * (page - 1))
                    .Take(count).ToList(),
                Languages = new Dictionary<decimal, string>()
            };

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
                return RedirectToAction("MainPage");
            }
            Session["Glossary"] = glossary.Name;
            Session["GlossaryId"] = glossary.Id;
            Session["LanguageId"] = glossary.LanguageId;

            return View(glossary);
        }

        public ActionResult AddPhrasesToGlossary(int count = 100, int page = 1)
        {
            var id = decimal.Parse(Session["GlossaryId"].ToString());
            var glossary = _glossaryRepository.Get(g => g.Id == id);
            if (glossary == null)
            {
                TempData["Message"] = string.Format("Glossary with Id = {0} not found.", id);
                return RedirectToAction("MainPage");
            }
            Session["ElementsPerPage"] = count;

            var list = _globalPhraseRepository.GlobalPhrases
                .Where(p => p.LanguageId == glossary.LanguageId)
                .OrderBy(p => p.Id)
                .Skip(count * (page - 1))
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
                    return RedirectToAction("MainPage");
                }

                var phrase = c.GlobalPhrases.First(p => p.Id == phraseId);
                if (phrase == null)
                {
                    TempData["Error"] = string.Format("Phrase with Id = {0} not found.", phraseId);
                    return RedirectToAction("MainPage");
                }

                glossary.GlobalPhrases.Add(phrase);
                b = c.SaveChanges() > 0;
            }
            TempData["Success"] = string.Format("Phrase with Id = {0} was successfully added to glossary.", phraseId);
            if (Request.IsAjaxRequest())
            {
                return Json(new { success = b, phraseId }, JsonRequestBehavior.AllowGet);
            }
            return RedirectToAction("AddPhrasesToGlossary", new { id, langId = Session["LanguageId"] });
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
                return Task.Factory.StartNew(() => ParsingTool.StartParse(model, AddToDataBase, glossary))
                    .ContinueWith(t => RedirectToAction("SelectedGlossary", new { id })).Result;
            }
            return View(model);
        }


        public ActionResult AddGlossary()
        {
            var model = new GlossaryViewModel();

            _languageRepository.Languages.ForEach(
                    l => model.Languages.Add(new SelectListItem { Text = l.FullName, Value = l.Id.ToString() }));

            model.Languages.First().Selected = true;

            return View(model);
        }
        [HttpPost]
        public ActionResult AddGlossary(GlossaryViewModel model, HttpPostedFileBase image)
        {
            if (image == null)
            {
                ModelState.AddModelError("", "Icon is empty. Please choose icon.");
            }
            if (ModelState.IsValid)
            {
                model.Glossary.LanguageId = decimal.Parse(model.SelectedLanguage);

                model.Glossary.Icon = new byte[image.ContentLength];
                image.InputStream.Read(model.Glossary.Icon, 0, image.ContentLength);

                _glossaryRepository.Add(model.Glossary);
                TempData["Success"] = string.Format("Glossary with Id = {0} was succesfully added", model.Glossary.Id);

                return RedirectToAction("MainPage");
            }
            model.Languages = new List<SelectListItem>();
            _languageRepository.Languages.ForEach(
                l => model.Languages.Add(new SelectListItem { Text = l.FullName, Value = l.Id.ToString() }));


            return View(model);
        }

        public ActionResult EditGlossary(decimal id)
        {
            var glossary = _glossaryRepository.Get(e => e.Id == id);
            if (glossary == null)
            {
                TempData["Error"] = string.Format("Glossary with Id = {0} not found.", id);
                return RedirectToAction("MainPage");
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

                return RedirectToAction("MainPage");
            }
            return View(glossary);
        }

        public ActionResult DeleteGlossary(decimal id)
        {
            var glossary = _glossaryRepository.Get(t => t.Id == id);
            if (glossary == null)
            {
                TempData["Error"] = string.Format("Glossary with Id = {0} not found.", id);
                return RedirectToAction("MainPage");
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
            return RedirectToAction("MainPage");
        }

        public ActionResult DeleteGlobalPhraseFromGlossary(decimal id, decimal glosId)
        {
            using (var ctx = new EFDbContext())
            {
                var globalPhrase = ctx.GlobalPhrases.FirstOrDefault(p => p.Id == id);
                if (globalPhrase == null)
                {
                    TempData["Error"] = string.Format("Phrase with Id = {0} not found.", id);
                    return RedirectToAction("SelectedGlossary", new { id = glosId });
                }
                var glossary = ctx.Glossaries.FirstOrDefault(g => g.Id == glosId);
                globalPhrase.Glossaries.Remove(glossary);
                ctx.SaveChanges();
                TempData["Success"] = string.Format("Phrase with Id = {0} was successfully deleted.", id);
            }
            if (Request.IsAjaxRequest())
            {
                return Json(new { result = TempData["Success"] }, JsonRequestBehavior.AllowGet);
            }
            return RedirectToAction("SelectedGlossary", new { id = glosId });
        }

        #endregion Main

        #region Helpers

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

        public ActionResult GetGlossaryIcon(decimal id)
        {
            var example = _glossaryRepository.Get(p => p.Id == id);
            if (example.Icon == null)
            {
                return null;
            }
            return File(example.Icon, "image/png");
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
