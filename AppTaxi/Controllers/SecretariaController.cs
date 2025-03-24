using AppTaxi.Funciones;
using AppTaxi.Models;
using AppTaxi.Servicios;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;
using Newtonsoft.Json;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Drawing;
using System.Transactions;

namespace AppTaxi.Controllers
{
    public class SecretariaController : Controller
    {
        // Servicios inyectados para manejar las operaciones de vehículos, horarios, propietarios, empresas y conductores.
        private readonly I_Vehiculo _vehiculo;
        private readonly I_Horario _horario;
        private readonly I_Propietario _propietario;
        private readonly I_Empresa _empresa;
        private readonly I_Conductor _conductor;
        private readonly I_Usuario _usuario;
        private readonly I_Transaccion _transaccion;

        // Constructor que recibe las dependencias inyectadas.
        public SecretariaController(I_Vehiculo vehiculo, I_Horario horario, I_Propietario propietario, I_Empresa empresa, I_Conductor conductor, I_Usuario usuario, I_Transaccion transaccion)
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
        public async Task<IActionResult> Inicio()
        {
            var usuario = GetUsuarioFromSession();
            if (usuario == null)
            {
                ViewBag.Mensaje = "Usuario no autenticado.";
                return View();
            }

            var login = CreateLogin(usuario);
            
            var usuarios = await _usuario.Lista(login);
            
            return View(usuario);
        }
        public async Task<IActionResult> Empresas()
        {
            var usuario = GetUsuarioFromSession();
            if (usuario == null)
            {
                ViewBag.Mensaje = "Usuario no autenticado.";
                return View();
            }

            var login = CreateLogin(usuario);

            // Obtener la lista de empresas
            var empresasTotales = await _empresa.Lista(login);

            // Crear lista de tareas para obtener los usuarios en paralelo
            var tareasUsuarios = empresasTotales.Select(emp => _usuario.Obtener(emp.IdUsuario, login));

            // Ejecutar todas las llamadas en paralelo
            var usuarios = await Task.WhenAll(tareasUsuarios);

            // Construir el modelo con los resultados
            ModeloVista modelo = new ModeloVista
            {
                Empresas = empresasTotales,
                Usuarios = usuarios.ToList()
            };

            int i = 0;
            while (true)
            {
                modelo.Empresas[i].Contador = i + 1 ;
                if (i == modelo.Empresas.Count() - 1)
                {
                    break; 
                }
                else
                {
                    i++;
                }
            }


            return View(modelo);
        }

        public async Task<IActionResult> Detalle_Empresa(int IdEmpresa)
        {
            var usuario = GetUsuarioFromSession();
            if (usuario == null)
            {
                ViewBag.Mensaje = "Usuario no autenticado.";
                return View();
            }

            var login = CreateLogin(usuario);

            ModeloVista modelo = new ModeloVista();
            modelo.Empresa = await _empresa.Obtener(IdEmpresa, login);

            modelo.Usuario = await _usuario.Obtener(modelo.Empresa.IdUsuario, login);
            
            return View(modelo);
        }

        //----------------------------- Vista de Objetos registrados por Empresa
        public async Task<IActionResult> Conductores(int IdEmpresa)
        {
            var usuario = GetUsuarioFromSession();
            if (usuario == null)
            {
                ViewBag.Mensaje = "Usuario no autenticado.";
                return View();
            }

            var login = CreateLogin(usuario);
            ModeloVista modelo = new ModeloVista();

            var conductoresTotales = await _conductor.Lista(login);
            var conductoresEmpresa = conductoresTotales?.Where(c => c.IdEmpresa == IdEmpresa).ToList();
            if(conductoresEmpresa.Count > 0)
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
            modelo.Conductores = conductoresEmpresa;
            modelo.Empresa = await _empresa.Obtener(IdEmpresa, login);
            return View(modelo);
        }

