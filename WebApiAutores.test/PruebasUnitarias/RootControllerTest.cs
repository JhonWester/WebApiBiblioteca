using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using WebApiAutores.Controllers.V1;
using WebApiAutores.test.Mocks;

namespace WebApiAutores.test.PruebasUnitarias
{
    [TestClass]
    public class RootControllerTest
    {
        [TestMethod]
        public async Task SiUsuarioEsAdmin_Obtenemos4Links()
        {
            //Preparation
            var authorizationService = new AuthorizationServicesSuccesMock(); //Mock
            authorizationService.Resultado = AuthorizationResult.Success();

            var rootController = new RouteController(authorizationService);
            rootController.Url = new UrlHelperMock();

            //Ejecution
            var result = await rootController.Get();

            //Verification
            Assert.AreEqual(4, result.Value.Count());
        }

        [TestMethod]
        public async Task SiUsuarioNoEsAdmin_Obtenemos2Links()
        {
            //Preparation
            var authorizationService = new AuthorizationServicesSuccesMock(); //Mock
            authorizationService.Resultado = AuthorizationResult.Failed();

            var rootController = new RouteController(authorizationService);
            rootController.Url = new UrlHelperMock();

            //Ejecution
            var result = await rootController.Get();

            //Verification
            Assert.AreEqual(2, result.Value.Count());
        }

        [TestMethod]
        public async Task SiUsuarioNoEsAdmin_Obtenemos2Links_UsandoMoq()
        {
            //Preparation
            var MockAuthorizationService = new Mock<IAuthorizationService>();
            MockAuthorizationService.Setup(x => x.AuthorizeAsync(
                It.IsAny<ClaimsPrincipal>(),
                It.IsAny<object>(),
                It.IsAny<IEnumerable<IAuthorizationRequirement>>()
                )).Returns(Task.FromResult(AuthorizationResult.Failed()));

            MockAuthorizationService.Setup(x => x.AuthorizeAsync(
                It.IsAny<ClaimsPrincipal>(),
                It.IsAny<object>(),
                It.IsAny<string>()
                )).Returns(Task.FromResult(AuthorizationResult.Failed()));


            var MockUrlHelper = new Mock<IUrlHelper>();
            MockUrlHelper.Setup(x => x.Link(It.IsAny<string>(), It.IsAny<object>()))
                .Returns(string.Empty);

            var rootController = new RouteController(MockAuthorizationService.Object);
            rootController.Url = MockUrlHelper.Object;

            //Ejecution
            var result = await rootController.Get();

            //Verification
            Assert.AreEqual(2, result.Value.Count());
        }
    }
}
