using ARISE.Dominio.Enums;
using ARISE.Dominio.Habilidades;
using ARISE.Dominio.Interfaces;

namespace ARISE.Dominio.Personagens;

public class Arcanist : Personagem
{
    protected override int VidaBaseClasse => 14;
    protected override int EnergiaBaseClasse => 25;
    public override int AtributoDeAtaque => Foco;

    public Arcanist(string nome)
    {
        Nome = nome;
        Elemento = Elemento.Arcane; 
        Fisico = 2;
        Reflexo = 3;
        Tecnica = 4;
        Foco = 8;
        Armadura = 2;
        InicializarVitalidade();

        // Habilidades Iniciais (Nível 1)
        AdicionarHabilidadeInicial(new HabilidadeAtaque("Míssil Mágico", danoBase: 14, custo: 8, Elemento.Arcane));
        AdicionarHabilidadeInicial(new HabilidadeAtaque("Toque Espectral", danoBase: 12, custo: 6, Elemento.Undead));
    }

    
    public override List<IHabilidade> ObterOpcoesDeHabilidadesPorNivel(int nivel)
    {
        return nivel switch
        {
            2 => new List<IHabilidade>
            {
                new HabilidadeAtaque("Explosão Mística", danoBase: 28, custo: 16, Elemento.Arcane),
                new HabilidadeAtaque("Chama Feérica", danoBase: 26, custo: 14, Elemento.Fairy)
            },
            3 => new List<IHabilidade>
            {
                new HabilidadeAtaque("Dreno de Alma", danoBase: 48, custo: 26, Elemento.Undead),
                new HabilidadeAtaque("Orbe Arcano", danoBase: 52, custo: 28, Elemento.Arcane)
            },
            4 => new List<IHabilidade>
            {
                new HabilidadeAtaque("Singularidade Arcana", danoBase: 88, custo: 42, Elemento.Arcane),
                new HabilidadeAtaque("Furia Quimérica", danoBase: 82, custo: 38, Elemento.Bestial)
            },
            _ => new List<IHabilidade>()
        };
    }
}