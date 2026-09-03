using CentroDiurnoAATEGRE.Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CentroDiurnoAATEGRE.Web.ViewComponents
{
    // Se invoca desde el footer del _Layout (una sola línea:
    // @await Component.InvokeAsync("FooterInfo")) para que TODAS las
    // páginas muestren los enlaces de Facebook / Instagram reales,
    // sin depender de que cada controller llene ViewBag.Informacion.
    public class FooterInfoViewComponent : ViewComponent
    {
        private readonly IInformacionInstitucionalService _infoService;

        public FooterInfoViewComponent(IInformacionInstitucionalService infoService)
        {
            _infoService = infoService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var info = await _infoService.ObtenerPrimeraAsync();
            return View(info);
        }
    }
}
