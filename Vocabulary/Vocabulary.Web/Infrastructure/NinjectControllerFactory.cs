using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Providers.Entities;
using System.Web.Routing;
using Ninject;
using Vocabulary.Domain.Abstract;
using Vocabulary.Domain.Concrete;

namespace Vocabulary.Web.Infrastructure
{
    public class NinjectControllerFactory : DefaultControllerFactory
    {
        private IKernel _nKernel;
        public NinjectControllerFactory() : base()
        {
            _nKernel = new StandardKernel();
            ApplyBindings();
        }

        private void ApplyBindings()
        {
            _nKernel.Bind<IGlobalPhraseRepository>().To<GlobalPhraseRepository>();
            _nKernel.Bind<IGlobalExampleRepository>().To<GlobalExampleRepository>();
            _nKernel.Bind<IGlobalTranslationRepository>().To<GlobalTranslationRepository>();
            _nKernel.Bind<IGlossaryRepository>().To<GlossaryRepository>();
            _nKernel.Bind<ILanguageRepository>().To<LanguageRepository>();
            _nKernel.Bind<IUserRepository>().To<UserRepository>();
            _nKernel.Bind<IUsersPhraseRepository>().To<UsersPhraseRepository>();
            _nKernel.Bind<IUsersExampleRepository>().To<UsersExampleRepository>();
            _nKernel.Bind<IUsersTranslationRepository>().To<UsersTranslationRepository>();

        }

        protected override IController GetControllerInstance(RequestContext requestContext, Type controllerType)
        {
            return controllerType == null
                ? null
                : (IController)_nKernel.Get(controllerType);
        }
    }
}