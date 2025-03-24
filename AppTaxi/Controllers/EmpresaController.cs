using AppTaxi.Funciones;
using AppTaxi.Models;
using AppTaxi.Servicios;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;
using Newtonsoft.Json;
using NPOI.OpenXmlFormats.Spreadsheet;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace AppTaxi.Controllers
{
    public class EmpresaController : Controller
    {
        // Servicios inyectados para manejar las operaciones de vehículos, horarios, propietarios, empresas y conductores.
        [System.Diagnostics.CodeAnalysis.SuppressMessage("SonarLint", "S1068:Unused private fields should be removed", Justification = "El campo se utiliza mediante inyección y llamadas a sus métodos.")]
        private readonly I_Vehiculo _vehiculo;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("SonarLint", "S1068:Unused private fields should be removed", Justification = "El campo se utiliza mediante inyección y llamadas a sus métodos.")]
        private readonly I_Horario _horario;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("SonarLint", "S1068:Unused private fields should be removed", Justification = "El campo se utiliza mediante inyección y llamadas a sus métodos.")]
        private readonly I_Propietario _propietario;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("SonarLint", "S1068:Unused private fields should be removed", Justification = "El campo se utiliza mediante inyección y llamadas a sus métodos.")]
        private readonly I_Empresa _empresa;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("SonarLint", "S1068:Unused private fields should be removed", Justification = "El campo se utiliza mediante inyección y llamadas a sus métodos.")]
        private readonly I_Conductor _conductor;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("SonarLint", "S1068:Unused private fields should be removed", Justification = "El campo se utiliza mediante inyección y llamadas a sus métodos.")]
        private readonly I_Transaccion _transaccion;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("SonarLint", "S1068:Unused private fields should be removed", Justification = "El campo se utiliza mediante inyección y llamadas a sus métodos.")]
        private readonly I_Usuario _usuario;
        
        // Constructor que recibe las dependencias inyectadas.
        public EmpresaController(I_Vehiculo vehiculo, I_Horario horario, I_Propietario propietario, I_Empresa empresa, I_Conductor conductor, I_Transaccion transaccion, I_Usuario usuario)
        {
            _vehiculo = vehiculo;
            _horario = horario;
            _propietario = propietario;
            _empresa = empresa;
            _conductor = conductor;
            _transaccion = transaccion;
            _usuario = usuario;
        }


        // En tu servicio de usuarios
        
        private async Task<int> Cupos()
        {
            var usuario = GetUsuarioFromSession();
            if (usuario == null)
            {
                Console.WriteLine("No Hay Usuario Registrado");
            }

            var login = CreateLogin(usuario);

            var empresas = await _empresa.Lista(login);
            var empresa = empresas.FirstOrDefault(e => e.IdUsuario == usuario.IdUsuario);

            var vehiculos = await _vehiculo.Lista(login);
            int Contador = vehiculos.Where(v => v.IdEmpresa == empresa.IdEmpresa && v.Estado).Count();
            return Contador;
        }
        //------------ Métodos auxiliares ------------

        //Encripta Todo
        public async void Contrasenas(Models.Login login)
        {
            // Obtener la lista completa de usuarios
            List<Usuario> listaUsuarios = await _usuario.Lista(login);

            if (listaUsuarios == null || !listaUsuarios.Any())
            {
                //return Content("No se encontraron usuarios para actualizar.");
            }

            int actualizados = 0;
            foreach (var usuario in listaUsuarios)
            {
                if (usuario.Correo != login.Correo)
                {
                    // Solo encriptar si la contraseña no está en formato hash
                    if (usuario.Contrasena.Length != 64)
                    {
                        string contrasena = Encriptado.GetSHA256(usuario.Contrasena);
                        usuario.Contrasena = contrasena;

                        // Actualizamos el usuario en la base de datos mediante el método Editar de la API
                        bool resultado = await _usuario.Editar(usuario, login);

                        if (resultado)
                        {
                            actualizados++;
                        }

                    }
                }
            }

            //return Content($"Proceso completado. Se han actualizado {actualizados} contraseñas.");
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
                Console.WriteLine("No Hay Usuario Registrado");
            }

            var login = CreateLogin(usuario);

            Transaccion transaccion = new Transaccion();

            transaccion.IdUsuario = usuario.IdUsuario;
            transaccion.Modelo = modelo;
            transaccion.Accion = accion;
            transaccion.Fecha = DateTime.Now.Date;
            transaccion.Hora = DateTime.Now.TimeOfDay;
            return transaccion;

        }



        //------------ Acciones principales ------------

        // Muestra la página de inicio con los datos de la empresa, vehículos, horarios y conductores asociados.
        public async Task<IActionResult> Inicio()
        {
            var usuario = GetUsuarioFromSession();
            if (usuario == null)
            {
                ViewBag.Mensaje = "Usuario no autenticado.";
                return View();
            }

            var login = CreateLogin(usuario);

            // Ejecutar las llamadas a la API en paralelo
            var empresasTask = _empresa.Lista(login);
            var vehiculosTask = _vehiculo.Lista(login);
            var conductoresTask = _conductor.Lista(login);
            var propietariosTask = _propietario.Lista(login);
            var horariosTask = _horario.Lista(login);

            await Task.WhenAll(empresasTask, vehiculosTask, conductoresTask, propietariosTask, horariosTask);

            var empresasTotales = empresasTask.Result;
            var vehiculosTotales = vehiculosTask.Result;
            var conductoresTotales = conductoresTask.Result;
            var propietariosTotales = propietariosTask.Result;
            var horariosTotales = horariosTask.Result;

            // Obtener la empresa actual
            var empresaActual = empresasTotales.FirstOrDefault(e => e.IdUsuario == usuario.IdUsuario);
            if (empresaActual == null)
            {
                ViewBag.Mensaje = "Empresa no encontrada.";
                return View();
            }
            int idEmpresa = empresaActual.IdEmpresa;

            // Crear diccionarios para búsquedas rápidas
            var vehiculosDict = vehiculosTotales.ToDictionary(v => v.IdVehiculo);
            // Filtramos conductores de la empresa y con estado activo directamente
            var conductoresDict = conductoresTotales
                                  .Where(c => c.IdEmpresa == idEmpresa && c.Estado)
                                  .ToDictionary(c => c.IdConductor);
            var propietariosDict = propietariosTotales.ToDictionary(p => p.IdPropietario);

            // Construir la lista de datos de forma concisa
            var datosIniciales = horariosTotales
                .Where(h => conductoresDict.ContainsKey(h.IdConductor))
                .Select((h, index) =>
                {
                    var c = conductoresDict[h.IdConductor];
                    if (vehiculosDict.TryGetValue(h.IdVehiculo, out var v) &&
                        propietariosDict.TryGetValue(v.IdPropietario, out var p))
                    {
                        return new DatosEmpresa
                        {
                            IdDato = index + 1,
                            Foto = c.Foto,
                            IdVehiculo = h.IdVehiculo,
                            Placa = v.Placa,
                            NombrePropietario = p.Nombre,
                            Conductor = c.Nombre,
                            Fecha = h.Fecha,
                            HoraInicio = h.HoraInicio,
                            HoraFin = h.HoraFin
                        };
                    }
                    return null;
                })
                .Where(d => d != null)
                .ToList();

            // Evitar múltiples llamadas a Cupos() almacenando su resultado en una variable
            int cuposDisponibles = empresaActual.Cupos - await Cupos();
            ViewBag.Cupos = cuposDisponibles;
            ViewBag.Mensaje = $"Bienvenid@ {usuario.Nombre}";

            return View(datosIniciales);
        }



        // Muestra los detalles de un registro específico (vehículo, conductor, horario, etc.).
        public async Task<IActionResult> Detalle(int IdDato)
        {
            var usuario = GetUsuarioFromSession();
            if (usuario == null)
            {
                ViewBag.Mensaje = "Usuario no autenticado.";
                return View();
            }

            var login = CreateLogin(usuario);

            // Ejecutar todas las llamadas API en paralelo para reducir el tiempo de espera
            var empresasTask = _empresa.Lista(login);
            var vehiculosTask = _vehiculo.Lista(login);
            var conductoresTask = _conductor.Lista(login);
            var propietariosTask = _propietario.Lista(login);
            var horariosTask = _horario.Lista(login);

            await Task.WhenAll(empresasTask, vehiculosTask, conductoresTask, propietariosTask, horariosTask);

            var empresasTotales = await empresasTask;
            var vehiculosTotales = await vehiculosTask;
            var conductoresTotales = await conductoresTask;
            var propietariosTotales = await propietariosTask;
            var horariosTotales = await horariosTask;

            int IdEmpresa = empresasTotales
                .Where(e => e.IdUsuario == usuario.IdUsuario)
                .Select(e => e.IdEmpresa)
                .FirstOrDefault();

            // Convertimos las listas en diccionarios para acceso rápido
            var vehiculosDict = vehiculosTotales.ToDictionary(v => v.IdVehiculo);
            var conductoresDict = conductoresTotales.ToDictionary(c => c.IdConductor);
            var propietariosDict = propietariosTotales.ToDictionary(p => p.IdPropietario);

            List<DatosEmpresa> DatosIniciales = new List<DatosEmpresa>();
            int i = 1;

            foreach (var h in horariosTotales)
            {
                if (conductoresDict.TryGetValue(h.IdConductor, out var c) && c.IdEmpresa == IdEmpresa && c.Estado)
                {
                    if (vehiculosDict.TryGetValue(h.IdVehiculo, out var v) && propietariosDict.TryGetValue(v.IdPropietario, out var p))
                    {
                        DatosIniciales.Add(new DatosEmpresa
                        {
                            IdDato = i++,
                            Foto = c.Foto,
                            IdVehiculo = h.IdVehiculo,
                            Placa = v.Placa,
                            NombrePropietario = p.Nombre,
                            Conductor = c.Nombre,
                            Fecha = h.Fecha,
                            HoraInicio = h.HoraInicio,
                            HoraFin = h.HoraFin
                        });
                    }
                }
            }

            // Obtiene el registro específico por su IdDato.
            var dato = DatosIniciales.FirstOrDefault(item => item.IdDato == IdDato);

            List<Empresa> empresas = await _empresa.Lista(login);

            var empresa = empresas.Where(e => e.IdUsuario == usuario.IdUsuario).FirstOrDefault();
            ViewBag.Cupos = empresa.Cupos - await Cupos();


            return View(dato);
        }

        // Muestra la lista de vehículos asociados a la empresa del usuario.
        public async Task<IActionResult> Vehiculos()
        {
            var usuario = GetUsuarioFromSession();
            if (usuario == null)
            {
                ViewBag.Mensaje = "Usuario no autenticado.";
                return RedirectToAction("Login", "Inicio");
            }

            var login = CreateLogin(usuario);

            // Ejecutar en paralelo la obtención de empresas y vehículos
            var empresasTask = _empresa.Lista(login);
            var vehiculosTask = _vehiculo.Lista(login);
            await Task.WhenAll(empresasTask, vehiculosTask);

            var empresas = empresasTask.Result;
            var vehiculosTotales = vehiculosTask.Result;

            // Filtrar la empresa actual y los vehículos asociados activos
            var empresaActual = empresas.FirstOrDefault(e => e.IdUsuario == usuario.IdUsuario);
            if (empresaActual == null)
            {
                ViewBag.Mensaje = "Empresa no encontrada.";
                return View(new List<Vehiculo>());
            }
            int idEmpresa = empresaActual.IdEmpresa;
            var vehiculosEmpresa = vehiculosTotales.Where(v => v.IdEmpresa == idEmpresa && v.Estado).ToList();

            // Asignar el contador a cada vehículo utilizando un bucle for
            for (int i = 0; i < vehiculosEmpresa.Count; i++)
            {
                vehiculosEmpresa[i].Contador = i + 1;
            }

            // Se utiliza la empresa obtenida anteriormente para calcular los cupos disponibles
            ViewBag.Cupos = empresaActual.Cupos - await Cupos();

            return View(vehiculosEmpresa);
        }


        public async Task<IActionResult> Detalle_Vehiculo(int IdVehiculo)
        {
            var usuario = GetUsuarioFromSession();
            if (usuario == null)
            {
                ViewBag.Mensaje = "Usuario no autenticado.";
                return RedirectToAction("Login", "Inicio");
            }

            var login = CreateLogin(usuario);

            // Obtiene el vehículo de forma directa (necesario para obtener el IdPropietario)
            var vehiculo = await _vehiculo.Obtener(IdVehiculo, login);
            // Obtiene el propietario asociado al vehículo
            var propietario = await _propietario.Obtener(vehiculo.IdPropietario, login);
            ViewBag.Propietario = propietario?.Nombre;

            // Lanza en paralelo la obtención de la lista de empresas y el cálculo de cupos
            var empresasTask = _empresa.Lista(login);
            var cuposTask = Cupos();
            await Task.WhenAll(empresasTask, cuposTask);

            var empresasTot = empresasTask.Result;
            var cuposValue = cuposTask.Result;

            var empresa = empresasTot.FirstOrDefault(e => e.IdUsuario == usuario.IdUsuario);
            ViewBag.Cupos = empresa.Cupos - cuposValue;

            return View(vehiculo);
        }


        
        // Muestra el formulario para editar un vehículo.
        public async Task<IActionResult> Editar_Vehiculo(int IdVehiculo)
        {
            var usuario = GetUsuarioFromSession();
            if (usuario == null)
            {
                ViewBag.Mensaje = "Usuario no autenticado.";
                return RedirectToAction("Login", "Inicio");
            }

            var login = CreateLogin(usuario);

            // Lanzar en paralelo la obtención del vehículo, la lista de empresas y el cálculo de cupos.
            var vehiculoTask = _vehiculo.Obtener(IdVehiculo, login);
            var empresasTask = _empresa.Lista(login);
            var cuposTask = Cupos();

            await Task.WhenAll(vehiculoTask, empresasTask, cuposTask);

            var vehiculo = vehiculoTask.Result;
            var empresasTot = empresasTask.Result;
            int cuposValue = cuposTask.Result;

            var empresa = empresasTot.FirstOrDefault(e => e.IdUsuario == usuario.IdUsuario);
            ViewBag.Cupos = empresa?.Cupos - cuposValue;

            ModeloVista modelo = new ModeloVista { Vehiculo = vehiculo };
            return View(modelo);
        }

        // Guarda los cambios realizados en un vehículo.
        [HttpPost]
        public async Task<IActionResult> Guardar_Vehiculo(ModeloVista modelo)
        {
            var usuario = GetUsuarioFromSession();
            if (usuario == null)
            {
                ViewBag.Mensaje = "Usuario no autenticado.";
                return RedirectToAction("Login", "Inicio");
            }
            var login = CreateLogin(usuario);

            // Obtener empresas y cupos en paralelo.
            var empresasTask = _empresa.Lista(login);
            var cuposTask = Cupos();
            await Task.WhenAll(empresasTask, cuposTask);

            var empresas = empresasTask.Result;
            var empresa = empresas.FirstOrDefault(e => e.IdUsuario == usuario.IdUsuario);
            ViewBag.Cupos = empresa?.Cupos - cuposTask.Result;

            // Asegurarse de que el vehículo se marque como activo.
            modelo.Vehiculo.Estado = true;

            // Convertir archivos PDF a Base64, si se han subido.
            if (modelo.Archivo_1 != null)
            {
                using (var ms = new MemoryStream())
                {
                    await modelo.Archivo_1.CopyToAsync(ms);
                    modelo.Vehiculo.Soat = Convert.ToBase64String(ms.ToArray());
                }
            }
            if (modelo.Archivo_2 != null)
            {
                using (var ms = new MemoryStream())
                {
                    await modelo.Archivo_2.CopyToAsync(ms);
                    modelo.Vehiculo.TecnicoMecanica = Convert.ToBase64String(ms.ToArray());
                }
            }

            // Validar el modelo del vehículo.
            ValidarModelo valida = ValidarModelos.validarVehiculo(modelo.Vehiculo);
            if (!valida.Respuesta)
            {
                TempData["Mensaje"] = valida.Mensaje;
                return RedirectToAction("Editar_Vehiculo", new { IdVehiculo = modelo.Vehiculo.IdVehiculo });
            }

            // Guardar los cambios mediante la API.
            bool respuesta = await _vehiculo.Editar(modelo.Vehiculo, login);
            if (respuesta)
            {
                Transaccion t = Crear_Transaccion("Editar", "Vehiculo");
                await _transaccion.Guardar(t, login);
                return RedirectToAction("Vehiculos");
            }
            else
            {
                ViewBag.Mensaje = "No se pudo Guardar";
                TempData["Mensaje"] = "No se pudo Guardar";
                return RedirectToAction("Editar_Vehiculo", new { IdVehiculo = modelo.Vehiculo.IdVehiculo });
            }
        }


        // Desactiva un vehículo (cambia su estado a false).
        [HttpPost]
        public async Task<IActionResult> Eliminar_Vehiculo(int IdVehiculo)
        {
            var usuario = GetUsuarioFromSession();
            if (usuario == null)
            {
                ViewBag.Mensaje = "Usuario no autenticado.";
                return RedirectToAction("Login", "Inicio");
            }

            var login = CreateLogin(usuario);
            var empresas = await _empresa.Lista(login);
            var empresa = empresas.FirstOrDefault(e => e.IdUsuario == usuario.IdUsuario);

            ViewBag.Cupos = empresa.Cupos - await Cupos();
            var vehiculo = await _vehiculo.Obtener(IdVehiculo, login);
            vehiculo.Estado = false;

            bool respuesta = await _vehiculo.Editar(vehiculo, login);

            if (respuesta)
            {
                Transaccion t = Crear_Transaccion("Editar", "Vehiculo");
                bool guardar = await _transaccion.Guardar(t, login);
                return RedirectToAction("Vehiculos");
            }
            else
            {
                ViewBag.Mensaje = "No se pudo Guardar";
                TempData["Mensaje"] = "No se pudo Guardar";
                return RedirectToAction("Editar_Vehiculo", new { IdVehiculo = IdVehiculo });
            }
        }

        // Muestra el formulario para agregar un nuevo vehículo.
        public async Task<IActionResult> Agregar_Vehiculo()
        {
            var usuario = GetUsuarioFromSession();
            if (usuario == null)
            {
                ViewBag.Mensaje = "Usuario no autenticado.";
                return RedirectToAction("Login", "Inicio");
            }
            var login = CreateLogin(usuario);

            // Obtiene las empresas y propietarios asociados al usuario.
            var empresas = await _empresa.Lista(login);
            var IdEmpresa = empresas.FirstOrDefault(item => item.IdUsuario == usuario.IdUsuario)?.IdEmpresa;

            var propietariosTotales = await _propietario.Lista(login);
            var propietariosEmpresa = propietariosTotales?.Where(p => p.IdEmpresa == IdEmpresa && p.Estado).ToList();

            // Crea el ViewModel con el vehículo y la lista de propietarios.
            var viewModel = new ModeloVista
            {
                Vehiculo = new Vehiculo(), // Inicializa el objeto Vehiculo
                Propietarios = propietariosEmpresa // Asigna la lista de propietarios
            };

            List<Empresa> empresasTot = await _empresa.Lista(login);

            var empresa = empresasTot.Where(e => e.IdUsuario == usuario.IdUsuario).FirstOrDefault();
            ViewBag.Cupos = empresa.Cupos - await Cupos();

            return View(viewModel);
        }

        // Guarda un nuevo vehículo en la base de datos.
        [HttpPost]
        public async Task<IActionResult> Crear_Vehiculo(ModeloVista viewModel)
        {

            var usuario = GetUsuarioFromSession();
            if (usuario == null)
            {
                ViewBag.Mensaje = "Usuario no autenticado.";
                return RedirectToAction("Login", "Inicio");
            }

            var login = CreateLogin(usuario);


            var empresas = await _empresa.Lista(login);
            var empresa = empresas.FirstOrDefault(e => e.IdUsuario == usuario.IdUsuario);

            ViewBag.Cupos = empresa.Cupos - await Cupos();
            if (empresa.Cupos - await Cupos() <= 0)
            {
                TempData["Mensaje"] = "No se puede agregar, No hay Cupos";
                //var propietariosTotales = await _propietario.Lista(login);
                //viewModel.Propietarios = propietariosTotales?.Where(p => p.IdEmpresa == viewModel.Vehiculo.IdEmpresa && p.Estado).ToList();

                return View("Agregar_Vehiculo", viewModel);
            }
            var vehiculos = await _vehiculo.Lista(login);

            viewModel.Vehiculo.Estado = true;
            viewModel.Vehiculo.IdEmpresa = empresas.FirstOrDefault(e => e.IdUsuario == usuario.IdUsuario)?.IdEmpresa ?? 0;
            if (viewModel.Vehiculo.Placa != null)
            {
                viewModel.Vehiculo.Placa = viewModel.Vehiculo.Placa.ToUpper();
            }
                
            // Validar si la placa ya existe
            if (vehiculos.Any(v => v.Placa == viewModel.Vehiculo.Placa))
            {
                ViewBag.Mensaje = "La placa ya está en uso";
                var propietariosTotales = await _propietario.Lista(login);
                viewModel.Propietarios = propietariosTotales?.Where(p => p.IdEmpresa == viewModel.Vehiculo.IdEmpresa && p.Estado).ToList();

                return View("Agregar_Vehiculo", viewModel);
            }

            // Convertir archivos PDF a Base64
            if (viewModel.Archivo_1 != null)
            {
                using (var ms = new MemoryStream())
                {
                    await viewModel.Archivo_1.CopyToAsync(ms);
                    viewModel.Vehiculo.Soat = Convert.ToBase64String(ms.ToArray());
                }
            }


            if (viewModel.Archivo_2 != null)
            {
                using (var ms = new MemoryStream())
                {
                    await viewModel.Archivo_2.CopyToAsync(ms);
                    viewModel.Vehiculo.TecnicoMecanica = Convert.ToBase64String(ms.ToArray());
                }
            }


            ValidarModelo valida = new ValidarModelo();
            valida = ValidarModelos.validarVehiculo(viewModel.Vehiculo);
            if(valida.Respuesta)
            {
                // Guardar el vehículo en la BD
                bool respuesta = await _vehiculo.Guardar(viewModel.Vehiculo, login);

                if (respuesta)
                {
                    Transaccion t = Crear_Transaccion("Guardar", "Vehiculo");
                    bool guardar = await _transaccion.Guardar(t, login);

                    TempData["Mensaje"] = "Agregado Correctamente";
                    return RedirectToAction("Vehiculos");
                }
                else
                {
                    ViewBag.Mensaje = $"No se pudo Guardar {viewModel.Vehiculo.Placa}";
                    var propietariosTotales = await _propietario.Lista(login);
                    viewModel.Propietarios = propietariosTotales?.Where(p => p.IdEmpresa == viewModel.Vehiculo.IdEmpresa && p.Estado).ToList();

                    return View("Agregar_Vehiculo", viewModel);
                }
            }
            else
            {
                TempData["Mensaje"] = valida.Mensaje;
                return RedirectToAction("Agregar_Vehiculo","Empresa");
            }
            
        }


        //---------------------------- Conductores ------------------------------------

        // Muestra la lista de conductores asociados a la empresa del usuario.
        public async Task<IActionResult> Conductores()
        {
            var usuario = GetUsuarioFromSession();
            if (usuario == null)
            {
                ViewBag.Mensaje = "Usuario no autenticado.";
                return RedirectToAction("Login", "Inicio");
            }

            var login = CreateLogin(usuario);

            // Obtiene las empresas y conductores asociados al usuario.
            var empresas = await _empresa.Lista(login);
            var IdEmpresa = empresas.FirstOrDefault(item => item.IdUsuario == usuario.IdUsuario)?.IdEmpresa;

            var conductoresTotales = await _conductor.Lista(login);
            var conductoresEmpresa = conductoresTotales?.Where(c => c.IdEmpresa == IdEmpresa && c.Estado).ToList();
            if (conductoresEmpresa.Count > 0)
            {
                int i = 0;
                while (true)
                {
                    conductoresEmpresa[i].Contador = i + 1;
                    if (i == conductoresEmpresa.Count() - 1)
                    {
                        break;
                    }
                    else
                    {
                        i++;
                    }
                }

            }

            List<Empresa> empresasTot = await _empresa.Lista(login);

            var empresa = empresasTot.Where(e => e.IdUsuario == usuario.IdUsuario).FirstOrDefault();
            ViewBag.Cupos = empresa.Cupos - await Cupos();

            return View(conductoresEmpresa);
        }

        // Muestra los detalles de un conductor específico.
        public async Task<IActionResult> Detalle_Conductor(int IdConductor)
        {
            var usuario = GetUsuarioFromSession();
            if (usuario == null)
            {
                ViewBag.Mensaje = "Usuario no autenticado.";
                return RedirectToAction("Login", "Inicio");
            }

            var login = CreateLogin(usuario);
            var conductor = await _conductor.Obtener(IdConductor, login);
            Encriptado enc = new Encriptado();

            List<Empresa> empresasTot = await _empresa.Lista(login);

            var empresa = empresasTot.Where(e => e.IdUsuario == usuario.IdUsuario).FirstOrDefault();
            ViewBag.Cupos = empresa.Cupos - await Cupos();
            return View(conductor);
        }

        // Muestra el formulario para editar un conductor.
        public async Task<IActionResult> Editar_Conductor(int IdConductor)
        {
            Encriptado enc = new Encriptado();
            var usuario = GetUsuarioFromSession();
            if (usuario == null)
            {
                ViewBag.Mensaje = "Usuario no autenticado.";
                return RedirectToAction("Login", "Inicio");
            }
            ModeloVista modelo = new ModeloVista();
            var login = CreateLogin(usuario);
            var conductor = await _conductor.Obtener(IdConductor, login);
            //string Contrasena = enc.DesencriptarSimple(conductor.Contrasena);
            //conductor.Contrasena = Contrasena;

            modelo.Conductor = conductor;

            List<Empresa> empresasTot = await _empresa.Lista(login);

            var empresa = empresasTot.Where(e => e.IdUsuario == usuario.IdUsuario).FirstOrDefault();
            ViewBag.Cupos = empresa.Cupos - await Cupos();

            return View(modelo);
        }

        // Guarda los cambios realizados en un conductor.
        [HttpPost]
        public async Task<IActionResult> Guardar_Conductor(ModeloVista modelo)
        {
            Encriptado enc = new Encriptado();
            var usuario = GetUsuarioFromSession();
            if (usuario == null)
            {
                TempData["Mensaje"] = "Usuario no autenticado.";
                return RedirectToAction("Login", "Inicio");
            }

            var login = CreateLogin(usuario);

            var empresas = await _empresa.Lista(login);
            var empresa = empresas.FirstOrDefault(e => e.IdUsuario == usuario.IdUsuario);

            ViewBag.Cupos = empresa.Cupos - await Cupos();
            modelo.Conductor.Estado = true;

            var conductor = await _conductor.Obtener(modelo.Conductor.IdConductor, login);
            var usuarios = await _usuario.Lista(login);

            var usuarioConductor = usuarios.Where(u => u.Correo == conductor.Correo && u.Contrasena == conductor.Contrasena).FirstOrDefault();

            // Convertir archivos PDF a Base64
            if (modelo.Archivo_1 != null && modelo.Archivo_1.Length > 0)
            {
                try
                {
                    ValidacionDocumentos sistema = new ValidacionDocumentos();

                    // Aplicar OCR al PDF y extraer texto
                    string textoExtraido = sistema.ProcesarPdfConOCR(modelo.Archivo_1);

                    // Validar si el documento es una cédula
                    bool esDocumento = sistema.Contiene(textoExtraido.ToUpper(), new string[] { "REPÚBLICA", "COLOMBIA" }, 'Y');

                    if (esDocumento)
                    {
                        using (var ms = new MemoryStream())
                        {
                            await modelo.Archivo_1.CopyToAsync(ms);
                            modelo.Conductor.DocumentoCedula = Convert.ToBase64String(ms.ToArray());
                        }
                    }
                    else
                    {
                        //TempData["Mensaje"] = textoExtraido;
                        TempData["Mensaje"] = $"El documento ingresado no es una Cédula o no es legible";
                        return RedirectToAction("Editar_Conductor", new { IdConductor = modelo.Conductor.IdConductor });
                    }
                }
                catch (Exception ex)
                {
                    TempData["Mensaje"] = $"Error al procesar el documento: {ex.Message}";
                    return RedirectToAction("Editar_Conductor", new { IdConductor = modelo.Conductor.IdConductor });
                }
            }
            string contraSinSha = "";

            if (modelo.Archivo_2 != null)
            {
                using (var ms = new MemoryStream())
                {
                    await modelo.Archivo_2.CopyToAsync(ms);
                    modelo.Conductor.DocumentoEps = Convert.ToBase64String(ms.ToArray());
                }
            }

            if (modelo.Archivo_3 != null)
            {
                using (var ms = new MemoryStream())
                {
                    await modelo.Archivo_3.CopyToAsync(ms);
                    modelo.Conductor.DocumentoArl = Convert.ToBase64String(ms.ToArray());
                }
            }

            if (modelo.Archivo_4 != null)
            {
                using (var ms = new MemoryStream())
                {
                    await modelo.Archivo_4.CopyToAsync(ms);
                    modelo.Conductor.Foto = Convert.ToBase64String(ms.ToArray());
                }
            }
            if (modelo.Conductor.Contrasena != null)
            {
                //--------------------------------------------- Encriptado

                 contraSinSha = modelo.Conductor.Contrasena;
                string Contrasena = Encriptado.GetSHA256(modelo.Conductor.Contrasena);
                modelo.Conductor.Contrasena = Contrasena;
            }


            ValidarModelo valida = new ValidarModelo();

            if (usuarioConductor != null)
            {
                valida = ValidarModelos.validarConductor(modelo.Conductor);
                if(valida.Respuesta)
                {
                    bool respuesta = await _conductor.Editar(modelo.Conductor, login);
                    if (respuesta)
                    {
                        usuarioConductor.Correo = modelo.Conductor.Correo;
                        usuarioConductor.Contrasena = modelo.Conductor.Contrasena;
                        usuarioConductor.Foto = modelo.Conductor.Foto;
                        usuarioConductor.Nombre = modelo.Conductor.Nombre;
                        usuarioConductor.Telefono = modelo.Conductor.Telefono;
                        usuarioConductor.Direccion = modelo.Conductor.Direccion;
                        usuarioConductor.Ciudad = modelo.Conductor.Ciudad;
                        usuarioConductor.Celular = modelo.Conductor.Celular;
                        usuarioConductor.Estado = true;
                        bool respuestaUsuario = await _usuario.Editar(usuarioConductor, login);
                        if (respuestaUsuario)
                        {
                            Transaccion t1 = Crear_Transaccion("Editar", "Conductor");
                            bool guardar1 = await _transaccion.Guardar(t1, login);
                            Transaccion t2 = Crear_Transaccion("Editar", "Usuario");
                            bool guardar2 = await _transaccion.Guardar(t2, login);

                            TempData["Mensaje"] = $"Editado Correctamente \n" +
                                $"Correo: {usuarioConductor.Correo}  \n" +
                                $"Contraseña: {contraSinSha}";
                            return RedirectToAction("Conductores");
                        }
                        else
                        {
                            TempData["Mensaje"] = "No se pudo Guardar el Usuario";
                            return RedirectToAction("Editar_Conductor", new { IdConductor = modelo.Conductor.IdConductor });
                        }

                    }
                    else
                    {
                        TempData["Mensaje"] = $"Respuesta Negativa al Guardar";
                        return RedirectToAction("Editar_Conductor", new { IdConductor = modelo.Conductor.IdConductor });
                    }
                }
                else
                {
                    TempData["Mensaje"] = valida.Mensaje;

                    return RedirectToAction("Editar_Conductor", new { IdConductor = modelo.Conductor.IdConductor });
                }
                

            }
            else
            {
                TempData["Mensaje"] = "No se encontró usuario asignado";
                
                return RedirectToAction("Editar_Conductor", new { IdConductor = modelo.Conductor.IdConductor });
            }
        }

        // Desactiva un conductor (cambia su estado a false).
        [HttpPost]
        public async Task<IActionResult> Eliminar_Conductor(int IdConductor)
        {
            var usuario = GetUsuarioFromSession();
            if (usuario == null)
            {
                ViewBag.Mensaje = "Usuario no autenticado.";
                return RedirectToAction("Login", "Inicio");
            }

            var login = CreateLogin(usuario);
            var empresas = await _empresa.Lista(login);
            var empresa = empresas.FirstOrDefault(e => e.IdUsuario == usuario.IdUsuario);

            ViewBag.Cupos = empresa.Cupos - await Cupos();
            var conductor = await _conductor.Obtener(IdConductor, login);
            conductor.Estado = false;

            bool respuesta = await _conductor.Editar(conductor, login);

            if (respuesta)
            {
                Transaccion t = Crear_Transaccion("Editar", "Conductor");
                bool guardar = await _transaccion.Guardar(t, login);
                return RedirectToAction("Conductores");
            }
            else
            {
                ViewBag.Mensaje = "No se pudo Guardar";
                TempData["Mensaje"] = "No se pudo Guardar";
                return RedirectToAction("Editar_Conductor", new { IdConductor = IdConductor });
            }
        }

        // Muestra el formulario para agregar un nuevo conductor.
        public async Task<IActionResult> Agregar_Conductor()
        {
            var usuario = GetUsuarioFromSession();
            if (usuario == null)
            {
                ViewBag.Mensaje = "Usuario no autenticado.";
                return RedirectToAction("Login", "Inicio");
            }

            var login = CreateLogin(usuario);

            List<Empresa> empresasTot = await _empresa.Lista(login);

            var empresa = empresasTot.Where(e => e.IdUsuario == usuario.IdUsuario).FirstOrDefault();
            ViewBag.Cupos = empresa.Cupos - await Cupos();
            return View();
        }

        // Guarda un nuevo conductor en la base de datos.
        [HttpPost]
        public async Task<IActionResult> Crear_Conductor(ModeloVista modelo)
        {
            var usuario = GetUsuarioFromSession();
            if (usuario == null)
            {
                ViewBag.Mensaje = "Usuario no autenticado.";
                return RedirectToAction("Login", "Inicio");
            }

            var login = CreateLogin(usuario);

            var empresas = await _empresa.Lista(login);

            var empresa = empresas.FirstOrDefault(e => e.IdUsuario == usuario.IdUsuario);

            ViewBag.Cupos = empresa.Cupos - await Cupos();
            var conductores = await _conductor.Lista(login);

            modelo.Conductor.Estado = true;
            modelo.Conductor.IdEmpresa = empresas.FirstOrDefault(e => e.IdUsuario == usuario.IdUsuario)?.IdEmpresa ?? 0;

            // Valida si el conductor ya está registrado.
            if (conductores.Any(c => c.NumeroCedula == modelo.Conductor.NumeroCedula || c.Correo == modelo.Conductor.Correo || c.Nombre == modelo.Conductor.Nombre))
            {
                TempData["Mensaje"] = "El conductor ya está registrado";
                return RedirectToAction("Agregar_Conductor");
            }

            // Convertir archivos PDF a Base64
            if (modelo.Archivo_1 != null && modelo.Archivo_1.Length > 0)
            {
                try
                {

                    ValidacionDocumentos sistema = new ValidacionDocumentos();

                    // Aplicar OCR al PDF y extraer texto
                    string textoExtraido = sistema.ProcesarPdfConOCR(modelo.Archivo_1);

                    // Validar si el documento es una cédula
                    bool esDocumento = sistema.Contiene(textoExtraido.ToUpper(), new string[] { "REPÚBLICA", "COLOMBIA"}, 'Y');

                    if (esDocumento)
                    {
                        using (var ms = new MemoryStream())
                        {
                            await modelo.Archivo_1.CopyToAsync(ms);
                            modelo.Conductor.DocumentoCedula = Convert.ToBase64String(ms.ToArray());
                        }
                    }
                    else
                    {
                        //TempData["Mensaje"] = textoExtraido;
                        TempData["Mensaje"] = $"El documento ingresado no es una Cédula o no está al 150%";
                        return RedirectToAction("Agregar_Conductor");
                    }
                }
                catch (Exception ex)
                {
                    TempData["Mensaje"] = $"Error al procesar el documento: {ex.Message}";
                    return RedirectToAction("Agregar_Conductor");
                }
            }
            else
            {
                TempData["Mensaje"] = "No se ha subido ningún archivo.";
                return RedirectToAction("Agregar_Conductor");
            }


            if (modelo.Archivo_2 != null)
            {
                using (var ms = new MemoryStream())
                {
                    await modelo.Archivo_2.CopyToAsync(ms);
                    modelo.Conductor.DocumentoEps = Convert.ToBase64String(ms.ToArray());
                }
            }


            if (modelo.Archivo_3 != null)
            {
                using (var ms = new MemoryStream())
                {
                    await modelo.Archivo_3.CopyToAsync(ms);
                    modelo.Conductor.DocumentoArl = Convert.ToBase64String(ms.ToArray());
                }
            }


            if (modelo.Archivo_4 != null)
            {
                using (var ms = new MemoryStream())
                {
                    await modelo.Archivo_4.CopyToAsync(ms);
                    modelo.Conductor.Foto = Convert.ToBase64String(ms.ToArray());
                }
            }
            else
            {
                modelo.Conductor.Foto = "N/F";
            }
            
            string contrasenaBase;
            if (modelo.Conductor.NumeroCedula != 0 && modelo.Conductor.Nombre != null)
            {
                contrasenaBase = Encriptado.GenerarContrasena(modelo.Conductor.NumeroCedula.ToString(), modelo.Conductor.Nombre);

            }
            else
            {
                contrasenaBase = "Password123";
            }

            string contrasena = Encriptado.GetSHA256(contrasenaBase);
            modelo.Conductor.Contrasena = contrasena;


            // Guarda el conductor.
            ValidarModelo valida = new ValidarModelo();
            valida = ValidarModelos.validarConductor(modelo.Conductor);
            if(valida.Respuesta)
            {
                
                bool respuesta = await _conductor.Guardar(modelo.Conductor, login);

                if (respuesta)
                {
                    Usuario usuarioConductor = new Usuario();

                    usuarioConductor.Correo = modelo.Conductor.Correo;
                    usuarioConductor.Contrasena = modelo.Conductor.Contrasena;
                    usuarioConductor.Foto = modelo.Conductor.Foto;
                    usuarioConductor.Nombre = modelo.Conductor.Nombre;
                    usuarioConductor.Telefono = modelo.Conductor.Telefono;
                    usuarioConductor.Direccion = modelo.Conductor.Direccion;
                    usuarioConductor.Ciudad = modelo.Conductor.Ciudad;
                    usuarioConductor.Celular = modelo.Conductor.Celular;
                    usuarioConductor.Estado = true;
                    usuarioConductor.IdRol = 1006;

                    bool crearUsuario = await _usuario.Guardar(usuarioConductor, login);
                    if (crearUsuario)
                    {
                        var conductoresGuardados = await _conductor.Lista(login);
                        var conductorGuardado = conductoresGuardados.FirstOrDefault(c => c.NumeroCedula == modelo.Conductor.NumeroCedula);
                        ViewBag.IdConductor = conductorGuardado?.IdConductor;
                        ViewBag.Exito = true;


                        Transaccion t1 = Crear_Transaccion("Guardar", "Conductor");
                        bool guardar1 = await _transaccion.Guardar(t1, login);
                        Transaccion t2 = Crear_Transaccion("Guardar", "Usuario");
                        bool guardar2 = await _transaccion.Guardar(t2, login);

                        TempData["Mensaje"] = $"Guardado Exitosamente \n" +
                            $"Correo: {usuarioConductor.Correo} \n" +
                            $"Contraseña: {contrasenaBase}";
                        return RedirectToAction("Conductores");
                    }
                    else
                    {
                        TempData["Mensaje"] = "No se pudo Crear el Usuario";
                        return RedirectToAction("Agregar_Conductor");
                    }

                }
                else
                {
                    TempData["Mensaje"] = "No se pudo Guardar";
                    return RedirectToAction("Agregar_Conductor");
                }
            }
            else
            {
                TempData["Mensaje"] = valida.Mensaje;
                return RedirectToAction("Agregar_Conductor");
            }
            
        }

        //---------------------------- Propietarios ------------------------------------

        // Muestra la lista de propietarios asociados a la empresa del usuario.
        public async Task<IActionResult> Propietarios()
        {
            var usuario = GetUsuarioFromSession();
            if (usuario == null)
            {
                ViewBag.Mensaje = "Usuario no autenticado.";
                return RedirectToAction("Login", "Inicio");
            }

            var login = CreateLogin(usuario);

            // Obtiene las empresas y propietarios asociados al usuario.
            var empresas = await _empresa.Lista(login);
            var IdEmpresa = empresas.FirstOrDefault(item => item.IdUsuario == usuario.IdUsuario)?.IdEmpresa;

            var propietariosTotales = await _propietario.Lista(login);
            var propietariosEmpresa = propietariosTotales?.Where(p => p.IdEmpresa == IdEmpresa && p.Estado).ToList();

            if (propietariosEmpresa.Count() > 0)
            {
                int i = 0;
                while (true)
                {
                    propietariosEmpresa[i].Contador = i + 1;
                    if (i == propietariosEmpresa.Count() - 1)
                    {
                        break;
                    }
                    else
                    {
                        i++;
                    }
                }
            }
            List<Empresa> empresasTot = await _empresa.Lista(login);

            var empresa = empresasTot.Where(e => e.IdUsuario == usuario.IdUsuario).FirstOrDefault();
            ViewBag.Cupos = empresa.Cupos - await Cupos();
            return View(propietariosEmpresa);
        }

        // Muestra los detalles de un propietario específico.
        public async Task<IActionResult> Detalle_Propietario(int IdPropietario)
        {
            var usuario = GetUsuarioFromSession();
            if (usuario == null)
            {
                ViewBag.Mensaje = "Usuario no autenticado.";
                return RedirectToAction("Login", "Inicio");
            }
            ModeloVista modelo = new ModeloVista();
            var login = CreateLogin(usuario);

            // Obtiene el propietario y los vehículos asociados.
            modelo.Propietario = await _propietario.Obtener(IdPropietario, login);

            var vehiculosTotales = await _vehiculo.Lista(login);
            modelo.Vehiculos = vehiculosTotales?.Where(v => v.IdPropietario == IdPropietario && v.Estado).ToList();

            List<Empresa> empresasTot = await _empresa.Lista(login);

            var empresa = empresasTot.Where(e => e.IdUsuario == usuario.IdUsuario).FirstOrDefault();
            ViewBag.Cupos = empresa.Cupos - await Cupos();

            //var ocrService = new ValidacionDocumentos();
            //string texto = ocrService.ProcesarImagenConOCR("wwwroot/temp/Cedula3.png");
            //TempData["Mensaje"] = texto;
            return View(modelo);
        }

        // Muestra el formulario para editar un propietario.
        public async Task<IActionResult> Editar_Propietario(int IdPropietario)
        {
            ModeloVista modelo = new ModeloVista();
            var usuario = GetUsuarioFromSession();
            if (usuario == null)
            {
                ViewBag.Mensaje = "Usuario no autenticado.";
                return RedirectToAction("Login", "Inicio");
            }

            var login = CreateLogin(usuario);
            var propietario = await _propietario.Obtener(IdPropietario, login);
            modelo.Propietario = propietario;

            List<Empresa> empresasTot = await _empresa.Lista(login);

            var empresa = empresasTot.Where(e => e.IdUsuario == usuario.IdUsuario).FirstOrDefault();
            ViewBag.Cupos = empresa.Cupos - await Cupos();

            return View(modelo);
        }

        // Guarda los cambios realizados en un propietario.
        [HttpPost]
        public async Task<IActionResult> Guardar_Propietario(ModeloVista modelo)
        {
            var usuario = GetUsuarioFromSession();
            if (usuario == null)
            {
                ViewBag.Mensaje = "Usuario no autenticado.";
                return RedirectToAction("Login", "Inicio");
            }

            var login = CreateLogin(usuario);
            var empresas = await _empresa.Lista(login);
            var empresa = empresas.FirstOrDefault(e => e.IdUsuario == usuario.IdUsuario);

            ViewBag.Cupos = empresa.Cupos - await Cupos();
            modelo.Propietario.Estado = true;
            if (modelo.Archivo_4 != null)
            {
                using (var ms = new MemoryStream())
                {
                    await modelo.Archivo_4.CopyToAsync(ms);
                    modelo.Propietario.Foto = Convert.ToBase64String(ms.ToArray());
                }
            }
            ValidarModelo valida = new ValidarModelo();
            valida = ValidarModelos.validarPropietario(modelo.Propietario);
            if(valida.Respuesta)
            {
                bool respuesta = await _propietario.Editar(modelo.Propietario, login);

                if (respuesta)
                {
                    Transaccion t = Crear_Transaccion("Editar", "Propietario");
                    bool guardar = await _transaccion.Guardar(t, login);
                    return RedirectToAction("Propietarios");
                }
                else
                {
                    ViewBag.Mensaje = "No se pudo Guardar";
                    TempData["Mensaje"] = "No se pudo Guardar";
                    return RedirectToAction("Editar_Propietario", new { IdPropietario = modelo.Propietario.IdPropietario });
                }
            }
            else
            {
                TempData["Mensaje"] = valida.Mensaje;
                return RedirectToAction("Editar_Propietario", new { IdPropietario = modelo.Propietario.IdPropietario });
            }
            
        }

        // Desactiva un propietario (cambia su estado a false).
        [HttpPost]
        public async Task<IActionResult> Eliminar_Propietario(int IdPropietario)
        {
            var usuario = GetUsuarioFromSession();
            if (usuario == null)
            {
                ViewBag.Mensaje = "Usuario no autenticado.";
                return RedirectToAction("Login", "Inicio");
            }

            var login = CreateLogin(usuario);
            var empresas = await _empresa.Lista(login);
            var empresa = empresas.FirstOrDefault(e => e.IdUsuario == usuario.IdUsuario);

            ViewBag.Cupos = empresa.Cupos - await Cupos();
            var propietario = await _propietario.Obtener(IdPropietario, login);
            propietario.Estado = false;

            bool respuesta = await _propietario.Editar(propietario, login);

            if (respuesta)
            {
                Transaccion t = Crear_Transaccion("Editar", "Propietario");
                bool guardar = await _transaccion.Guardar(t, login);
                return RedirectToAction("Propietarios");
            }
            else
            {
                ViewBag.Mensaje = "No se pudo Guardar";
                TempData["Mensaje"] = "No se pudo Guardar";
                return RedirectToAction("Editar_Propietario", new { IdPropietario = IdPropietario });
            }
        }

        // Muestra el formulario para agregar un nuevo propietario.
        public async Task<IActionResult> Agregar_Propietario()
        {
            var usuario = GetUsuarioFromSession();
            if (usuario == null)
            {
                ViewBag.Mensaje = "Usuario no autenticado.";
                return RedirectToAction("Login", "Inicio");
            }

            var login = CreateLogin(usuario);
            List<Empresa> empresasTot = await _empresa.Lista(login);

            var empresa = empresasTot.Where(e => e.IdUsuario == usuario.IdUsuario).FirstOrDefault();
            ViewBag.Cupos = empresa.Cupos - await Cupos();
            return View();
        }

        // Guarda un nuevo propietario en la base de datos.
        [HttpPost]
        public async Task<IActionResult> Crear_Propietario(ModeloVista modelo)
        {
            var usuario = GetUsuarioFromSession();
            if (usuario == null)
            {
                ViewBag.Mensaje = "Usuario no autenticado.";
                return RedirectToAction("Login", "Inicio");
            }

            var login = CreateLogin(usuario);
            var empresas = await _empresa.Lista(login);
            var empresa = empresas.FirstOrDefault(e => e.IdUsuario == usuario.IdUsuario);

            ViewBag.Cupos = empresa.Cupos - await Cupos();
            var propietarios = await _propietario.Lista(login);

            
            modelo.Propietario.Estado = true;
            modelo.Propietario.IdEmpresa = empresas.FirstOrDefault(e => e.IdUsuario == usuario.IdUsuario)?.IdEmpresa ?? 0;

            // Valida si el propietario ya está registrado.
            if (propietarios.Any(c => c.NumeroCedula == modelo.Propietario.NumeroCedula || c.Correo == modelo.Propietario.Correo || c.Nombre == modelo.Propietario.Nombre))
            {
                ViewBag.Mensaje = "El propietario ya está registrado";
                return View("Agregar_Propietario");
            }
            if (modelo.Archivo_4 != null)
            {
                using (var ms = new MemoryStream())
                {
                    await modelo.Archivo_4.CopyToAsync(ms);
                    modelo.Propietario.Foto = Convert.ToBase64String(ms.ToArray());
                }
            }

            if (modelo.Archivo_1 != null && modelo.Archivo_1.Length > 0)
            {
                try
                {
                    ValidacionDocumentos sistema = new ValidacionDocumentos();

                    // Aplicar OCR al PDF y extraer texto
                    string textoExtraido = sistema.ProcesarPdfConOCR(modelo.Archivo_1);

                    // Validar si el documento es una cédula
                    bool esDocumento = sistema.Contiene(textoExtraido.ToUpper(), new string[] { "REPÚBLICA", "COLOMBIA" }, 'Y');

                    if (esDocumento)
                    {
                        using (var ms = new MemoryStream())
                        {
                            await modelo.Archivo_1.CopyToAsync(ms);
                            modelo.Propietario.DocumentoCedula = Convert.ToBase64String(ms.ToArray());
                        }
                    }
                    else
                    {
                        //TempData["Mensaje"] = textoExtraido;
                        ViewBag.Mensaje = $"El documento ingresado no es una Cédula o no es legible";
                        return View("Agregar_Propietario");
                    }
                }
                catch (Exception ex)
                {
                    ViewBag.Mensaje = $"Error al procesar el documento: {ex.Message}";
                    return View("Agregar_Propietario");
                }
            }
            else
            {
                ViewBag.Mensaje = "No se ha subido ningún archivo.";
                return View("Agregar_Propietario");
            }
            ValidarModelo valida = new ValidarModelo();
            valida = ValidarModelos.validarPropietario(modelo.Propietario);
            if(valida.Respuesta)
            {
                // Guarda el propietario.
                bool respuesta = await _propietario.Guardar(modelo.Propietario, login);

                if (respuesta)
                {
                    var propietariosGuardados = await _propietario.Lista(login);
                    var propietarioGuardado = propietariosGuardados.FirstOrDefault(p => p.NumeroCedula == modelo.Propietario.NumeroCedula);
                    ViewBag.IdPropietario = modelo.Propietario?.IdPropietario;
                    ViewBag.Exito = true;
                    Transaccion t = Crear_Transaccion("Guardar", "Propietario");
                    bool guardar = await _transaccion.Guardar(t, login);
                    return RedirectToAction("Propietarios");
                }
                else
                {
                    TempData["Mensaje"] = "No se pudo Guardar";
                    return View("Agregar_Propietario");
                }
            }
            else
            {
                TempData["Mensaje"] = valida.Mensaje;
                return View("Agregar_Propietario");
            }
            
        }

        //------------------------- Horario ----------------------------------------------
        public async Task<IActionResult> Ver_Horario_Conductor(int IdConductor)
        {
            var usuario = GetUsuarioFromSession();
            if (usuario == null)
            {
                ViewBag.Mensaje = "Usuario no autenticado.";
                return RedirectToAction("Login", "Inicio");
            }

            var login = CreateLogin(usuario);
            ModeloVista modelo = new ModeloVista();
            modelo.Conductor = await _conductor.Obtener(IdConductor, login);
            var Horarios = await _horario.Lista(login);
            modelo.Vehiculos = await _vehiculo.Lista(login);

            modelo.Horarios = Horarios?.Where(h => h.IdConductor == IdConductor).ToList();

            List<Empresa> empresasTot = await _empresa.Lista(login);

            var empresa = empresasTot.Where(e => e.IdUsuario == usuario.IdUsuario).FirstOrDefault();
            ViewBag.Cupos = empresa.Cupos - await Cupos();

            return View(modelo);
        }

        public async Task<IActionResult> Editar_Horario(int IdHorario)
        {
            var usuario = GetUsuarioFromSession();
            if (usuario == null)
            {
                ViewBag.Mensaje = "Usuario no autenticado.";
                return RedirectToAction("Login", "Inicio");
            }
            var login = CreateLogin(usuario);
            ModeloVista modelo = new ModeloVista();

            var vehiculos = await _vehiculo.Lista(login);
            var conductores = await _conductor.Lista(login);
            var empresas = await _empresa.Lista(login);
            int IdEmpresa = empresas.Where(e => e.IdUsuario == usuario.IdUsuario).FirstOrDefault().IdEmpresa;

            modelo.Vehiculos = vehiculos?.Where(v => v.IdEmpresa == IdEmpresa && v.Estado).ToList();
            modelo.Conductores = conductores?.Where(c => c.IdEmpresa == IdEmpresa && c.Estado).ToList();

            modelo.Horario = await _horario.Obtener(IdHorario, login);
            modelo.Conductor = await _conductor.Obtener(modelo.Horario.IdConductor, login);

            List<Empresa> empresasTot = await _empresa.Lista(login);

            var empresa = empresasTot.Where(e => e.IdUsuario == usuario.IdUsuario).FirstOrDefault();
            ViewBag.Cupos = empresa.Cupos - await Cupos();

            return View(modelo);

        }

        [HttpPost]
        public async Task<IActionResult> Guardar_Horario(ModeloVista modelo)
        {
            var usuario = GetUsuarioFromSession();
            if (usuario == null)
            {
                ViewBag.Mensaje = "Usuario no autenticado.";
                return RedirectToAction("Login", "Inicio");
            }
            var login = CreateLogin(usuario);

            var empresas = await _empresa.Lista(login);
            var empresa = empresas.FirstOrDefault(e => e.IdUsuario == usuario.IdUsuario);

            ViewBag.Cupos = empresa.Cupos - await Cupos();
            Horario h = modelo.Horario;
            bool respuesta = await _horario.Editar(h, login);
            if (respuesta)
            {
                int IdConductor = modelo.Conductor.IdConductor;
                Transaccion t = Crear_Transaccion("Editar", "Horario");
                bool guardar = await _transaccion.Guardar(t, login);
                return RedirectToAction("Ver_Horario", new { IdConductor = IdConductor });
            }
            else
            {
                ViewBag.Mensaje = "No se pudo Guardar";
                TempData["Mensaje"] = "No se pudo Guardar";
                return RedirectToAction("Editar_Horario", new { IdHorario= modelo.Horario.IdHorario });
            }

        }

        [HttpGet]
        public async Task<IActionResult> Eliminar_Horario(int IdHorario)
        {
            var usuario = GetUsuarioFromSession();
            if (usuario == null)
            {
                ViewBag.Mensaje = "Usuario no autenticado.";
                return RedirectToAction("Login", "Inicio");
            }

            var login = CreateLogin(usuario);

            var empresas = await _empresa.Lista(login);
            var empresa = empresas.FirstOrDefault(e => e.IdUsuario == usuario.IdUsuario);

            ViewBag.Cupos = empresa.Cupos - await Cupos();
            var horario = await _horario.Obtener(IdHorario, login);
            bool respuesta = await _horario.Eliminar(IdHorario, login);
            if (respuesta)
            {
                Transaccion t = Crear_Transaccion("Eliminar", "Horario");
                bool guardar = await _transaccion.Guardar(t, login);
                TempData["Mensaje"] = "Eliminado Correctamente";
                return RedirectToAction("Ver_Horario_Conductor", new { IdConductor = horario.IdConductor});
            }
            else
            {
                ViewBag.Mensaje = "No se pudo Guardar";
                TempData["Mensaje"] = "No se pudo Guardar";
                return RedirectToAction("Horarios");
            }
        }

        public async Task<IActionResult> Asignar_Horario()
        {
            var usuario = GetUsuarioFromSession();
            if (usuario == null)
            {
                ViewBag.Mensaje = "Usuario no autenticado.";
                return RedirectToAction("Login", "Inicio");
            }

            var login = CreateLogin(usuario);
            ModeloVista modelo = new ModeloVista();

            var vehiculos = await _vehiculo.Lista(login);
            var empresas = await _empresa.Lista(login);
            var conductores = await _conductor.Lista(login);
            int IdEmpresa = empresas.Where(e => e.IdUsuario == usuario.IdUsuario).FirstOrDefault().IdEmpresa;

            modelo.Vehiculos = vehiculos?.Where(v => v.IdEmpresa == IdEmpresa && v.Estado).ToList();

            modelo.Conductores = conductores.Where(c => c.IdEmpresa == IdEmpresa && c.Estado).ToList();
            List<Empresa> empresasTot = await _empresa.Lista(login);

            var empresa = empresasTot.Where(e => e.IdUsuario == usuario.IdUsuario).FirstOrDefault();
            ViewBag.Cupos = empresa.Cupos - await Cupos();
            return View(modelo);
        }

        [HttpPost]
        public async Task<IActionResult> Crear_Horario(ModeloVista modelo)
        {
            var usuario = GetUsuarioFromSession();
            if (usuario == null)
            {
                ViewBag.Mensaje = "Usuario no autenticado.";
                return RedirectToAction("Login", "Inicio");
            }

            var login = CreateLogin(usuario);

            var empresas = await _empresa.Lista(login);
            var empresa = empresas.FirstOrDefault(e => e.IdUsuario == usuario.IdUsuario);

            ViewBag.Cupos = empresa.Cupos - await Cupos();
            // Asignar el IdConductor al horario
            //modelo.Horario.IdConductor = modelo.Conductor.IdConductor;

            List<Horario> horarios = await _horario.Lista(login);

            bool existeConflicto = horarios.Any(h =>
                h.Fecha.Date == modelo.Horario.Fecha.Date &&
                (h.IdVehiculo == modelo.Horario.IdVehiculo || h.IdConductor == modelo.Horario.IdConductor) &&
                (modelo.Horario.HoraInicio < h.HoraFin && modelo.Horario.HoraFin > h.HoraInicio)
            );

            if (!existeConflicto)
            {
                // No existe conflicto, se puede guardar el horario
                bool respuesta = await _horario.Guardar(modelo.Horario, login);

                if (respuesta)
                {

                    ViewBag.Mensaje = "Horario guardado correctamente.";
                    Transaccion t = Crear_Transaccion("Guardar", "Horario");
                    bool guardar = await _transaccion.Guardar(t, login);
                    return RedirectToAction("Ver_Horario_Conductor", new { IdConductor = modelo.Horario.IdConductor });
                }
                else
                {
                    ViewBag.Mensaje = "No se pudo Guardar el Horario";
                    TempData["Mensaje"] = "No se pudo Guardar  el Horario";
                    return RedirectToAction("Asignar_Horario", new { IdConductor = modelo.Conductor.IdConductor });
                }
            }
            else
            {
                TempData["Mensaje"] = "Ya hay un horario asignado en ese horario";

                ViewBag.Mensaje = "Ya hay un horario asignado en ese horario";

                return RedirectToAction("Asignar_Horario", new { IdConductor = modelo.Conductor.IdConductor });
                
            }
            // Guardar el horario
            
        }

        [HttpPost]
        public async Task<IActionResult> Crear_RangoHorarios(ModeloVista modelo, DateTime FechaInicio, DateTime FechaFin, TimeSpan HoraInicio, TimeSpan HoraFin, int IdVehiculo)
        {
            var usuario = GetUsuarioFromSession();
            if (usuario == null)
            {
                ViewBag.Mensaje = "Usuario no autenticado.";
                return RedirectToAction("Login", "Inicio");
            }

            var login = CreateLogin(usuario);

            var empresas = await _empresa.Lista(login);
            var empresa = empresas.FirstOrDefault(e => e.IdUsuario == usuario.IdUsuario);

            ViewBag.Cupos = empresa.Cupos - await Cupos();

            // Obtener todos los horarios existentes (se podría filtrar por rango de fechas si la cantidad es grande)
            List<Horario> horariosExistentes = await _horario.Lista(login);

            // Verificar conflicto para cada fecha en el rango
            for (var fecha = FechaInicio; fecha <= FechaFin; fecha = fecha.AddDays(1))
            {
                bool existeConflicto = horariosExistentes.Any(h =>
                    h.Fecha.Date == fecha.Date &&
                    (h.IdVehiculo == IdVehiculo || h.IdConductor == modelo.Conductor.IdConductor) &&
                    (HoraInicio < h.HoraFin && HoraFin > h.HoraInicio)
                );

                if (existeConflicto)
                {
                    TempData["Mensaje"] = "Ya hay un horario asignado en el rango de fechas y horas asignadas";
                    ViewBag.Mensaje = "Ya hay un horario asignado en el rango de fechas y horas asignadas";
                    return RedirectToAction("Asignar_Horario", new { IdConductor = modelo.Conductor.IdConductor });
                }
            }

            // Si no hay conflictos, se crea la lista de horarios a guardar
            var horarios = new List<Horario>();
            for (var fecha = FechaInicio; fecha <= FechaFin; fecha = fecha.AddDays(1))
            {
                horarios.Add(new Horario
                {
                    Fecha = fecha,
                    HoraInicio = HoraInicio,
                    HoraFin = HoraFin,
                    IdVehiculo = IdVehiculo,
                    IdConductor = modelo.Conductor.IdConductor
                });
            }

            // Guardar los horarios
            bool respuesta = true;
            foreach (var horario in horarios)
            {
                respuesta &= await _horario.Guardar(horario, login);
            }

            if (respuesta)
            {
                Transaccion t = Crear_Transaccion("Guardar", "Horarios");
                bool guardar = await _transaccion.Guardar(t, login);

                TempData["Mensaje"] = "Horarios guardados correctamente.";
                ViewBag.Mensaje = "Horarios guardados correctamente.";
                return RedirectToAction("Ver_Horario", new { IdConductor = modelo.Conductor.IdConductor });
            }
            else
            {
                ViewBag.Mensaje = "No se pudo Guardar el Horario";
                TempData["Mensaje"] = "No se pudo Guardar  el Horario";
                return RedirectToAction("Asignar_Horario", new { IdConductor = modelo.Conductor.IdConductor });
            }
        }


        public async Task<IActionResult> Papelera()
        {
            var usuario = GetUsuarioFromSession();
            if (usuario == null)
            {
                ViewBag.Mensaje = "Usuario no autenticado.";
                return RedirectToAction("Login", "Inicio");
            }

            var login = CreateLogin(usuario);

            ModeloVista modeloTotal = new ModeloVista();
            modeloTotal.Conductores = await _conductor.Lista(login);
            modeloTotal.Propietarios = await _propietario.Lista(login);
            modeloTotal.Vehiculos = await _vehiculo.Lista(login);
            modeloTotal.Empresas = await _empresa.Lista(login);

            int IdEmpresa = modeloTotal.Empresas.Where(e => e.IdUsuario == usuario.IdUsuario).FirstOrDefault().IdEmpresa;


            ModeloVista modelo = new ModeloVista();
            modelo.Conductores = modeloTotal.Conductores.Where(item => item.Estado == false && item.IdEmpresa == IdEmpresa).ToList();
            modelo.Propietarios = modeloTotal.Propietarios.Where(item => item.Estado == false && item.IdEmpresa == IdEmpresa).ToList();
            modelo.Vehiculos = modeloTotal.Vehiculos.Where(item => item.Estado == false && item.IdEmpresa == IdEmpresa).ToList();

            List<Empresa> empresasTot = await _empresa.Lista(login);

            var empresa = empresasTot.Where(e => e.IdUsuario == usuario.IdUsuario).FirstOrDefault();
            ViewBag.Cupos = empresa.Cupos - await Cupos();
            return View(modelo);
        }

        [HttpPost]
        public async Task<IActionResult> Recuperar_Conductor(int IdConductor)
        {
            var usuario = GetUsuarioFromSession();
            if (usuario == null)
            {
                ViewBag.Mensaje = "Usuario no autenticado.";
                return RedirectToAction("Login", "Inicio");
            }

            var login = CreateLogin(usuario);
            var empresas = await _empresa.Lista(login);
            var empresa = empresas.FirstOrDefault(e => e.IdUsuario == usuario.IdUsuario);

            ViewBag.Cupos = empresa.Cupos - await Cupos();
            var conductor = await _conductor.Obtener(IdConductor, login);
            conductor.Estado = true;

            bool respuesta = await _conductor.Editar(conductor, login);

            if (respuesta)
            {
                Transaccion t = Crear_Transaccion("Recuperar", "Conductor");
                bool guardar = await _transaccion.Guardar(t, login);
                return RedirectToAction("Papelera");
            }
            else
            {
                ViewBag.Mensaje = "No se pudo Recuperar";
                return NoContent();
            }
        }

        [HttpPost]
        public async Task<IActionResult> Recuperar_Vehiculo(int IdVehiculo)
        {
            var usuario = GetUsuarioFromSession();
            if (usuario == null)
            {
                ViewBag.Mensaje = "Usuario no autenticado.";
                return RedirectToAction("Login", "Inicio");
            }

            var login = CreateLogin(usuario);
            var empresas = await _empresa.Lista(login);
            var empresa = empresas.FirstOrDefault(e => e.IdUsuario == usuario.IdUsuario);

            ViewBag.Cupos = empresa.Cupos - await Cupos();
            var vehiculo = await _vehiculo.Obtener(IdVehiculo, login);
            vehiculo.Estado = true;

            bool respuesta = await _vehiculo.Editar(vehiculo, login);

            if (respuesta)
            {
                Transaccion t = Crear_Transaccion("Recuperar", "Vehiculo");
                bool guardar = await _transaccion.Guardar(t, login);
                return RedirectToAction("Papelera");
            }
            else
            {
                ViewBag.Mensaje = "No se pudo Recuperar";
                return NoContent();
            }
        }

        [HttpPost]
        public async Task<IActionResult> Recuperar_Propietario(int IdPropietario)
        {
            var usuario = GetUsuarioFromSession();
            if (usuario == null)
            {
                ViewBag.Mensaje = "Usuario no autenticado.";
                return RedirectToAction("Login", "Inicio");
            }

            var login = CreateLogin(usuario);
            var empresas = await _empresa.Lista(login);
            var empresa = empresas.FirstOrDefault(e => e.IdUsuario == usuario.IdUsuario);

            ViewBag.Cupos = empresa.Cupos - await Cupos();
            var propietario = await _propietario.Obtener(IdPropietario, login);
            propietario.Estado = true;

            bool respuesta = await _propietario.Editar(propietario, login);

            if (respuesta)
            {
                Transaccion t = Crear_Transaccion("Recuperar", "Propietario");
                bool guardar = await _transaccion.Guardar(t, login);
                return RedirectToAction("Papelera");
            }
            else
            {
                ViewBag.Mensaje = "No se pudo Recuperar";
                return NoContent();
            }
        }

        public async Task<IActionResult> Horarios()
        {
            var usuario = GetUsuarioFromSession();
            if (usuario == null)
            {
                ViewBag.Mensaje = "Usuario no autenticado.";
                return RedirectToAction("Login", "Inicio");
            }

            var login = CreateLogin(usuario);

            var empresas = await _empresa.Lista(login);
            var empresa = empresas.FirstOrDefault(e => e.IdUsuario == usuario.IdUsuario);

            ViewBag.Cupos = empresa.Cupos - await Cupos();

            var conductoresTotales = await _conductor.Lista(login);
            var vehiculosTotales = await _vehiculo.Lista(login);

            ModeloVista modelo = new ModeloVista();
            modelo.Conductores = conductoresTotales.Where(i => i.IdEmpresa == empresa.IdEmpresa && i.Estado).ToList();
            modelo.Vehiculos = vehiculosTotales.Where(i => i.IdEmpresa == empresa.IdEmpresa && i.Estado).ToList();
            if (modelo.Conductores.Count() > 0)
            {

                int i = 0;
                while (true)
                {
                    modelo.Conductores[i].Contador = i + 1;
                    if (i == modelo.Conductores.Count() - 1)
                    {
                        break;
                    }
                    else
                    {
                        i++;
                    }
                }
            }

            if (modelo.Vehiculos.Count() > 0)
            {

                int i = 0;
                while (true)
                {
                    modelo.Vehiculos[i].Contador = i + 1;
                    if (i == modelo.Vehiculos.Count() - 1)
                    {
                        break;
                    }
                    else
                    {
                        i++;
                    }
                }
            }
            return View(modelo);
        }

                
        public async Task<IActionResult> Ver_Horario_Vehiculo(int IdVehiculo)
        {
            var usuario = GetUsuarioFromSession();
            if (usuario == null)
            {
                ViewBag.Mensaje = "Usuario no autenticado.";
                return RedirectToAction("Login", "Inicio");
            }

            var login = CreateLogin(usuario);
            ModeloVista modelo = new ModeloVista();
            modelo.Vehiculo = await _vehiculo.Obtener(IdVehiculo, login);
            var Horarios = await _horario.Lista(login);
            var conductoresTotales = await _conductor.Lista(login);
            

            modelo.Horarios = Horarios?.Where(h => h.IdVehiculo == IdVehiculo).ToList();

            List<Empresa> empresasTot = await _empresa.Lista(login);

            var empresa = empresasTot.Where(e => e.IdUsuario == usuario.IdUsuario).FirstOrDefault();

            modelo.Conductores = conductoresTotales.Where(c => c.IdEmpresa == empresa.IdEmpresa).ToList();
            ViewBag.Cupos = empresa.Cupos - await Cupos();

            return View(modelo);
        }

        public async Task<IActionResult> Crear_Usuario(int IdConductor)
        {
            var usuario = GetUsuarioFromSession();
            if (usuario == null)
            {
                ViewBag.Mensaje = "Usuario no autenticado.";
                return RedirectToAction("Login", "Inicio");
            }

            var login = CreateLogin(usuario);

            var conductor = await _conductor.Obtener(IdConductor,login);
            Usuario usuarioConductor = new Usuario();
            usuarioConductor.Correo = conductor.Correo;
            usuarioConductor.Contrasena = conductor.Contrasena;
            usuarioConductor.Foto = conductor.Foto;
            usuarioConductor.Nombre = conductor.Nombre;
            usuarioConductor.Telefono = conductor.Telefono;
            usuarioConductor.Direccion = conductor.Direccion;
            usuarioConductor.Ciudad = conductor.Ciudad;
            usuarioConductor.Celular = conductor.Celular;
            usuarioConductor.Estado = true;
            usuarioConductor.IdRol = 1006;

            ValidarModelo valida = new ValidarModelo();
            valida = ValidarModelos.validarUsuario(usuarioConductor);
            if(valida.Respuesta)
            {
                bool respuesta = await _usuario.Guardar(usuarioConductor, login);
                if (respuesta)
                {
                    TempData["Mensaje"] = "Usuario Creado";


                }
                else
                {
                    TempData["Mensaje"] = "Error al Crear Usuario";

                }
            }
            else
            {
                TempData["Mensaje"] = $"No se pudo Crear: {valida.Mensaje}";
            }
            
            return RedirectToAction("Detalle_Conductor", new { IdConductor = IdConductor });

        }

    }

}