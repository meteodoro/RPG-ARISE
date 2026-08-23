using ARISE.Dominio.Personagens;

namespace ARISE.Dominio.Itens;

public class PocaoVida : Item
{
    public int QuantidadeCura { get; private set; }

    public PocaoVida(string nome = "Pocao de Vida P", int quantidadeCura = 30) 
        : base(nome, $"Restaura {quantidadeCura} de HP")
    {
        QuantidadeCura = quantidadeCura;
    }

    public override ARISE.Dominio.Enums.ResultadoUsoItem Usar(Personagem alvo)
    {
        if (alvo.Vida >= alvo.VidaMaxima)
            return ARISE.Dominio.Enums.ResultadoUsoItem.VidaCheia;

        alvo.RestaurarVida(QuantidadeCura);
        return ARISE.Dominio.Enums.ResultadoUsoItem.Sucesso;
    }
}
