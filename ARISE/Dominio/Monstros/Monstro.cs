using ARISE.Dominio.Enums;
using ARISE.Dominio.Itens;
using ARISE.Dominio.Personagens;
using ARISE.Regras;

namespace ARISE.Dominio.Monstros;

public abstract class Monstro : Personagem
{
    public abstract Rank Rank { get; }
    
    public abstract AcaoDeMonstro DecidirAcao(Personagem alvo, int rodadaAtual);
    
    public virtual int ExperienciaConcedida { get; protected set; } = 50;
    
    public List<EntradaDeDrop> TabelaDeDrops { get; protected set; } = new();
    
    public Item? SortearDrop()
    {
        int pesoTotal = TabelaDeDrops.Sum(e => e.Peso);
        if (pesoTotal <= 0) return null;

        int sorteio = GeradorAleatorio.Rolar(1, pesoTotal);
        int acumulado = 0;

        foreach (var entrada in TabelaDeDrops)
        {
            acumulado += entrada.Peso;
            if (sorteio <= acumulado)
                return entrada.Item;
        }

        return null; // segurança, não deveria chegar aqui se os pesos batem
    }   
    
    public string CaminhoImagem { get; protected set; } = string.Empty;        
    public virtual int AplicarEfeitoEspecial(Personagem alvo, int danoCausado)
    {
        return 0;
    }
    
    public virtual void RecarregarEscudo()
    {
        // Sobrescrito por quem usa escudo (Sentinel, Igris)
    }
    
    public override Personagem EscolherAlvo(Personagem jogador, Personagem monstro)
    {
        if (jogador.AliadoAtivo != null && jogador.AliadoAtivo.EstaVivo())
            return GeradorAleatorio.Rolar(1, 2) == 1 ? jogador : jogador.AliadoAtivo;
        return jogador;
    }
}
