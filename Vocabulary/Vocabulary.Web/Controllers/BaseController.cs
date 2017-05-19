using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using Vocabulary.Domain.Abstract;
using Vocabulary.Domain.Entities;

namespace Vocabulary.Web.Controllers
{
    public class BaseController : Controller
    {
        public string CurrentLangCode { get; protected set; }
        public Language CurrentLang { get; protected set; }
        protected readonly ILanguageRepository _languageRepository;

        public BaseController(ILanguageRepository l)
        {
            _languageRepository = l;
        }

        protected override void Initialize(System.Web.Routing.RequestContext requestContext)
        {
            if (requestContext.RouteData.Values["lang"] != null && requestContext.RouteData.Values["lang"] as string != "null")
            {
                CurrentLangCode = requestContext.RouteData.Values["lang"] as string;
                CurrentLang = _languageRepository.Languages.FirstOrDefault(p => p.Code == CurrentLangCode);

                var ci = new CultureInfo(CurrentLangCode);
                Thread.CurrentThread.CurrentUICulture = ci;
                Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture(ci.Name);
            }
            base.Initialize(requestContext);
        }

    }
}
