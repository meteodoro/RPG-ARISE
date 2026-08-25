using ARISE.Dominio.Enums;
using ARISE.Dominio.Itens;
using ARISE.Dominio.Personagens;

namespace ARISE.Dominio.Monstros;

// Elemento Arcane, Rank A — especialista em Rompimento Arcano
public class Sentinel : Monstro
{
    protected override int VidaBaseClasse => 130;
    protected override int EnergiaBaseClasse => 30;
    public override int AtributoDeAtaque => Tecnica;
    public override Rank Rank => Rank.A;
    public override int ExperienciaConcedida => 350;

    public Sentinel(string nome)
    {
        Nome = nome;
        Elemento = Elemento.Arcane;
        Fisico = 15;
        Reflexo = 10;
        Tecnica = 14;
        Foco = 12;
        Armadura = 8;
        InicializarVitalidade();

        TabelaDeDrops = new List<EntradaDeDrop>
        {
            new(new PocaoVida("Elixir Vital Supremo", 90), 90),
            new(null, 10)
        };
        CaminhoImagem = "Imagens/sentinel.png";

    }

    public override AcaoDeMonstro DecidirAcao(Personagem alvo, int rodadaAtual)
        => AcaoDeMonstro.RompimentoArcano;
}
