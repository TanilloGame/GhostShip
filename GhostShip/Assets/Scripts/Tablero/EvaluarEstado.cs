//using UnityEngine;
//using static UnityEngine.Rendering.DebugUI;

//public class EvaluarEstado : MonoBehaviour
//{
//    public float EvaluarEstado(Tablero tablero, bool esTurnoIA)
//    {
//        float valorIA = 0;
//        float valorJugador = 0;

//        foreach (var tropa in tablero.ObtenerTropas())
//        {
//            float peso = ObtenerPesoTropa(tropa.tipo);
//            float distanciaFactor = 1f / (1f + tropa.DistanciaALineaVictoria());

//            float valorTropa = peso * distanciaFactor;

//            if (tropa.esIA)
//                valorIA += valorTropa;
//            else
//                valorJugador += valorTropa;
//        }

//        return esTurnoIA ? valorIA - valorJugador : valorJugador - valorIA;
//    }

//    private float ObtenerPesoTropa(TipoTropa tipo)
//    {
//        switch (tipo)
//        {
//            case TipoTropa.Large: return 3f;
//            case TipoTropa.Medium: return 2f;
//            case TipoTropa.Small: return 1f;
//            default: return 1f;
//        }
//    }
//}
