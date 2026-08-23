using ARISE.Dominio.Personagens;

namespace ARISE.Dominio.Itens;

public class PocaoEnergia : Item
{
    public int QuantidadeRestauracao { get; private set; }

    public PocaoEnergia(string nome = "Poção de Energia Pequena", int quantidadeRestauracao = 15) 
        : base(nome, $"Restaura {quantidadeRestauracao} de MP")
    {
        QuantidadeRestauracao = quantidadeRestauracao;
    }

    public override ARISE.Dominio.Enums.ResultadoUsoItem Usar(Personagem alvo)
    {
        if (alvo.Energia >= alvo.EnergiaMaxima)
            return ARISE.Dominio.Enums.ResultadoUsoItem.EnergiaCheia;

        alvo.RestaurarEnergia(QuantidadeRestauracao);
        return ARISE.Dominio.Enums.ResultadoUsoItem.Sucesso;
    }
}
