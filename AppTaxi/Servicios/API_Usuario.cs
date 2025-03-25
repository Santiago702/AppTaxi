using AppTaxi.Models;
using Newtonsoft.Json;
using System.Text;

namespace AppTaxi.Servicios
{
    public class API_Usuario : Autenticacion, I_Usuario
    {
        private const string ListaUsuario = "api/Usuario/Lista";
        private const string ObtenerUsuario = "api/Usuario/Obtener/";
        private const string GuardarUsuario = "api/Usuario/Guardar/";
        private const string EditarUsuario = "api/Usuario/Editar/";
        private const string EliminarUsuario = "api/Usuario/Eliminar/";

        public async Task<List<Usuario>> Lista(Login login)
        {
            List<Usuario> lista = new List<Usuario>();
            await Autenticar(login);

            var response = await _httpClient.GetAsync(ListaUsuario);

            if (response.IsSuccessStatusCode)
            {
                var jsonRespuesta = await response.Content.ReadAsStringAsync();
                var resultado = JsonConvert.DeserializeObject<ResultadoApi<List<Usuario>>>(jsonRespuesta);
                lista = resultado.Response;
            }
            return lista;
        }

        public async Task<Usuario> Obtener(int IdUsuario, Login login)
        {
            if (IdUsuario <= 0)
            {
                throw new ArgumentException("El ID del usuario debe ser mayor que 0.", nameof(IdUsuario));
            }

            Usuario usuario = new Usuario();
            await Autenticar(login);

            string ruta = ObtenerUsuario + IdUsuario.ToString();
            var response = await _httpClient.GetAsync(ruta);

            if (response.IsSuccessStatusCode)
            {
                var jsonRespuesta = await response.Content.ReadAsStringAsync();
                var resultado = JsonConvert.DeserializeObject<ResultadoApi<Usuario>>(jsonRespuesta);
                usuario = resultado.Response;
            }
            return usuario;
        }

        public async Task<bool> Guardar(Usuario usuario, Login login)
        {
            bool respuesta = false;
            await Autenticar(login);

            var contenido = new StringContent(JsonConvert.SerializeObject(usuario), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(GuardarUsuario, contenido);

            if (response.IsSuccessStatusCode)
            {
                respuesta = true;
            }
            return respuesta;
        }

        public async Task<bool> Editar(Usuario usuario, Login login)
        {
            bool respuesta = false;
            await Autenticar(login);

            var contenido = new StringContent(JsonConvert.SerializeObject(usuario), Encoding.UTF8, "application/json");
            var response = await _httpClient.PutAsync(EditarUsuario, contenido);

            if (response.IsSuccessStatusCode)
            {
                respuesta = true;
            }
            return respuesta;
        }

        public async Task<bool> Eliminar(int IdUsuario, Login login)
        {
            if (IdUsuario <= 0)
            {
                throw new ArgumentException("El ID del usuario debe ser mayor que 0.", nameof(IdUsuario));
            }

            bool respuesta = false;
            await Autenticar(login);

            string ruta = EliminarUsuario + IdUsuario.ToString();
            var response = await _httpClient.DeleteAsync(ruta);

            if (response.IsSuccessStatusCode)
            {
                respuesta = true;
            }
            return respuesta;
        }
    }
}
