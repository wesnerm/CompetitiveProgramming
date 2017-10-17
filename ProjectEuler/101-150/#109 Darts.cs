﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.IO;
using System.Text;
using static System.Array;
using static System.Math;
using static Library;
using T = System.Int64;

class Solution
{
	public const int MOD = 1000 * 1000 * 1000 + 9;

	public void solve()
	{

		var n = Nl();

		var coefficients = new long[60];

		for (int i = 1; i <= 20; i++)
		{
			coefficients[i - 1]++;
			coefficients[2 * i - 1]++;
			coefficients[3 * i - 1]++;
		}

		coefficients[50 - 1]++;
		coefficients[25 - 1]++;


		var m = RecurrenceSumMatrix(coefficients);

		var sumi = coefficients.Length;
		for (int i = 0; i < coefficients.Length; i++)
			m[sumi, i] = 0;
		for (int i = 1; i <= 20; i++)
			m[sumi, i * 2 - 1] = 1;
		m[sumi, 50 - 1] = 1;
		m[sumi, sumi] = 1;

#if DEBUG
		for (int i = 0; i < 65; i++)
		{
			var pp = Pow(m, i, MOD);
			Console.Error.WriteLine($"{i} -> f={pp[0, 0]} sum={pp[sumi, 0]}");
		}
#endif

		var p = Pow(m, n, MOD);

		long sum = p[sumi, 0];
		WriteLine(sum % MOD);
	}


	public static T[,] RecurrenceSumMatrix(T[] coefficients)
	{
		int n = coefficients.Length;
		var result = new T[n + 1, n + 1];
		result[0, 0] = coefficients[0];
		for (int i = 1; i < n; i++)
		{
			result[0, i] = coefficients[i];
			result[i, i - 1] = 1;
		}

		result[n, n] = 1;
		for (int i = 0; i < n; i++)
		{
			result[n, i] = coefficients[i];
		}

		return result;

	}


	#region Construction

	public static T[,] Diagonal(int n, T d = 1)
	{
		var id = new T[n, n];
		for (int i = 0; i < n; i++)
			id[i, i] = d;
		return id;
	}

	public static T[,] Clone(T[,] m)
	{
		return (T[,])m.Clone();
	}

	public static void Assign(T[,] dest, T[,] src)
	{
		Array.Copy(src, dest, src.Length);
	}

	#endregion


	public static T[,] Pow(T[,] a, long p, int mod)
	{
		int n = a.GetLength(0);
		var tmp = new T[n, n];
		var result = Diagonal(n);
		var b = Clone(a);

		while (p > 0)
		{
			if ((p & 1) != 0)
			{
				Mult(result, b, mod, tmp);
				Assign(result, tmp);
			}
			p >>= 1;
			Mult(b, b, mod, tmp);
			Assign(b, tmp);
		}
		return result;
	}

	public static T[,] Mult(T[,] a, T[,] b, T mod, T[,] c = null)
	{
		int arows = a.GetLength(0);
		int bcols = b.GetLength(1);
		int mid = a.GetLength(1);
		if (c == null) c = new T[arows, bcols];

		for (int i = 0; i < arows; i++)
			for (int j = 0; j < bcols; j++)
			{
				T t = 0;
				for (int k = 0; k < mid; k++)
				{
					t += (T)(1L * a[i, k] * b[k, j] % mod);
					if (t >= mod) t -= mod;
				}
				c[i, j] = t % mod;
			}

		return c;
	}


}


class CaideConstants {
    public const string InputFile = null;
    public const string OutputFile = null;
}

static partial class Library
{

	#region Common

	public static void Swap<T>(ref T a, ref T b)
	{
		var tmp = a;
		a = b;
		b = tmp;
	}

	public static void Clear<T>(T[] t, T value = default(T))
	{
		for (int i = 0; i < t.Length; i++)
			t[i] = value;
	}

	public static int BinarySearch<T>(T[] array, T value, bool upper = false)
		where T : IComparable<T>
	{
		int left = 0;
		int right = array.Length - 1;

		while (left <= right)
		{
			int mid = left + (right - left) / 2;
			int cmp = value.CompareTo(array[mid]);
			if (cmp > 0 || cmp == 0 && upper)
				left = mid + 1;
			else
				right = mid - 1;
		}
		return left;
	}

	#endregion

	#region  Input
	static System.IO.Stream inputStream;
	static int inputIndex, bytesRead;
	static byte[] inputBuffer;
	static System.Text.StringBuilder builder;
	const int MonoBufferSize = 4096;

	public static void InitInput(System.IO.Stream input = null, int stringCapacity = 16)
	{
		builder = new System.Text.StringBuilder(stringCapacity);
		inputStream = input ?? Console.OpenStandardInput();
		inputIndex = bytesRead = 0;
		inputBuffer = new byte[MonoBufferSize];
	}

	static void ReadMore()
	{
		inputIndex = 0;
		bytesRead = inputStream.Read(inputBuffer, 0, inputBuffer.Length);
		if (bytesRead <= 0) inputBuffer[0] = 32;
	}

