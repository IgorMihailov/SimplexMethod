using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SImplex
{
    public class SimplexTable
    {
        private double[,] coefficients;
        private double[] funcCoefficients;
        private double[] bfs;
        private int[] basis;
        private double M;

        private Estimate[] estimates;
        private Estimate funcValue;

        public SimplexTable(int m, int n, double[] funcCoefficients, double[,] coefficients, double[] bfs)
        {
            //матрица коэффициентов со штрафными переменными
            this.coefficients = new double[m, n + m];

            // изначально задаём M как максимум из коэфф-в целевой функции
            M = funcCoefficients.Max();
            for (int i = 0; i < m; i++)
                for (int j = 0; j < n + m; j++)
                {
                    //копируем коэфф-ты исходных переменных
                    if (j < n)
                        this.coefficients[i, j] = coefficients[i, j];
                    else
                    {
                        //создаём единичную матрицу из штрафных переменных
                        if (j == n + i)
                            this.coefficients[i, j] = 1;
                    }

                    //обновление M, если коэффициет превосходит его
                    updateM(this.coefficients[i, j]);
                }

            // копируем вектор правых частей и обновляем M при необходимости
            this.bfs = bfs;
            foreach (var value in this.bfs)
                updateM(value);

            //коэффициенты целевой функции
            this.funcCoefficients = new double[n + m];
            M++;

            for (int i = 0; i < n + m; i++)
            {
                if (i < n)
                    this.funcCoefficients[i] = funcCoefficients[i];
                else
                    this.funcCoefficients[i] = M;
            }

            // оценки и значение функции
            this.estimates = new Estimate[n + m];
            for (int i = 0; i < estimates.Length; i++)
                estimates[i] = new Estimate(M);
            this.funcValue = new Estimate(M);

            // начальный базис состоит из штрафных переменных
            this.basis = new int[m];
            for (int i = 0; i < m; i++)
                basis[i] = n + i;
        }

        private void updateM(double value)
        {
            if (value > M)
                M = value;
        }

        public string FindSolution()
        {
            while (true)
            {
                //считаем оценки
                CalculateEstimates();

                //проверяем оптимальность
                if (isOptimum())
                {
                    for (int i = 0; i < basis.Length; i++)
                    {
                        // если в базисе есть штрафная переменная больше 0
                        if (basis[i] > funcCoefficients.Length - basis.Length && bfs[i] > 1e-7)
                            return "No";
                    }

                    return "Yes";
                }

                //находим ведущий столбец и проверяем ограниченность функции
                int leadColumn = FindMaxEstimate();
                for (int i = 0; i < basis.Length; i++)
                {
                    if (coefficients[i, leadColumn] > 0)
                        break;

                    // когда все элементы ведущего столбца не положительны
                    if (i == basis.Length - 1)
                        return "Unbounded";
                }

                //находим ведущую строку
                int leadRow = FindLeadRow(leadColumn);

                //заменяем переменную в базисе и пересчитываем таблицу
                RebuildTable(leadRow, leadColumn);
            }
        }

        private void CalculateEstimates()
        {
            // оценка = Сумма(коэфф. * коэфф. целевой функции строки) по столбцу - коэфф целевой функции столбца
            funcValue.CalculateFuncValue(bfs, funcCoefficients, basis);
            for (int j = 0; j < estimates.Length; j++)
            {
                estimates[j].CalculateEstimate(coefficients, funcCoefficients, basis, j);
            }
        }

        private bool isOptimum()
        {
            foreach (var estimate in estimates)
            {
                // коэфф. при М положительный
                if (estimate.GetA() > 0)
                    return false;

                // коэфф. при М = 0 и свободный член положительный
                if (estimate.GetA() == 0 && estimate.GetB() > 0)
                    return false;
            }
            return true;
        }

        private int FindMaxEstimate()
        {
            int maxPos = 0;
            double maxA = estimates[0].GetA();
            double maxB = estimates[0].GetB();

            for (int i = 1; i < estimates.Length; i++)
            {
                if (estimates[i].GetA() > maxA)
                {
                    maxA = estimates[i].GetA();
                    maxB = estimates[i].GetB();
                    maxPos = i;
                    continue;
                }

                if (estimates[i].GetA() < maxA)
                    continue;

                if (estimates[i].GetB() > maxB)
                {
                    maxA = estimates[i].GetA();
                    maxB = estimates[i].GetB();
                    maxPos = i;
                }
            }

            return maxPos;
        }

        private int FindLeadRow(int leadColumn)
        {
            int leadRow = 0; double minValue = M;
            for (int i = 0; i < basis.Length; i++)
            {
                // коэфф-нт должен быть положительным
                if (bfs[i] / coefficients[i, leadColumn] < minValue && coefficients[i, leadColumn] > 0)
                {
                    minValue = bfs[i] / coefficients[i, leadColumn];
                    leadRow = i;
                }
            }

            return leadRow;
        }

        private void RebuildTable(int i, int j)
        {
            // записываем новую переменную в базис
            basis[i] = j;

            // опорный элемент
            var e = coefficients[i, j];

            // пересчитываем остальные элементы
            for (int l = 0; l < basis.Length; l++)
            {
                // пропускаем опорную строку
                if (l == i)
                    continue;

                // пересчёт бдр
                bfs[l] = (bfs[l] * e - bfs[i] * coefficients[l, j]) / e;

                for (int k = 0; k < funcCoefficients.Length; k++)
                {
                    // пропускаем опорный столбец
                    if (k == j)
                        continue;

                    coefficients[l, k] = (coefficients[l, k] * e - coefficients[i, k] * coefficients[l, j]) / e;
                }
            }

            // делим все элементы ведущей строки на опорный элемент           
            for (int k = 0; k < funcCoefficients.Length; k++)
            {
                coefficients[i, k] /= e;
            }
            bfs[i] /= e;

            // зануляем ведущий столбец
            for (int l = 0; l < basis.Length; l++)
            {
                if (l != i)
                    coefficients[l, j] = 0;
            }
        }

        public string GetAnswerValue()
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
            return funcValue.GetB().ToString();
        }

        public double[] GetAnswerVector()
        {
            double[] values = new double[funcCoefficients.Length - bfs.Length];
            for (int i = 0; i < basis.Length; i++)
            {
                int index = basis[i];
                if (index < values.Length)
                    values[index] = bfs[i];
            }
            return values;
        }
    }
}
