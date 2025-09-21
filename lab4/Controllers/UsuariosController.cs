using lab4.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace lab4.Controllers
{
    public class UsuariosController : Controller
    {
        private static readonly object lockObj = new object();
        private static List<Usuario> usuarios = new List<Usuario>();
        private readonly string filePath = "usuarios.json";

        public IActionResult Index()
        {
            usuarios = LeerUsuarios();
            return View(usuarios);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Usuario usuario)
        {
            lock (lockObj)
            {
                usuarios = LeerUsuarios();
                usuario.Id = usuarios.Count + 1;
                usuarios.Add(usuario);
                GuardarUsuarios(usuarios);
            }
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Edit(int id)
        {
            var usuario = LeerUsuarios().FirstOrDefault(u => u.Id == id);
            if (usuario == null) return NotFound();
            return View(usuario);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Usuario usuario)
        {
            lock (lockObj)
            {
                usuarios = LeerUsuarios();
                var existente = usuarios.FirstOrDefault(u => u.Id == usuario.Id);
                if (existente != null)
                {
                    existente.Nombre = usuario.Nombre;
                    existente.Rol = usuario.Rol;
                    GuardarUsuarios(usuarios);
                }
            }
            return RedirectToAction(nameof(Index));
        }

     

        private List<Usuario> LeerUsuarios()
        {
            lock (lockObj)
            {
                if (!System.IO.File.Exists(filePath))
                    return new List<Usuario>();

                using (var sr = new StreamReader(filePath))
                {
                    string json = sr.ReadToEnd();
                    return JsonSerializer.Deserialize<List<Usuario>>(json) ?? new List<Usuario>();
                }
            }
        }

        private void GuardarUsuarios(List<Usuario> usuarios)
        {
            lock (lockObj)
            {
                using (var sw = new StreamWriter(filePath, false))
                {
                    string json = JsonSerializer.Serialize(usuarios);
                    sw.Write(json);
                }
            }
        }
    }
}
