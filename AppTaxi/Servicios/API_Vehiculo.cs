﻿using AppTaxi.Models;
using Newtonsoft.Json;
using System.Text;

namespace AppTaxi.Servicios
{
    public class API_Vehiculo : Autenticacion, I_Vehiculo
    {

        public async Task<List<Vehiculo>> Lista()
        {
            List<Vehiculo> lista = new List<Vehiculo>();
            await Autenticar();

            var response = await _httpClient.GetAsync("api/Vehiculo/Lista");

            if (response.IsSuccessStatusCode)
            {
                var json_respuesta = await response.Content.ReadAsStringAsync();
                var resultado = JsonConvert.DeserializeObject<ResultadoApi<List<Vehiculo>>>(json_respuesta);
                lista = resultado.Response;

            }
            return lista;

        }

        public async Task<Vehiculo> Obtener(int IdVehiculo)
        {
            Vehiculo vehiculo = new Vehiculo();
            await Autenticar();

            var response = await _httpClient.GetAsync($"api/Vehiculo/Obtener/{IdVehiculo}");

            if (response.IsSuccessStatusCode)
            {
                var json_respuesta = await response.Content.ReadAsStringAsync();
                var resultado = JsonConvert.DeserializeObject<ResultadoApi<Vehiculo>>(json_respuesta);
                vehiculo = resultado.Response;

            }
            return vehiculo;
        }

        public async Task<bool> Guardar(Vehiculo vehiculo)
        {
            bool Respuesta = false;
            await Autenticar();

            var contenido = new StringContent(JsonConvert.SerializeObject(vehiculo), Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("api/Vehiculo/Guardar/", contenido);

            if (response.IsSuccessStatusCode)
            {
                Respuesta = true;

            }
            return Respuesta;
        }

        public async Task<bool> Editar(Vehiculo vehiculo)
        {
            bool Respuesta = false;
            await Autenticar();

            var contenido = new StringContent(JsonConvert.SerializeObject(vehiculo), Encoding.UTF8, "application/json");

            var response = await _httpClient.PutAsync("api/Vehiculo/Editar/", contenido);

            if (response.IsSuccessStatusCode)
            {
                Respuesta = true;

            }
            return Respuesta;
        }

        public async Task<bool> Eliminar(int IdVehiculo)
        {
            bool Respuesta = false;
            await Autenticar();

            var response = await _httpClient.DeleteAsync($"api/Vehiculo/Eliminar/{IdVehiculo}");

            if (response.IsSuccessStatusCode)
            {
                Respuesta = true;

            }
            return Respuesta;
        }




    }
}
