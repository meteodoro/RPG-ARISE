using System.Numerics;
using ARISE.Dominio.Enums;
using ARISE.Dominio.Interfaces;
using ARISE.Dominio.Monstros;
using ARISE.Dominio.Personagens;
using ARISE.Regras;
using ARISE.Aplicacao;
using Raylib_cs;
using Color_Raylib = Raylib_cs.Color;

namespace ARISE.Apresentacao;

public static class JogoRaylib
{
    private static EstadoJogo estadoAtual = EstadoJogo.Capa;
    private static Personagem? jogador;
    private static Monstro? monstroAtual;
    private static Monstro? ultimoMonstroDerrotado;
    private static MotorDeTurnos? motor;

    private static Texture2D texturaMonstroAtual;
    private static string caminhoTexturaCarregada = "";
    private static Font fonteEmoji;
    private static Font fonteTexto;
    private static Music musicaFundo;
    private static bool musicaFundoCarregada;

    private static Texture2D texturaCapa;
    private static Texture2D texturaClasse;

    private static float temporizadorTurno = 0f;
    private static bool aguardandoPausa = false;

    private static int indiceFase = 0;
    private static int indiceMonstroFase = 0;
    private static readonly List<(string NomeFase, List<Func<Monstro>> MonstrosDoNivel)> dungeon =
        FabricaDungeon.Criar();

    private static SubmenuCombate submenuAtivo = SubmenuCombate.Principal;
    private static List<IHabilidade> habilidadesAprender = new();
    private static string mensagemStatus = "";
    private static string logItemDrop = "";
    private static float timerTransicao = 0f;

    private static string nomeDigitado = "";
    private static Personagem? classeSelecionada;

    private static float CalcularTempoDeLeitura()
    {
        int totalCaracteres = mensagemStatus.Length;
        return Math.Clamp(totalCaracteres * 0.07f, 3f, 8f);
    }

    public static void Executar()
    {
        Raylib.InitWindow(1280, 720, "★ ARISE: THE DUNGEON CRAWLER ★");
        Raylib.InitAudioDevice();
        Raylib.SetTargetFPS(60);

        string caminhoFonteEmoji = ResolverCaminhoRecurso("Fontes/NotoEmoji-VariableFont_wght.ttf")
                                   ?? @"C:\Windows\Fonts\seguiemj.ttf";
        string caminhoFonteTexto = ResolverCaminhoRecurso("Fontes/Alegreya.bold.ttf")
                                   ?? @"C:\Windows\Fonts\seguiemj.ttf";

        string? caminhoMusica = ResolverCaminhoRecurso("Musicas/A-Sweet-Goodye.ogg");
        if (caminhoMusica != null)
        {
            musicaFundo = Raylib.LoadMusicStream(caminhoMusica);
            Raylib.SetMusicVolume(musicaFundo, 0.25f);
            Raylib.PlayMusicStream(musicaFundo);
            musicaFundoCarregada = true;
        }

        if (System.IO.File.Exists(caminhoFonteEmoji))
        {
            List<int> codepoints = new List<int>();
            for (int i = 0x0020; i <= 0x00FF; i++) codepoints.Add(i);
            for (int i = 0x2600; i <= 0x27BF; i++) codepoints.Add(i);
            for (int i = 0x1F000; i <= 0x1F9FF; i++) codepoints.Add(i);
            int[] arrayCodepoints = codepoints.ToArray();
            fonteEmoji = Raylib.LoadFontEx(caminhoFonteEmoji, 24, arrayCodepoints, arrayCodepoints.Length);
        }
        else
        {
            fonteEmoji = Raylib.GetFontDefault();
        }

        if (System.IO.File.Exists(caminhoFonteTexto))
        {
            List<int> codepointsTexto = new List<int>();
            for (int i = 32; i <= 126; i++) codepointsTexto.Add(i);
            for (int i = 160; i <= 255; i++) codepointsTexto.Add(i);
            int[] arrayCodepointsTexto = codepointsTexto.ToArray();
            fonteTexto = Raylib.LoadFontEx(caminhoFonteTexto, 30, arrayCodepointsTexto, arrayCodepointsTexto.Length);
            Raylib.SetTextureFilter(fonteTexto.Texture, TextureFilter.Bilinear);
        }
        else
        {
            fonteTexto = Raylib.GetFontDefault();
        }

        texturaCapa = Raylib.LoadTexture("Imagens/capa.png");
        texturaClasse = Raylib.LoadTexture("Imagens/classe.png");

        while (!Raylib.WindowShouldClose())
        {
            if (musicaFundoCarregada)
                Raylib.UpdateMusicStream(musicaFundo);

            float deltaTime = Raylib.GetFrameTime();
            Atualizar(deltaTime);

            Raylib.BeginDrawing();
            Raylib.ClearBackground(new Color_Raylib(15, 15, 20, 255));
            Desenhar();
            Raylib.EndDrawing();
        }

        Raylib.UnloadFont(fonteEmoji);
        if (texturaCapa.Id != 0) Raylib.UnloadTexture(texturaCapa);
        if (texturaClasse.Id != 0) Raylib.UnloadTexture(texturaClasse);
        LimparTexturaMonstro();
        if (musicaFundoCarregada)
        {
            Raylib.StopMusicStream(musicaFundo);
            Raylib.UnloadMusicStream(musicaFundo);
        }
        Raylib.CloseAudioDevice();
        Raylib.CloseWindow();
    }

