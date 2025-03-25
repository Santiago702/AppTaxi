using AppTaxi.Models;
using Newtonsoft.Json;
using System.Text;

namespace AppTaxi.Servicios
{
    public class API_Propietario : Autenticacion, I_Propietario
    {
        private const string ListaPropietario = "api/Propietario/Lista";
        private const string ObtenerPropietario = "api/Propietario/Obtener/";
        private const string GuardarPropietario = "api/Propietario/Guardar/";
        private const string EditarPropietario = "api/Propietario/Editar/";
        private const string EliminarPropietario = "api/Propietario/Eliminar/";

        public async Task<List<Propietario>> Lista(Login login)
        {
            List<Propietario> lista = new List<Propietario>();
            await Autenticar(login);

            var response = await _httpClient.GetAsync(ListaPropietario);

            if (response.IsSuccessStatusCode)
            {
                var jsonRespuesta = await response.Content.ReadAsStringAsync();
                var resultado = JsonConvert.DeserializeObject<ResultadoApi<List<Propietario>>>(jsonRespuesta);
                lista = resultado.Response;
            }
            return lista;
        }

        public async Task<Propietario> Obtener(int IdPropietario, Login login)
        {
            if (IdPropietario <= 0)
            {
                throw new ArgumentException("El ID del propietario debe ser mayor que 0.", nameof(IdPropietario));
            }

            Propietario propietario = new Propietario();
            await Autenticar(login);

            string ruta = ObtenerPropietario + IdPropietario.ToString();
            var response = await _httpClient.GetAsync(ruta);

            if (response.IsSuccessStatusCode)
            {
                var jsonRespuesta = await response.Content.ReadAsStringAsync();
                var resultado = JsonConvert.DeserializeObject<ResultadoApi<Propietario>>(jsonRespuesta);
                propietario = resultado.Response;
            }
            return propietario;
        }

        public async Task<bool> Guardar(Propietario propietario, Login login)
        {
            bool respuesta = false;
            await Autenticar(login);

            var contenido = new StringContent(JsonConvert.SerializeObject(propietario), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(GuardarPropietario, contenido);

            if (response.IsSuccessStatusCode)
            {
                respuesta = true;
            }
            return respuesta;
        }

        public async Task<bool> Editar(Propietario propietario, Login login)
        {
            bool respuesta = false;
            await Autenticar(login);

            var contenido = new StringContent(JsonConvert.SerializeObject(propietario), Encoding.UTF8, "application/json");
            var response = await _httpClient.PutAsync(EditarPropietario, contenido);

            if (response.IsSuccessStatusCode)
            {
                respuesta = true;
            }
            return respuesta;
        }

        public async Task<bool> Eliminar(int IdPropietario, Login login)
        {
            if (IdPropietario <= 0)
            {
                throw new ArgumentException("El ID del propietario debe ser mayor que 0.", nameof(IdPropietario));
            }

            bool respuesta = false;
            await Autenticar(login);

            string ruta = EliminarPropietario + IdPropietario.ToString();
            var response = await _httpClient.DeleteAsync(ruta);

            if (response.IsSuccessStatusCode)
            {
                respuesta = true;
            }
            return respuesta;
        }
    }
}
