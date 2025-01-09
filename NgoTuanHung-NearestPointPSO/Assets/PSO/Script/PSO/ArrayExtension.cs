using System;
using UnityEngine;

public static class ArrayExtension
{
	public static double[] Divide(this double[] vector, double scalar)
	{
		if (scalar == 0)
			throw new DivideByZeroException("Cannot divide by zero.");

		double[] result = new double[vector.Length];
		for (int i = 0; i < vector.Length; i++)
		{
			result[i] = vector[i] / scalar;
		}
		return result;
	}
	
	public static double[] Multiply(this double[] vector, double scalar)
	{
		double[] result = new double[vector.Length];
		for (int i = 0; i < vector.Length; i++)
		{
			result[i] = vector[i] * scalar;
		}
		return result;
	}
	
	public static double[] Add(this double[] vector, double[] add)
	{
		double[] result = new double[vector.Length];
		for (int i = 0; i < vector.Length; i++)
		{
			result[i] = vector[i] + add[i];
		}
		return result;
	}
	
	public static double[] Subtract(this double[] vector, double[] sub)
	{
		double[] result = new double[vector.Length];
		for (int i = 0; i < vector.Length; i++)
		{
			result[i] = vector[i] - sub[i];
		}
		return result;
	}
	
	public static double[] Normalized(this double[] vector)
	{
		double length = 0;
		for (int i=0;i<vector.Length;i++) length += vector[i] * vector[i];
		length = Math.Sqrt(length);
		
		return vector.Divide(length);
	}
	
	public static double Dot(this double[] vector1, double[] vector2)
	{
		// Check if both vectors have the same dimension (length)
		if (vector1.Length != vector2.Length)
		{
			throw new ArgumentException("Vectors must have the same dimension.");
		}

		double result = 0;
		for (int i = 0; i < vector1.Length; i++)
		{
			result += vector1[i] * vector2[i]; // Dot product calculation
		}
		return result;
	}
	
	public static Vector3 ToVector3(this double[] vector)
	{
		if (vector.Length != 3)
		{
			throw new ArgumentException("Vectors must have the dimension of 3");
		}
		
		return new Vector3((float)vector[0], (float)vector[1], (float)vector[2]);
	}
	
	public static string ToString(this double[] vector)
	{
		return "(" + string.Join(", ", vector) + ")";
	}
}