using ARISE.Dominio.Enums;
using ARISE.Dominio.Habilidades;
using ARISE.Dominio.Interfaces;

namespace ARISE.Dominio.Personagens;

public class Vanguard : Personagem
{
    protected override int VidaBaseClasse => 30;
    protected override int EnergiaBaseClasse => 10;
    public override int AtributoDeAtaque => Fisico;

    public Vanguard(string nome)
    {
        Nome = nome;
        Elemento = Elemento.Bestial;
        Fisico = 8;
        Reflexo = 3;
        Tecnica = 2;
        Foco = 2;
        Armadura = 5;
        InicializarVitalidade();

        // Habilidades Iniciais (Nível 1)
        AdicionarHabilidadeInicial(new HabilidadeAtaque("Golpe Brutal", danoBase: 16, custo: 5, Elemento.Bestial));
        AdicionarHabilidadeInicial(new HabilidadeAtaque("Pisotão de Titan", danoBase: 12, custo: 4, Elemento.Bestial));
    }

    /// <summary>
    /// Retorna 2 opções de habilidades de impacto/combate para o jogador escolher ao subir de nível.
    /// </summary>
    public override List<IHabilidade> ObterOpcoesDeHabilidadesPorNivel(int nivel)
    {
        return nivel switch
        {
            2 => new List<IHabilidade>
            {
                new HabilidadeAtaque("Investida Feroz", danoBase: 32, custo: 10, Elemento.Bestial),
                new HabilidadeAtaque("Corte Ossudo", danoBase: 29, custo: 9, Elemento.Undead)
            },
            3 => new List<IHabilidade>
            {
                new HabilidadeAtaque("Impacto Titânico", danoBase: 56, custo: 18, Elemento.Bestial),
                new HabilidadeAtaque("Lâmina do Encanto", danoBase: 50, custo: 16, Elemento.Fairy)
            },
            4 => new List<IHabilidade>
            {
                new HabilidadeAtaque("Fúria Primordial", danoBase: 81, custo: 28, Elemento.Bestial),
                new HabilidadeAtaque("Golpe Ruína Arcana", danoBase: 77, custo: 26, Elemento.Arcane)
            },
            _ => new List<IHabilidade>()
        };
    }
}
