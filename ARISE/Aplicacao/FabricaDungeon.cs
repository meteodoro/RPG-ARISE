using ARISE.Dominio.Monstros;

namespace ARISE.Aplicacao;

public static class FabricaDungeon
{
    public static List<(string NomeFase, List<Func<Monstro>> MonstrosDoNivel)> Criar()
    {
        return new List<(string NomeFase, List<Func<Monstro>> MonstrosDoNivel)>
        {
            ("NÍVEL 1", new List<Func<Monstro>>
            {
                () => new Spectrum("Spectrum"),
                () => new Zombie("Zombie"),
                () => new Driade("Boss Driade")
            }),
            ("NÍVEL 2", new List<Func<Monstro>>
            {
                () => new Bugbear("Bugbear"),
                () => new Golem("Golem"),
                () => new PackAncient("Boss Pack Ancient")
            }),
            ("NÍVEL 3", new List<Func<Monstro>>
            {
                () => new Orcus("Orcus"),
                () => new GreenWitch("Green Witch"),
                () => new Sentinel("Boss Sentinel")
            }),
            ("DESAFIO FINAL", new List<Func<Monstro>>
            {
                () => new Igris("BOSS FINAL IGRIS")
            })
        };
    }
}
