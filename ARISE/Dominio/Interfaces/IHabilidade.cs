using ARISE.Dominio.Enums;
using ARISE.Dominio.Personagens;

namespace ARISE.Dominio.Interfaces;

public interface IHabilidade
{
    string Nome { get; }
    Elemento? Elemento { get; }
    int DanoBase { get; }
    int Custo { get; }

    string Executar(Personagem conjurador, IAtacavel alvo);
}