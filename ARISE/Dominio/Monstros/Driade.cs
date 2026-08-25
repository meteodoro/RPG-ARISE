using ARISE.Dominio.Enums;
using ARISE.Dominio.Itens;
using ARISE.Dominio.Personagens;
using ARISE.Regras;

namespace ARISE.Dominio.Monstros;

// Elemento Fairy, Rank C, boss nível 1, muda de Ilusão em Massa para Ataque Básico quando a vida cai a 50% ou menos
public class Driade : Monstro
{
    private const int LimiarDeVidaBaixa = 50;

    protected override int VidaBaseClasse => 48;
    protected override int EnergiaBaseClasse => 20;
    public override int AtributoDeAtaque => Foco;
    public override Rank Rank => Rank.C;
    public override int ExperienciaConcedida => 120;

    public Driade(string nome)
    {
        Nome = nome;
        Elemento = Elemento.Fairy;
        Fisico = 3;
        Reflexo = 5;
        Tecnica = 4;
        Foco = 7;
        Armadura = 2;
        InicializarVitalidade();
        
        TabelaDeDrops = new List<EntradaDeDrop>
        {
            new(new PocaoVida("Poção de Vida Média", 30), 100)
        };
        
        CaminhoImagem = "Imagens/driade.png";
    }

    public override AcaoDeMonstro DecidirAcao(Personagem alvo, int rodadaAtual)
    {
        int percentualVida = (int)((double)Vida / VidaMaxima * 100);

        // Quando a vida estiver <= 50%, prioriza ataque básico (mais agressiva)
        if (percentualVida <= LimiarDeVidaBaixa)
            return AcaoDeMonstro.AtaqueBasico;

        // Acima de 50%, revezar entre Ilusão em Massa e Ataque Básico
        int sorteio = GeradorAleatorio.Rolar(1, 2);

        return (sorteio == 1)
            ? AcaoDeMonstro.AtaqueBasico
            : AcaoDeMonstro.IlusaoEmMassa;
    }
}
