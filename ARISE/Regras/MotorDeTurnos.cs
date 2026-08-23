using ARISE.Dominio.Enums;
using ARISE.Dominio.Excecoes;
using ARISE.Dominio.Habilidades; 
using ARISE.Dominio.Interfaces;
using ARISE.Dominio.Itens;
using ARISE.Dominio.Monstros;
using ARISE.Dominio.Personagens;

namespace ARISE.Regras;

public class MotorDeTurnos
{
    public Personagem Jogador { get; }
    public Monstro MonstroAtual { get; }

    public int NumeroRodada { get; private set; } = 1;
    public bool JogadorFugiu { get; private set; }
    public bool JogadorTemIniciativa { get; private set; }
    public string MensagemIniciativa { get; private set; } = string.Empty;
    public bool AguardandoAcaoDoJogador { get; private set; }

    public string LogItemDrop { get; private set; } = "";
    public bool SubiuDeNivel { get; private set; }

    private readonly List<Personagem> _ordemDoTurno = new();
    private int _indiceOrdemAtual;
    private readonly List<string> _logRodada = new();
    private bool _primeiraRodadaDoCombate = true;

    public IReadOnlyList<string> LogRodada => _logRodada;

    public MotorDeTurnos(Personagem jogador, Monstro monstro)
    {
        Jogador = jogador;
        MonstroAtual = monstro;
    }

    // Iniciativa 

    public void CalcularIniciativa()
    {
        var ordem = ObterOrdemDeIniciativa();
        JogadorTemIniciativa = ordem.Count > 0 && ordem[0] == Jogador;
        MensagemIniciativa = CriarMensagemIniciativa(ordem);
    }

    private List<Personagem> ObterOrdemDeIniciativa()
    {
        var lista = new List<Personagem> { Jogador, MonstroAtual };

        if (Jogador.AliadoAtivo != null && Jogador.AliadoAtivo.EstaVivo())
            lista.Add(Jogador.AliadoAtivo);

        return lista.Where(p => p.EstaVivo()).OrderByDescending(p => p.Reflexo).ThenByDescending(p => p.Tecnica).ToList();
    }

    private static string CriarMensagemIniciativa(IReadOnlyList<Personagem> ordem)
    {
        if (ordem.Count == 0)
            return string.Empty;

        Personagem primeiro = ordem[0];
        if (ordem.Count == 1)
            return $"⚡ {primeiro.Nome} ataca primeiro!";

        Personagem segundo = ordem[1];
        if (primeiro.Reflexo > segundo.Reflexo)
            return $"⚡ {primeiro.Nome} tem maior Reflexo e ataca primeiro!";

        if (primeiro.Tecnica > segundo.Tecnica)
            return $"⚡ Empate em Reflexo: {primeiro.Nome} tem maior Técnica e ataca primeiro!";

        return $"⚡ Empate em Reflexo e Técnica: {primeiro.Nome} ataca primeiro pela ordem estável!";
    }

    // Combate

    public void IniciarCombate()
    {
        NumeroRodada = 1;
        JogadorFugiu = false;
        _primeiraRodadaDoCombate = true;

        IniciarNovaRodada();
    }

    private void IniciarNovaRodada()
    {
        _ordemDoTurno.Clear();
        _ordemDoTurno.AddRange(ObterOrdemDeIniciativa());
        _indiceOrdemAtual = 0;

        while (_indiceOrdemAtual < _ordemDoTurno.Count && !_ordemDoTurno[_indiceOrdemAtual].EstaVivo())
            _indiceOrdemAtual++;

        _logRodada.Clear();
        if (_primeiraRodadaDoCombate && _ordemDoTurno.Count > 0)
        {
            MensagemIniciativa = CriarMensagemIniciativa(_ordemDoTurno);
            _logRodada.Add(MensagemIniciativa);
            _primeiraRodadaDoCombate = false;
        }

        ProcessarTurnoAtual();
    }

