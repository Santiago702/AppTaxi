using AppTaxi.Funciones;
using AppTaxi.Models;
using AppTaxi.Servicios;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualBasic;
using Newtonsoft.Json;
using NPOI.SS.Formula.Functions;

namespace AppTaxi.Controllers
{
    public class InicioController : Controller
    {
        private readonly I_Invitado _invitado;
        private readonly I_Usuario _usuario;
        private readonly I_Empresa _empresa;

        public InicioController(I_Invitado invitado, I_Usuario usuario, I_Empresa empresa) //Como en Windows Forms de Controlador
        {
            _invitado = invitado;
            _usuario = usuario;
            _empresa = empresa;
        }

        public IActionResult Index()
        {
            
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Consultar(Consulta consulta)
        {
            if (!ModelState.IsValid)
            {
                TempData["Mensaje"] = "Error con el modelo";
                return RedirectToAction("Inicio");
            }
            ViewBag.Mensaje = "";
            Invitado inv = new Invitado();
            if (string.IsNullOrEmpty(consulta.Placa) && consulta.Documento == 0)
            {
                ViewBag.Mensaje = "Se debe digitar los campos solicitados";
                return View("Index"); 
            }

            if (consulta.Placa != null && consulta.Placa.Length != 6)
            {
                ViewBag.Mensaje = "Placa no Admitida";
                return View("Index");
            }

            inv = await _invitado.Consulta(consulta);

            if (inv == null)
            {
                ViewBag.Mensaje = "No se encontraron datos para la consulta.";
                return View("Index"); 
            }
            return View(inv);
        }

        public IActionResult Login()
        {
            /*Encriptado enc = new Encriptado();
            string texto = "hola123";
            string encriptado = enc.EncriptarSimple(texto);
            string desencriptado = enc.DesencriptarSimple(encriptado);
            TempData["Mensaje"] = $"E {encriptado} y D {desencriptado}";*/
            return View();
        }

        public async Task<IActionResult> Autenticar(Login login)
        {
            if (!ModelState.IsValid)
            {
                TempData["Mensaje"] = "Error con el modelo";
                return RedirectToAction("Inicio");
            }

            // Validación de campos obligatorios
            if (string.IsNullOrEmpty(login.Correo) || string.IsNullOrEmpty(login.Contrasena))
            {
                ViewBag.Mensaje = "Se debe digitar los campos solicitados";
                return View("Login");
            }

            // Validación de caracteres permitidos
            if (!ValidacionDato.ValidarTexto(login.Correo) || !ValidacionDato.ValidarTexto(login.Contrasena))
            {
                ViewBag.Mensaje = "Error, intenta ingresar caracteres no permitidos";
                return View("Login");
            }

            // Encriptar la contraseña
            login.Contrasena = Encriptado.GetSHA256(login.Contrasena);

            // Obtener usuarios
            List<Usuario> lista = await _usuario.Lista(login);
            if (lista == null || !lista.Any())
            {
                ViewBag.Mensaje = "Usuario o Contraseña incorrecta";
                return View("Login");
            }

            Usuario usuario = lista.FirstOrDefault(item => item.Correo == login.Correo && item.Contrasena == login.Contrasena);
            if (usuario == null)
            {
                ViewBag.Mensaje = "Usuario o Contraseña incorrecta";
                return View("Login");
            }

            if (!usuario.Estado)
            {
                ViewBag.Mensaje = "¡Error! Usuario Deshabilitado";
                return View("Login");
            }

            // Según el rol, validar existencia de empresa u otras redirecciones
            switch (usuario.IdRol)
            {
                case 1:
                    var empresas = await _empresa.Lista(login);
                    var empresa = empresas.FirstOrDefault(e => e.IdUsuario == usuario.IdUsuario);
                    if (empresa == null)
                    {
                        ViewBag.Mensaje = "¡Error! No tienes empresa Asignada";
                        return View("Login");
                    }
                    ViewBag.Mensaje = $"Bienvenido {usuario.Nombre}";
                    HttpContext.Session.SetString("Usuario", JsonConvert.SerializeObject(usuario));
                    return RedirectToAction("Inicio", "Empresa");

                case 2:
                    ViewBag.Mensaje = $"Bienvenido {usuario.Nombre}";
                    HttpContext.Session.SetString("Usuario", JsonConvert.SerializeObject(usuario));
                    return RedirectToAction("Inicio", "Secretaria");

                case 1006:
                    ViewBag.Mensaje = $"Bienvenido {usuario.Nombre}";
                    HttpContext.Session.SetString("Usuario", JsonConvert.SerializeObject(usuario));
                    return RedirectToAction("Inicio", "Conductor");

                default:
                    return View("Login");
            }
        }


        public IActionResult CerrarSesion()
        {
            // Limpiar todos los datos de la sesión
            HttpContext.Session.Clear();

            // Opcional: Redirigir a la página de inicio o login
            return RedirectToAction("Login");
        }




    }
}
