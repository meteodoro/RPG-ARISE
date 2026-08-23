using ARISE.Dominio.Personagens;

namespace ARISE.Dominio.Itens;

public abstract class Item
{
    public string Nome { get; protected set; }
    public string Descricao { get; protected set; }

    protected Item(string nome, string descricao)
    {
        Nome = nome;
        Descricao = descricao;
    }

    public abstract ARISE.Dominio.Enums.ResultadoUsoItem Usar(Personagem alvo);

    public override string ToString()
    {
        return $"{Nome} - {Descricao}";
    }
}
