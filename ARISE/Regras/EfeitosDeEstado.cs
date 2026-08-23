using ARISE.Dominio.Enums;

namespace ARISE.Regras;

public static class EfeitosDeEstado
{
    private const int DuracaoConfusao = 1;
    private const int DuracaoIlusaoEmMassa = 2;
    private const int DuracaoEnfraquecido = 2;
    private const int DuracaoMaldicaoEnfraquecedora = 3;
    private const int DuracaoAtordoado = 1;

    public static TipoEstado? ObterEstadoCorrespondente(AcaoDeMonstro monstro) => monstro switch
    {
        AcaoDeMonstro.Confundir => TipoEstado.Confuso,
        AcaoDeMonstro.IlusaoEmMassa => TipoEstado.Confuso,
        AcaoDeMonstro.AtaqueContagiante => TipoEstado.Enfraquecido,
        AcaoDeMonstro.MaldicaoEnfraquecedora => TipoEstado.Enfraquecido,
        AcaoDeMonstro.UivoDeGuerra => TipoEstado.Atordoado,
        _ => null
    };

    public static int ObterDuracao(AcaoDeMonstro monstro) => monstro switch
    {
        AcaoDeMonstro.Confundir => DuracaoConfusao,
        AcaoDeMonstro.IlusaoEmMassa => DuracaoIlusaoEmMassa,
        AcaoDeMonstro.AtaqueContagiante => DuracaoEnfraquecido,
        AcaoDeMonstro.MaldicaoEnfraquecedora => DuracaoMaldicaoEnfraquecedora,
        AcaoDeMonstro.UivoDeGuerra => DuracaoAtordoado,
        _ => 0
    };
}