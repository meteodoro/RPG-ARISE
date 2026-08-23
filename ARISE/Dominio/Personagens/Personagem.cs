using ARISE.Dominio.Excecoes;
using ARISE.Dominio.Estados;
using ARISE.Dominio.Enums;
using ARISE.Dominio.Interfaces;
using ARISE.Dominio.Itens;
using ARISE.Dominio.Monstros;

namespace ARISE.Dominio.Personagens;

public abstract class Personagem : IAtacavel
{
    // Atributos Básicos e Elemento
    public string Nome { get; protected set; } = string.Empty;
    public Elemento Elemento { get; protected set; } 
    public int Nivel { get; protected set; } = 1;
    public int Fisico { get; protected set; }
    public int Reflexo { get; protected set; }
    public int Tecnica { get; protected set; }
    public int Foco { get; protected set; }
    public int Armadura { get; protected set; }
    public Inventario Inventario { get; protected set; } = new Inventario();

    // Regras de Classe Abstrata
    protected abstract int VidaBaseClasse { get; }
    protected abstract int EnergiaBaseClasse { get; }
    public abstract int AtributoDeAtaque { get; }

    // Estados e Habilidades
    private readonly List<EstadoAtivo> _estadosAtivos = new();
    public IReadOnlyList<EstadoAtivo> EstadosAtivos => _estadosAtivos;
    private readonly List<IHabilidade> _habilidades = new();
    public IReadOnlyList<IHabilidade> Habilidades => _habilidades;
    
    protected void AdicionarHabilidadeInicial(IHabilidade habilidade)
    {
        _habilidades.Add(habilidade);
    }

   
    public virtual List<IHabilidade> ObterOpcoesDeHabilidadesPorNivel(int nivel)
    {
        return new List<IHabilidade>();
    }

    // Propriedades Calculadas
    public int VidaMaxima => VidaBaseClasse + Fisico * Nivel;
    public int EnergiaMaxima => EnergiaBaseClasse + Foco * Nivel;
    public int ClasseDeArmadura => Armadura + (Reflexo / 2);
    public int Vida { get; protected set; }
    public int Energia { get; protected set; }
    public int ExperienciaAtual {get; private set; } = 0;


    // Constantes do Sistema
    private const int PercentualBaseReducao = 25;
    private const int PercentualMaximoReducao = 40;
    private const int FatorReducaoPorPontoDeFisico = 2;

    private const int PercentualBaseFuga = 25;
    private const int PercentualMaximoFuga = 40;
    private const int FatorFugaPorPontoDeReflexo = 2;

    public int PercentualReducaoAoDefender
        => Math.Min(PercentualBaseReducao + Fisico * FatorReducaoPorPontoDeFisico, PercentualMaximoReducao);

    public int PercentualChanceDeFuga
        => Math.Min(PercentualBaseFuga + Reflexo * FatorFugaPorPontoDeReflexo, PercentualMaximoFuga);
    
    // Engineer
    public virtual Personagem? AliadoAtivo => null;

    public virtual bool PodeUsarAcaoEspecial => false;
    public virtual string NomeAcaoEspecial => string.Empty;
    
    public virtual void ExecutarAcaoEspecial(Monstro? contexto)
    {
        throw new RegraDeJogoException("Este personagem não possui ação especial.");
    }

    public virtual void RemoverAliadoDerrotado()
    {
    }

    // Controle de Vida, Energia e Interface IAtacavel
    protected void InicializarVitalidade()
    {
        Vida = VidaMaxima;
        Energia = EnergiaMaxima;
    }

    public virtual void ReceberDano(int qtdDano) => Vida = Math.Max(Vida - qtdDano, 0);
    
    public void Curar(int qtdVida) => Vida = Math.Min(Vida + qtdVida, VidaMaxima);
    public void Curar() => Curar(VidaMaxima);
    
    public void GastarEnergia(int quantidade)
    {
        if (quantidade > Energia)
            throw new RegraDeJogoException($"{Nome} não tem energia suficiente.");
            
        Energia -= quantidade;
    }

    public bool EstaVivo() => Vida > 0;
    
    bool IAtacavel.EstaVivo => EstaVivo();

    // Gestão de Estados
    public void AplicarEstado(TipoEstado tipoEstado, int duracaoRodadas)
    {
        _estadosAtivos.RemoveAll(e => e.TipoEstado == tipoEstado);
        _estadosAtivos.Add(new EstadoAtivo(tipoEstado, duracaoRodadas));
    }
    
    public bool TemEstado(TipoEstado tipoEstado) => _estadosAtivos.Any(e => e.TipoEstado == tipoEstado);

    public void DecrementarEstado()
    {
        foreach (var estadoAtivo in _estadosAtivos)
            estadoAtivo.Decrementar();

        _estadosAtivos.RemoveAll(e => e.Expirou);
    }

    // Ações do Personagem
    public void AprenderHabilidade(IHabilidade novaHabilidade)
    {
        if (!_habilidades.Any(h => h.Nome == novaHabilidade.Nome))
            _habilidades.Add(novaHabilidade);
    }
    
    public virtual int Defender()
    {
        AplicarEstado(TipoEstado.Defendendo, duracaoRodadas: 1);
        int mpRecuperado = Math.Max((int)(EnergiaMaxima * 0.15), 2);
        Energia = Math.Min(Energia + mpRecuperado, EnergiaMaxima);
        return mpRecuperado;
    }
    
    public virtual Personagem EscolherAlvo(Personagem jogador, Personagem monstro)
        => monstro; 
    
    public override string ToString() => $"{Nome} [{Vida}/{VidaMaxima} HP]";
    
    // Experiencia e subir de nível
    public int ExperienciaProximoNivel => Nivel * 100 + (Nivel - 1) * 50;
    public bool GanharExperiencia(int quantidade)
    {
        if (quantidade <= 0) return false;

        ExperienciaAtual += quantidade;
        bool subiuDeNivel = false;

        while (ExperienciaAtual >= ExperienciaProximoNivel)
        {
            ExperienciaAtual -= ExperienciaProximoNivel;
            SubirDeNivel(); 
            subiuDeNivel = true;
        }

        return subiuDeNivel;
    }
    public void SubirDeNivel(int porcentagemCura = 30)
    {
        Nivel++;
        Fisico += 2;
        Reflexo += 1;
        Tecnica += 1;
        Foco += 1;
        Armadura += 1;

        int quantidadeCura = (VidaMaxima * porcentagemCura) / 100;
        Curar(quantidadeCura);
    }
    
    // Métodos para os itens
    public void RestaurarVida(int quantidade)
    {
        if (quantidade <= 0 || !EstaVivo()) return;
        Vida = Math.Min(Vida + quantidade, VidaMaxima);
    }
    
    public void RestaurarEnergia(int quantidade)
    {
        if (quantidade <= 0 || !EstaVivo()) return;
        Energia = Math.Min(Energia + quantidade, EnergiaMaxima);
    }
}
