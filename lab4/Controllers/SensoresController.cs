using lab4.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace lab4.Controllers
{
    public class SensoresController : Controller
    {
        private static readonly object lockObj = new object();
        private static List<SensorData> lecturas = new List<SensorData>();
        private readonly string logDir = "Logs";
        private readonly string fileName = "lecturas.json";
        private readonly long maxFileSize = 5 * 1024;

        public SensoresController()
        {
            if (!Directory.Exists(logDir))
                Directory.CreateDirectory(logDir);
        }

        public IActionResult Index()
        {
            lecturas = LeerDatosActuales();
            return View(lecturas);
        }

        // Simula registrar una nueva lectura de sensor
        public IActionResult RegistrarLectura()
        {
            var nueva = new SensorData
            {
                Id = lecturas.Count + 1,
                Timestamp = DateTime.Now,
                Temperatura = new Random().Next(15, 35),
                Humedad = new Random().Next(30, 90)
            };

            lecturas.Add(nueva);
            GuardarConRotacion(nueva);

            return RedirectToAction(nameof(Index));
        }

        private void GuardarConRotacion(SensorData data)
        {
            lock (lockObj)
            {
                string path = Path.Combine(logDir, fileName);

                if (System.IO.File.Exists(path))
                {
                    var size = new FileInfo(path).Length;
                    if (size >= maxFileSize)
                    {
                        string newName = $"lecturas_{DateTime.Now:yyyyMMdd_HHmmss}.json";
                        System.IO.File.Move(path, Path.Combine(logDir, newName));
                    }
                }

                List<SensorData> datosPrevios = LeerDatosActuales();
                datosPrevios.Add(data);

                using (var sw = new StreamWriter(path, false))
                {
                    string json = JsonSerializer.Serialize(datosPrevios, new JsonSerializerOptions { WriteIndented = true });
                    sw.Write(json);
                }
            }
        }

        private List<SensorData> LeerDatosActuales()
        {
            string path = Path.Combine(logDir, fileName);
            if (!System.IO.File.Exists(path))
                return new List<SensorData>();

            using (var sr = new StreamReader(path))
            {
                string json = sr.ReadToEnd();
                return JsonSerializer.Deserialize<List<SensorData>>(json) ?? new List<SensorData>();
            }
        }

        // Para deserializar todos los archivos (histórico completo)
        public List<SensorData> LeerHistorico()
        {
            var lista = new List<SensorData>();

            foreach (var file in Directory.GetFiles(logDir, "*.json"))
            {
                string json = System.IO.File.ReadAllText(file);
                var datos = JsonSerializer.Deserialize<List<SensorData>>(json);
                if (datos != null) lista.AddRange(datos);
            }

            return lista.OrderBy(x => x.Timestamp).ToList();
        }

        public IActionResult Graficos()
        {
            var historico = LeerHistorico();
            return View(historico);
        }

    }
}
