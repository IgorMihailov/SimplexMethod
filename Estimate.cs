using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SImplex
{
    class Estimate
    {
        private double A, B, M;

        public Estimate(double M)
        {
            // коэффициент при А
            A = int.MaxValue;

            // свободный член
            B = int.MaxValue;

            this.M = M;
        }

        public double GetA()
        {
            return A;
        }
        public double GetB()
        {
            return B;
        }

        // оценка под столбцом бдр
        public void CalculateFuncValue(double[] bfs, double[] funcCoefficients, int[] basis)
        {
            double a = 0; double b = 0;
            for (int i = 0; i < basis.Length; i++)
            {
                // считаем коэффициент при M
                if (funcCoefficients[basis[i]] == M)
                    a += bfs[i];
                // считаем свободный член
                else
                    b += bfs[i] * funcCoefficients[basis[i]];
            }
            this.A = a;
            this.B = b;
        }

        // остальные оценки
        public void CalculateEstimate(double[,] coefficients, double[] funcCoefficients, int[] basis, int column)
        {
            double a = 0; double b = 0;
            for (int i = 0; i < basis.Length; i++)
            {
                if (funcCoefficients[basis[i]] == M)
                    a += coefficients[i, column];
                else
                    b += coefficients[i, column] * funcCoefficients[basis[i]];
            }

            // если коэффициент над столбцом М
            if (funcCoefficients[column] == this.M)
            {
                this.A = a - 1;
                this.B = b;
            }
            else
            {
                this.A = a;
                this.B = b - funcCoefficients[column];
            }
        }
    }
}