    private void ProcessarTurnoAtual()
    {
        if (_indiceOrdemAtual >= _ordemDoTurno.Count)
        {
            AguardandoAcaoDoJogador = false;
            return;
        }

        var participante = _ordemDoTurno[_indiceOrdemAtual];

        if (participante.TemEstado(TipoEstado.Atordoado))
        {
            _logRodada.Add($"😵 {participante.Nome} está atordoado e perdeu o turno!");
            AguardandoAcaoDoJogador = false;
            return;
        }

        if (participante == Jogador)
        {
            _logRodada.Add("⚔️ Selecione o próximo ataque...");
            AguardandoAcaoDoJogador = true;
        }
        else
        {
            if (participante == MonstroAtual)
                AcaoMonstro();
            else if (participante == Jogador.AliadoAtivo && Jogador.AliadoAtivo!.EstaVivo())
                AcaoAliado(Jogador.AliadoAtivo);

            AguardandoAcaoDoJogador = false;
        }
    }

    public bool ExecutarAcaoJogador(AcaoDoJogador acao, IHabilidade? habilidade, int indiceItem)
    {
        _logRodada.Clear();
        bool sucesso = AcaoJogador(acao, habilidade, indiceItem);
        if (sucesso)
            AguardandoAcaoDoJogador = false;

        return sucesso;
    }

    public ResultadoCombate AvancarTurno()
    {
        if (JogadorFugiu)
            return ResultadoCombate.FugaJogador;

        if (!MonstroAtual.EstaVivo())
            return ProcessarVitoria();

        if (!Jogador.EstaVivo())
            return ResultadoCombate.DerrotaJogador;

        _indiceOrdemAtual++;
        while (_indiceOrdemAtual < _ordemDoTurno.Count && !_ordemDoTurno[_indiceOrdemAtual].EstaVivo())
            _indiceOrdemAtual++;

        if (_indiceOrdemAtual >= _ordemDoTurno.Count)
        {
            Jogador.DecrementarEstado();
            MonstroAtual.DecrementarEstado();
            Jogador.AliadoAtivo?.DecrementarEstado();

            NumeroRodada++;
            IniciarNovaRodada();
            return ResultadoCombate.EmAndamento;
        }

        _logRodada.Clear();
        ProcessarTurnoAtual();
        return ResultadoCombate.EmAndamento;
    }

    private ResultadoCombate ProcessarVitoria()
    {
        Jogador.RemoverAliadoDerrotado();

        Item? itemGanho = MonstroAtual.SortearDrop();
        if (itemGanho != null)
        {
            bool guardou = Jogador.Inventario.AdicionarItem(itemGanho);
            LogItemDrop = guardou
                ? $"🎁 Dropou: [{itemGanho.Nome}]!"
                : $"🎁 Dropou [{itemGanho.Nome}], mas o inventário está cheio!";
        }
        else
        {
            LogItemDrop = "";
        }

        SubiuDeNivel = Jogador.GanharExperiencia(MonstroAtual.ExperienciaConcedida);
        if (SubiuDeNivel)
        {
            Jogador.Curar();
            Jogador.RestaurarEnergia(Jogador.EnergiaMaxima);
        }

        return ResultadoCombate.VitoriaJogador;
    }

    public bool TentarUsarAcaoEspecial(Monstro? ultimoMonstroDerrotado, out string mensagemErro)
    {
        mensagemErro = "";

        if (!Jogador.PodeUsarAcaoEspecial)
        {
            mensagemErro = "Este personagem não possui ação especial.";
            return false;
        }

        try
        {
            Jogador.ExecutarAcaoEspecial(ultimoMonstroDerrotado);
            _logRodada.Add($"✨ {Jogador.AliadoAtivo!.Nome} foi reanimado para lutar com você!");
            return true;
        }
        catch (RegraDeJogoException ex)
        {
            mensagemErro = ex.Message;
            return false;
        }
    }

    // Ações

    private static IHabilidade CriarAtaqueBasico(Personagem p)
        => new HabilidadeAtaque("Ataque", danoBase: 0, custo: 0, elemento: p.Elemento);

