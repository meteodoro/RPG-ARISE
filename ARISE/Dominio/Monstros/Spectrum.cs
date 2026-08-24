using ARISE.Dominio.Enums;
using ARISE.Dominio.Itens;
using ARISE.Dominio.Personagens;
using ARISE.Regras;

namespace ARISE.Dominio.Monstros;

// Elemento Fairy, Rank E, ataque especial Iludir (Confundir) com 20% de probabilidade
public class Spectrum : Monstro
{
    private const int FatorDeConversaoIlusao = 4;

    protected override int VidaBaseClasse => 16;
    protected override int EnergiaBaseClasse => 10;
    public override int AtributoDeAtaque => Foco;
    public override Rank Rank => Rank.E;
    public override int ExperienciaConcedida => 40;
    

    public int Ilusao { get; protected set; } = 5;

    public Spectrum(string nome)
    {
        Nome = nome;
        Elemento = Elemento.Fairy;
        Fisico = 1;
        Reflexo = 4;
        Tecnica = 2;
        Foco = 6;
        Armadura = 0;
        InicializarVitalidade();
        
        TabelaDeDrops = new List<EntradaDeDrop>
        {
            new(new PocaoEnergia("Poção de Energia Pequena", 15), 35),
            new(null, 65) 
        };
        CaminhoImagem = "Imagens/spectrum.png";
    }

    public override AcaoDeMonstro DecidirAcao(Personagem alvo, int rodadaAtual)
    {
        // 5 * 4 = 20% de chance de usar Confundir
        if (GeradorAleatorio.Rolar(1, 100) <= Ilusao * FatorDeConversaoIlusao) 
            return AcaoDeMonstro.Confundir;

        return AcaoDeMonstro.AtaqueBasico;
    }
}
