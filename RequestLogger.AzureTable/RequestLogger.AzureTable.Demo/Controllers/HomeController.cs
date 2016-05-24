using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Threading.Tasks;

namespace RequestLogger.AzureTable.Demo.Controllers
{
    public class HomeController : Controller
    {
        public async Task<IActionResult> Index()
        {
            var hc = new System.Net.Http.HttpClient(new iflight.RequestLogger.AzureTable.LoggingHandler(new HttpClientHandler()));
            var t = await hc.GetStringAsync("https://www.example.com");

            return View();
        }

        public IActionResult Demo()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }
    }
}
