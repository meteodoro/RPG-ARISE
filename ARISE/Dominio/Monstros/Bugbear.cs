using ARISE.Dominio.Enums;
using ARISE.Dominio.Itens;
using ARISE.Dominio.Personagens;

namespace ARISE.Dominio.Monstros;

// Elemento Bestial, Rank C, fica mais agressivo (Golpe Selvagem) conforme perde vida; Fúria = quanto mais baixa a vida, maior a fúria
public class Bugbear : Monstro
{
    private const int LimiarDeFuria = 60;

    protected override int VidaBaseClasse => 30;
    protected override int EnergiaBaseClasse => 8;
    public override int AtributoDeAtaque => Fisico;
    public override Rank Rank => Rank.C;
    public override int ExperienciaConcedida => 70;

    public int Furia { get; protected set; }

    public Bugbear(string nome)
    {
        Nome = nome;
        Elemento = Elemento.Bestial;
        Fisico = 8;
        Reflexo = 4;
        Tecnica = 3;
        Foco = 2;
        Armadura = 3;
        InicializarVitalidade();
        
        TabelaDeDrops = new List<EntradaDeDrop>
        {
            new(new PocaoEnergia("Poção de Energia Média", 25), 60),
            new(null, 40) 
        };
        CaminhoImagem = "Imagens/bugbear.png";
    }

    public override AcaoDeMonstro DecidirAcao(Personagem alvo, int rodadaAtual)
    {
        Furia = 100 - (int)((double)Vida / VidaMaxima * 100);

        if (Furia >= LimiarDeFuria) 
            return AcaoDeMonstro.GolpeSelvagem;

        return AcaoDeMonstro.AtaqueBasico;
    }
}