    private static void Atualizar(float deltaTime)
    {
        switch (estadoAtual)
        {
            case EstadoJogo.Capa:
                if (Raylib.IsKeyPressed(KeyboardKey.Enter) || Raylib.IsKeyPressed(KeyboardKey.Space))
                    estadoAtual = EstadoJogo.NomeHeroi;
                break;

            case EstadoJogo.NomeHeroi:
                AtualizarEntradaDeNome();
                break;

            case EstadoJogo.CriacaoPersonagem:
                if (classeSelecionada == null)
                {
                    if (Raylib.IsKeyPressed(KeyboardKey.One)) classeSelecionada = new Engineer(nomeDigitado);
                    if (Raylib.IsKeyPressed(KeyboardKey.Two)) classeSelecionada = new Arcanist(nomeDigitado);
                    if (Raylib.IsKeyPressed(KeyboardKey.Three)) classeSelecionada = new Phantom(nomeDigitado);
                    if (Raylib.IsKeyPressed(KeyboardKey.Four)) classeSelecionada = new Vanguard(nomeDigitado);
                }
                else if (Raylib.IsKeyPressed(KeyboardKey.Zero))
                {
                    classeSelecionada = null;
                }
                else if (Raylib.IsKeyPressed(KeyboardKey.Enter))
                {
                    jogador = classeSelecionada;
                    classeSelecionada = null;
                    ProximaFase();
                }
                break;

            case EstadoJogo.EntrandoFase:
                timerTransicao += deltaTime;
                if (timerTransicao >= 2.0f)
                {
                    timerTransicao = 0f;
                    CarregarProximoMonstro();
                }
                break;

            case EstadoJogo.ApareceuMonstro:
                timerTransicao += deltaTime;
                if (timerTransicao >= 3.0f)
                {
                    timerTransicao = 0f;

                    if (jogador!.PodeUsarAcaoEspecial && ultimoMonstroDerrotado != null &&
                        (jogador.AliadoAtivo == null || !jogador.AliadoAtivo.EstaVivo()))
                        estadoAtual = EstadoJogo.PerguntaReanimar;
                    else
                        IniciarCombate();
                }
                break;

            case EstadoJogo.PerguntaReanimar:
                if (Raylib.IsKeyPressed(KeyboardKey.S) && jogador!.PodeUsarAcaoEspecial && motor != null)
                {
                    bool sucesso = motor.TentarUsarAcaoEspecial(ultimoMonstroDerrotado, out string erro);
                    mensagemStatus = sucesso
                        ? $"{string.Join("\n", motor.LogRodada)}\n⚔️ Selecione o próximo ataque..."
                        : $"⚠️ Falha ao reanimar: {erro}";
                    IniciarCombate();
                }
                if (Raylib.IsKeyPressed(KeyboardKey.N))
                    IniciarCombate();
                break;
            
            case EstadoJogo.Combate:
                AtualizarCombate();
                break;

            case EstadoJogo.LevelUpInstantaneo:
                if (Raylib.IsKeyPressed(KeyboardKey.Enter))
                    estadoAtual = EstadoJogo.ResultadoCombate;
                break;

            case EstadoJogo.ResultadoCombate:
                if (Raylib.IsKeyPressed(KeyboardKey.Enter))
                {
                    indiceMonstroFase++;
                    if (indiceMonstroFase < dungeon[indiceFase].MonstrosDoNivel.Count)
                    {
                        CarregarProximoMonstro();
                    }
                    else
                    {
                        habilidadesAprender = jogador!.ObterOpcoesDeHabilidadesPorNivel(jogador.Nivel);
                        estadoAtual = EstadoJogo.FimDeFase;
                    }
                }
                break;

            case EstadoJogo.FimDeFase:
                if (Raylib.IsKeyPressed(KeyboardKey.Enter))
                {
                    if (habilidadesAprender.Count > 0)
                        estadoAtual = EstadoJogo.LevelUp;
                    else
                        AvancarNivelDungeon();
                }
                break;

            case EstadoJogo.LevelUp:
                for (int i = 0; i < habilidadesAprender.Count; i++)
                {
                    if (Raylib.IsKeyPressed((KeyboardKey)((int)KeyboardKey.One + i)))
                    {
                        jogador!.AprenderHabilidade(habilidadesAprender[i]);
                        mensagemStatus = $"✨ Você aprendeu [{habilidadesAprender[i].Nome}]!";
                        AvancarNivelDungeon();
                        break;
                    }
                }
                break;
        }
    }

    private static void AtualizarEntradaDeNome()
    {
        int tecla = Raylib.GetCharPressed();
        while (tecla > 0)
        {
            if (tecla >= 32 && tecla <= 125 && nomeDigitado.Length < 20)
                nomeDigitado += (char)tecla;

            tecla = Raylib.GetCharPressed();
        }

        if (Raylib.IsKeyPressed(KeyboardKey.Backspace) && nomeDigitado.Length > 0)
            nomeDigitado = nomeDigitado[..^1];

        if (Raylib.IsKeyPressed(KeyboardKey.Enter))
        {
            if (string.IsNullOrWhiteSpace(nomeDigitado))
                nomeDigitado = "Shadow Hunter";

            estadoAtual = EstadoJogo.CriacaoPersonagem;
        }
    }

    // Combate/turnos

