using ARISE.Dominio.Enums;
using ARISE.Dominio.Itens;
using ARISE.Dominio.Personagens;

namespace ARISE.Dominio.Monstros;

// Elemento Bestial, Rank B — ao cair a 40% de vida ou menos, uiva uma vez (atordoando o alvo) e entra em fúria permanente; depois disso, ataca normalmente
public class PackAncient : Monstro
{
    private const int LimiarDeVidaBaixa = 40;

    protected override int VidaBaseClasse => 80;
    protected override int EnergiaBaseClasse => 15;
    public override int AtributoDeAtaque => Fisico;
    public override Rank Rank => Rank.B;
    public override int ExperienciaConcedida => 200;

    public bool JaUivou { get; private set; }

    public PackAncient(string nome)
    {
        Nome = nome;
        Elemento = Elemento.Bestial;
        Fisico = 12;
        Reflexo = 7;
        Tecnica = 5;
        Foco = 4;
        Armadura = 4;
        InicializarVitalidade();
        
        TabelaDeDrops = new List<EntradaDeDrop>
        {
            new(new PocaoEnergia("Poção de Energia Grande", 35), 80),
            new(new PocaoVida("Poção de Vida Grande", 50), 10),
            new(null, 10) 
        };
        CaminhoImagem = "Imagens/packancient.png";

    }

    public override AcaoDeMonstro DecidirAcao(Personagem alvo, int rodadaAtual)
    {
        int percentualVida = (int)((double)Vida / VidaMaxima * 100);

        // Se estiver abaixo do limiar e ainda não uivou, executa o Uivo de Guerra
        if (percentualVida <= LimiarDeVidaBaixa && !JaUivou)
        {
            JaUivou = true;
            return AcaoDeMonstro.UivoDeGuerra;
        }

        return AcaoDeMonstro.AtaqueBasico;
    }
}
