using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeamProject
{
    public class ConvolutionMatrix
    {
        public int matrixSize;
        public double[,] Matrix;
        public double Factor = 1;
        public double Offset = 0;

        public ConvolutionMatrix(int size)
        {
            Matrix = new double[size, size];
            matrixSize = size;
        }

        public void defaultValue(double value)
        {
            for (int i = 0; i < matrixSize; i++)
            {
                for (int j = 0; j < matrixSize; j++)
                {
                    Matrix[i, j] = value;
                }
            }
        }

    }
}
