using ARISE.Dominio.Enums;
using ARISE.Dominio.Itens;
using ARISE.Dominio.Personagens;

namespace ARISE.Dominio.Monstros;

// Elemento Arcane, Rank D, sempre ataca com raio de energia
public class Golem : Monstro
{
    protected override int VidaBaseClasse => 40;
    protected override int EnergiaBaseClasse => 18;
    public override int AtributoDeAtaque => Fisico;
    public override Rank Rank => Rank.D;
    public override int ExperienciaConcedida => 90;

    public Golem(string nome)
    {
        Nome = nome;
        Elemento = Elemento.Arcane;
        Fisico = 7;
        Reflexo = 1;
        Tecnica = 2;
        Foco = 4;
        Armadura = 5;
        InicializarVitalidade();
        
        TabelaDeDrops = new List<EntradaDeDrop>
        {
            new(new PocaoVida("Poção de Vida Média", 40), 45),
            new(new PocaoEnergia("Poção de Energia Média", 25), 35),
            new(null, 20) 
        };
        CaminhoImagem = "Imagens/golem.png";

    }

    public override AcaoDeMonstro DecidirAcao(Personagem alvo, int rodadaAtual) 
        => AcaoDeMonstro.RaioDeEnergia;
}
