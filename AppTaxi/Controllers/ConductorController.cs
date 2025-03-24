using AppTaxi.Models;
using AppTaxi.Servicios;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace AppTaxi.Controllers
{
    public class ConductorController : Controller
    {

        // Servicios inyectados para manejar las operaciones de vehículos, horarios, propietarios, empresas y conductores.

        [System.Diagnostics.CodeAnalysis.SuppressMessage("SonarLint", "S1068:Unused private fields should be removed", Justification = "El campo se utiliza mediante inyección y llamadas a sus métodos.")] private readonly I_Vehiculo _vehiculo;
        private readonly I_Horario _horario;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("SonarLint", "S1068:Unused private fields should be removed", Justification = "El campo se utiliza mediante inyección y llamadas a sus métodos.")]
        private readonly I_Propietario _propietario;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("SonarLint", "S1068:Unused private fields should be removed", Justification = "El campo se utiliza mediante inyección y llamadas a sus métodos.")]
        private readonly I_Empresa _empresa;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("SonarLint", "S1068:Unused private fields should be removed", Justification = "El campo se utiliza mediante inyección y llamadas a sus métodos.")]
        private readonly I_Conductor _conductor;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("SonarLint", "S1068:Unused private fields should be removed", Justification = "El campo se utiliza mediante inyección y llamadas a sus métodos.")]
        private readonly I_Usuario _usuario;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("SonarLint", "S1068:Unused private fields should be removed", Justification = "El campo se utiliza mediante inyección y llamadas a sus métodos.")]
        private readonly I_Transaccion _transaccion;
        private const string UsuarioNoAutenticado = "Usuario no autenticado.";

        // Constructor que recibe las dependencias inyectadas.
        public ConductorController(I_Vehiculo vehiculo, I_Horario horario, I_Propietario propietario, I_Empresa empresa, I_Conductor conductor, I_Usuario usuario, I_Transaccion transaccion)
        {
            _vehiculo = vehiculo;
            _horario = horario;
            _propietario = propietario;
            _empresa = empresa;
            _conductor = conductor;
            _usuario = usuario;
            _transaccion = transaccion;
        }

        //------------ Métodos auxiliares ------------
        //Remover Validaciones del Modelo: 
        private void RemoverValidaciones(ModeloVista modelo)
        {
            if (modelo.Vehiculo == null)
                ModelState.Remove(nameof(modelo.Vehiculo));
            if (modelo.Conductor == null)
                ModelState.Remove(nameof(modelo.Conductor));
            if (modelo.Empresa == null)
                ModelState.Remove(nameof(modelo.Empresa));
            if (modelo.Propietario == null)
                ModelState.Remove(nameof(modelo.Propietario));
            if (modelo.Horario == null)
                ModelState.Remove(nameof(modelo.Horario));
            if (modelo.Usuario == null)
                ModelState.Remove(nameof(modelo.Usuario));
            if (modelo.Transaccion == null)
                ModelState.Remove(nameof(modelo.Transaccion));

            if (modelo.Contador == null)
                ModelState.Remove(nameof(modelo.Contador));
            if (modelo.Vehiculos == null)
                ModelState.Remove(nameof(modelo.Vehiculos));
            if (modelo.Conductores == null)
                ModelState.Remove(nameof(modelo.Conductores));
            if (modelo.Empresas == null)
                ModelState.Remove(nameof(modelo.Empresas));
            if (modelo.Propietarios == null)
                ModelState.Remove(nameof(modelo.Propietarios));
            if (modelo.Horarios == null)
                ModelState.Remove(nameof(modelo.Horarios));
            if (modelo.Usuarios == null)
                ModelState.Remove(nameof(modelo.Usuarios));
            if (modelo.Transacciones == null)
                ModelState.Remove(nameof(modelo.Transacciones));

            if (modelo.Archivo_1 == null)
                ModelState.Remove(nameof(modelo.Archivo_1));
            if (modelo.Archivo_2 == null)
                ModelState.Remove(nameof(modelo.Archivo_2));
            if (modelo.Archivo_3 == null)
                ModelState.Remove(nameof(modelo.Archivo_3));
            if (modelo.Archivo_4 == null)
                ModelState.Remove(nameof(modelo.Archivo_4));
        }
        // Obtiene el usuario actual desde la sesión.
        private Usuario GetUsuarioFromSession()
        {
            var usuarioJson = HttpContext.Session.GetString("Usuario");
            if (string.IsNullOrEmpty(usuarioJson))
            {
                return null;
            }
            return JsonConvert.DeserializeObject<Usuario>(usuarioJson);
        }

        // Crea un objeto Login a partir del usuario actual.
        private Models.Login CreateLogin(Usuario usuario)
        {
            return new Models.Login { Correo = usuario.Correo, Contrasena = usuario.Contrasena };
        }

        private Transaccion Crear_Transaccion(string accion, string modelo)
        {
            var usuario = GetUsuarioFromSession();
            if (usuario == null)
            {
                // Aquí puedes optar por lanzar una excepción, registrar el error o redirigir a la página de login.
                throw new InvalidOperationException("No hay un usuario autenticado en la sesión.");
            }

            Transaccion transaccion = new Transaccion
            {
                IdUsuario = usuario.IdUsuario,
                Modelo = modelo,
                Accion = accion,
                Fecha = DateTime.Now.Date,
                Hora = DateTime.Now.TimeOfDay
            };
            return transaccion;
        }

        public IActionResult Inicio()
        {
            var usuario = GetUsuarioFromSession();
            if (usuario == null)
            {
                TempData["Mensaje"] = UsuarioNoAutenticado;
                return View();
            }

            


            return View(usuario);

        }

        public async Task<IActionResult> Perfil()
        {
            var usuario = GetUsuarioFromSession();
            if (usuario == null)
            {
                TempData["Mensaje"] = UsuarioNoAutenticado;
                return View();
            }

            var login = CreateLogin(usuario);

            var conductores = await _conductor.Lista(login);
            var conductor = conductores.FirstOrDefault(c => c.Correo == usuario.Correo && c.Contrasena == usuario.Contrasena);
            var empresa = await _empresa.Obtener(conductor.IdEmpresa, login);
            ModeloVista modelo = new ModeloVista();
            modelo.Conductor = conductor;
            modelo.Empresa = empresa;
            return View(modelo);
        }

        public async Task<IActionResult> Horarios()
        {
            var usuario = GetUsuarioFromSession();
            if (usuario == null)
            {
                TempData["Mensaje"] = UsuarioNoAutenticado;
                return View();
            }

            var login = CreateLogin(usuario);

            ModeloVista modelo = new ModeloVista();

            var conductores = await _conductor.Lista(login);
            var conductor = conductores.FirstOrDefault(c => c.Correo == usuario.Correo && c.Contrasena == usuario.Contrasena);

            var vehiculos = await _vehiculo.Lista(login);
            var horarios = await _horario.Lista(login);
            modelo.Horarios = horarios.Where(h => h.IdConductor == conductor.IdConductor).ToList();
            modelo.Conductor = conductor;
            modelo.Vehiculos = vehiculos.Where(v => v.IdEmpresa == conductor.IdEmpresa).ToList();

            return View(modelo);
        }

        public async Task<IActionResult> Registro_Horario()
        {
            var usuario = GetUsuarioFromSession();
            if (usuario == null)
            {
                TempData["Mensaje"] = UsuarioNoAutenticado;
                return View();
            }

            var login = CreateLogin(usuario);

            ModeloVista modelo = new ModeloVista();

            var conductores = await _conductor.Lista(login);
            var conductor = conductores.FirstOrDefault(c => c.Correo == usuario.Correo && c.Contrasena == usuario.Contrasena);
            var empresa = await _empresa.Obtener(conductor.IdEmpresa, login);
            var vehiculos = await _vehiculo.Lista(login);
            var horarios = await _horario.Lista(login);

            DateTime fechaActual = DateTime.Now.Date;

            modelo.Horarios = horarios.Where(h => h.IdConductor == conductor.IdConductor && h.Fecha.Date == fechaActual).ToList();
            modelo.Vehiculos = vehiculos.Where(c => c.IdEmpresa == empresa.IdEmpresa).ToList();
            modelo.Conductor = conductor;
            modelo.Empresa = empresa;
            return View(modelo);

        }

        public async Task<IActionResult> Crear_Horario(int IdConductor)
        {
            var usuario = GetUsuarioFromSession();
            if (usuario == null)
            {
                TempData["Mensaje"] = UsuarioNoAutenticado;
                return View();
            }

            var login = CreateLogin(usuario);

            ModeloVista modelo = new ModeloVista();

            var conductor = await _conductor.Obtener(IdConductor, login);

            var empresa = await _empresa.Obtener(conductor.IdEmpresa, login);

            var vehiculos = await _vehiculo.Lista(login);

            modelo.Conductor = conductor;
            modelo.Empresa = empresa;
            modelo.Vehiculos = vehiculos.Where(v => v.IdEmpresa == empresa.IdEmpresa).ToList();
            return View(modelo);
        }

        [HttpPost]
        public async Task<IActionResult> Agregar_Horario(ModeloVista modelo)
        {
            RemoverValidaciones(modelo);
            if (!ModelState.IsValid)
            {
                TempData["Mensaje"] = "Error con el modelo";
                return RedirectToAction("Inicio");
            }

            var usuario = GetUsuarioFromSession();
            if (usuario == null)
            {
                TempData["Mensaje"] = UsuarioNoAutenticado;
                return View();
            }
            var login = CreateLogin(usuario);

            // Obtener datos necesarios
            var horariosTotales = await _horario.Lista(login);
            var conductoresTotales = await _conductor.Lista(login);
            var vehiculosTotales = await _vehiculo.Lista(login);

            var conductor = conductoresTotales.FirstOrDefault(c => c.Correo == usuario.Correo && c.Contrasena == usuario.Contrasena);

            if (conductor == null)
            {
                TempData["Mensaje"] = "Conductor no encontrado";
                return RedirectToAction("Registro_Horario", "Conductor");
            }

            modelo.Horario.Fecha = DateTime.Now.Date;
            modelo.Horario.IdConductor = conductor.IdConductor;

            // Validación 1: Rango horario válido (00:00 a 23:59)
            if (modelo.Horario.HoraInicio >= modelo.Horario.HoraFin)
            {
                TempData["Mensaje"] = "El horario de fin debe ser posterior al de inicio y ambos dentro del mismo día";
                return RedirectToAction("Crear_Horario", new { IdConductor = conductor.IdConductor });
            }

            // Validación 2: Conductor con horario solapado
            var horariosConductor = horariosTotales
                .Where(h => h.IdConductor == modelo.Horario.IdConductor &&
                           h.Fecha.Date == modelo.Horario.Fecha.Date)
                .ToList();

            var solapamientoConductor = horariosConductor
                .Any(h => (modelo.Horario.HoraInicio < h.HoraFin &&
                          modelo.Horario.HoraFin > h.HoraInicio));

            if (solapamientoConductor)
            {
                var horarioConflictivo = horariosConductor.FirstOrDefault(h =>
                    modelo.Horario.HoraInicio < h.HoraFin &&
                    modelo.Horario.HoraFin > h.HoraInicio);

                TempData["Mensaje"] = $"Ya tienes un horario asignado que se cruza: " +
                                     $"{horarioConflictivo.HoraInicio:hh\\:mm} - " +
                                     $"{horarioConflictivo.HoraFin:hh\\:mm}";
                return RedirectToAction("Crear_Horario", new { IdConductor = conductor.IdConductor });
            }

            // Validación 3: Vehículo en uso
            var horariosVehiculo = horariosTotales
                .Where(h => h.IdVehiculo == modelo.Horario.IdVehiculo &&
                           h.Fecha.Date == modelo.Horario.Fecha.Date)
                .ToList();

            var solapamientoVehiculo = horariosVehiculo
                .Any(h => (modelo.Horario.HoraInicio < h.HoraFin &&
                          modelo.Horario.HoraFin > h.HoraInicio));

            if (solapamientoVehiculo)
            {
                var horarioConflictivo = horariosVehiculo.FirstOrDefault(h =>
                    modelo.Horario.HoraInicio < h.HoraFin &&
                    modelo.Horario.HoraFin > h.HoraInicio);

                var conductorConflictivo = await _conductor.Obtener(horarioConflictivo.IdConductor, login);
                var vehiculoConflictivo = await _vehiculo.Obtener(horarioConflictivo.IdVehiculo, login);

                TempData["Mensaje"] = $"El vehículo {vehiculoConflictivo.Placa} está siendo usado por: " +
                                     $"{conductorConflictivo.Nombre} ({conductorConflictivo.Telefono}) " +
                                     $"de {horarioConflictivo.HoraInicio:hh\\:mm} a " +
                                     $"{horarioConflictivo.HoraFin:hh\\:mm}";
                return RedirectToAction("Crear_Horario", new { IdConductor = conductor.IdConductor });
            }

            // Si pasa las validaciones, guardar
            bool respuesta = await _horario.Guardar(modelo.Horario, login);

            if (respuesta)
            {
                Transaccion t = Crear_Transaccion("Crear", "Horario");
                bool guardar = await _transaccion.Guardar(t, login);
                TempData["Mensaje"] = "Horario guardado correctamente";
                return RedirectToAction("Registro_Horario", "Conductor");
            }
            else
            {
                TempData["Mensaje"] = "Error al guardar el horario";
                return RedirectToAction("Crear_Horario", new { IdConductor = conductor.IdConductor });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Eliminar_Horario(int IdHorario)
        {

            if (!ModelState.IsValid)
            {
                TempData["Mensaje"] = "Error con el modelo";
                return RedirectToAction("Inicio");
            }
            var usuario = GetUsuarioFromSession();
            if (usuario == null)
            {
                TempData["Mensaje"] = UsuarioNoAutenticado;
                return View();
            }
            var login = CreateLogin(usuario);

            bool respuesta = await _horario.Eliminar(IdHorario, login);
            if (respuesta)
            {
                Transaccion t = Crear_Transaccion("Eliminar", "Horario");
                bool guardar = await _transaccion.Guardar(t, login);
                TempData["Mensaje"] = "Horario eliminado correctamente";

            }
            else
            {
                TempData["Mensaje"] = "Error al eliminar el horario";

            }
            return RedirectToAction("Registro_Horario", "Conductor");
        }
    }
}