        public async Task<IActionResult> Propietarios (int IdEmpresa)
        {
            var usuario = GetUsuarioFromSession();
            if (usuario == null)
            {
                ViewBag.Mensaje = "Usuario no autenticado.";
                return View();
            }

            var login = CreateLogin(usuario);

            var propietariosTotales = await _propietario.Lista(login);
            var propietariosEmpresa = propietariosTotales?.Where(p => p.IdEmpresa == IdEmpresa).ToList();
            ModeloVista modelo = new ModeloVista();
            if(propietariosEmpresa.Count() > 0)
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
                       
            modelo.Propietarios = propietariosEmpresa;
            modelo.Empresa = await _empresa.Obtener(IdEmpresa, login);
            return View(modelo);
        }

        public async Task<IActionResult> Vehiculos(int IdEmpresa)
        {
            var usuario = GetUsuarioFromSession();
            if (usuario == null)
            {
                ViewBag.Mensaje = "Usuario no autenticado.";
                return View();
            }

            var login = CreateLogin(usuario);

            var vehiculosTotales = await _vehiculo.Lista(login);
            var vehiculosEmpresa = vehiculosTotales?.Where(p => p.IdEmpresa == IdEmpresa).ToList();
            ModeloVista modelo = new ModeloVista();
            if(vehiculosEmpresa.Count() > 0)
            {

                int i = 0;
                while (true)
                {
                    vehiculosEmpresa[i].Contador = i + 1;
                    if (i == vehiculosEmpresa.Count() - 1)
                    {
                        break;
                    }
                    else
                    {
                        i++;
                    }
                }
            }
            
            modelo.Vehiculos = vehiculosEmpresa;
            modelo.Empresa = await _empresa.Obtener(IdEmpresa, login);
            return View(modelo);
        }


        public async Task<IActionResult> Detalle_Conductor(int IdConductor)
        {
            var usuario = GetUsuarioFromSession();
            if (usuario == null)
            {
                ViewBag.Mensaje = "Usuario no autenticado.";
                return View();
            }

            var login = CreateLogin(usuario);
            var conductor = await _conductor.Obtener(IdConductor, login);

            return View(conductor);
        }

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

            return View(modelo);
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

            // Obtiene el vehículo y el propietario asociado.
            var vehiculo = await _vehiculo.Obtener(IdVehiculo, login);
            var propietario = await _propietario.Obtener(vehiculo.IdPropietario, login);

            ViewBag.Propietario = propietario?.Nombre;
            return View(vehiculo);
        }

        public async Task<IActionResult> Ver_Horario(int IdConductor)
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


