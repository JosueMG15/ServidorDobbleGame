using Logica;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace DobbleServicio
{
    [ServiceContract(CallbackContract = typeof(IPartidaCallback))]
    public interface IGestionPartida
    {
        [OperationContract]
        bool CrearNuevaPartida(string codigoSala);
        [OperationContract(IsOneWay = true)]
        void UnirJugadorAPartida(string nombreUsuario, string codigoSala);
        [OperationContract]
        bool AbandonarPartida(string nombreUsuario, string codigoSala);
        [OperationContract(IsOneWay = true)]
        void NotificarActualizacionDeJugadoresEnPartida(string codigoSala);
        [OperationContract(IsOneWay = true)]
        void NotificarInicioPartida(string codigoSala);
        [OperationContract(IsOneWay = true)]
        void NotificarDistribucionCartas (string codigoSala);
        [OperationContract(IsOneWay = true)]
        void ValidarCarta(string nombreUsuario, string rutaIcono, string codigoSala);
        [OperationContract]
        RespuestaServicio<bool> GuardarPuntosJugador(string nombreJugador, int puntosGanados);
        [OperationContract(IsOneWay = true)]
        void RegresarASala(string nombreUsuario, string codigoSala);
    }

    [ServiceContract]
    interface IPartidaCallback
    {
        [OperationContract(IsOneWay = true)]
        void ActualizarJugadoresEnPartida(List<Jugador> jugadoresConectados);
        [OperationContract(IsOneWay = true)]
        void IniciarPartida();
        [OperationContract(IsOneWay = true)]
        void FinalizarPartida();
        [OperationContract(IsOneWay = true)]
        void ConvertirEnAnfitrionDesdePartida();
        [OperationContract(IsOneWay = true)]
        void AsignarCarta(Carta carta);
        [OperationContract (IsOneWay = true)]
        void AsignarCartaCentral(Carta cartaCentral, int cartasRestantes);
        [OperationContract(IsOneWay = true)]
        void ActualizarPuntosEnPartida(string nombreUsuario, int puntosEnPartida);
        [OperationContract(IsOneWay = true)]
        void BloquearCarta();
        [OperationContract(IsOneWay = true)]
        void DesbloquearCarta();
    }
}