    private static void IniciarCombate()
    {
        if (motor == null) return;

        motor.IniciarCombate();
        submenuAtivo = SubmenuCombate.Principal;
        estadoAtual = EstadoJogo.Combate;

        SincronizarAposMotor();
    }

    private static void AtualizarCombate()
    {
        if (jogador == null || monstroAtual == null || motor == null) return;

        if (aguardandoPausa)
        {
            temporizadorTurno += Raylib.GetFrameTime();
            bool pulou = Raylib.IsKeyPressed(KeyboardKey.Enter) || Raylib.IsKeyPressed(KeyboardKey.Space);
            if (temporizadorTurno >= CalcularTempoDeLeitura() || pulou)
            {
                temporizadorTurno = 0f;
                aguardandoPausa = false;
                TratarResultadoCombate(motor.AvancarTurno());
            }
            return;
        }

        if (motor.AguardandoAcaoDoJogador)
            AtualizarMenuDoJogador();
    }

    private static void SincronizarAposMotor()
    {
        if (motor == null) return;

        mensagemStatus = string.Join("\n", motor.LogRodada);

        if (!motor.AguardandoAcaoDoJogador)
        {
            aguardandoPausa = true;
            temporizadorTurno = 0f;
        }
    }

    private static void TratarResultadoCombate(ResultadoCombate resultado)
    {
        if (motor == null) return;

        switch (resultado)
        {
            case ResultadoCombate.EmAndamento:
                SincronizarAposMotor();
                break;

            case ResultadoCombate.FugaJogador:
                estadoAtual = EstadoJogo.FimDeJogo;
                break;

            case ResultadoCombate.DerrotaJogador:
                estadoAtual = EstadoJogo.FimDeJogo;
                break;

            case ResultadoCombate.VitoriaJogador:
                ultimoMonstroDerrotado = motor.MonstroAtual;
                logItemDrop = motor.LogItemDrop;
                estadoAtual = motor.SubiuDeNivel ? EstadoJogo.LevelUpInstantaneo : EstadoJogo.ResultadoCombate;
                break;
        }
    }

    private static void AtualizarMenuDoJogador()
    {
        if (jogador == null || motor == null) return;

        if (submenuAtivo == SubmenuCombate.Principal)
        {
            if (Raylib.IsKeyPressed(KeyboardKey.One))
                ExecutarAcaoDoJogador(AcaoDoJogador.Atacar, null, -1);

            else if (Raylib.IsKeyPressed(KeyboardKey.Two))
                ExecutarAcaoDoJogador(AcaoDoJogador.Defender, null, -1);

            else if (Raylib.IsKeyPressed(KeyboardKey.Three))
            {
                if (jogador.Habilidades.Count > 0) submenuAtivo = SubmenuCombate.Habilidades;
                else mensagemStatus = "⚠️ Você não possui habilidades!";
            }
            else if (Raylib.IsKeyPressed(KeyboardKey.Four))
            {
                if (jogador.Inventario.Itens.Count > 0) submenuAtivo = SubmenuCombate.Inventario;
                else mensagemStatus = "⚠️ Seu inventário está vazio!";
            }
            else if (Raylib.IsKeyPressed(KeyboardKey.Five))
                ExecutarAcaoDoJogador(AcaoDoJogador.Fugir, null, -1);

            else if (Raylib.IsKeyPressed(KeyboardKey.Six) && jogador!.PodeUsarAcaoEspecial)
            {
                bool sucesso = motor.TentarUsarAcaoEspecial(ultimoMonstroDerrotado, out string erro);
                mensagemStatus = sucesso ? string.Join("\n", motor.LogRodada) : $"⚠️ {erro}";
            }
        }
        else if (submenuAtivo == SubmenuCombate.Habilidades)
        {
            if (Raylib.IsKeyPressed(KeyboardKey.Zero)) submenuAtivo = SubmenuCombate.Principal;
            for (int i = 0; i < jogador.Habilidades.Count; i++)
            {
                if (Raylib.IsKeyPressed((KeyboardKey)((int)KeyboardKey.One + i)))
                {
                    submenuAtivo = SubmenuCombate.Principal;
                    ExecutarAcaoDoJogador(AcaoDoJogador.UsarHabilidade, jogador.Habilidades[i], -1);
                }
            }
        }
        else if (submenuAtivo == SubmenuCombate.Inventario)
        {
            if (Raylib.IsKeyPressed(KeyboardKey.Zero)) submenuAtivo = SubmenuCombate.Principal;
            for (int i = 0; i < jogador.Inventario.Itens.Count; i++)
            {
                if (Raylib.IsKeyPressed((KeyboardKey)((int)KeyboardKey.One + i)))
                {
                    submenuAtivo = SubmenuCombate.Principal;
                    ExecutarAcaoDoJogador(AcaoDoJogador.UsarItem, null, i);
                }
            }
        }
    }

    private static void ExecutarAcaoDoJogador(AcaoDoJogador acao, IHabilidade? habilidade, int indiceItem)
    {
        if (motor == null) return;

        bool sucesso = motor.ExecutarAcaoJogador(acao, habilidade, indiceItem);
        if (!sucesso)
        {
            mensagemStatus = motor.LogRodada.Count > 0
                ? string.Join("\n", motor.LogRodada)
                : "⚠️ Ação cancelada ou item indisponível.";
            return;
        }

        mensagemStatus = string.Join("\n", motor.LogRodada);
        aguardandoPausa = true;
        temporizadorTurno = 0f;
    }

