using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace WebApiAutores.Utilities
{
    public class SwaggerAgrupaVersion : IControllerModelConvention
    {
        public void Apply(ControllerModel controller)
        {
            var namespaceController = controller.ControllerType.Namespace; //Controllers.v1
            var version = namespaceController.Split(".").Last().ToLower();
            controller.ApiExplorer.GroupName = version;
        }
    }
}
