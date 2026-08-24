using ARISE.Dominio.Enums;
using ARISE.Dominio.Excecoes;
using ARISE.Dominio.Habilidades;
using ARISE.Dominio.Interfaces;
using ARISE.Dominio.Monstros;

namespace ARISE.Dominio.Personagens;

public class Engineer : Personagem
{
    private const int _custoDeReanimar = 15;
    public static int CustoDeReanimar => _custoDeReanimar;

    protected override int VidaBaseClasse => 20;
    protected override int EnergiaBaseClasse => 20;
    public override int AtributoDeAtaque => Tecnica;

    private Personagem? _aliadoAtivo;
    public override Personagem? AliadoAtivo => _aliadoAtivo;
    public Engineer(string nome)
    {
        Nome = nome;
        Elemento = Elemento.Fairy;
        Fisico = 3;
        Reflexo = 3;
        Tecnica = 8;
        Foco = 3;
        Armadura = 3;
        InicializarVitalidade();

        // Habilidades Iniciais (Nível 1)
        AdicionarHabilidadeInicial(new HabilidadeAtaque("Tiro Encantado", danoBase: 12, custo: 8, Elemento.Fairy));
        AdicionarHabilidadeInicial(new HabilidadeAtaque("Descarga Arcana", danoBase: 10, custo: 6, Elemento.Arcane));
    }
    

    /// <summary>
    /// Retorna 2 opções de habilidades para o jogador escolher ao subir para o nível indicado.
    /// </summary>
    public override List<IHabilidade> ObterOpcoesDeHabilidadesPorNivel(int nivel)
    {
        return nivel switch
        {
            2 => new List<IHabilidade>
            {
                new HabilidadeAtaque("Armadilha Espectral", danoBase: 25, custo: 15, Elemento.Undead),
                new HabilidadeAtaque("Disparo Elemental", danoBase: 30, custo: 18, Elemento.Arcane)
            },
            3 => new List<IHabilidade>
            {
                new HabilidadeAtaque("Sintonia Feérica", danoBase: 45, custo: 25, Elemento.Fairy),
                new HabilidadeAtaque("Projétil Necrótico", danoBase: 50, custo: 28, Elemento.Undead)
            },
            4 => new List<IHabilidade>
            {
                new HabilidadeAtaque("Canhão Arcano", danoBase: 72, custo: 40, Elemento.Arcane),
                new HabilidadeAtaque("Fúria da Invocação", danoBase: 77, custo: 42, Elemento.Bestial)
            },
            _ => new List<IHabilidade>()
        };
    }
    
    

    public override bool PodeUsarAcaoEspecial => true;
    public override string NomeAcaoEspecial => "Reanimar";
    
    public override void ExecutarAcaoEspecial(Monstro? contexto)
    {
        if (contexto == null)
            throw new RegraDeJogoException("Nenhum monstro derrotado disponível para reanimar.");

        Reanimar(contexto); 
    }

    public override void RemoverAliadoDerrotado()
    {
        if (_aliadoAtivo != null && !_aliadoAtivo.EstaVivo())
            _aliadoAtivo = null;
    }

    public void Reanimar(Monstro monstroDerrotado)
    {
        if (monstroDerrotado.EstaVivo())
            throw new RegraDeJogoException($"{monstroDerrotado.Nome} ainda está vivo — não pode ser reanimado.");

        if (AliadoAtivo != null && AliadoAtivo.EstaVivo())
            throw new RegraDeJogoException($"Você já tem {AliadoAtivo.Nome} lutando ao seu lado. Espere-o morrer antes de reanimar outro aliado.");

        GastarEnergia(CustoDeReanimar);
        _aliadoAtivo = new AliadoInvocado(monstroDerrotado);    
    }
}

