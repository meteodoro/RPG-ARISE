using ARISE.Dominio.Enums;
using ARISE.Dominio.Interfaces;
using ARISE.Dominio.Personagens;
using ARISE.Regras;

namespace ARISE.Dominio.Habilidades;

public class HabilidadeAtaque : IHabilidade
{
    public string Nome { get; }
    public Elemento? Elemento { get; }
    public int DanoBase { get; }
    public int Custo { get; }

    public HabilidadeAtaque(string nome, int danoBase, int custo, Elemento? elemento = null)
    {
        Nome = nome;
        DanoBase = danoBase;
        Custo = custo;
        Elemento = elemento;
    }

    public string Executar(Personagem conjurador, IAtacavel alvo)
    {
        if (!conjurador.EstaVivo() || !alvo.EstaVivo)
            return string.Empty;

        bool alvoDefendendo = alvo.TemEstado(TipoEstado.Defendendo);
        int danoFinal = CalculadoraDeDano.CalcularDano(this, conjurador, alvo);

        alvo.ReceberDano(danoFinal);

        string sufixoDefesa = alvoDefendendo ? " (reduzido pela defesa)" : "";
        string mensagem = $"⚔️ {conjurador.Nome} usou [{Nome}] em {alvo.Nome} causando {danoFinal} de dano!{sufixoDefesa}";

        Elemento elementoEfetivo = Elemento ?? conjurador.Elemento;
        double multiplicador = TabelaElemental.CalcularMultiplicador(elementoEfetivo, alvo.Elemento);
        if (multiplicador > 1.0)
            mensagem += " 🔥 SUPER EFETIVO!";
        else if (multiplicador < 1.0)
            mensagem += " 🛡️ POUCO EFETIVO...";

        return mensagem;
    }
}
