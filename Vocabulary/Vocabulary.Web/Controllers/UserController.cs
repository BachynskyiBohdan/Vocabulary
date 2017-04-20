using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Speech.Synthesis;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using Vocabulary.Domain.Abstract;
using Vocabulary.Domain.Concrete;
using Vocabulary.Domain.Entities;
using Vocabulary.Web.Models.User;
using WebGrease.Css.Extensions;
using WebMatrix.WebData;

namespace Vocabulary.Web.Controllers
{
    [Authorize]
    public class UserController : Controller
    {
        #region Initialize

        private readonly IUsersExampleRepository _usersExampleRepository;
        private readonly IUsersPhraseRepository _usersPhraseRepository;
        private readonly IUsersTranslationRepository _usersTranslationRepository;
        private readonly ILanguageRepository _languageRepository;
        private readonly IGlossaryRepository _glossaryRepository;
        private readonly IGlobalExampleRepository _globalExampleRepository;
        private readonly IGlobalPhraseRepository _globalPhraseRepository;
        private readonly IGlobalTranslationRepository _globalTranslationRepository;
        private readonly SpeechSynthesizer _synthesizer;

        public UserController(IUsersExampleRepository usersExampleRepository,
            IUsersPhraseRepository usersPhraseRepository,
            IUsersTranslationRepository usersTranslationRepository, ILanguageRepository languageRepository,
            IGlossaryRepository glossaryRepository, IGlobalExampleRepository globalExampleRepository,
            IGlobalPhraseRepository globalPhraseRepository, IGlobalTranslationRepository globalTranslationRepository)
        {
            _synthesizer = new SpeechSynthesizer();

            _usersExampleRepository = usersExampleRepository;
            _usersPhraseRepository = usersPhraseRepository;
            _usersTranslationRepository = usersTranslationRepository;
            _languageRepository = languageRepository;
            _glossaryRepository = glossaryRepository;
            _globalExampleRepository = globalExampleRepository;
            _globalPhraseRepository = globalPhraseRepository;
            _globalTranslationRepository = globalTranslationRepository;
        }

        #endregion Initialize

        public ActionResult Index()
        {
            return RedirectToAction("Vocabulary");
        }

