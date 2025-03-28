﻿using AppTaxi.Models;
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
        private const string Mensaje = "Mensaje";

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
            var validaciones = new (object Valor, string Nombre)[]
            {
                (modelo.Vehiculo, nameof(modelo.Vehiculo)),
                (modelo.Conductor, nameof(modelo.Conductor)),
                (modelo.Empresa, nameof(modelo.Empresa)),
                (modelo.Propietario, nameof(modelo.Propietario)),
                (modelo.Horario, nameof(modelo.Horario)),
                (modelo.Usuario, nameof(modelo.Usuario)),
                (modelo.Transaccion, nameof(modelo.Transaccion)),
                (modelo.Contador, nameof(modelo.Contador)),
                (modelo.Vehiculos, nameof(modelo.Vehiculos)),
                (modelo.Conductores, nameof(modelo.Conductores)),
                (modelo.Empresas, nameof(modelo.Empresas)),
                (modelo.Propietarios, nameof(modelo.Propietarios)),
                (modelo.Horarios, nameof(modelo.Horarios)),
                (modelo.Usuarios, nameof(modelo.Usuarios)),
                (modelo.Transacciones, nameof(modelo.Transacciones)),
                (modelo.Archivo_1, nameof(modelo.Archivo_1)),
                (modelo.Archivo_2, nameof(modelo.Archivo_2)),
                (modelo.Archivo_3, nameof(modelo.Archivo_3)),
                (modelo.Archivo_4, nameof(modelo.Archivo_4))
            };

            foreach (var validacion in validaciones)
            {
                if (validacion.Valor == null)
                    ModelState.Remove(validacion.Nombre);
            }
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
        private static Models.Login CreateLogin(Usuario usuario)
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
                TempData[Mensaje] = UsuarioNoAutenticado;
                return View();
            }




            return View(usuario);

        }

        public async Task<IActionResult> Perfil()
        {
            var usuario = GetUsuarioFromSession();
            if (usuario == null)
            {
                TempData[Mensaje] = UsuarioNoAutenticado;
                return View();
            }

            var login = CreateLogin(usuario);
            var conductores = await _conductor.Lista(login);
            var conductor = conductores.FirstOrDefault(c => c.Correo == usuario.Correo && c.Contrasena == usuario.Contrasena);

            

            var empresa = await _empresa.Obtener(conductor.IdEmpresa, login);

            ModeloVista modelo = new ModeloVista
            {
                Conductor = conductor,
                Empresa = empresa
            };

            return View(modelo);
        }


        public async Task<IActionResult> Horarios()
        {
            var usuario = GetUsuarioFromSession();
            if (usuario == null)
            {
                TempData[Mensaje] = UsuarioNoAutenticado;
                return View();
            }

            var login = CreateLogin(usuario);

            ModeloVista modelo = new ModeloVista();

            var conductores = await _conductor.Lista(login);
            var conductor = conductores.FirstOrDefault(c => c.Correo == usuario.Correo && c.Contrasena == usuario.Contrasena);

            if (conductor == null)
            {
                TempData[Mensaje] = "Conductor no encontrado.";
                // Podrías redirigir a una acción específica o mostrar una vista de error
                return RedirectToAction("Inicio");
            }
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
                TempData[Mensaje] = UsuarioNoAutenticado;
                return View();
            }

            var login = CreateLogin(usuario);

            ModeloVista modelo = new ModeloVista();

            var conductores = await _conductor.Lista(login);
            var conductor = conductores.FirstOrDefault(c => c.Correo == usuario.Correo && c.Contrasena == usuario.Contrasena);

            if (conductor == null)
            {
                TempData[Mensaje] = "Conductor no encontrado.";
                // Podrías redirigir a una acción específica o mostrar una vista de error
                return RedirectToAction("Inicio");
            }
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
            if (!ModelState.IsValid)
            {
                TempData[Mensaje] = "Error con el modelo";
                return RedirectToAction("Inicio");
            }
            var usuario = GetUsuarioFromSession();
            if (usuario == null)
            {
                TempData[Mensaje] = UsuarioNoAutenticado;
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
                TempData[Mensaje] = "Error con el modelo";
                return RedirectToAction("Inicio");
            }

            var usuario = GetUsuarioFromSession();
            if (usuario == null)
            {
                TempData[Mensaje] = UsuarioNoAutenticado;
                return View();
            }
            var login = CreateLogin(usuario);

            // Obtener datos necesarios
            var horariosTotales = await _horario.Lista(login);
            var conductoresTotales = await _conductor.Lista(login);

            var conductor = conductoresTotales.FirstOrDefault(c => c.Correo == usuario.Correo && c.Contrasena == usuario.Contrasena);

            if (conductor == null)
            {
                TempData[Mensaje] = "Conductor no encontrado";
                return RedirectToAction("Registro_Horario", "Conductor");
            }

            modelo.Horario.Fecha = DateTime.Now.Date;
            modelo.Horario.IdConductor = conductor.IdConductor;

            // Validación 1: Rango horario válido (00:00 a 23:59)
            if (modelo.Horario.HoraInicio >= modelo.Horario.HoraFin)
            {
                TempData[Mensaje] = "El horario de fin debe ser posterior al de inicio y ambos dentro del mismo día";
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

                TempData[Mensaje] = $"Ya tienes un horario asignado que se cruza: " +
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

                TempData[Mensaje] = $"El vehículo {vehiculoConflictivo.Placa} está siendo usado por: " +
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
                TempData[Mensaje] = "Horario guardado correctamente";
                return RedirectToAction("Registro_Horario", "Conductor");
            }
            else
            {
                TempData[Mensaje] = "Error al guardar el horario";
                return RedirectToAction("Crear_Horario", new { IdConductor = conductor.IdConductor });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Eliminar_Horario(int IdHorario)
        {

            if (!ModelState.IsValid)
            {
                TempData[Mensaje] = "Error con el modelo";
                return RedirectToAction("Inicio");
            }
            var usuario = GetUsuarioFromSession();
            if (usuario == null)
            {
                TempData[Mensaje] = UsuarioNoAutenticado;
                return View();
            }
            var login = CreateLogin(usuario);

            bool respuesta = await _horario.Eliminar(IdHorario, login);
            if (respuesta)
            {
                Transaccion t = Crear_Transaccion("Eliminar", "Horario");
                bool guardar = await _transaccion.Guardar(t, login);
                TempData[Mensaje] = "Horario eliminado correctamente";

            }
            else
            {
                TempData[Mensaje] = "Error al eliminar el horario";

            }
            return RedirectToAction("Registro_Horario", "Conductor");
        }
    }
}
