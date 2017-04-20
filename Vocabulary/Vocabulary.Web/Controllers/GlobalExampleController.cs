using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Vocabulary.Domain.Abstract;

namespace Vocabulary.Web.Controllers
{
    public class GlobalExampleController : ApiController
    {
        private IGlobalExampleRepository _globalExampleRepository;
        public GlobalExampleController(IGlobalExampleRepository repo)
        {
            _globalExampleRepository = repo;
        }



    }
}