    private void ExecutarAtaque(Personagem atacante, Personagem defensor, IHabilidade habilidade)
    {
        if (!atacante.EstaVivo() || !defensor.EstaVivo()) return;

        int danoFinal = CalculadoraDeDano.CalcularDano(habilidade, atacante, defensor);
        defensor.ReceberDano(danoFinal);

        bool defensorEstaDefendendo = defensor.TemEstado(TipoEstado.Defendendo);
        string sufixoDefesa = defensorEstaDefendendo ? " (reduzido pela defesa)" : "";
        string textoAcao = habilidade.Nome == "Ataque" ? "atacou" : $"usou [{habilidade.Nome}] em";

        _logRodada.Add($"⚔️ {atacante.Nome} {textoAcao} {defensor.Nome} causando {danoFinal} de dano!{sufixoDefesa}");

        Elemento elementoEfetivo = habilidade.Elemento ?? atacante.Elemento;
        double multiplicador = TabelaElemental.CalcularMultiplicador(elementoEfetivo, defensor.Elemento);
        if (multiplicador > 1.0)
            _logRodada.Add($" 🔥 SUPER EFETIVO! ({elementoEfetivo} > {defensor.Elemento}) | +50% de Dano!");
        else if (multiplicador < 1.0)
            _logRodada.Add($" 🛡️ POUCO EFETIVO... ({elementoEfetivo} < {defensor.Elemento}) | -50% de Dano!");
    }

    private bool AcaoJogador(AcaoDoJogador acao, IHabilidade? habilidade, int indiceItem)
    {
        switch (acao)
        {
            case AcaoDoJogador.Atacar:
                Personagem alvoReal = EscolherAlvoConsiderandoConfusao(Jogador);

                if (alvoReal == Jogador)
                {
                    int danoAuto = Math.Max(1, (Jogador.AtributoDeAtaque - Jogador.Armadura) / 2);
                    Jogador.ReceberDano(danoAuto);
                    _logRodada.Add($"😵 {Jogador.Nome} está confuso e feriu a si mesmo por {danoAuto} de dano!");
                }
                else
                {
                    ExecutarAtaque(Jogador, alvoReal, CriarAtaqueBasico(Jogador));
                }
                return true;

            case AcaoDoJogador.Defender:
                int mpRecuperado = Jogador.Defender();
                _logRodada.Add($"🛡️ {Jogador.Nome} assumiu postura defensiva e recuperou {mpRecuperado} MP!");
                return true;

            case AcaoDoJogador.UsarHabilidade:
                if (habilidade == null) return false;
                if (Jogador.Energia < habilidade.Custo)
                {
                    _logRodada.Add("⚠️ Energia insuficiente!");
                    return false;
                }
                Jogador.GastarEnergia(habilidade.Custo);
                ExecutarAtaque(Jogador, MonstroAtual, habilidade);
                return true;

            case AcaoDoJogador.UsarItem:
                if (indiceItem < 0) return false;
                ResultadoUsoItem resultadoUso = Jogador.Inventario.UsarItem(indiceItem, Jogador);
                switch (resultadoUso)
                {
                    case ResultadoUsoItem.Sucesso:
                        _logRodada.Add("🧪 Item utilizado com sucesso!");
                        return true;
                    case ResultadoUsoItem.VidaCheia:
                        _logRodada.Add($"⚠️ {Jogador.Nome} já está com a vida cheia!");
                        return false;
                    case ResultadoUsoItem.EnergiaCheia:
                        _logRodada.Add($"⚠️ {Jogador.Nome} já está com a energia cheia!");
                        return false;
                    default:
                        _logRodada.Add("⚠️ Item inválido!");
                        return false;
                }

            case AcaoDoJogador.Fugir:
                bool conseguiuFugir = GeradorAleatorio.Rolar(1, 100) <= Jogador.PercentualChanceDeFuga;
                if (conseguiuFugir)
                {
                    _logRodada.Add($"🏃 {Jogador.Nome} conseguiu fugir!");
                    JogadorFugiu = true;
                }
                else
                {
                    _logRodada.Add($"❌ {Jogador.Nome} falhou ao tentar fugir!");
                }
                return true;

            default:
                return false;
        }
    }

    private void AcaoAliado(Personagem aliado)
    {
        if (!aliado.EstaVivo()) return;
        ExecutarAtaque(aliado, MonstroAtual, CriarAtaqueBasico(aliado));
    }

