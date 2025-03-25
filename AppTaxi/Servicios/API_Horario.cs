using AppTaxi.Models;
using Newtonsoft.Json;
using System.Text;

namespace AppTaxi.Servicios
{
    public class API_Horario : Autenticacion, I_Horario
    {
        private const string ListaHorario = "api/Horario/Lista";
        private const string ObtenerHorario = "api/Horario/Obtener/";
        private const string GuardarHorario = "api/Horario/Guardar/";
        private const string EditarHorario = "api/Horario/Editar/";
        private const string EliminarHorario = "api/Horario/Eliminar/";

        public async Task<List<Horario>> Lista(Login login)
        {
            List<Horario> lista = new List<Horario>();
            await Autenticar(login);

            var response = await _httpClient.GetAsync(ListaHorario);

            if (response.IsSuccessStatusCode)
            {
                var jsonRespuesta = await response.Content.ReadAsStringAsync();
                var resultado = JsonConvert.DeserializeObject<ResultadoApi<List<Horario>>>(jsonRespuesta);
                lista = resultado.Response;
            }
            return lista;
        }

        public async Task<Horario> Obtener(int IdHorario, Login login)
        {
            if (IdHorario <= 0)
            {
                throw new ArgumentException("El ID del horario debe ser mayor que 0.", nameof(IdHorario));
            }

            Horario horario = new Horario();
            await Autenticar(login);

            string ruta = ObtenerHorario + IdHorario.ToString();
            var response = await _httpClient.GetAsync(ruta);

            if (response.IsSuccessStatusCode)
            {
                var jsonRespuesta = await response.Content.ReadAsStringAsync();
                var resultado = JsonConvert.DeserializeObject<ResultadoApi<Horario>>(jsonRespuesta);
                horario = resultado.Response;
            }
            return horario;
        }

        public async Task<bool> Guardar(Horario horario, Login login)
        {
            bool respuesta = false;
            await Autenticar(login);

            var contenido = new StringContent(JsonConvert.SerializeObject(horario), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(GuardarHorario, contenido);

            if (response.IsSuccessStatusCode)
            {
                respuesta = true;
            }
            return respuesta;
        }

        public async Task<bool> Editar(Horario horario, Login login)
        {
            bool respuesta = false;
            await Autenticar(login);

            var contenido = new StringContent(JsonConvert.SerializeObject(horario), Encoding.UTF8, "application/json");
            var response = await _httpClient.PutAsync(EditarHorario, contenido);

            if (response.IsSuccessStatusCode)
            {
                respuesta = true;
            }
            return respuesta;
        }

        public async Task<bool> Eliminar(int IdHorario, Login login)
        {
            if (IdHorario <= 0)
            {
                throw new ArgumentException("El ID del horario debe ser mayor que 0.", nameof(IdHorario));
            }

            bool respuesta = false;
            await Autenticar(login);

            string ruta = EliminarHorario + IdHorario.ToString();
            var response = await _httpClient.DeleteAsync(ruta);

            if (response.IsSuccessStatusCode)
            {
                respuesta = true;
            }
            return respuesta;
        }
    }
}
