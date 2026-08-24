using ARISE.Dominio.Enums;
using ARISE.Dominio.Itens;
using ARISE.Dominio.Personagens;

namespace ARISE.Dominio.Monstros;

// Elemento Fairy, Rank B, sempre ataca com maldição enfraquecedora
public class GreenWitch : Monstro
{
    protected override int VidaBaseClasse => 65;
    protected override int EnergiaBaseClasse => 25;
    public override int AtributoDeAtaque => Foco;
    public override Rank Rank => Rank.B;
    public override int ExperienciaConcedida => 150;
    public bool JaAmaldicoou { get; private set; }

    public GreenWitch(string nome)
    {
        Nome = nome;
        Elemento = Elemento.Fairy;
        Fisico = 4;
        Reflexo = 8;
        Tecnica = 10;
        Foco = 14;
        Armadura = 3;
        InicializarVitalidade();
        
        TabelaDeDrops = new List<EntradaDeDrop>
        {
            new(new PocaoEnergia("Poção de Energia Grande", 35), 50),
            new(null, 50) 
        };
        CaminhoImagem = "Imagens/greenwitch.png";

    }

    public override AcaoDeMonstro DecidirAcao(Personagem alvo, int rodadaAtual)
    {
        if (!JaAmaldicoou)
        {
            JaAmaldicoou = true;
            return AcaoDeMonstro.MaldicaoEnfraquecedora;
        }

        return AcaoDeMonstro.AtaqueBasico;
    }
}
