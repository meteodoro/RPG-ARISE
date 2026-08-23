using ARISE.Dominio.Enums;
using ARISE.Dominio.Itens;
using ARISE.Dominio.Personagens;

namespace ARISE.Dominio.Monstros;

// Elemento Arcane, Rank A — protegido por um escudo de barreira que absorve dano; ataca com Rompimento Arcano só depois do escudo esgotar
public class Sentinel : Monstro
{
    private const int EscudoMaximo = 35;
    private const int PontosDeRecargaPorTurno = 10;

    protected override int VidaBaseClasse => 180;
    protected override int EnergiaBaseClasse => 30;
    public override int AtributoDeAtaque => Tecnica;
    public override Rank Rank => Rank.A;
    public override int ExperienciaConcedida => 350;

    public int EscudoDeBarreira { get; protected set; } = EscudoMaximo;

    public Sentinel(string nome)
    {
        Nome = nome;
        Elemento = Elemento.Arcane;
        Fisico = 15;
        Reflexo = 10;
        Tecnica = 18;
        Foco = 12;
        Armadura = 8;
        InicializarVitalidade();

        TabelaDeDrops = new List<EntradaDeDrop>
        {
            new(new PocaoVida("Elixir Vital Supremo", 100), 90),
            new(null, 10)
        };
        CaminhoImagem = "Imagens/sentinel.png";

    }

    public override void ReceberDano(int qtdDano)
    {
        if (EscudoDeBarreira > 0)
        {
            int absorvido = Math.Min(EscudoDeBarreira, qtdDano);
            EscudoDeBarreira -= absorvido;
            qtdDano -= absorvido;
        }

        base.ReceberDano(qtdDano);
    }

    public override void RecarregarEscudo()
        => EscudoDeBarreira = Math.Min(EscudoDeBarreira + PontosDeRecargaPorTurno, EscudoMaximo);

    public override AcaoDeMonstro DecidirAcao(Personagem alvo, int rodadaAtual)
    {
        if (EscudoDeBarreira <= 0) 
            return AcaoDeMonstro.RecarregarEscudo;

        return AcaoDeMonstro.RompimentoArcano;
    }
}