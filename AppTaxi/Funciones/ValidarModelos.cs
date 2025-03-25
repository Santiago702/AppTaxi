using AppTaxi.Models;
using System.Security.Policy;

namespace AppTaxi.Funciones
{
    public class ValidarModelos
    {
        //------------ Conductor 
        public static ValidarModelo validarConductor(Conductor c)
        {
            ValidarModelo vm = new ValidarModelo();
            vm.Mensaje = "Correcto";
            
            bool nom = ValidacionDato.ValidarTexto(c.Nombre);
            bool ced = ValidacionDato.ValidarNumero(c.NumeroCedula,false);
            bool tel = ValidacionDato.ValidarNumero(c.Telefono, true);
            bool cel = ValidacionDato.ValidarNumero(c.Celular, true);
            bool ciu = ValidacionDato.ValidarTexto(c.Ciudad);
            bool eps = ValidacionDato.ValidarTexto(c.Eps);
            bool arl = ValidacionDato.ValidarTexto(c.Arl);
            

            if (!nom) vm.Mensaje = "El Nombre no debe contener simbolos";
            if (!ced) vm.Mensaje = "La cedula debe contener entre 8 y 10 digitos";
            if (!tel) vm.Mensaje = "El Telefono Principal debe tener 10 digitos";
            if (!cel) vm.Mensaje = "El Telefono Secundario debe tener 10 digitos";
            if (!ciu) vm.Mensaje = "La ciudad no debe contener simbolos";
            if (!eps) vm.Mensaje = "El campo EPS no debe contener simbolos";
            if (!arl) vm.Mensaje = "El campo ARL no debe contener simbolos";

            if(nom && ced && tel && cel && ciu && eps && arl)
            {
                vm.Respuesta = true;
            }
            else
            {
                vm.Respuesta = false;
            }
            return vm;
        }

        //Vehiculo:
        public static ValidarModelo validarVehiculo(Vehiculo v)
        {
            ValidarModelo vm = new ValidarModelo();
            bool pl = ValidacionDato.ValidarTexto(v.Placa);
            if (!pl)
            {
                vm.Mensaje = "La placa no debe contener simbolos"; 
                vm.Respuesta = false;
            }
            else
            {
                vm.Respuesta = true;
                vm.Mensaje = "Correcto";
            }
            if(v.VenceSoat.Date <  DateTime.Now.Date)
            {
                vm.Mensaje = "El SOAT debe estar actualizado";
                vm.Respuesta = false;
            }
            else
            {
                vm.Respuesta = true;
                vm.Mensaje = "Correcto";
            }
            if (v.VenceTecnicoMecanica.Date < DateTime.Now.Date)
            {
                vm.Mensaje = "El registro Técnico debe estar actualizado";
                vm.Respuesta = false;
            }
            else
            {
                vm.Respuesta = true;
                vm.Mensaje = "Correcto";
            }
            return vm;
        }

        //Propietario:
        public static ValidarModelo validarPropietario(Propietario p)
        {
            ValidarModelo vm = new ValidarModelo();
            vm.Mensaje = "Correcto";

            bool nom = ValidacionDato.ValidarTexto(p.Nombre);
            bool ced = ValidacionDato.ValidarNumero(p.NumeroCedula,false);
            bool tel = ValidacionDato.ValidarNumero(p.Telefono, true);
            bool cel = ValidacionDato.ValidarNumero(p.Celular, true);
            bool ciu = ValidacionDato.ValidarTexto(p.Ciudad);

            if (!nom) vm.Mensaje = "El Nombre no debe contener simbolos";
            if (!ced) vm.Mensaje = "La cedula debe contener entre 8 y 10 digitos";
            if (!tel) vm.Mensaje = "El Telefono Principal debe tener 10 digitos";
            if (!cel) vm.Mensaje = "El Telefono Secundario debe tener 10 digitos";
            if (!ciu) vm.Mensaje = "La ciudad no debe contener simbolos";

            if (nom && ced && tel && cel && ciu)
            {
                vm.Respuesta = true;
            }
            else
            {
                vm.Respuesta = false;
            }
            return vm;
        }
        //Usuario:
        public static ValidarModelo validarUsuario(Usuario u)
        {
            ValidarModelo vm = new ValidarModelo();
            vm.Mensaje = "Correcto";

            bool nom = ValidacionDato.ValidarTexto(u.Nombre);
            bool tel = ValidacionDato.ValidarNumero(u.Telefono,true);
            bool cel = ValidacionDato.ValidarNumero(u.Celular, true);
            bool ciu = ValidacionDato.ValidarTexto(u.Ciudad);

            if (!nom) vm.Mensaje = "El Nombre no debe contener simbolos";
            
            if (!tel) vm.Mensaje = "El Telefono Principal debe tener 10 digitos";
            if (!cel) vm.Mensaje = "El Telefono Secundario debe tener 10 digitos";
            if (!ciu) vm.Mensaje = "La ciudad no debe contener simbolos";

            if (nom && tel && cel && ciu)
            {
                vm.Respuesta = true;
            }
            else
            {
                vm.Respuesta = false;
            }
            return vm;
        }
        //Empresa
        public static ValidarModelo validarEmpresa(Empresa e)
        {
            ValidarModelo vm = new ValidarModelo();
            vm.Mensaje = "Correcto";
            bool nom = ValidacionDato.ValidarTexto(e.Nombre);
            bool nit = ValidacionDato.ValidarTexto(e.Nit);
            bool rep = ValidacionDato.ValidarTexto(e.Representante);
            bool red1 = ValidacionDato.ValidarTexto(e.RedPrincipal);
            bool red2 = ValidacionDato.ValidarTexto(e.RedSecundaria);

            if (!nom) vm.Mensaje = "El Nombre no debe contener simbolos";

            if (!nit) vm.Mensaje = "El Nit no debe contener simbolos diferentes a '-'";
            if (!rep) vm.Mensaje = "El Nombre del Representante no debe contener simbolos";
            if (!red1) vm.Mensaje = "El contacto principal no debe contener simbolos";
            if (!red1) vm.Mensaje = "El contacto secundario no debe contener simbolos";
            

            if (nom && rep && red1 && red2)
            {
                vm.Respuesta = true;
            }
            else
            {
                vm.Respuesta = false;
            }
            return vm;
        }

    }
}
