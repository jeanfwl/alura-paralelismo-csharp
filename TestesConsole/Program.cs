using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TestesConsole
{
    class Program
    {
        public static long FatorialEmParalelo(int valor)
        {
            long resultado = 0;

            var t_calculo = new Thread(() => resultado = Fatorial(valor));
            t_calculo.Start();

            while (t_calculo.IsAlive)
            {

            }

            return resultado;
        }

        private static long Fatorial(int valor)
        {
            var resultado = 1L;
            for (int i = valor; i >= 2; i--)
                resultado *= i;
            return resultado;
        }

        static void Main(string[] args)
        {
            Console.WriteLine(FatorialEmParalelo(5));
        }
    }
}
