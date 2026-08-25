using ARISE.Dominio.Personagens;
using System.Collections.Generic;

namespace ARISE.Dominio.Itens;

public class Inventario
{
    // Lista exposta que o Program.cs acessa via jogador.Inventario.Itens
    public List<Item> Itens { get; private set; }
    public int CapacidadeMaxima { get; private set; }

    public Inventario(int capacidadeMaxima = 10)
    {
        CapacidadeMaxima = capacidadeMaxima;
        Itens = new List<Item>();
        
        AdicionarItem(new PocaoVida("Poção de Vida Pequena", 20));
        AdicionarItem(new PocaoVida("Poção de Vida Pequena", 20));
    }

    public bool AdicionarItem(Item item)
    {
        if (Itens.Count >= CapacidadeMaxima)
            return false;

        Itens.Add(item);
        return true;
    }

    public ARISE.Dominio.Enums.ResultadoUsoItem UsarItem(int indice, Personagem alvo)
    {
        if (indice < 0 || indice >= Itens.Count)
            return ARISE.Dominio.Enums.ResultadoUsoItem.IndiceInvalido;

        Item item = Itens[indice];
        ARISE.Dominio.Enums.ResultadoUsoItem resultado = item.Usar(alvo);

        if (resultado == ARISE.Dominio.Enums.ResultadoUsoItem.Sucesso)
            Itens.RemoveAt(indice);
        
        return resultado;
    }
}
