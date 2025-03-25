using AppTaxi.Models;
using Newtonsoft.Json;
using System.Text;

namespace AppTaxi.Servicios
{
    public class API_Vehiculo : Autenticacion, I_Vehiculo
    {
        private const string ListaVehiculo = "api/Vehiculo/Lista";
        private const string ObtenerVehiculo = "api/Vehiculo/Obtener/";
        private const string GuardarVehiculo = "api/Vehiculo/Guardar/";
        private const string EditarVehiculo = "api/Vehiculo/Editar/";
        private const string EliminarVehiculo = "api/Vehiculo/Eliminar/";

        public async Task<List<Vehiculo>> Lista(Login login)
        {
            List<Vehiculo> lista = new List<Vehiculo>();
            await Autenticar(login);

            var response = await _httpClient.GetAsync(ListaVehiculo);

            if (response.IsSuccessStatusCode)
            {
                var jsonRespuesta = await response.Content.ReadAsStringAsync();
                var resultado = JsonConvert.DeserializeObject<ResultadoApi<List<Vehiculo>>>(jsonRespuesta);
                lista = resultado.Response;
            }
            return lista;
        }

        public async Task<Vehiculo> Obtener(int IdVehiculo, Login login)
        {
            if (IdVehiculo <= 0)
            {
                throw new ArgumentException("El ID del vehículo debe ser mayor que 0.", nameof(IdVehiculo));
            }

            Vehiculo vehiculo = new Vehiculo();
            await Autenticar(login);

            string ruta = ObtenerVehiculo + IdVehiculo.ToString();
            var response = await _httpClient.GetAsync(ruta);

            if (response.IsSuccessStatusCode)
            {
                var jsonRespuesta = await response.Content.ReadAsStringAsync();
                var resultado = JsonConvert.DeserializeObject<ResultadoApi<Vehiculo>>(jsonRespuesta);
                vehiculo = resultado.Response;
            }
            return vehiculo;
        }

        public async Task<bool> Guardar(Vehiculo vehiculo, Login login)
        {
            bool respuesta = false;
            await Autenticar(login);

            var contenido = new StringContent(JsonConvert.SerializeObject(vehiculo), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(GuardarVehiculo, contenido);

            if (response.IsSuccessStatusCode)
            {
                respuesta = true;
            }
            return respuesta;
        }

        public async Task<bool> Editar(Vehiculo vehiculo, Login login)
        {
            bool respuesta = false;
            await Autenticar(login);

            var contenido = new StringContent(JsonConvert.SerializeObject(vehiculo), Encoding.UTF8, "application/json");
            var response = await _httpClient.PutAsync(EditarVehiculo, contenido);

            if (response.IsSuccessStatusCode)
            {
                respuesta = true;
            }
            return respuesta;
        }

        public async Task<bool> Eliminar(int IdVehiculo, Login login)
        {
            if (IdVehiculo <= 0)
            {
                throw new ArgumentException("El ID del vehículo debe ser mayor que 0.", nameof(IdVehiculo));
            }

            bool respuesta = false;
            await Autenticar(login);

            string ruta = EliminarVehiculo + IdVehiculo.ToString();
            var response = await _httpClient.DeleteAsync(ruta);

            if (response.IsSuccessStatusCode)
            {
                respuesta = true;
            }
            return respuesta;
        }
    }
}
