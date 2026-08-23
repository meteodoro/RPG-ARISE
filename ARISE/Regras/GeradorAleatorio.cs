using System;

namespace ARISE.Regras;

public static class GeradorAleatorio
{
    public static int Rolar(int min, int max)
    {
        if (min > max)
            throw new ArgumentOutOfRangeException(nameof(min), "O valor mínimo não pode ser maior que o valor máximo.");

        if (max == int.MaxValue)
            return Random.Shared.Next(min, max);

        return Random.Shared.Next(min, max + 1);
    }
}