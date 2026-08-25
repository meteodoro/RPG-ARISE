using ARISE.Dominio.Enums;
using ARISE.Dominio.Habilidades;
using ARISE.Dominio.Interfaces;

namespace ARISE.Dominio.Personagens;

public class Phantom : Personagem
{
    protected override int VidaBaseClasse => 16;
    protected override int EnergiaBaseClasse => 15;
    public override int AtributoDeAtaque => Reflexo;

    public Phantom(string nome)
    {
        Nome = nome;
        Elemento = Elemento.Undead;
        Fisico = 3;
        Reflexo = 10;
        Tecnica = 4;
        Foco = 2;
        Armadura = 1;
        InicializarVitalidade();

        // Habilidades Iniciais (Nível 1)
        AdicionarHabilidadeInicial(new HabilidadeAtaque("Golpe Sombrio", danoBase: 15, custo: 7, Elemento.Undead));
        AdicionarHabilidadeInicial(new HabilidadeAtaque("Passo Espectral", danoBase: 12, custo: 6, Elemento.Arcane));
    }

    public override List<IHabilidade> ObterOpcoesDeHabilidadesPorNivel(int nivel)
    {
        return nivel switch
        {
            2 => new List<IHabilidade>
            {
                new HabilidadeAtaque("Lâmina Necrótica", danoBase: 30, custo: 14, Elemento.Undead),
                new HabilidadeAtaque("Bote Selvagem", danoBase: 28, custo: 12, Elemento.Bestial)
            },
            3 => new List<IHabilidade>
            {
                new HabilidadeAtaque("Corte Ilusório", danoBase: 50, custo: 24, Elemento.Fairy),
                new HabilidadeAtaque("Sombra Perfurante", danoBase: 54, custo: 26, Elemento.Undead)
            },
            4 => new List<IHabilidade>
            {
                new HabilidadeAtaque("Execução Profana", danoBase: 77, custo: 32, Elemento.Undead),
                new HabilidadeAtaque("Dança das Sombras", danoBase: 76, custo: 30, Elemento.Arcane)
            },
            _ => new List<IHabilidade>()
        };
    }
}
