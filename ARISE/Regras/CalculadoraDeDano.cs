using ARISE.Dominio.Enums;
using ARISE.Dominio.Interfaces;
using ARISE.Dominio.Personagens;

namespace ARISE.Regras;

public static class CalculadoraDeDano
{
    private const int DanoMinimo = 1;
    private const int VariacaoMinima = -2;
    private const int VariacaoMaxima = 2;

    public static int CalcularDano(
        IHabilidade habilidade,
        Personagem atacante,
        IAtacavel alvo,
        bool aplicarMultiplicadorElemental = true)
    {
        Elemento elementoEfetivo = habilidade.Elemento ?? atacante.Elemento;
        int danoBase = habilidade.DanoBase + atacante.AtributoDeAtaque;
        double multiplicadorElemental = aplicarMultiplicadorElemental
            ? TabelaElemental.CalcularMultiplicador(elementoEfetivo, alvo.Elemento)
            : 1.0;
        
        if (alvo.TemEstado(TipoEstado.Defendendo))
        {
            multiplicadorElemental *= (100.0 - alvo.PercentualReducaoAoDefender) / 100.0;
        }

        int danoBruto = (int)(danoBase * multiplicadorElemental) - alvo.ClasseDeArmadura;
        int variacaoAleatoria = GeradorAleatorio.Rolar(VariacaoMinima, VariacaoMaxima);
        int danoFinal = danoBruto + variacaoAleatoria;

        return Math.Max(danoFinal, DanoMinimo);
    }
}
