using AppTaxi.Models;
using Newtonsoft.Json;
using System.Text;

namespace AppTaxi.Servicios
{
    public class API_Transaccion : Autenticacion, I_Transaccion
    {
        private const string ListaTransaccion = "api/Transaccion/Lista";
        private const string ObtenerTransaccion = "api/Transaccion/Obtener/";
        private const string GuardarTransaccion = "api/Transaccion/Guardar/";
        private const string EditarTransaccion = "api/Transaccion/Editar/";
        private const string EliminarTransaccion = "api/Transaccion/Eliminar/";

        public async Task<List<Transaccion>> Lista(Login login)
        {
            List<Transaccion> lista = new List<Transaccion>();
            await Autenticar(login);

            var response = await _httpClient.GetAsync(ListaTransaccion);

            if (response.IsSuccessStatusCode)
            {
                var jsonRespuesta = await response.Content.ReadAsStringAsync();
                var resultado = JsonConvert.DeserializeObject<ResultadoApi<List<Transaccion>>>(jsonRespuesta);
                lista = resultado.Response;
            }
            return lista;
        }

        public async Task<Transaccion> Obtener(int IdTransaccion, Login login)
        {
            if (IdTransaccion <= 0)
            {
                throw new ArgumentException("El ID de la transacción debe ser mayor que 0.", nameof(IdTransaccion));
            }

            Transaccion transaccion = new Transaccion();
            await Autenticar(login);

            string ruta = ObtenerTransaccion + IdTransaccion.ToString();
            var response = await _httpClient.GetAsync(ruta);

            if (response.IsSuccessStatusCode)
            {
                var jsonRespuesta = await response.Content.ReadAsStringAsync();
                var resultado = JsonConvert.DeserializeObject<ResultadoApi<Transaccion>>(jsonRespuesta);
                transaccion = resultado.Response;
            }
            return transaccion;
        }

        public async Task<bool> Guardar(Transaccion transaccion, Login login)
        {
            bool respuesta = false;
            await Autenticar(login);

            var contenido = new StringContent(JsonConvert.SerializeObject(transaccion), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(GuardarTransaccion, contenido);

            if (response.IsSuccessStatusCode)
            {
                respuesta = true;
            }
            return respuesta;
        }

        public async Task<bool> Editar(Transaccion transaccion, Login login)
        {
            bool respuesta = false;
            await Autenticar(login);

            var contenido = new StringContent(JsonConvert.SerializeObject(transaccion), Encoding.UTF8, "application/json");
            var response = await _httpClient.PutAsync(EditarTransaccion, contenido);

            if (response.IsSuccessStatusCode)
            {
                respuesta = true;
            }
            return respuesta;
        }

        public async Task<bool> Eliminar(int IdTransaccion, Login login)
        {
            if (IdTransaccion <= 0)
            {
                throw new ArgumentException("El ID de la transacción debe ser mayor que 0.", nameof(IdTransaccion));
            }

            bool respuesta = false;
            await Autenticar(login);

            string ruta = EliminarTransaccion + IdTransaccion.ToString();
            var response = await _httpClient.DeleteAsync(ruta);

            if (response.IsSuccessStatusCode)
            {
                respuesta = true;
            }
            return respuesta;
        }
    }
}
