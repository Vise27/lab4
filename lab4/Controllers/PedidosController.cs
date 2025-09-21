using lab4.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace lab4.Controllers
{
    public class PedidosController : Controller
    {
        private static List<Pedido> pedidos = new List<Pedido>();
        private readonly string filePath = "pedidos.json";

        public IActionResult Index()
        {
            if (System.IO.File.Exists(filePath))
            {
                var data = System.IO.File.ReadAllText(filePath);
                pedidos = JsonSerializer.Deserialize<List<Pedido>>(data) ?? new List<Pedido>();
            }
            return View(pedidos);
        }

        
        public IActionResult Create()
        {
            return View();
        }

      
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Pedido pedido, string productosTexto)
        {
            pedido.Productos = string.IsNullOrEmpty(productosTexto)
                ? new List<string>()
                : productosTexto.Split(',').Select(p => p.Trim()).ToList();

            pedido.Id = pedidos.Count + 1;
            pedidos.Add(pedido);

            System.IO.File.WriteAllText(filePath, JsonSerializer.Serialize(pedidos));

            return RedirectToAction(nameof(Index));
        }
    }
}
