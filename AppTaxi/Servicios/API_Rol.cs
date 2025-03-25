using AppTaxi.Models;
using Newtonsoft.Json;
using System.Text;

namespace AppTaxi.Servicios
{
    public class API_Rol : Autenticacion, I_Rol
    {
        private const string ListaRol = "api/Rol/Lista";
        private const string ObtenerRol = "api/Rol/Obtener/";
        private const string GuardarRol = "api/Rol/Guardar/";
        private const string EditarRol = "api/Rol/Editar/";
        private const string EliminarRol = "api/Rol/Eliminar/";

        public async Task<List<Rol>> Lista(Login login)
        {
            List<Rol> lista = new List<Rol>();
            await Autenticar(login);

            var response = await _httpClient.GetAsync(ListaRol);

            if (response.IsSuccessStatusCode)
            {
                var jsonRespuesta = await response.Content.ReadAsStringAsync();
                var resultado = JsonConvert.DeserializeObject<ResultadoApi<List<Rol>>>(jsonRespuesta);
                lista = resultado.Response;
            }
            return lista;
        }

        public async Task<Rol> Obtener(int IdRol, Login login)
        {
            if (IdRol <= 0)
            {
                throw new ArgumentException("El ID del rol debe ser mayor que 0.", nameof(IdRol));
            }

            Rol rol = new Rol();
            await Autenticar(login);

            string ruta = ObtenerRol + IdRol.ToString();
            var response = await _httpClient.GetAsync(ruta);

            if (response.IsSuccessStatusCode)
            {
                var jsonRespuesta = await response.Content.ReadAsStringAsync();
                var resultado = JsonConvert.DeserializeObject<ResultadoApi<Rol>>(jsonRespuesta);
                rol = resultado.Response;
            }
            return rol;
        }

        public async Task<bool> Guardar(Rol rol, Login login)
        {
            bool respuesta = false;
            await Autenticar(login);

            var contenido = new StringContent(JsonConvert.SerializeObject(rol), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(GuardarRol, contenido);

            if (response.IsSuccessStatusCode)
            {
                respuesta = true;
            }
            return respuesta;
        }

        public async Task<bool> Editar(Rol rol, Login login)
        {
            bool respuesta = false;
            await Autenticar(login);

            var contenido = new StringContent(JsonConvert.SerializeObject(rol), Encoding.UTF8, "application/json");
            var response = await _httpClient.PutAsync(EditarRol, contenido);

            if (response.IsSuccessStatusCode)
            {
                respuesta = true;
            }
            return respuesta;
        }

        public async Task<bool> Eliminar(int IdRol, Login login)
        {
            if (IdRol <= 0)
            {
                throw new ArgumentException("El ID del rol debe ser mayor que 0.", nameof(IdRol));
            }

            bool respuesta = false;
            await Autenticar(login);

            string ruta = EliminarRol + IdRol.ToString();
            var response = await _httpClient.DeleteAsync(ruta);

            if (response.IsSuccessStatusCode)
            {
                respuesta = true;
            }
            return respuesta;
        }
    }
}
