using AppTaxi.Models;
using Newtonsoft.Json;
using System.Text;

namespace AppTaxi.Servicios
{
    public class API_Empresa : Autenticacion, I_Empresa
    {
        private const string ListaEmpresa = "api/Empresa/Lista";
        private const string ObtenerEmpresa = "api/Empresa/Obtener/";
        private const string GuardarEmpresa = "api/Empresa/Guardar/";
        private const string EditarEmpresa = "api/Empresa/Editar/";
        private const string EliminarEmpresa = "api/Empresa/Eliminar/";

        public async Task<List<Empresa>> Lista(Login login)
        {
            List<Empresa> lista = new List<Empresa>();
            await Autenticar(login);

            var response = await _httpClient.GetAsync(ListaEmpresa);

            if (response.IsSuccessStatusCode)
            {
                var jsonRespuesta = await response.Content.ReadAsStringAsync();
                var resultado = JsonConvert.DeserializeObject<ResultadoApi<List<Empresa>>>(jsonRespuesta);
                lista = resultado.Response;
            }
            return lista;
        }

        public async Task<Empresa> Obtener(int IdEmpresa, Login login)
        {
            if (IdEmpresa <= 0)
            {
                throw new ArgumentException("El ID de la empresa debe ser mayor que 0.", nameof(IdEmpresa));
            }

            Empresa empresa = new Empresa();
            await Autenticar(login);

            string ruta = ObtenerEmpresa + IdEmpresa.ToString();
            var response = await _httpClient.GetAsync(ruta);

            if (response.IsSuccessStatusCode)
            {
                var jsonRespuesta = await response.Content.ReadAsStringAsync();
                var resultado = JsonConvert.DeserializeObject<ResultadoApi<Empresa>>(jsonRespuesta);
                empresa = resultado.Response;
            }
            return empresa;
        }

        public async Task<bool> Guardar(Empresa empresa, Login login)
        {
            bool respuesta = false;
            await Autenticar(login);

            var contenido = new StringContent(JsonConvert.SerializeObject(empresa), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(GuardarEmpresa, contenido);

            if (response.IsSuccessStatusCode)
            {
                respuesta = true;
            }
            return respuesta;
        }

        public async Task<bool> Editar(Empresa empresa, Login login)
        {
            bool respuesta = false;
            await Autenticar(login);

            var contenido = new StringContent(JsonConvert.SerializeObject(empresa), Encoding.UTF8, "application/json");
            var response = await _httpClient.PutAsync(EditarEmpresa, contenido);

            if (response.IsSuccessStatusCode)
            {
                respuesta = true;
            }
            return respuesta;
        }

        public async Task<bool> Eliminar(int IdEmpresa, Login login)
        {
            if (IdEmpresa <= 0)
            {
                throw new ArgumentException("El ID de la empresa debe ser mayor que 0.", nameof(IdEmpresa));
            }

            bool respuesta = false;
            await Autenticar(login);

            string ruta = EliminarEmpresa + IdEmpresa.ToString();
            var response = await _httpClient.DeleteAsync(ruta);

            if (response.IsSuccessStatusCode)
            {
                respuesta = true;
            }
            return respuesta;
        }
    }
}
