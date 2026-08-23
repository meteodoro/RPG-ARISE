using ARISE.Dominio.Enums;

namespace ARISE.Dominio.Interfaces;

public interface IAtacavel
{
    string Nome { get; }
    bool EstaVivo { get; }
    Elemento Elemento { get; }
    int Armadura { get; }
    int ClasseDeArmadura { get; }
    int PercentualReducaoAoDefender { get; }
    void ReceberDano(int quantidade);
    bool TemEstado(TipoEstado tipoEstado);
}
