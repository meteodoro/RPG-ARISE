using ARISE.Dominio.Enums;

namespace ARISE.Dominio.Estados;

public class EstadoAtivo
{
    public TipoEstado TipoEstado { get; }
    public int RodadasRestantes { get; private set; }
    public bool Expirou => RodadasRestantes <= 0;

    public EstadoAtivo(TipoEstado tipoEstado, int duracaoRodadas)
    {
        TipoEstado = tipoEstado;
        RodadasRestantes = Math.Max(0, duracaoRodadas);
    }

    public void Decrementar()
    {
        if (RodadasRestantes > 0)
            RodadasRestantes--;
    }
}