        public ActionResult Vocabulary()
        {
            return View();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="phraseLangId"></param>
        /// <param name="transLangId"></param>
        /// <param name="phraseType"></param>
        /// <param name="learnState">Learing State. [0.0, 1.0], by default = -1 (all states)</param>
        /// <param name="search"></param>
        /// <returns></returns>
        public ActionResult VocabularyData(decimal phraseLangId = 1, decimal transLangId = 3, 
            string phraseType = "All", int learnState = -1, string search = "")
        {
            var model = ParseRequestParametersForVocabulary(phraseLangId, transLangId, phraseType, learnState, search);

            if (Request.IsAjaxRequest())
            {
                var serializer = new JavaScriptSerializer {MaxJsonLength = Int32.MaxValue};
                var result = new ContentResult
                {
                    Content = serializer.Serialize(new {phrases = model.Phrases, translations = model.Translations}),
                    ContentType = "application/json"
                };
                return result;
            }
            return PartialView(model);
        }
        private VocabularyListViewModel ParseRequestParametersForVocabulary(decimal phraseLangId, decimal transLangId, string phraseType,
            int learnState, string search)
        {
            var model = new VocabularyListViewModel();
            decimal userId = WebSecurity.CurrentUserId;
            PhraseType type;
            PhraseType.TryParse(phraseType, out type);

            var query = _usersPhraseRepository.UsersPhrases
                .Where(p => p.UserId == userId)
                .Where(p => p.LanguageId == phraseLangId);
            
            if (type != PhraseType.All)
                query = query.Where(p => p.PhraseType == type);
            if (learnState != -1)
            {
                if (learnState == 2)
                {
                    query = query.Where(p => p.LearningState > 0 && p.LearningState < 1);
                }
                else
                {
                    query = query.Where(p => p.LearningState == learnState);
                }
            }
            if (!string.IsNullOrEmpty(search))
                query = query.Where(p => p.Phrase.StartsWith(search));

            model.Phrases = query.AsEnumerable()
                .Select(p =>
                {
                    p.Audio = null;
                    return p;
                })
                .ToList();

            model.Phrases.ForEach(
                p => model.Translations.Add(_usersTranslationRepository.UsersTranslations
                    .Where(t => t.UserPhraseId == p.Id)
                    .Where(t => t.LanguageId == transLangId)
                    .FirstOrDefault(t => t.LanguageId == 3) ?? new UsersTranslation()));

            _languageRepository.Languages
                .ForEach(
                    l =>
                        model.Languages.Add(new SelectListItem()
                        {
                            Text = l.FullName,
                            Value = l.Id.ToString(CultureInfo.InvariantCulture)
                        }));

            return model;
        }

        public ActionResult VocabularyElementData(decimal id, decimal langId = 1, decimal transLangId = 3)
        {
            if (!Request.IsAjaxRequest())
            {
                return RedirectToAction("Vocabulary");
            }
            
            var phrase =  _usersPhraseRepository.UsersPhrases
                .Where(up => up.UserId == WebSecurity.CurrentUserId)
                .Where(up => up.LanguageId == langId)
                .FirstOrDefault(up => up.Id == id);
            if (phrase == null)
            {
                return HttpNotFound();
            }
            phrase.Audio = null;

            var translation = _usersTranslationRepository.UsersTranslations
                .Where(t => t.UserPhraseId == phrase.Id)
                .FirstOrDefault(t => t.LanguageId == transLangId) ?? new UsersTranslation();

            var examples = _usersExampleRepository.UsersExamples
                .Where(e => e.UserPhraseId == phrase.Id)
                .AsEnumerable()
                .Select( e => { e.Audio = null; return e; })
                .ToList();

            var serializer = new JavaScriptSerializer { MaxJsonLength = Int32.MaxValue };
            var result = new ContentResult
            {
                Content = serializer.Serialize(new { phrase, translation, examples }),
                ContentType = "application/json"
            };
            return result;
        }

        public ActionResult DeleteFromVocabulary(int[] phraseIdList)
        {
            var userId = WebSecurity.CurrentUserId;
            foreach (var id in phraseIdList)
            {
                var phrase = _usersPhraseRepository.UsersPhrases
                    .Where(p => p.UserId == userId)
                    .FirstOrDefault(p => p.Id == id);

                if (phrase == null) continue;
                var ex = _usersExampleRepository.UsersExamples
                    .Where(e => e.UserPhraseId == phrase.Id)
                    .ToList();

                var tr = _usersTranslationRepository.UsersTranslations
                    .Where(t => t.UserPhraseId == phrase.Id)
                    .ToList();

                //ctx.UsersExamples.RemoveRange(ex);
                foreach (var e in ex)
                {
                    _usersExampleRepository.Delete(e);
                }
                foreach (var t in tr)
                {
                    _usersTranslationRepository.Delete(t);
                }
                _usersPhraseRepository.Delete(phrase);
            }
            if (Request.IsAjaxRequest())
            {
                return Json(new {result = true}, JsonRequestBehavior.AllowGet);
            }
            return RedirectToAction("Vocabulary");
        }

        public ActionResult ChangeState(int state, int[] phraseIdList)
        {
            if (!Request.IsAjaxRequest())
            {
                return RedirectToAction("Vocabulary");
            }
            
            var uId = WebSecurity.CurrentUserId;
            foreach (var id in phraseIdList)
            {
                var phrase = _usersPhraseRepository.UsersPhrases
                    .Where(p => p.UserId == uId)
                    .FirstOrDefault(p => p.Id == id);

                if (phrase == null) continue;

                phrase.LearningState = (float)state / 100;
                using (var rep = new EFDbContext())
                {
                    rep.Entry(phrase).State = EntityState.Modified;
                    rep.SaveChanges();
                }
                
            }

            return Json(new {result = true}, JsonRequestBehavior.AllowGet);
        }

        #region Helpers

        public ActionResult GetUsersPhraseAudio(decimal id)
        {
            var phrase = _usersPhraseRepository.Get(p => p.Id == id);
            if (phrase.Audio == null)
            {
                return null;
            }
            return File(phrase.Audio, "audio/mpeg");
        }
        public ActionResult GetUsersExampleAudio(decimal id)
        {
            var example = _usersExampleRepository.Get(p => p.Id == id);
            if (example.Audio == null)
            {
                return null;
            }
            return File(example.Audio, "audio/mpeg");
        }

        public ActionResult GetGlossaryIcon(decimal id)
        {
            var glossary = _glossaryRepository.Get(p => p.Id == id);
            if (glossary.Icon == null)
            {
                return null;
            }
            return File(glossary.Icon, "image/png");
        }

        #endregion Helpers
    }
}