            return View(modelo);
        }


        public async Task<IActionResult> Vista_Agregar_Empresa()
        {
            var usuario = GetUsuarioFromSession();
            if (usuario == null)
            {
                ViewBag.Mensaje = "Usuario no autenticado.";
                return RedirectToAction("Login", "Inicio");
            }

            var login = CreateLogin(usuario);
            ModeloVista modelo = new ModeloVista();

            var usuariosTotales = await _usuario.Lista(login);
            var empresasRegistradas = await _empresa.Lista(login);

            // Filtrar usuarios que no están en empresasRegistradas y que tengan IdRol = 1
            modelo.Usuarios = usuariosTotales
                .Where(u => u.IdRol == 1 && !empresasRegistradas.Any(e => e.IdUsuario == u.IdUsuario))
                .ToList();

            return View(modelo);
        }

        public async Task<IActionResult> Editar_Empresa(int IdEmpresa)
        {
            var usuario = GetUsuarioFromSession();
            if (usuario == null)
            {
                ViewBag.Mensaje = "Usuario no autenticado.";
                return RedirectToAction("Login", "Inicio");
            }

            var login = CreateLogin(usuario);
            
            ModeloVista modelo = new ModeloVista();
            
            var usuariosTotales = await _usuario.Lista(login);
            var empresasRegistradas = await _empresa.Lista(login);
            var empresa = await _empresa.Obtener(IdEmpresa, login);
            var usuarioEmp = await _usuario.Obtener(empresa.IdUsuario, login);

            // Filtrar usuarios que no están en empresasRegistradas y que tengan IdRol = 1
           
            modelo.Usuario = usuarioEmp;
            modelo.Empresa = empresa;
            modelo.Empresas = empresasRegistradas;
            modelo.Usuarios = usuariosTotales
                .Where(u => u.IdRol == 1 && !empresasRegistradas.Any(e => e.IdUsuario == u.IdUsuario))
                .ToList();



            return View(modelo);
        }


        [HttpPost]
        public async Task<IActionResult> Guardar_Empresa(ModeloVista modelo)
        {
            
            var usuario = GetUsuarioFromSession();
            if (usuario == null)
            {
                ViewBag.Mensaje = "Usuario no autenticado.";
                return RedirectToAction("Login", "Inicio");
            }

            var login = CreateLogin(usuario);

            ValidarModelo val = new ValidarModelo();
            val = ValidarModelos.validarEmpresa(modelo.Empresa);

            if(val.Respuesta)
            {
                bool respuesta = await _empresa.Editar(modelo.Empresa, login);

                if (respuesta)
                {
                    Transaccion t = Crear_Transaccion("Editar", "Empresa");
                    bool guardar = await _transaccion.Guardar(t, login);
                    return RedirectToAction("Empresas");
                }
                else
                {
                    ViewBag.Mensaje = "No se pudo guardar";
                    TempData["Mensaje"] = "No se pudo guardar";
                    return RedirectToAction("Editar_Empresa", new { IdEmpresa = modelo.Empresa.IdEmpresa });
                }
            }
            else
            {
                TempData["Mensaje"] = val.Mensaje;
                return RedirectToAction("Editar_Empresa", new { IdEmpresa = modelo.Empresa.IdEmpresa });
            }
            

            
        }


        [HttpPost]
        public async Task<IActionResult> Agregar_Empresa(ModeloVista modelo)
        {
            
            var usuario = GetUsuarioFromSession();
            if (usuario == null)
            {
                ViewBag.Mensaje = "Usuario no autenticado.";
                return RedirectToAction("Login", "Inicio");
            }

            var login = CreateLogin(usuario);

            var empresas = await _empresa.Lista(login);
            if(empresas.Any(e => e.Nit == modelo.Empresa.Nit || e.Nombre == modelo.Empresa.Nombre))
            {
                TempData["Mensaje"] = "La empresa ya está registrada";
                return RedirectToAction("Vista_Agregar_Empresa");
            }
            Empresa empresa = modelo.Empresa;

            ValidarModelo val = new ValidarModelo();
            val = ValidarModelos.validarEmpresa(modelo.Empresa);

            if (val.Respuesta)
            {
                bool respuesta = await _empresa.Guardar(empresa, login);

                if (respuesta)
                {
                    Transaccion t = Crear_Transaccion("Guardar", "Empresa");
                    bool guardar = await _transaccion.Guardar(t, login);
                    TempData["Mensaje"] = "Editado Correctamente";
                    return RedirectToAction("Empresas", "Secretaria");
                }
                else
                {
                    TempData["Mensaje"] = "No se pudo Guardar";
                    return RedirectToAction("Vista_Agregar_Empresa");
                }
            }
            else
            {
                TempData["Mensaje"] = val.Mensaje;
                return RedirectToAction("Vista_Agregar_Empresa");
            }
            
        }


        public IActionResult Reporte()
        {
            return View();
        }

        public async Task<IActionResult> Generar_Reporte(ReporteSeleccion campos)
        {
            // Obtener el usuario autenticado (ajusta según tu lógica de login)
            var usuario = GetUsuarioFromSession();
            if (usuario == null)
            {
                ViewBag.Mensaje = "Usuario no autenticado.";
                return RedirectToAction("Login", "Inicio");
            }

            var login = CreateLogin(usuario);
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            var stream = new MemoryStream();


            using (var excelPackage = new ExcelPackage())
            {

                // Procesar cada modelo según los campos seleccionados
                await ProcesarModelo<Conductor>(excelPackage, campos, login, "Conductor");
                await ProcesarModelo<Empresa>(excelPackage, campos, login, "Empresa");
                await ProcesarModelo<Horario>(excelPackage, campos, login, "Horario");
                await ProcesarModelo<Transaccion>(excelPackage, campos, login, "Transaccion");
                await ProcesarModelo<Vehiculo>(excelPackage, campos, login, "Vehiculo");
                await ProcesarModelo<Usuario>(excelPackage, campos, login, "Usuario");
                await ProcesarModelo<Propietario>(excelPackage, campos, login, "Propietario");

                // Generar el archivo Excel
                
                excelPackage.SaveAs(stream);
                
            }
            stream.Position = 0;

            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Reporte.xlsx");
        }

        

        public async Task<IActionResult> Usuarios()
        {
            var usuario = GetUsuarioFromSession();
            if (usuario == null)
            {
                ViewBag.Mensaje = "Usuario no autenticado.";
                return RedirectToAction("Login", "Inicio");
            }

            var login = CreateLogin(usuario);
            ModeloVista modelo = new ModeloVista();

            var usuarios = await _usuario.Lista(login);
            var empresas = await _empresa.Lista(login);
            modelo.Usuarios = usuarios.Where(u => u.IdRol == 1).ToList();

            modelo.Empresas = empresas;
            if (modelo.Usuarios.Count() > 0)
            {

                int i = 0;
                while (true)
                {
                    modelo.Usuarios[i].Contador = i + 1;
                    if (i == modelo.Usuarios.Count() - 1)
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

        public IActionResult Agregar_Usuario()
        {
            var usuario = GetUsuarioFromSession();
            if (usuario == null)
            {
                ViewBag.Mensaje = "Usuario no autenticado.";
                return RedirectToAction("Login", "Inicio");
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Crear_Usuario(ModeloVista modelo)
        {
            
            var usuario = GetUsuarioFromSession();
            if (usuario == null)
            {
                ViewBag.Mensaje = "Usuario no autenticado.";
                return RedirectToAction("Login", "Inicio");
            }
            var login = CreateLogin(usuario);

            var usuarios = await _usuario.Lista(login);

            if(usuarios.Any(u => u.Nombre == modelo.Usuario.Nombre || u.Correo == modelo.Usuario.Correo))
            {
                TempData["Mensaje"] = "Usuario ya existe";
                return View("Agregar_Usuario");
            }
            modelo.Usuario.IdRol = 1;
            if (modelo.Archivo_4 != null)
            {
                using (var ms = new MemoryStream())
                {
                    await modelo.Archivo_4.CopyToAsync(ms);
                    modelo.Usuario.Foto = Convert.ToBase64String(ms.ToArray());
                }
            }
            else
            {
                modelo.Usuario.Foto = "N/F";
            }

            modelo.Usuario.Estado = true;
            ValidarModelo valida = new ValidarModelo();
            valida = ValidarModelos.validarUsuario(modelo.Usuario);
            if (valida.Respuesta)
            {
                bool respuesta = await _usuario.Guardar(modelo.Usuario, login);

                if (respuesta)
                {
                    Transaccion t = Crear_Transaccion("Guardar", "Usuario");
                    bool guardar = await _transaccion.Guardar(t, login);
                    TempData["Mensaje"] = "Guardado Correctamente";
                    return RedirectToAction("Usuarios", "Secretaria");
                }
                else
                {
                    TempData["Mensaje"] = "No se Pudo Guardar";
                    return View("Agregar_Usuario");
                }
            }
            else
            {
                TempData["Mensaje"] = valida.Mensaje;
                return View("Agregar_Usuario");
            }
            

        }
        [HttpPost]
        public async Task<IActionResult> Deshabilitar_Usuario(int IdUsuario)
        {
            if (!ModelState.IsValid)
            {
                TempData["Mensaje"] = "Error con el modelo";
                return RedirectToAction("Inicio");
            }
            var usuario = GetUsuarioFromSession();
            if (usuario == null)
            {
                TempData["Mensaje"] = "Usuario no autenticado.";
                return RedirectToAction("Login", "Inicio");
            }

            var login = CreateLogin(usuario);
            Usuario user = await _usuario.Obtener(IdUsuario, login);
            user.Estado = false;
            bool respuesta = await _usuario.Editar(user, login);
            if(respuesta)
            {
                Transaccion t = Crear_Transaccion("Editar", "Usuario");
                bool guardar = await _transaccion.Guardar(t, login);
                TempData["Mensaje"] = "Editado Correctamente";
                return RedirectToAction("Usuarios", "Secretaria");
            }
            else
            {
                TempData["Mensaje"] = "No se Pudo Guardar";
                return View("Agregar_Usuario");
            }
        }
        [HttpPost]
        public async Task<IActionResult> Habilitar_Usuario(int IdUsuario)
        {
            if (!ModelState.IsValid)
            {
                TempData["Mensaje"] = "Error con el modelo";
                return RedirectToAction("Inicio");
            }
            var usuario = GetUsuarioFromSession();
            if (usuario == null)
            {
                TempData["Mensaje"] = "Usuario no autenticado.";
                return RedirectToAction("Login", "Inicio");
            }

            var login = CreateLogin(usuario);
            Usuario user = await _usuario.Obtener(IdUsuario, login);
            user.Estado = true;
            bool respuesta = await _usuario.Editar(user, login);
            if (respuesta)
            {
                Transaccion t = Crear_Transaccion("Editar", "Usuario");
                bool guardar = await _transaccion.Guardar(t, login);
                TempData["Mensaje"] = "Editado Correctamente";
                return RedirectToAction("Usuarios", "Secretaria");
            }
            else
            {
                TempData["Mensaje"] = "No se Pudo Guardar";
                return View("Agregar_Usuario");
            }
        }

        public async Task<IActionResult> Editar_Usuario(int IdUsuario)
        {
            var usuario = GetUsuarioFromSession();
            if (usuario == null)
            {
                ViewBag.Mensaje = "Usuario no autenticado.";
                return RedirectToAction("Login", "Inicio");
            }

            var login = CreateLogin(usuario);
            var usuarioEditar = await _usuario.Obtener(IdUsuario, login);
            ModeloVista modelo = new ModeloVista();
            modelo.Usuario = usuarioEditar;
            return View(modelo);
        }
        [HttpPost]
        public async Task<IActionResult> Guardar_Usuario(ModeloVista modelo)
        {
            
            var usuario = GetUsuarioFromSession();
            if (usuario == null)
            {
                ViewBag.Mensaje = "Usuario no autenticado.";
                return RedirectToAction("Login", "Inicio");
            }

            var login = CreateLogin(usuario);

            if (modelo.Archivo_4 != null)
            {
                using (var ms = new MemoryStream())
                {
                    await modelo.Archivo_4.CopyToAsync(ms);
                    modelo.Usuario.Foto = Convert.ToBase64String(ms.ToArray());
                }
            }
            

            modelo.Usuario.Estado = true;
            ValidarModelo val = new ValidarModelo();
            val = ValidarModelos.validarUsuario(modelo.Usuario);
            if(val.Respuesta)
            {
                bool respuesta = await _usuario.Editar(modelo.Usuario, login);

                if (respuesta)
                {
                    Transaccion t = Crear_Transaccion("Editar", "Usuario");
                    bool guardar = await _transaccion.Guardar(t, login);
                    TempData["Mensaje"] = "Editado Correctamente";
                    return RedirectToAction("Usuarios", "Secretaria");
                }
                else
                {
                    TempData["Mensaje"] = "No se Pudo Editar";
                    return View("Agregar_Usuario");
                }
            }
            else
            {
                TempData["Mensaje"] = val.Mensaje;
                return View("Agregar_Usuario");
            }
            
        }
        public async Task<IActionResult> Transacciones()
        {
            var usuario = GetUsuarioFromSession();
            if (usuario == null)
            {
                ViewBag.Mensaje = "Usuario no autenticado.";
                return RedirectToAction("Login", "Inicio");
            }

            var login = CreateLogin(usuario);

            var transacciones = await _transaccion.Lista(login);
            var usuarios = await _usuario.Lista(login);
            var empresas = await _empresa.Lista(login);

            if (transacciones.Count() > 0)
            {

                int i = 0;
                while (true)
                {
                    transacciones[i].Contador = i + 1;
                    if (i == transacciones.Count() - 1)
                    {
                        break;
                    }
                    else
                    {
                        i++;
                    }
                }
            }

            ModeloVista modelo = new ModeloVista();
            modelo.Transacciones = transacciones;
            modelo.Usuarios = usuarios;
            modelo.Empresas = empresas;
            return View(modelo);
        }

        private async Task ProcesarModelo<T>(ExcelPackage excelPackage, ReporteSeleccion campos, Models.Login login, string nombreModelo) where T : class
        {
            var propiedadesSeleccionadas = ObtenerPropiedadesSeleccionadas(campos, nombreModelo);
            if (propiedadesSeleccionadas.Count == 0) return;

            var datos = await ObtenerDatosDesdeServicio<T>(login,nombreModelo);
            if (datos == null || datos.Count == 0) return;

            var worksheet = excelPackage.Workbook.Worksheets.Add(nombreModelo);

            // Encabezados
            for (int i = 0; i < propiedadesSeleccionadas.Count; i++)
            {

                worksheet.Cells[1, i + 1].Value = propiedadesSeleccionadas[i];

            }
            using (var range = worksheet.Cells["1:1"])
            {
                worksheet.View.FreezePanes(2, 1);
                range.Style.Font.Bold = true;
                range.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                range.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                range.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                range.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                range.Style.Fill.BackgroundColor.SetColor(Color.LightBlue);

            }

            // Datos
            for (int row = 0; row < datos.Count; row++)
            {
                var item = datos[row];
                for (int col = 0; col < propiedadesSeleccionadas.Count; col++)
                {
                    var propiedad = typeof(T).GetProperty(propiedadesSeleccionadas[col]);
                    var valor = propiedad?.GetValue(item);

                    // Manejar valores nulos y fechas
                    worksheet.Cells[row + 2, col + 1].Value = valor switch
                    {
                        DateTime date => date.ToString("yyyy-MM-dd"),
                        TimeSpan time => time.ToString(@"hh\:mm"),
                        _ => valor?.ToString() ?? ""
                    };
                }
            }

            // Ajustar ancho de columnas
            worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
        }


        private List<string> ObtenerPropiedadesSeleccionadas(ReporteSeleccion campos, string nombreModelo)
        {
            return campos.GetType().GetProperties()
                .Where(p => p.Name.EndsWith(nombreModelo)
                            && p.PropertyType == typeof(bool)
                            && (bool)p.GetValue(campos))
                .Select(p =>
                {
                    if (p.Name.Equals("Id" + nombreModelo))
                        return p.Name;
                    
                    else
                        return p.Name.Replace(nombreModelo, ""); // Ej: "NombreConductor" → "Nombre"
                })
                .ToList();
        }

        private async Task<List<T>> ObtenerDatosDesdeServicio<T>(Models.Login login, string nombreModelo)
        {
            switch (nombreModelo)
            {
                case "Conductor":
                    return (await _conductor.Lista(login)).Cast<T>().ToList();
                case "Empresa":
                    return (await _empresa.Lista(login)).Cast<T>().ToList();
                case "Horario":
                    return (await _horario.Lista(login)).Cast<T>().ToList();
                case "Transaccion":
                    return (await _transaccion.Lista(login)).Cast<T>().ToList();
                case "Vehiculo":
                    return (await _vehiculo.Lista(login)).Cast<T>().ToList();
                case "Usuario":
                    return (await _usuario.Lista(login)).Cast<T>().ToList();
                case "Propietario":
                    return (await _propietario.Lista(login)).Cast<T>().ToList();
                default:
                    return null;
            }
        }

        
    }
}
