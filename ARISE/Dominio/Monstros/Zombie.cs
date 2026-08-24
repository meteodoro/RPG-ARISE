using ARISE.Dominio.Enums;
using ARISE.Dominio.Itens;
using ARISE.Dominio.Personagens;

namespace ARISE.Dominio.Monstros;

// Elemento Undead, Rank E, sempre ataca contagiando o alvo (e o próprio Zombie recupera parte desse dano como vida)
public class Zombie : Monstro
{
    private const int FatorDeConversaoContagio = 10;

    protected override int VidaBaseClasse => 22;
    protected override int EnergiaBaseClasse => 5;
    public override int AtributoDeAtaque => Fisico;
    public override Rank Rank => Rank.E;
    public override int ExperienciaConcedida => 45;

    public int Contagio { get; protected set; } = 3;
    

    public Zombie(string nome)
    {
        Nome = nome;
        Elemento = Elemento.Undead;
        Fisico = 4;
        Reflexo = 1;
        Tecnica = 1;
        Foco = 2;
        Armadura = 1;
        InicializarVitalidade();
        
        TabelaDeDrops = new List<EntradaDeDrop>
        {
            new(new PocaoVida("Poção de Vida Pequena", 30), 30),
            new(null, 70) 
        };
        CaminhoImagem = "Imagens/zombie.png";

    }

    public override AcaoDeMonstro DecidirAcao(Personagem alvo, int rodadaAtual) 
        => AcaoDeMonstro.AtaqueContagiante;

    public override int AplicarEfeitoEspecial(Personagem alvo, int danoCausado)
    {
        int vidaRecuperada = danoCausado * Contagio / FatorDeConversaoContagio;
        int vidaAntesDaCura = Vida;
        Curar(vidaRecuperada);
        return Vida - vidaAntesDaCura;
    }
}
