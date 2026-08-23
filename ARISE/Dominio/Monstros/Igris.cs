using ARISE.Dominio.Enums;
using ARISE.Dominio.Itens;
using ARISE.Dominio.Personagens;

namespace ARISE.Dominio.Monstros;

// Elemento Arcane, Rank S, boss final — 3 fases por limiar de vida, protegido por escudo até a Fase 1 esgotar
public class Igris : Monstro
{
    private const int EscudoMaximo = 35;
    private const int PontosDeRecargaPorTurno = 10;
    private const int LimiarDeFaseIntermediaria = 55;
    private const int LimiarDeFaseFinal = 25;

    protected override int VidaBaseClasse => 400;
    protected override int EnergiaBaseClasse => 50;
    public override int AtributoDeAtaque => Tecnica;
    public override Rank Rank => Rank.S;
    public override int ExperienciaConcedida => 600;

    public int EscudoDeBarreira { get; protected set; } = EscudoMaximo;
    public int Fase { get; private set; } = 1;

    public Igris(string nome)
    {
        Nome = nome;
        Elemento = Elemento.Arcane;
        Fisico = 25;
        Reflexo = 22;
        Tecnica = 20;
        Foco = 22;
        Armadura = 12;
        InicializarVitalidade();
        
        TabelaDeDrops = new List<EntradaDeDrop>
        {
            new(new PocaoVida("Elixir Divino", 200), 80),
            new(null, 20) 
        };
        CaminhoImagem = "Imagens/igris.png";

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
        int percentualVida = (int)((double)Vida / VidaMaxima * 100);

        // Transições de Fase acionadas no limiar de vida
        if (percentualVida <= LimiarDeFaseFinal && Fase != 3) 
        { 
            Fase = 3; 
            return AcaoDeMonstro.TransicaoFinal; 
        }
        
        if (percentualVida <= LimiarDeFaseIntermediaria && Fase < 2) 
        { 
            Fase = 2; 
            return AcaoDeMonstro.TransicaoIntermediaria; 
        }

        // Se o escudo quebrar (zerar), ele usa a ação de recarregar
        if (EscudoDeBarreira <= 0) 
            return AcaoDeMonstro.RecarregarEscudo;

        // Ataques baseados na fase atual enquanto tem escudo
        return Fase switch
        {
            1 => AcaoDeMonstro.RaioDeEnergia,
            2 => AcaoDeMonstro.RompimentoArcano,
            _ => AcaoDeMonstro.TempestadeArcanaFinal
        };
    }
}