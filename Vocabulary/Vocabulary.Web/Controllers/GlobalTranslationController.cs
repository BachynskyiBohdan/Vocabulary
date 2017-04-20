using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Vocabulary.Domain.Abstract;

namespace Vocabulary.Web.Controllers
{
    public class GlobalTranslationController : ApiController
    {
        private IGlobalTranslationRepository _globalTranslationRepository;
        public GlobalTranslationController(IGlobalTranslationRepository repo)
        {
            _globalTranslationRepository = repo;
        }


    }
}
