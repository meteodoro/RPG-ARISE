using ARISE.Dominio.Enums;

namespace ARISE.Regras;

public static class TabelaElemental
{
    private const double MultiplicadorVantagem = 1.5;
    private const double MultiplicadorResistencia = 0.5;
    private const double MultiplicadorNeutro = 1.0;

    public static Elemento VenceContra(Elemento elemento) => elemento switch
    {
        Elemento.Fairy => Elemento.Undead,
        Elemento.Undead => Elemento.Bestial,
        Elemento.Bestial => Elemento.Arcane,
        Elemento.Arcane => Elemento.Fairy,
        _ => throw new ArgumentOutOfRangeException(nameof(elemento), elemento, "Elemento não mapeado na tabela.")
    };

    public static double CalcularMultiplicador(Elemento? atacante, Elemento defensor)
    {
        if (atacante is null) return MultiplicadorNeutro;
        if (VenceContra(atacante.Value) == defensor) return MultiplicadorVantagem;
        if (VenceContra(defensor) == atacante.Value) return MultiplicadorResistencia;

        return MultiplicadorNeutro;
    }
}