	public static int Read()
	{
		if (inputIndex >= bytesRead) ReadMore();
		return inputBuffer[inputIndex++];
	}

	public static T[] N<T>(int n, Func<T> func)
	{
		var list = new T[n];
		for (int i = 0; i < n; i++) list[i] = func();
		return list;
	}

	public static int[] Ni(int n)
	{
		var list = new int[n];
		for (int i = 0; i < n; i++) list[i] = Ni();
		return list;
	}

	public static long[] Nl(int n)
	{
		var list = new long[n];
		for (int i = 0; i < n; i++) list[i] = Nl();
		return list;
	}

	public static string[] Ns(int n)
	{
		var list = new string[n];
		for (int i = 0; i < n; i++) list[i] = Ns();
		return list;
	}

    public static int Ni()
    {
        var c = SkipSpaces();
        bool neg = c == '-';
        if (neg) { c = Read(); }

        int number = c - '0';
        while (true)
        {
            var d = Read() - '0';
            if (unchecked((uint)d > 9)) break;
            number = number * 10 + d;
	        if (number < 0) throw new FormatException();
        }
        return neg ? -number : number;
    }

    public static long Nl()
    {
        var c = SkipSpaces();
        bool neg = c=='-';
        if (neg) { c = Read(); }

        long number = c - '0';
        while (true)
        {
            var d = Read() - '0';
            if (unchecked((uint)d > 9)) break;
            number = number * 10 + d;
	        if (number < 0) throw new FormatException();
        }
		return neg ? -number : number;
    }

	public static char[] Nc(int n)
	{
		var list = new char[n];
		for (int i = 0, c = SkipSpaces(); i < n; i++, c = Read()) list[i] = (char)c;
		return list;
	}

	public static byte[] Nb(int n)
	{
		var list = new byte[n];
		for (int i = 0, c = SkipSpaces(); i < n; i++, c = Read()) list[i] = (byte)c;
		return list;
	}

	public static string Ns()
	{
		var c = SkipSpaces();
		builder.Clear();
		while (true)
		{
			if ((uint)c - 33 >= (127 - 33)) break;
			builder.Append((char)c);
			c = Read();
		}
		return builder.ToString();
	}

	public static int SkipSpaces()
	{
		int c;
		do c = Read(); while ((uint)c - 33 >= (127 - 33));
		return c;
	}
	#endregion

	#region Output

	static System.IO.Stream outputStream;
	static byte[] outputBuffer;
	static int outputIndex;

	public static void InitOutput(System.IO.Stream output = null)
	{
		outputStream = output ?? Console.OpenStandardOutput();
		outputIndex = 0;
		outputBuffer = new byte[65535];
		AppDomain.CurrentDomain.ProcessExit += delegate { Flush(); };
	}

	public static void WriteLine(object obj = null)
	{
		Write(obj);
		Write('\n');
	}

	public static void WriteLine(long number)
	{
		Write(number);
		Write('\n');
	}

	public static void Write(long signedNumber)
	{
		ulong number = (ulong)signedNumber;
		if (signedNumber < 0)
		{
			Write('-');
			number = (ulong)(-signedNumber);
		}

		Reserve(20 + 1); // 20 digits + 1 extra
		int left = outputIndex;
		do
		{
			outputBuffer[outputIndex++] = (byte)('0' + number % 10);
			number /= 10;
		}
		while (number > 0);

		int right = outputIndex - 1;
		while (left < right)
		{
			byte tmp = outputBuffer[left];
			outputBuffer[left++] = outputBuffer[right];
			outputBuffer[right--] = tmp;
		}
	}

	public static void Write(object obj)
	{
		if (obj == null) return;

		var s = obj.ToString();
		Reserve(s.Length);
		for (int i = 0; i < s.Length; i++)
			outputBuffer[outputIndex++] = (byte)s[i];
	}

	public static void Write(char c)
	{
		Reserve(1);
		outputBuffer[outputIndex++] = (byte)c;
	}

	public static void Write(byte[] array, int count)
	{
		Reserve(count);
		Array.Copy(array, 0, outputBuffer, outputIndex, count);
		outputIndex += count;
	}

	static void Reserve(int n)
	{
		if (outputIndex + n <= outputBuffer.Length)
			return;

		Dump();
		if (n > outputBuffer.Length)
			Array.Resize(ref outputBuffer, Math.Max(outputBuffer.Length * 2, n));
	}

	static void Dump()
	{
		outputStream.Write(outputBuffer, 0, outputIndex);
		outputIndex = 0;
	}

	public static void Flush()
	{
		Dump();
		outputStream.Flush();
	}

	#endregion

}


public class Program
{
	public static void Main(string[] args)
	{
		InitInput(Console.OpenStandardInput());
		InitOutput(Console.OpenStandardOutput());
		Solution solution = new Solution();
		solution.solve();
		Flush();
#if DEBUG
		Console.Error.WriteLine(System.Diagnostics.Process.GetCurrentProcess().TotalProcessorTime);
#endif
	}
}