    // Controle de níveis/fases

    private static void AvancarNivelDungeon()
    {
        indiceFase++;
        indiceMonstroFase = 0;
        if (indiceFase >= dungeon.Count)
            estadoAtual = EstadoJogo.VitoriaFinal;
        else
            ProximaFase();
    }

    private static void ProximaFase()
    {
        timerTransicao = 0f;
        estadoAtual = EstadoJogo.EntrandoFase;
    }

    private static void CarregarProximoMonstro()
    {
        monstroAtual = dungeon[indiceFase].MonstrosDoNivel[indiceMonstroFase]();
        CarregarImagemMonstro(monstroAtual.CaminhoImagem);

        motor = new MotorDeTurnos(jogador!, monstroAtual);
        motor.CalcularIniciativa();

        timerTransicao = 0f;
        estadoAtual = EstadoJogo.ApareceuMonstro;
    }

    // Parte visual/interface
    
    private static void CarregarImagemMonstro(string caminho)
    {
        if (caminhoTexturaCarregada == caminho) return;

        LimparTexturaMonstro();

        string? caminhoResolvido = ResolverCaminhoRecurso(caminho);
        if (caminhoResolvido != null)
        {
            texturaMonstroAtual = Raylib.LoadTexture(caminhoResolvido);
            caminhoTexturaCarregada = caminho;
        }
    }

    private static string? ResolverCaminhoRecurso(string caminhoRelativo)
    {
        if (string.IsNullOrEmpty(caminhoRelativo)) return null;

        string caminhoNaBase = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, caminhoRelativo);
        if (File.Exists(caminhoNaBase)) return caminhoNaBase;

        if (File.Exists(caminhoRelativo)) return caminhoRelativo;

