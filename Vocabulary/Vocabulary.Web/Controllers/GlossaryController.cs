using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Speech.Synthesis;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using Vocabulary.Domain.Abstract;
using Vocabulary.Domain.Entities;
using Vocabulary.Web.Models.GlossaryController;
using WebGrease.Css.Extensions;
using WebMatrix.WebData;

namespace Vocabulary.Web.Controllers
{
    [Authorize(Roles = "User, PremiumUser")]
    public class GlossaryController : Controller
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

        public GlossaryController(IUsersExampleRepository usersExampleRepository,
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
            return RedirectToAction("Glossaries");
        }

        public ActionResult Glossaries(decimal langId = 1) // English as default
        {
            return View(langId);
        } 
        public ActionResult GlossariesData(decimal langId, int page = 1, int count = 9)
        {
            var glossaries = _glossaryRepository.Glossaries
                .Where(g => g.LanguageId == langId)
                .OrderBy(g => g.Id)
                .Skip((page - 1)*count)
                .Take(count)
                .AsEnumerable()
                .Select(g =>
                        new Glossary
                        {
                            Count = g.GlobalPhrases.Count,
                            Icon = null,
                            GlobalPhrases = null,
                            Id = g.Id,
                            LanguageId = g.LanguageId,
                            Name = g.Name
                        })
                .ToList();
            
            var model = new GlossaryListViewModel
            {
                Glossaries = glossaries,
                Languages = new Dictionary<string, string>()
            };
            _languageRepository.Languages.ForEach(l => model.Languages.Add(l.Id.ToString(CultureInfo.InvariantCulture), l.FullName));
            
            model.SelectedLanduage = langId.ToString(CultureInfo.InvariantCulture);
            if (Request.IsAjaxRequest())
            {
                var serializer = new JavaScriptSerializer { MaxJsonLength = Int32.MaxValue };
                var result = new ContentResult
                {
                    Content = serializer.Serialize(new { glossaries, list = model.Languages, language = langId }),
                    ContentType = "application/json"
                };
                return result;
            }
            return PartialView(model);
        }

        public ActionResult Glossary(decimal id, decimal langId)
        {
            var gl = _glossaryRepository.Get(g => g.Id == id);
            if (gl == null)
            {
                return HttpNotFound();
            }

            var model = new GlossaryViewModel {Glossary = gl};
            foreach (var p in gl.GlobalPhrases)
            {
                var glId = p.Id;
                model.Translations.Add(_globalTranslationRepository.Get(t => t.GlobalPhraseId == glId && t.LanguageId == langId) ??
                                       new GlobalTranslation());
            }

            decimal userId = WebSecurity.CurrentUserId;
            var query = _usersPhraseRepository.UsersPhrases
                .Where(up => up.UserId == userId)
                .AsQueryable();
            var i = 0;
            gl.GlobalPhrases.ForEach(p =>
            {
                if (query.Any(up => p.Phrase == up.Phrase))
                    model.StateList.Add(true);
                else
                    model.StateList.Add(false);
                i++;
            });

            return View(model);
        }
        public ActionResult GlossaryData(decimal glossaryId, decimal langId, int index)
        {
            var glossary = _glossaryRepository.Get(g => g.Id == glossaryId);

            var phrase = glossary.GlobalPhrases
                .ElementAt(index);
            phrase.Audio = null;
            phrase.Glossaries = null;
            
            var nav = new { leftIndex = index - 1,
                            rightIndex = index + 1 > glossary.GlobalPhrases.Count - 1 ? -1 : index + 1 };

            var isAdded = _usersPhraseRepository.UsersPhrases
                .Where(us => us.UserId == WebSecurity.CurrentUserId)
                .Any(up => up.Phrase == phrase.Phrase);
            
            var id = phrase.Id;
            var translation = _globalTranslationRepository.Get(t => t.GlobalPhraseId == id && t.LanguageId == langId) ?? new GlobalTranslation();
            var examples = _globalExampleRepository.GlobalExamples
                .Where(e => e.GlobalPhraseId == id)
                .Where(e => e.GlobalTranslationId == translation.Id)
                .AsEnumerable()
                .Select(e =>
                        new GlobalExample
                        {
                            Id = e.Id,
                            GlobalPhraseId = e.GlobalPhraseId,
                            GlobalTranslationId = e.GlobalTranslationId,
                            Phrase = e.Phrase,
                            Translation = e.Translation
                        });
            var serializer = new JavaScriptSerializer { MaxJsonLength = Int32.MaxValue };
            var result = new ContentResult
            {
                Content = serializer.Serialize(new { examples, translation, phrase, nav, isAdded }),
                ContentType = "application/json"
            };
            return result;
        }

        public ActionResult AddPhraseToVocabulary(decimal id, decimal glossaryId, bool status = false) 
        {
            var phrase = _globalPhraseRepository.Get(p => p.Id == id);
            if (phrase == null)
            {
                return HttpNotFound();
            }

            var glossary = _glossaryRepository.Get(g => g.Id == glossaryId);
            if (glossary == null)
            {
                return HttpNotFound();
            }
            if (!glossary.GlobalPhrases.Any(p => p.Id == id))
            {
                return HttpNotFound();
            }

            _usersPhraseRepository.Add(new UsersPhrase(phrase)
            {
                GlossaryId = glossaryId,
                GlossaryName = glossary.Name,
                UserId = WebSecurity.CurrentUserId,
                LearningState = status ? 1 : 0
            });
            var userPhrase =_usersPhraseRepository.UsersPhrases
                .Where(up => up.UserId == WebSecurity.CurrentUserId)
                .First(up => up.Phrase == phrase.Phrase);

            var tr = _globalTranslationRepository.GlobalTranslations
                .Where(t => t.GlobalPhraseId == phrase.Id).ToList();
            foreach (var translation in tr)
            {
                _usersTranslationRepository.Add(new UsersTranslation(translation) {UserPhraseId = userPhrase.Id});
            }

           var usersTranslations = _usersTranslationRepository.UsersTranslations
                .Where(t => t.UserPhraseId == userPhrase.Id).ToList();
            for (var i = 0; i < tr.Count; i++)
            {
                var gtid = tr[i].Id;
                var utid = usersTranslations[i].Id;
                _globalExampleRepository.GlobalExamples
                    .Where(e => e.GlobalPhraseId == phrase.Id && e.GlobalTranslationId == gtid)
                    .ForEach(e => _usersExampleRepository.Add(
                        new UsersExample(e) {UserPhraseId = userPhrase.Id, UserTranslationId = utid}));
            }

            if (Request.IsAjaxRequest())
            {
                return Json(new {result = true}, JsonRequestBehavior.AllowGet);
            }

            return RedirectToAction("Glossary", new {id = glossary.Id, langId = glossary.LanguageId});
        }

        #region Helpers

        public ActionResult GetGlobalPhraseAudio(decimal id)
        {
            var phrase = _globalPhraseRepository.Get(p => p.Id == id);
            if (phrase.Audio == null)
            {
                return null;
            }
            return File(phrase.Audio, "audio/mpeg");
        }
        public ActionResult GetGlobalExampleAudio(decimal id)
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