    private void AcaoMonstro()
    {
        bool aliadoPresente = Jogador.AliadoAtivo != null && Jogador.AliadoAtivo.EstaVivo();
        Personagem alvoEscolhido = Jogador;

        if (aliadoPresente)
        {
            int sorteioAlvo = GeradorAleatorio.Rolar(0, 1);
            if (sorteioAlvo == 1)
                alvoEscolhido = Jogador.AliadoAtivo!;
        }

        AcaoDeMonstro acaoEscolhida = MonstroAtual.DecidirAcao(alvoEscolhido, NumeroRodada);
        string nomeAlvoExibicao = (alvoEscolhido == Jogador) ? "você" : alvoEscolhido.Nome;
        bool alvoEstaDefendendo = alvoEscolhido.TemEstado(TipoEstado.Defendendo);
        string sufixo = alvoEstaDefendendo ? " (reduzido pela defesa)" : "";

        int CalcularDanoBase(float multiplicador = 1f)
        {
            var habilidadeMonstro = new HabilidadeAtaque("Ataque", danoBase: 0, custo: 0, elemento: MonstroAtual.Elemento);
            int dano = CalculadoraDeDano.CalcularDano(habilidadeMonstro, MonstroAtual, alvoEscolhido);
            if (alvoEscolhido.TemEstado(TipoEstado.Enfraquecido))
                dano = (int)(dano * MultiplicadoresDeAtaque.DanoAlvoEnfraquecido);
            return (int)(dano * multiplicador);
        }

        switch (acaoEscolhida)
        {
            case AcaoDeMonstro.AtaqueBasico:
            {
                int dano = CalcularDanoBase();
                alvoEscolhido.ReceberDano(dano);
                _logRodada.Add($"💥 {MonstroAtual.Nome} usou [Ataque Básico] em {nomeAlvoExibicao} e causou {dano} de dano!{sufixo}");
                break;
            }

            case AcaoDeMonstro.AtaqueContagiante:
            {
                int dano = CalcularDanoBase();
                alvoEscolhido.ReceberDano(dano);
                AplicarEfeito(alvoEscolhido, acaoEscolhida);
                int vidaRecuperada = MonstroAtual.AplicarEfeitoEspecial(alvoEscolhido, dano);
                _logRodada.Add($"🧟 {MonstroAtual.Nome} usou [Ataque Contagiante] em {nomeAlvoExibicao}, causou {dano} de dano, aplicou Enfraquecido e recuperou {vidaRecuperada} HP!{sufixo}");
                break;
            }

            case AcaoDeMonstro.Confundir:
                AplicarEfeito(alvoEscolhido, acaoEscolhida);
                _logRodada.Add($"🌀 {MonstroAtual.Nome} usou [Confundir] e tentou perturbar a mente de {nomeAlvoExibicao}!");
                break;

            case AcaoDeMonstro.RecarregarEscudo:
                MonstroAtual.RecarregarEscudo();
                _logRodada.Add($"🛡️ {MonstroAtual.Nome} usou [Recarregar Escudo] e restaurou sua barreira!");
                break;

            case AcaoDeMonstro.RompimentoArcano:
            {
                int dano = CalcularDanoBase(MultiplicadoresDeAtaque.RompimentoArcano);
                alvoEscolhido.ReceberDano(dano);
                _logRodada.Add($"🔮 {MonstroAtual.Nome} usou [Rompimento Arcano] em {nomeAlvoExibicao} causando {dano} de dano mágico!{sufixo}");
                break;
            }

            case AcaoDeMonstro.GolpeSelvagem:
            {
                int dano = CalcularDanoBase(MultiplicadoresDeAtaque.GolpeSelvagem);
                alvoEscolhido.ReceberDano(dano);
                _logRodada.Add($"🐺 {MonstroAtual.Nome} desferiu um [Golpe Selvagem] furioso em {nomeAlvoExibicao} causando {dano} de dano!{sufixo}");
                break;
            }

            case AcaoDeMonstro.IlusaoEmMassa:
                AplicarEfeito(Jogador, acaoEscolhida);
                if (Jogador.AliadoAtivo != null && Jogador.AliadoAtivo.EstaVivo())
                    AplicarEfeito(Jogador.AliadoAtivo, acaoEscolhida);
                _logRodada.Add($"✨ {MonstroAtual.Nome} lançou [Ilusão em Massa], distorcendo o campo de batalha!");
                break;

            case AcaoDeMonstro.RaioDeEnergia:
            {
                int dano = CalcularDanoBase(MultiplicadoresDeAtaque.RaioDeEnergia);
                alvoEscolhido.ReceberDano(dano);
                _logRodada.Add($"⚡ {MonstroAtual.Nome} disparou um [Raio de Energia] em {nomeAlvoExibicao} causando {dano} de dano!{sufixo}");
                break;
            }

            case AcaoDeMonstro.MaldicaoEnfraquecedora:
                AplicarEfeito(alvoEscolhido, acaoEscolhida);
                _logRodada.Add($"💀 {MonstroAtual.Nome} lançou uma [Maldição Enfraquecedora] sobre {nomeAlvoExibicao}!");
                break;

            case AcaoDeMonstro.DrenarVida:
            {
                int dano = CalcularDanoBase();
                alvoEscolhido.ReceberDano(dano);
                int vidaRecuperada = MonstroAtual.AplicarEfeitoEspecial(alvoEscolhido, dano);
                _logRodada.Add($"🧛 {MonstroAtual.Nome} usou [Drenar Vida] em {nomeAlvoExibicao}, causou {dano} de dano{sufixo} e recuperou {vidaRecuperada} HP!");
                break;
            }

            case AcaoDeMonstro.TransicaoIntermediaria:
                _logRodada.Add($"⚠️ {MonstroAtual.Nome} rugiu e entrou na **Fase Intermediária**!");
                break;

            case AcaoDeMonstro.TransicaoFinal:
                _logRodada.Add($"🔥 {MonstroAtual.Nome} liberou seu poder máximo e entrou na **Fase Final**!");
                break;

            case AcaoDeMonstro.TempestadeArcanaFinal:
            {
                int dano = CalcularDanoBase(MultiplicadoresDeAtaque.TempestadeArcanaFinal);
                alvoEscolhido.ReceberDano(dano);
                _logRodada.Add($"🌪️ {MonstroAtual.Nome} conjurou a [Tempestade Arcana Final] em {nomeAlvoExibicao} causando {dano} de dano devastador!{sufixo}");
                break;
            }

            case AcaoDeMonstro.UivoDeGuerra:
                AplicarEfeito(alvoEscolhido, acaoEscolhida);
                _logRodada.Add($"🐺 {MonstroAtual.Nome} soltou um [Uivo de Guerra] que atordoou {nomeAlvoExibicao}!");
                break;

            default:
            {
                int dano = CalcularDanoBase();
                alvoEscolhido.ReceberDano(dano);
                _logRodada.Add($"💥 {MonstroAtual.Nome} causou {dano} de dano em {nomeAlvoExibicao}!{sufixo}");
                break;
            }
        }

        if (aliadoPresente && Jogador.AliadoAtivo != null && !Jogador.AliadoAtivo.EstaVivo())
        {
            _logRodada.Add($"💀 O aliado {Jogador.AliadoAtivo.Nome} foi destruído em combate!");
            Jogador.RemoverAliadoDerrotado();
        }
    }

    private void AplicarEfeito(Personagem alvo, AcaoDeMonstro acao)
    {
        TipoEstado? estado = EfeitosDeEstado.ObterEstadoCorrespondente(acao);
        if (estado is not null)
            alvo.AplicarEstado(estado.Value, EfeitosDeEstado.ObterDuracao(acao));
    }

    private Personagem EscolherAlvoConsiderandoConfusao(Personagem atacante)
    {
        if (atacante.TemEstado(TipoEstado.Confuso))
        {
            bool atingeASiMesmo = GeradorAleatorio.Rolar(0, 1) == 0;
            return atingeASiMesmo ? atacante : MonstroAtual;
        }

        return atacante.EscolherAlvo(Jogador, MonstroAtual);
    }
}
