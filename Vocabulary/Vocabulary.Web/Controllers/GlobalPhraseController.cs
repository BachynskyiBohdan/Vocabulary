using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Vocabulary.Domain.Abstract;
using Vocabulary.Domain.Entities;

namespace Vocabulary.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class GlobalPhraseController : ApiController
    {
        private readonly IGlobalPhraseRepository _globalPhraseRepository;
        public GlobalPhraseController(IGlobalPhraseRepository repo)
        {
            _globalPhraseRepository = repo;
        }

        public GlobalPhrase GetPhraseById(decimal id)
        {
            return _globalPhraseRepository.GetById(id);
        }
        public List<GlobalPhrase> GetAll()
        {
            return _globalPhraseRepository.GetAll().ToList();
        }

        [HttpPost]
        public HttpResponseMessage AddNewPhrase(GlobalPhrase phrase)
        {
            if (ModelState.IsValid)
            {
                _globalPhraseRepository.Add(phrase);
                return new HttpResponseMessage(HttpStatusCode.Accepted);
            }
            return new HttpResponseMessage(HttpStatusCode.BadRequest);
        }

        [HttpPut]
        public HttpResponseMessage UpdatePhrase(GlobalPhrase phrase)
        {
            if (ModelState.IsValid)
            {
                _globalPhraseRepository.Update(phrase);
                return new HttpResponseMessage(HttpStatusCode.Accepted);
            }
            return new HttpResponseMessage(HttpStatusCode.BadRequest);
        }

        [HttpDelete]
        public HttpResponseMessage RemovePhrase(decimal id)
        {
            
            if (ModelState.IsValid)
            {
                var phrase = _globalPhraseRepository.GlobalPhrases.FirstOrDefault(p => p.Id == id);
                if (phrase == null)
                    return new HttpResponseMessage(HttpStatusCode.BadRequest);
                _globalPhraseRepository.Delete(phrase);
                return new HttpResponseMessage(HttpStatusCode.Accepted);
            }
            return new HttpResponseMessage(HttpStatusCode.BadRequest);
        }
    }
}
