using ARISE.Dominio.Enums;
using ARISE.Dominio.Itens;
using ARISE.Dominio.Personagens;

namespace ARISE.Dominio.Monstros;

// Elemento Undead, Rank B, sempre ataca drenando o alvo (e o próprio Orcus recupera parte desse dano como vida)
public class Orcus : Monstro
{
    protected override int VidaBaseClasse => 75;
    protected override int EnergiaBaseClasse => 15;
    public override int AtributoDeAtaque => Fisico;
    public override Rank Rank => Rank.B;
    public override int ExperienciaConcedida => 130;
    
    private const int FatorDeConversaoDrenoDeVida = 10;

    public int DrenoDeVida { get; protected set; } = 6;

    public Orcus(string nome)
    {
        Nome = nome;
        Elemento = Elemento.Undead;
        Fisico = 11;
        Reflexo = 3;
        Tecnica = 6;
        Foco = 8;
        Armadura = 6;
        InicializarVitalidade();
        
        TabelaDeDrops = new List<EntradaDeDrop>
        {
            new(new PocaoVida("Poção de Vida Média", 30), 45),
            new(null, 55) 
        };
        CaminhoImagem = "Imagens/orcus.png";

    }

    public override AcaoDeMonstro DecidirAcao(Personagem alvo, int rodadaAtual) 
        => AcaoDeMonstro.DrenarVida;

    public override int AplicarEfeitoEspecial(Personagem alvo, int danoCausado)
    {
        int vidaRecuperada = danoCausado * DrenoDeVida / FatorDeConversaoDrenoDeVida;
        int vidaAntesDaCura = Vida;
        Curar(vidaRecuperada);
        return Vida - vidaAntesDaCura;
    }
}
