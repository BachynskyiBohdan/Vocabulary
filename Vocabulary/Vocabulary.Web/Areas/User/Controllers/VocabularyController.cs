using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using Vocabulary.Domain.Abstract;
using Vocabulary.Domain.Entities;
using Vocabulary.Web.App_LocalResources;
using Vocabulary.Web.Areas.User.Models;
using Vocabulary.Web.Controllers;
using WebGrease.Css.Extensions;
using WebMatrix.WebData;

namespace Vocabulary.Web.Areas.User.Controllers
{
    [Authorize]
    public class VocabularyController : BaseController
    {
        #region Initialize

        private readonly IUsersExampleRepository _usersExampleRepository;
        private readonly IUsersPhraseRepository _usersPhraseRepository;
        private readonly IUsersTranslationRepository _usersTranslationRepository;
        private readonly IGlossaryRepository _glossaryRepository;
        private readonly IGlobalExampleRepository _globalExampleRepository;

        public VocabularyController(IUsersExampleRepository usersExampleRepository,IUsersPhraseRepository usersPhraseRepository,
            IUsersTranslationRepository usersTranslationRepository, ILanguageRepository languageRepository,
            IGlossaryRepository glossaryRepository, IGlobalExampleRepository globalExampleRepository)
            : base(languageRepository)
        {
            _usersExampleRepository = usersExampleRepository;
            _usersPhraseRepository = usersPhraseRepository;
            _usersTranslationRepository = usersTranslationRepository;
            _glossaryRepository = glossaryRepository;
            _globalExampleRepository = globalExampleRepository;
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

        public ActionResult VocabularyData(decimal phraseLangId = 1, decimal transLangId = 3, string phraseType = "All", 
            int learnState = -1, string search = "", string glossary = "All", int page = 1, int count = 300)
        {
            var model = ParseRequestParametersForVocabulary(phraseLangId, transLangId, phraseType, 
                learnState, search, glossary, page, count);

            if (Request.IsAjaxRequest())
            {
                var serializer = new JavaScriptSerializer {MaxJsonLength = Int32.MaxValue};
                var result = new ContentResult
                {
                    Content = serializer.Serialize(new { data = CreateHtmlDataElements(model.Phrases, model.Translations, page, count) }),
                    ContentType = "application/json"
                };
                return result;
            }
            return PartialView(model);
        }
        private List<string> CreateHtmlDataElements(List<UsersPhrase> p, List<UsersTranslation> t, int page, int count)
        {
            var l = new List<string>();
            var begin = (page - 1)*count + 1;
            for (var i = 0; i < p.Count; i++)
            {
                var str = "";
                var phrase = p[i];
                var translation = t[i];
                str += "<div class='phrase-container'><div class='add-button' data-phrase-id='" + phrase.Id + "'><div class='not-added'></div></div>" +
                    "<div class='audio'><img src='/Content/soundIcon.png' style='width: 25px; height: 25px;' onclick='playAudio(\"audio-" + phrase.Id + "\")" +
                    "' title='" + GlobalRes.PlayAudio + "' alt='" + GlobalRes.PlayAudio + "' />" +
                    "<audio id='audio-" + phrase.Id + "'><source src='/User/Vocabulary/GetUsersPhraseAudio/" + phrase.Id + "'/></audio></div>" +
                    "<div class='content-container' data-phrase-id='" + phrase.Id + "' data-lang-id='" + phrase.LanguageId + "' data-index='" + (begin + i) + "'" +
                    " data-trans-lang-id='" + translation.LanguageId + "'>" + "<div class='phrase'>" + phrase.Phrase + "</div>";
                if (phrase.Transcription != null)
                    str += "<div class='transcription'>" + phrase.Transcription + "</div>";
                str += "—<div class='translation'>" + translation.TranslationPhrase + "</div></div><div class='information'>";
                if (phrase.GlossaryId != null)
                    str += "<div class='glossary-info' data-glossary-id='" + phrase.GlossaryId + "'><a href='javascript:void(0)' title='" + phrase.GlossaryName + "'>"
                        + phrase.GlossaryName + "</a> </div>";

                str += "<div class='remove-button' data-phrase-id='" + phrase.Id + "' title='" + GlobalRes.RemovePhraseFromVocabulary + "'></div>" +
                    "<div class='learning-state' title='" + GlobalRes.LearningState + ": " + (phrase.LearningState * 100) + "%'>";
                switch ((int)(phrase.LearningState * 100))
                {
                    case 0:
                        str += "<div class='unknown'></div>";
                        break;
                    case 25:
                        str += "<div class='first'></div>";
                        break;
                    case 50:
                        str += "<div class='second'></div>";
                        break;
                    case 75:
                        str += "<div class='third'></div>";
                        break;
                    case 100:
                        str += "<div class='known'></div>";
                        break;
                }
                str += "</div>";
                if (phrase.Frequency != null)
                    str += "<div class='frequency' title='" + GlobalRes.WordRank + "'>" + phrase.Frequency + "</div>";
                str += "</div></div>";
                l.Add(str);
            }
            return l;
        }
        private VocabularyListViewModel ParseRequestParametersForVocabulary(decimal phraseLangId, decimal transLangId, 
            string phraseType, int learnState, string search, string glossary, int page, int count)
        {
            var model = new VocabularyListViewModel();
            decimal userId = WebSecurity.CurrentUserId;
            PhraseType type;
            Enum.TryParse(phraseType, out type);

            var query = _usersPhraseRepository.UsersPhrases
                .Where(p => p.UserId == userId)
                .Where(p => p.LanguageId == phraseLangId);

            // select elements for selected glossary 
            if (glossary != "All")
            {
                var id = decimal.Parse(glossary);
                query = query.Where(p => p.GlossaryId == id);
            }
            // select elements for selected phrase type
            if (type != PhraseType.All)
            {
                query = query.Where(p => p.PhraseType == type);
            }
            // select elements for selected learning state
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
            // select elements for search query
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(p => p.Phrase.StartsWith(search));
            }

            model.Phrases = query.AsEnumerable()
                .OrderBy(p => p.Id)
                .Skip((page - 1) * count)
                .Take(count)
                .Select(p =>
                {
                    p.Audio = null;
                    return p;
                })
                .ToList();

            // select translations for all phrases or default object if no translation found
            model.Phrases.ForEach(
                p => model.Translations.Add(_usersTranslationRepository.UsersTranslations
                    .Where(t => t.UserPhraseId == p.Id)
                    .Where(t => t.LanguageId == transLangId)
                    .FirstOrDefault(t => t.LanguageId == 3) ?? new UsersTranslation()));
            
            // select languages
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

        public ActionResult VocabularyElementData(decimal id, decimal transLangId = 3)
        {
            if (!Request.IsAjaxRequest())
            {
                return RedirectToAction("Vocabulary");
            }
            
            var phrase =  _usersPhraseRepository.UsersPhrases
                .Where(up => up.UserId == WebSecurity.CurrentUserId)
                .FirstOrDefault(up => up.Id == id);
            if (phrase == null)
            {
                return HttpNotFound();
            }
            var audio = "data:audio/mpeg; base64, " + Convert.ToBase64String(phrase.Audio);
            phrase.Audio = null;

            var translation = _usersTranslationRepository.UsersTranslations
                .Where(t => t.UserPhraseId == phrase.Id)
                .FirstOrDefault(t => t.LanguageId == transLangId) ?? new UsersTranslation();

            var serializer = new JavaScriptSerializer { MaxJsonLength = Int32.MaxValue };
            var result = new ContentResult
            {
                Content = serializer.Serialize(new { phrase, translation, audio }),
                ContentType = "application/json"
            };
            return result;
        }

        public ActionResult VocabularyExamplesData(decimal id, decimal? glId = null, decimal transLangId = 3)
        {
            if (!Request.IsAjaxRequest())
            {
                return RedirectToAction("Vocabulary");
            }

            var examples = _usersExampleRepository.UsersExamples
                .Where(e => e.PhraseId == id)
                .AsEnumerable()
                .Select(e => { e.Audio = null; return e as BaseExample; })
                .ToList();
            if (glId != null)
                examples.AddRange(_globalExampleRepository
                    .GetAll(e => e.PhraseId == glId && e.TranslationLanguageId == transLangId));

            var serializer = new JavaScriptSerializer { MaxJsonLength = Int32.MaxValue };
            var result = new ContentResult
            {
                Content = serializer.Serialize(new { examples = CreateHtmlExamplesElements(examples)}),
                ContentType = "application/json"
            };
            return result;
        }
        private List<string> CreateHtmlExamplesElements(List<BaseExample> ex)
        {
            var l = new List<string>();
            for (var i = 0; i < ex.Count; i++)
            {
                var str = "<tr>" + "<td style='text-align: left'>" + ex[i].Phrase + "</td>" +
                    "<td style='text-align: left'>" + ex[i].Translation + "</td>" +
                    "<td><img src='/Content/soundIcon.png' style='width: 25px; height: 25px;' onclick='playAudio(" + ex[i].Id + ")'" +
                    " title='" + GlobalRes.PlayAudio + "' alt='" + GlobalRes.PlayAudio + "' />" +
                    " <audio id='" + ex[i].Id;
                if (ex[i].IsUsersExample)
                {
                    str += "'> <source src='/User/GetUsersExampleAudio?id=" + ex[i].Id + "'/></audio></td>";
                }
                else
                {
                    str += "'> <source src='/User/GetGlobalExampleAudio?id=" + ex[i].Id + "'/></audio></td>";
                }
                l.Add(str);
            }
            return l;
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
                    .Where(e => e.PhraseId == phrase.Id)
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
