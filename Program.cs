using System;
using System.IO;
using System.Linq;

namespace SImplex
{
    class Program
    {
        static void Main(string[] args)
        {
            string[] lines = File.ReadAllLines("simplex.in");
            string[] sizes = lines[0].Split();

            //m строк, n столбцов
            int n = Int32.Parse(sizes[0]);
            int m = Int32.Parse(sizes[1]);

            //коэффициенты целевой функции
            double[] funcCoefficients = new double[n];
            funcCoefficients = lines[1].Split(' ').Select(double.Parse).ToArray();

            //матрица коэффициентов
            double[,] coefficients = new double[m, n];
            double[] matrixLine;
            for (int i = 0; i < m; i++)
            {
                matrixLine = new double[n];
                matrixLine = lines[2 + i].Split(' ').Select(double.Parse).ToArray();
                for (int j = 0; j < n; j++)
                {
                    coefficients[i, j] = matrixLine[j];
                }
            }

            // базисное допустимое решение
            double[] bfs = new double[m];
            bfs = lines[m + 2].Split(' ').Select(double.Parse).ToArray();

            // экземпляр класса симплекс-таблицы
            var table = new SimplexTable(m, n, funcCoefficients, coefficients, bfs);
            string resolvability = table.FindSolution();

            // вывод ответа
            StreamWriter f = new StreamWriter("simplex.out");
            if (resolvability == "Yes")
            {
                f.WriteLine(resolvability);
                f.WriteLine(table.GetAnswerValue());

                System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
                double[] values = table.GetAnswerVector();
                for (int i = 0; i < values.Length; i++)
                {
                    f.Write(values[i]);
                    if (i < values.Length - 1)
                        f.Write(" ");
                }
            }
            else
            {
                f.WriteLine(resolvability);
            }

            f.Close();
        }
    }
}
