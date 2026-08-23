using ARISE.Dominio.Enums;
using ARISE.Dominio.Monstros;

namespace ARISE.Dominio.Personagens;

public class AliadoInvocado : Personagem
{
    private const double FatorEnfraquecimento = 0.5;

    public Monstro MonstroOrigem { get; private set; }

    protected override int VidaBaseClasse => (int)(MonstroOrigem.VidaMaxima * FatorEnfraquecimento);
    protected override int EnergiaBaseClasse => (int)(MonstroOrigem.EnergiaMaxima * FatorEnfraquecimento);
    public override int AtributoDeAtaque => (int)(MonstroOrigem.AtributoDeAtaque * FatorEnfraquecimento);

    public AliadoInvocado(Monstro monstroOrigem)
    {
        MonstroOrigem = monstroOrigem;

        Nome = $"Sombra de {monstroOrigem.Nome}";
        Elemento = monstroOrigem.Elemento;
        Nivel = monstroOrigem.Nivel;

        Fisico = (int)(monstroOrigem.Fisico * FatorEnfraquecimento);
        Reflexo = (int)(monstroOrigem.Reflexo * FatorEnfraquecimento);
        Tecnica = (int)(monstroOrigem.Tecnica * FatorEnfraquecimento);
        Foco = (int)(monstroOrigem.Foco * FatorEnfraquecimento);
        Armadura = (int)(monstroOrigem.Armadura * FatorEnfraquecimento);

        InicializarVitalidade();
    }
}