        return null;
    }

    private static void LimparTexturaMonstro()
    {
        if (texturaMonstroAtual.Id != 0)
        {
            Raylib.UnloadTexture(texturaMonstroAtual);
            texturaMonstroAtual = default;
            caminhoTexturaCarregada = "";
        }
    }

    private static void Desenhar()
    {
        switch (estadoAtual)
        {
            case EstadoJogo.Capa:
                if (texturaCapa.Id != 0)
                {
                    Rectangle recOrigem = new Rectangle(0, 0, texturaCapa.Width, texturaCapa.Height);
                    Rectangle recDestino = new Rectangle(0, 0, 1280, 720);
                    Raylib.DrawTexturePro(texturaCapa, recOrigem, recDestino, Vector2.Zero, 0f, Color_Raylib.White);

                    DesenharTextoCentralizado("A R I S E", 580, 50, Color_Raylib.Magenta);
                    DesenharTextoCentralizado("THE DUNGEON CRAWLER", 620, 30, Color_Raylib.Magenta);
                    DesenharTextoCentralizado("Pressione [ENTER] ou [ESPAÇO] para iniciar", 660, 30, Color_Raylib.White);
                }
                break;

            case EstadoJogo.NomeHeroi:
                DesenharTextoMisto("DIGITE O NOME DO SEU HERÓI:", new Vector2(420, 280), 35, 1, Color_Raylib.Magenta);
                DesenharTextoMisto(nomeDigitado + "_", new Vector2(420, 340), 30, 1, Color_Raylib.RayWhite);
                DesenharTextoMisto("Pressione [ENTER] para confirmar", new Vector2(420, 420), 25, 1, Color_Raylib.Gray);
                break;

            case EstadoJogo.CriacaoPersonagem:
                if (texturaClasse.Id != 0)
                {
                    Rectangle recOrigem = new Rectangle(0, 0, texturaClasse.Width, texturaClasse.Height);
                    Rectangle recDestino = new Rectangle(0, 0, 1280, 720);
                    Raylib.DrawTexturePro(texturaClasse, recOrigem, recDestino, Vector2.Zero, 0f, Color_Raylib.White);
                }
                else
                {
                    Raylib.ClearBackground(Color_Raylib.DarkGray);
                }
                DesenharTextoMisto("ESCOLHA SUA CLASSE:", new Vector2(480, 500), 35, 1, Color_Raylib.Magenta);
                Raylib.DrawTextEx(fonteTexto, "[1] Engineer (Especialista em Reanimação de Cadáveres)", new Vector2(380, 550), 30, 1, Color_Raylib.RayWhite);
                Raylib.DrawTextEx(fonteTexto, "[2] Arcanist (Mestre do Dano Elemental Mágico)", new Vector2(380, 590), 30, 1, Color_Raylib.RayWhite);
                Raylib.DrawTextEx(fonteTexto, "[3] Phantom (Assassino Furtivo de Alto Reflexo)", new Vector2(380, 630), 30, 1, Color_Raylib.RayWhite);
                Raylib.DrawTextEx(fonteTexto, "[4] Vanguard (Tanque de Força Bruta e Alta Armadura)", new Vector2(380, 670), 30, 1, Color_Raylib.RayWhite);

                if (classeSelecionada != null)
                    DesenharQuadroDaClasse(classeSelecionada);
                break;

            case EstadoJogo.EntrandoFase:
                DesenharTextoMisto($"ENTRANDO NO {dungeon[indiceFase].NomeFase}", new Vector2(450, 320), 40, 1, Color_Raylib.Magenta);
                break;

            case EstadoJogo.ApareceuMonstro:
            case EstadoJogo.PerguntaReanimar:
            case EstadoJogo.Combate:
                DesenharInterfaceCombate();
                break;

            case EstadoJogo.LevelUpInstantaneo:
                DesenharTelaLevelUpInstantaneo();
                break;

            case EstadoJogo.ResultadoCombate:
                DesenharImagemMonstroCentralizada(320f, 80);
                DesenharTextoMisto($"🎉 Você derrotou {monstroAtual?.Nome}!", new Vector2(420, 440), 35, 1, Color_Raylib.Gold);
                if (!string.IsNullOrEmpty(logItemDrop))
                    DesenharTextoMisto(logItemDrop, new Vector2(420, 480), 25, 1, Color_Raylib.White);
                DesenharTextoMisto($"🌟 XP Ganho: +{monstroAtual?.ExperienciaConcedida} XP", new Vector2(420, 510), 25, 1, Color_Raylib.White);
                DesenharBarra(420, 540, 400, 20, jogador!.ExperienciaAtual, jogador.ExperienciaProximoNivel, Color_Raylib.Yellow);
                DesenharTextoMisto("Pressione [ENTER] para continuar", new Vector2(450, 600), 25, 1, Color_Raylib.White);
                break;

            case EstadoJogo.FimDeFase:
                DesenharTelaFimDeFase();
                break;

            case EstadoJogo.LevelUp:
                DesenharTextoMisto("🔮 ESCOLHA UMA NOVA HABILIDADE:", new Vector2(420, 150), 28, 1, Color_Raylib.Gold);
                for (int i = 0; i < habilidadesAprender.Count; i++)
                {
                    var h = habilidadesAprender[i];
                    DesenharTextoMisto($"[{i + 1}] 📜 {h.Nome} (Dano: {h.DanoBase} | Custo: {h.Custo} MP | Elemento: {h.Elemento})", new Vector2(420, 250 + (i * 50)), 22, 1, Color_Raylib.White);
                }
                break;

            case EstadoJogo.VitoriaFinal:
                DesenharTextoMisto("🏆 PARABÉNS! VOCÊ ZEROU A DUNGEON E DERROTOU IGRIS!", new Vector2(300, 320), 28, 1, Color_Raylib.Gold);
                break;

            case EstadoJogo.FimDeJogo:
                if (motor != null && motor.JogadorFugiu)
                    DesenharTextoMisto("🏃 Você fugiu da Dungeon!", new Vector2(450, 320), 32, 1, Color_Raylib.Gold);
                else
                    DesenharTextoMisto("☠️ VOCÊ MORREU! Fim de jogo.", new Vector2(450, 320), 32, 1, Color_Raylib.Red);
                break;
        }
    }

    private static void DesenharTelaLevelUpInstantaneo()
    {
        if (jogador == null) return;
        DesenharImagemMonstroCentralizada(280f, 60);
        DesenharTextoMisto($"★ LEVEL UP! {jogador.Nome.ToUpper()} ALCANÇOU O NÍVEL {jogador.Nivel}! ★", new Vector2(350, 380), 30, 1, Color_Raylib.Gold);
        DesenharTextoMisto("✨ Atributos aumentados!", new Vector2(420, 430), 25, 1, Color_Raylib.White);
        DesenharTextoMisto("✨ Vida e Energia restauradas!", new Vector2(420, 460), 25, 1, Color_Raylib.White);
        DesenharTextoMisto("HP:", new Vector2(420, 500), 25, 1, Color_Raylib.White);
        DesenharBarra(460, 500, 300, 18, jogador.Vida, jogador.VidaMaxima, Color_Raylib.Green);
        DesenharTextoMisto("MP:", new Vector2(420, 530), 25, 1, Color_Raylib.White);
        DesenharBarra(460, 530, 300, 18, jogador.Energia, jogador.EnergiaMaxima, Color_Raylib.SkyBlue);
        DesenharTextoMisto("Pressione [ENTER] para continuar", new Vector2(450, 600), 25, 1, Color_Raylib.White);
    }

    private static void DesenharTelaFimDeFase()
    {
        if (jogador == null) return;
        DesenharTextoMisto($"🎉 VOCÊ CONCLUIU O {dungeon[indiceFase].NomeFase}!", new Vector2(420, 120), 28, 1, Color_Raylib.Gold);
        DesenharTextoMisto($"👤 {jogador.Nome} (Nível {jogador.Nivel})", new Vector2(420, 200), 25, 1, Color_Raylib.White);
        DesenharTextoMisto("HP:", new Vector2(420, 250), 25, 1, Color_Raylib.White);
        DesenharBarra(460, 250, 300, 18, jogador.Vida, jogador.VidaMaxima, Color_Raylib.Green);
        DesenharTextoMisto("MP:", new Vector2(420, 280), 25, 1, Color_Raylib.White);
        DesenharBarra(460, 280, 300, 18, jogador.Energia, jogador.EnergiaMaxima, Color_Raylib.SkyBlue);
        DesenharTextoMisto("XP:", new Vector2(420, 310), 25, 1, Color_Raylib.White);
        DesenharBarra(460, 310, 300, 18, jogador.ExperienciaAtual, jogador.ExperienciaProximoNivel, Color_Raylib.Yellow);
        DesenharTextoMisto("Pressione [ENTER] para continuar", new Vector2(450, 400), 20, 1, Color_Raylib.White);
    }

    private static void DesenharInterfaceCombate()
    {
        if (jogador == null || monstroAtual == null) return;

        DesenharImagemMonstroCentralizada(320f, 60);
        Raylib.DrawRectangle(40, 40, 380, 220, new Color_Raylib(30, 30, 45, 230));
        Raylib.DrawRectangleLines(40, 40, 380, 220, Color_Raylib.Magenta);
        DesenharTextoMisto($"👤 {jogador.Nome} - {jogador.Elemento} (Niv {jogador.Nivel})", new Vector2(55, 50), 28, 1, Color_Raylib.Magenta);
        DesenharTextoMisto($"HP: {jogador.Vida}/{jogador.VidaMaxima}", new Vector2(55, 80), 22, 1, Color_Raylib.White);
        DesenharBarra(180, 80, 220, 16, jogador.Vida, jogador.VidaMaxima, Color_Raylib.Green);
        DesenharTextoMisto($"MP: {jogador.Energia}/{jogador.EnergiaMaxima}", new Vector2(55, 110), 22, 1, Color_Raylib.White);
        DesenharBarra(180, 110, 220, 16, jogador.Energia, jogador.EnergiaMaxima, Color_Raylib.SkyBlue);
        DesenharTextoMisto($"XP: {jogador.ExperienciaAtual}/{jogador.ExperienciaProximoNivel}", new Vector2(55, 140), 22, 1, Color_Raylib.White);
        DesenharBarra(180, 140, 220, 16, jogador.ExperienciaAtual, jogador.ExperienciaProximoNivel, Color_Raylib.Yellow);

        float proximoY = 165;
        const float alturaLinha = 24;
        if (jogador.AliadoAtivo != null && jogador.AliadoAtivo.EstaVivo())
        {
            DesenharTextoMisto($"🧟 Aliado: {jogador.AliadoAtivo.Nome} (HP: {jogador.AliadoAtivo.Vida})", new Vector2(55, proximoY), 22, 1, Color_Raylib.Purple);
            proximoY += alturaLinha;
        }
        if (jogador.TemEstado(TipoEstado.Defendendo))
        {
            DesenharTextoMisto("🛡️ Defendendo neste round", new Vector2(55, proximoY), 22, 1, Color_Raylib.SkyBlue);
            proximoY += alturaLinha;
        }
        if (jogador.TemEstado(TipoEstado.Confuso))
        {
            DesenharTextoMisto("😵 Confuso!", new Vector2(55, proximoY), 22, 1, Color_Raylib.Orange);
            proximoY += alturaLinha;
        }
        if (jogador.TemEstado(TipoEstado.Enfraquecido))
        {
            DesenharTextoMisto("💀 Enfraquecido!", new Vector2(55, proximoY), 22, 1, Color_Raylib.Purple);
            proximoY += alturaLinha;
        }

        Raylib.DrawRectangle(860, 40, 380, 140, new Color_Raylib(45, 30, 30, 230));
        Raylib.DrawRectangleLines(860, 40, 380, 140, Color_Raylib.Red);
        DesenharTextoMisto($"⚠️ {monstroAtual.Nome} - {monstroAtual.Elemento} (Rank {monstroAtual.Rank})", new Vector2(875, 50), 28, 1, Color_Raylib.Red);
        DesenharTextoMisto($"HP: {monstroAtual.Vida}/{monstroAtual.VidaMaxima}", new Vector2(875, 90), 22, 1, Color_Raylib.White);
        DesenharBarra(1000, 90, 220, 16, monstroAtual.Vida, monstroAtual.VidaMaxima, Color_Raylib.Red);

        int? escudoAtual = monstroAtual is Igris igris ? igris.EscudoDeBarreira : null;

        if (escudoAtual.HasValue)
        {
            const int escudoMaximo = 35;
            DesenharTextoMisto($"Escudo: {escudoAtual}/{escudoMaximo}", new Vector2(875, 120), 20, 1, Color_Raylib.SkyBlue);
            DesenharBarra(1000, 120, 220, 14, escudoAtual.Value, escudoMaximo, Color_Raylib.SkyBlue);
        }

        Raylib.DrawRectangle(40, 480, 1200, 200, new Color_Raylib(25, 25, 35, 240));
        Raylib.DrawRectangleLines(40, 480, 1200, 200, Color_Raylib.Gray);
        
        if (motor != null)
            DesenharTextoMisto($"Rodada {motor.NumeroRodada}", new Vector2(1120, 490), 25, 1, Color_Raylib.Gray);

        if (estadoAtual == EstadoJogo.ApareceuMonstro)
        {
            DesenharTextoMisto($"⚠️ Um {monstroAtual.Nome} (Rank {monstroAtual.Rank}) apareceu!", new Vector2(60, 520), 28, 1, Color_Raylib.Gold);
            DesenharTextoMisto($"Seu reflexo: {jogador.Reflexo}", new Vector2(65, 560), 22, 1, Color_Raylib.White);
            DesenharTextoMisto($"Reflexo de {monstroAtual.Nome}: {monstroAtual.Reflexo}", new Vector2(65, 590), 22, 1, Color_Raylib.White);
            string mensagemIniciativa = motor?.MensagemIniciativa ?? string.Empty;
            DesenharTextoMisto(mensagemIniciativa, new Vector2(60, 620), 25, 1, Color_Raylib.Magenta);
        }
        else if (estadoAtual == EstadoJogo.PerguntaReanimar)
        {
            DesenharTextoMisto($"🧟 Deseja reanimar o corpo de {ultimoMonstroDerrotado?.Nome}? (Custo: {Engineer.CustoDeReanimar} MP)", new Vector2(60, 520), 25, 1, Color_Raylib.Orange);
            DesenharTextoMisto("Pressione [S] para Sim ou [N] para Não", new Vector2(60, 560), 25, 1, Color_Raylib.White);
        }
        else if (estadoAtual == EstadoJogo.Combate)
        {
            if (submenuAtivo == SubmenuCombate.Principal)
            {
                Raylib.DrawTextEx(fonteTexto, "Escolha sua ação:", new Vector2(60, 485), 25, 1, Color_Raylib.Gray);

                Raylib.DrawTextEx(fonteTexto, "[1]          Atacar", new Vector2(60, 515), 25, 1, Color_Raylib.White);
                Raylib.DrawTextEx(fonteEmoji, "⚔️", new Vector2(85, 515), 20, 0, Color_Raylib.White);
                Raylib.DrawTextEx(fonteTexto, "[4]          Item", new Vector2(60, 545), 25, 1, Color_Raylib.White);
                Raylib.DrawTextEx(fonteEmoji, "🧪", new Vector2(85, 545), 20, 0, Color_Raylib.White);

                Raylib.DrawTextEx(fonteTexto, "[2]          Defender", new Vector2(240, 515), 25, 1, Color_Raylib.White);
                Raylib.DrawTextEx(fonteEmoji, "🛡️", new Vector2(265, 515), 20, 0, Color_Raylib.White);
                Raylib.DrawTextEx(fonteTexto, "[5]          Fugir", new Vector2(240, 545), 25, 1, Color_Raylib.White);
                Raylib.DrawTextEx(fonteEmoji, "🏃", new Vector2(265, 545), 20, 0, Color_Raylib.White);

                Raylib.DrawTextEx(fonteTexto, "[3]          Habilidade", new Vector2(430, 515), 25, 1, Color_Raylib.White);
                Raylib.DrawTextEx(fonteEmoji, "🔮", new Vector2(455, 515), 20, 0, Color_Raylib.White);

                if (jogador.PodeUsarAcaoEspecial)
                {
                    Raylib.DrawTextEx(fonteTexto, "[6]          Reanimar", new Vector2(430, 545), 25, 1, Color_Raylib.White);
                    Raylib.DrawTextEx(fonteEmoji, "🧟", new Vector2(455, 545), 20, 0, Color_Raylib.White);
                }
            }
            else if (submenuAtivo == SubmenuCombate.Habilidades)
            {
                DesenharTextoMisto("🔮 HABILIDADES (Pressione o número ou [0] para voltar):", new Vector2(60, 500), 25, 1, Color_Raylib.Gold);
                for (int i = 0; i < jogador.Habilidades.Count; i++)
                {
                    var h = jogador.Habilidades[i];
                    DesenharTextoMisto($"[{i + 1}] {h.Nome} (Dano: {h.DanoBase} | Custo: {h.Custo} MP | {h.Elemento})", new Vector2(60, 530 + (i * 25)), 25, 1, Color_Raylib.White);
                }
            }
            else if (submenuAtivo == SubmenuCombate.Inventario)
            {
                DesenharTextoMisto("🧪 INVENTÁRIO (Pressione o número ou [0] para voltar):", new Vector2(60, 500), 25, 1, Color_Raylib.Gold);
                for (int i = 0; i < jogador.Inventario.Itens.Count; i++)
                {
                    DesenharTextoMisto($"[{i + 1}] {jogador.Inventario.Itens[i]}", new Vector2(60, 530 + (i * 25)), 25, 1, Color_Raylib.White);
                }
            }

            if (submenuAtivo == SubmenuCombate.Principal && !string.IsNullOrEmpty(mensagemStatus))
            {
                string[] linhas = mensagemStatus.Split('\n', StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < linhas.Length; i++)
                {
                    DesenharTextoMisto(linhas[i], new Vector2(60, 580 + (i * 26)), 25, 1, Color_Raylib.Yellow);
                }
            }
        }
    }

    private static void DesenharBarra(int x, int y, int largura, int altura, int valorAtual, int valorMaximo, Color_Raylib cor)
    {
        Raylib.DrawRectangle(x, y, largura, altura, new Color_Raylib(50, 50, 50, 255));
        float porcentagem = Math.Clamp((float)valorAtual / valorMaximo, 0f, 1f);
        Raylib.DrawRectangle(x, y, (int)(largura * porcentagem), altura, cor);
        Raylib.DrawRectangleLines(x, y, largura, altura, Color_Raylib.LightGray);
    }

    private static void DesenharQuadroDaClasse(Personagem classe)
    {
        const int x = 365;
        const int y = 105;
        const int largura = 550;
        const int altura = 500;

        Raylib.DrawRectangle(0, 0, 1280, 720, new Color_Raylib(0, 0, 0, 150));
        Raylib.DrawRectangle(x, y, largura, altura, new Color_Raylib(18, 13, 29, 245));
        Raylib.DrawRectangleLinesEx(new Rectangle(x, y, largura, altura), 3f, Color_Raylib.Magenta);

        string nomeClasse = classe.GetType().Name.ToUpperInvariant();
        DesenharTextoCentralizado(nomeClasse, y + 25, 38, Color_Raylib.Magenta);
        DesenharTextoCentralizado($"ELEMENTO: {classe.Elemento}", y + 75, 25, Color_Raylib.Gold);

        int colunaEsquerda = x + 55;
        int colunaDireita = x + 300;
        int linhaInicial = y + 135;
        const int espacamentoLinha = 42;

        DesenharTextoMisto($"VIDA: {classe.VidaMaxima}", new Vector2(colunaEsquerda, linhaInicial), 27, 1, Color_Raylib.RayWhite);
        DesenharTextoMisto($"ENERGIA: {classe.EnergiaMaxima}", new Vector2(colunaDireita, linhaInicial), 27, 1, Color_Raylib.RayWhite);
        DesenharTextoMisto($"FÍSICO: {classe.Fisico}", new Vector2(colunaEsquerda, linhaInicial + espacamentoLinha), 27, 1, Color_Raylib.RayWhite);
        DesenharTextoMisto($"REFLEXO: {classe.Reflexo}", new Vector2(colunaDireita, linhaInicial + espacamentoLinha), 27, 1, Color_Raylib.RayWhite);
        DesenharTextoMisto($"TÉCNICA: {classe.Tecnica}", new Vector2(colunaEsquerda, linhaInicial + espacamentoLinha * 2), 27, 1, Color_Raylib.RayWhite);
        DesenharTextoMisto($"FOCO: {classe.Foco}", new Vector2(colunaDireita, linhaInicial + espacamentoLinha * 2), 27, 1, Color_Raylib.RayWhite);
        DesenharTextoMisto($"ARMADURA: {classe.Armadura}", new Vector2(colunaEsquerda, linhaInicial + espacamentoLinha * 3), 27, 1, Color_Raylib.RayWhite);

        string atributoAtaque = classe switch
        {
            Engineer => $"TÉCNICA ({classe.AtributoDeAtaque})",
            Arcanist => $"FOCO ({classe.AtributoDeAtaque})",
            Phantom => $"REFLEXO ({classe.AtributoDeAtaque})",
            Vanguard => $"FÍSICO ({classe.AtributoDeAtaque})",
            _ => classe.AtributoDeAtaque.ToString()
        };

        DesenharTextoCentralizado($"ATRIBUTO DE ATAQUE: {atributoAtaque}", y + 325, 26, Color_Raylib.Gold);
        DesenharTextoCentralizado("[ENTER] CONFIRMAR     [0] VOLTAR", y + 435, 25, Color_Raylib.White);
    }

    private static void DesenharImagemMonstroCentralizada(float larguraDesejada, float alturaDesejada)
    {
        if (texturaMonstroAtual.Id == 0) return;
        Rectangle recOrigem = new Rectangle(0, 0, texturaMonstroAtual.Width, texturaMonstroAtual.Height);
        Rectangle recDestino = new Rectangle(0, 0, 1280, 720);
        Vector2 origem = new Vector2(0, 0);
        Raylib.DrawTexturePro(texturaMonstroAtual, recOrigem, recDestino, origem, 0f, Color_Raylib.White);
    }

    private static void DesenharTextoCentralizado(string texto, int posY, float tamanhoFonte, Color_Raylib cor, float espacamento = 1f)
    {
        Vector2 tamanho = MedirTextoMisto(texto, tamanhoFonte, espacamento);
        int posX = (int)((1280 - tamanho.X) / 2);
        DesenharTextoMisto(texto, new Vector2(posX, posY), tamanhoFonte, espacamento, cor);
    }

    private static List<(Font Fonte, string Trecho)> DividirPorFonte(string texto)
    {
        var resultado = new List<(Font, string)>();
        int i = 0;
        while (i < texto.Length)
        {
            int codepoint = char.ConvertToUtf32(texto, i);
            int tamanhoChar = char.IsSurrogatePair(texto, i) ? 2 : 1;
            bool ehEmoji = codepoint > 0x00FF;
            Font fonteAtual = ehEmoji ? fonteEmoji : fonteTexto;
            int fim = i + tamanhoChar;
            while (fim < texto.Length)
            {
                int proximoCp = char.ConvertToUtf32(texto, fim);
                bool proximoEhEmoji = proximoCp > 0x00FF;
                if (proximoEhEmoji != ehEmoji) break;
                fim += char.IsSurrogatePair(texto, fim) ? 2 : 1;
            }
            resultado.Add((fonteAtual, texto.Substring(i, fim - i)));
            i = fim;
        }
        return resultado;
    }

    private static Vector2 MedirTextoMisto(string texto, float tamanhoFonte, float espacamento = 1f)
    {
        float largura = 0f;
        float altura = tamanhoFonte;
        foreach (var (fonte, trecho) in DividirPorFonte(texto))
        {
            Vector2 tam = Raylib.MeasureTextEx(fonte, trecho, tamanhoFonte, espacamento);
            largura += tam.X;
            altura = Math.Max(altura, tam.Y);
        }
        return new Vector2(largura, altura);
    }

    private static void DesenharTextoMisto(string texto, Vector2 pos, float tamanhoFonte, float espacamento, Color_Raylib cor)
    {
        float x = pos.X;
        foreach (var (fonte, trecho) in DividirPorFonte(texto))
        {
            Raylib.DrawTextEx(fonte, trecho, new Vector2(x, pos.Y), tamanhoFonte, espacamento, cor);
            Vector2 tam = Raylib.MeasureTextEx(fonte, trecho, tamanhoFonte, espacamento);
            x += tam.X;
        }
    }
}
