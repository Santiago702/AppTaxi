
using AppTaxi.Models;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;

namespace AppTaxi.Servicios
{
    public class API_Conductor:Autenticacion,I_Conductor
    {
        private const string obtenerConductor = "api/Conductor/Obtener/";
        private const string listarConductor = "api/Conductor/Lista";
        private const string eliminarConductor = "api/Conductor/Eliminar/";
        private const string editarConductor = "api/Conductor/Editar/";
        private const string guardarConductor = "api/Conductor/Guardar/";

        public async Task<bool> Editar(Conductor conductor, Login login)
        {
            bool Respuesta = false;
            await Autenticar(login);

            var contenido = new StringContent(JsonConvert.SerializeObject(conductor), Encoding.UTF8, "application/json");

            var response = await _httpClient.PutAsync(editarConductor, contenido);

            if (response.IsSuccessStatusCode)
            {
                Respuesta = true;

            }
            return Respuesta;
        }

        public async Task<bool> Eliminar(int IdConductor, Login login)
        {
            if (IdConductor <= 0)
            {
                throw new ArgumentException("El ID del debe ser mayor que 0.", nameof(IdConductor));
            }
            bool Respuesta = false;
            await Autenticar(login);

         
            string ruta = eliminarConductor + IdConductor.ToString();
            var response = await _httpClient.DeleteAsync(ruta);

            if (response.IsSuccessStatusCode)
            {
                Respuesta = true;

            }
            return Respuesta;
        }

        public async Task<bool> Guardar(Conductor conductor, Login login)
        //public async Task<string> Guardar(Conductor conductor, Login login)
        {

            bool Respuesta = false;
            await Autenticar(login);

            var contenido = new StringContent(JsonConvert.SerializeObject(conductor), Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(guardarConductor,contenido);

            if (response.IsSuccessStatusCode)
            {
                Respuesta = true;

            }
            return Respuesta;
            //return response.ToString();
        }

        public async Task<List<Conductor>> Lista(Login login)
        {
            List<Conductor> lista = new List<Conductor>();
            await Autenticar(login);
            var response = await _httpClient.GetAsync(listarConductor);

            if (response.IsSuccessStatusCode)
            {
                var json_respuesta = await response.Content.ReadAsStringAsync();
                var resultado = JsonConvert.DeserializeObject<ResultadoApi<List<Conductor>>>(json_respuesta);
                lista = resultado.Response;

            }
            return lista;
        }

        public async Task<Conductor> Obtener(int IdConductor, Login login)
        {
            if (IdConductor <= 0)
            {
                throw new ArgumentException("El ID del debe ser mayor que 0.", nameof(IdConductor));
            }
            Conductor conductor = new Conductor();
            await Autenticar(login);

            string ruta = obtenerConductor + IdConductor.ToString();
            var response = await _httpClient.GetAsync(ruta);

            if (response.IsSuccessStatusCode)
            {
                var json_respuesta = await response.Content.ReadAsStringAsync();
                var resultado = JsonConvert.DeserializeObject<ResultadoApi<Conductor>>(json_respuesta);
                conductor = resultado.Response;

            }
            return conductor;
        }
    }
}
