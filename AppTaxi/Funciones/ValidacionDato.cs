namespace AppTaxi.Funciones
{
    public class ValidacionDato
    {
        public static bool ValidarTexto(string texto)
        {
            if(texto == null)
            {
                return true;
            }
                
            // Lista de símbolos no permitidos
            char[] simbolosNoPermitidos = { ';', '*', '=', '$', '&', '|','+','"','?','¿','!','(',')','%' };

            // Verificar si el texto contiene alguno de los símbolos no permitidos
            return !texto.Any(c => simbolosNoPermitidos.Contains(c));
        }

        public static bool ValidarNumero(long numero,bool EsTelefono)
        {
            if(numero == 0)
            {
                return true;
            }
            if(EsTelefono)
            {
                bool respuesta = numero.ToString().Length == 10;
                if (respuesta)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                bool respuesta = numero.ToString().Length >= 8 && numero.ToString().Length <= 10;
                if (respuesta)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            
        }
    }
}
