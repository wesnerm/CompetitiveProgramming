﻿using System;
using System.Diagnostics;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using static System.Array;
using static System.Math;
using static Library;
using T = System.Int64;



class Solution {
	public void solve()
	{
		int m = Ni(), t = Ni();

		var list = new List<int>();
		while (t-- > 0)
		{
			list.Add(Ni());
		}

		foreach (var n in list)
		{
			int k = n - 1;
			var mm = Min(m, k); // 
			var mm1 = Min(m, n) - 1;

			var x = new long[mm+1];
			var y = new long[mm1+1];
			for (int i = 0; i < x.Length; i++) x[i] = InverseFact(i);
			for (int i = 0; i < y.Length; i++) y[i] = InverseFact(i);

			var x2 = KaratsubaFast(x, x, k+1);
			var x4 = KaratsubaFast(x2, x2, k+1);
			var x8 = KaratsubaFast(x4, x4, k+1);
			var x9 = KaratsubaFast(x8, x, k+1);
			var x10 = KaratsubaFast(x9, y, k+1);

			WriteLine(9 * x10[k] % MOD * Fact(k) % MOD);
		}

	}

	public static long[] MultiplyPolynomials(long[] a, long[] b, int size = 0)
	{
		if (size == 0) size = a.Length + b.Length - 1;
		size = Min(a.Length + b.Length - 1, size);
		var result = new long[size];
		for (int i = 0; i < a.Length; i++)
		for (int j = Min(size - i, b.Length) - 1; j >= 0; j--)
			result[i + j] += a[i] * b[j];
		return result;
	}


	public static unsafe T[] KaratsubaFast(T[] a, T[] b, int size=0, int mod = 1000000007)
	{
		var expSize = a.Length + b.Length - 1;
		if (size == 0 || expSize < size) size = expSize;
		var result = new T[size];
		fixed (T* presult = &result[0])
		fixed (T* pa = &a[0])
		fixed (T* pb = &b[0])
		{
			KaratsubaFastCore(presult, result.Length, pa, a.Length, pb, b.Length, mod);
			for (int i = 0; i < result.Length; i++)
				if (result[i] < 0) result[i] += mod;
		}
		return result;
	}

	private static unsafe void KaratsubaFastCore(
		T* result, int resultLen,
		T* p, int plen,
		T* q, int qlen, int mod)
	{
		if (plen < qlen)
		{
			var tp = p; p = q; q = tp;
			var t = plen; plen = qlen; qlen = t;
		}

		int n = plen;
		int j;
		if (qlen <= 35 || resultLen <= 35)
		{
			if (qlen > 0 && resultLen > 0)
				for (int i = 0; i < plen; i++)
				{
					var pi = p[i];
					var presult = &result[i];
					int end = Math.Min(qlen, resultLen - i);
					for (j = 0; j < end; j++)
						presult[j] = (presult[j] + pi * q[j]) % mod;
				}
			return;
		}

		int m = (n + 1) >> 1;

		if (qlen <= m)
		{
			KaratsubaFastCore(result, resultLen, p, Math.Min(m, plen), q, Math.Min(m, qlen), mod);
			KaratsubaFastCore(result + m, resultLen - m, p + m, plen - m, q, qlen, mod);
			return;
		}

		var tmpLength = 2 * m - 1;
		var tmp = stackalloc T[2 * m];

		// Step 1
		// NOTE: StackAlloc may be preinitialized if local-init is set true
		for (T* ptmp = tmp, pMac = tmp + tmpLength; ptmp < pMac; ptmp++) *ptmp = 0;
		KaratsubaFastCore(tmp, tmpLength, p, Math.Min(m, plen), q, Math.Min(m, qlen), mod);

		for (int i = Math.Min(resultLen, tmpLength) - 1; i >= 0; i--)
			result[i] += tmp[i];

		var ptr = result + m;
		for (int i = Math.Min(resultLen - m, tmpLength) - 1; i >= 0; i--)
			ptr[i] -= tmp[i];

		// Step 2
		for (T* ptmp = tmp, pMac = tmp + tmpLength; ptmp < pMac; ptmp++) *ptmp = 0;
		KaratsubaFastCore(tmp, tmpLength, p + m, plen - m, q + m, qlen - m, mod);

		ptr = result + 2 * m;
		for (int i = Math.Min(resultLen - m * 2, tmpLength) - 1; i >= 0; i--)
			ptr[i] += tmp[i];
		ptr = result + m;
		for (int i = Math.Min(resultLen - m, tmpLength) - 1; i >= 0; i--)
			ptr[i] -= tmp[i];

		// Step 3
		for (T* ptmp = tmp, pMac = tmp + tmpLength; ptmp < pMac; ptmp++) *ptmp = 0;

		var f1 = tmp;
		var f2 = tmp + m;

		for (j = Math.Min(plen, n) - 1; j >= m; j--)
			f1[j - m] += p[j];
		for (; j >= 0; j--) f1[j] = (f1[j] + p[j]) % mod;
		for (j = Math.Min(qlen, n) - 1; j >= m; j--)
			f2[j - m] += q[j];
		for (; j >= 0; j--) f2[j] = (f2[j] + q[j]) % mod;

		KaratsubaFastCore(result + m, Math.Min(2 * m - 1, resultLen - m), f1, m, f2, m, mod);

		for (int i = Math.Min(plen + qlen - 1, resultLen) - 1; i >= 0; i--)
			result[i] %= mod;
	}


	#region Mod Math
	public const int MOD = 1000 * 1000 * 1000 + 7;

	static int[] _inverse;
	public static long Inverse(long n)
	{
		long result;

		if (_inverse == null)
			_inverse = new int[3000];

		if (n < _inverse.Length && (result = _inverse[n]) != 0)
			return result - 1;

		result = ModPow(n, MOD - 2);
		if (n < _inverse.Length)
			_inverse[n] = (int)(result + 1);
		return result;
	}

	public static long Mult(long left, long right)
	{
		return (left * right) % MOD;
	}

	public static long Div(long left, long divisor)
	{
		return left % divisor == 0
			? left / divisor
			: Mult(left, Inverse(divisor));
	}

	public static long Subtract(long left, long right)
	{
		return (left + (MOD - right)) % MOD;
	}

	public static long Fix(long m)
	{
		var result = m % MOD;
		if (result < 0) result += MOD;
		return result;
	}

	public static long ModPow(long n, long p, long mod = MOD)
	{
		long b = n;
		long result = 1;
		while (p != 0)
		{
			if ((p & 1) != 0)
				result = (result * b) % mod;
			p >>= 1;
			b = (b * b) % mod;
		}
		return result;
	}

	public static long Pow(long n, long p)
	{
		long b = n;
		long result = 1;
		while (p != 0)
		{
			if ((p & 1) != 0)
				result *= b;
			p >>= 1;
			b *= b;
		}
		return result;
	}

	#endregion

	#region Combinatorics
	static List<long> _fact;
	static List<long> _ifact;

	public static long Fact(int n)
	{
		if (_fact == null) _fact = new List<long>(100) { 1 };
		for (int i = _fact.Count; i <= n; i++)
			_fact.Add(Mult(_fact[i - 1], i));
		return _fact[n];
	}

	public static long InverseFact(int n)
	{
		if (_ifact == null) _ifact = new List<long>(100) { 1 };
		for (int i = _ifact.Count; i <= n; i++)
			_ifact.Add(Div(_ifact[i - 1], i));
		return _ifact[n];
	}

	public static long Comb(int n, int k)
	{
		if (k <= 1) return k == 1 ? n : k == 0 ? 1 : 0;
		if (k + k > n) return Comb(n, n - k);
		return Mult(Mult(Fact(n), InverseFact(k)), InverseFact(n - k));
	}

	#endregion


}

public class KaratsubaMultiplication
{
	// Karatsuba Multiplication
	// a = p[0,m)
	// b = q[0,m)
	// c = p[m,n)
	// d = q[m,n]

	// hi = c * d
	// lo = a * b

	// rem2 = (a+c)*(b+d)
	// (a+c)*(b+d) - ab - cd = ab + ad + bc + cd - ab - cd = ad + bc

	public static T[] Karatsuba(T[] a, T[] b, int mod = 1000000007)
	{
		var result = new T[a.Length + b.Length - 1];
		KaratsubaCore(result, a, 0, a.Length, b, 0, b.Length, mod);
		return result;
	}

	private static void KaratsubaCore(
		T[] result,
		T[] p, int ip, int plen,
		T[] q, int iq, int qlen, int mod)
	{
		int resultLen = result.Length;
		if (ip >= plen || iq >= qlen) return;

		int n = Math.Max(plen - ip, qlen - iq);
		int j;
		if (n <= 35)
		{
			if (plen < qlen)
			{
				Swap(ref p, ref q);
				var t = ip;
				ip = iq;
				iq = t;
				t = plen;
				plen = qlen;
				qlen = t;
			}

			for (int i = ip; i < plen; i++)
			{
				int ind = i - ip - iq;
				int end = Math.Min(qlen, resultLen - ind);
				for (j = iq; j < end; j++)
					result[ind + j] = (result[ind + j] + p[i] * q[j]) % mod;
			}
			return;
		}

		int m = (n + 1) >> 1;

		KaratsubaCore(result, p, ip, Math.Min(ip + m, plen), q, iq, Math.Min(iq + m, qlen), mod);

		for (int i = Math.Min(resultLen - m, m * 2 - 1) - 1; i >= 0; i--)
			result[m + i] -= result[i];

		var tmp = new T[m * 2 - 1];
		KaratsubaCore(tmp, p, ip + m, plen, q, iq + m, qlen, mod);

		for (int i = Math.Min(resultLen - m, tmp.Length) - 1; i >= 0; i--)
			result[m + i] -= tmp[i];
		for (int i = Math.Min(resultLen - m * 2, tmp.Length) - 1; i >= 0; i--)
			result[m * 2 + i] += tmp[i];

		var f1 = new T[m];
		var f2 = new T[m];

		for (j = Math.Min(plen - ip, n) - 1; j >= m; j--)
			f1[j - m] += p[j + ip];
		for (; j >= 0; j--) f1[j] = (f1[j] + p[j + ip]) % mod;
		for (j = Math.Min(qlen - iq, n) - 1; j >= m; j--)
			f2[j - m] += q[j + iq];
		for (; j >= 0; j--) f2[j] = (f2[j] + q[j + iq]) % mod;

		Array.Clear(tmp, 0, tmp.Length);
		KaratsubaCore(tmp, f1, 0, m, f2, 0, m, mod);

		for (int i = Math.Min(tmp.Length, resultLen - m) - 1; i >= 0; i--)
			result[m + i] += tmp[i];

		for (int i = Math.Min(plen + qlen - 1, resultLen) - 1; i >= 0; i--)
		{
			result[i] %= mod;
			if (result[i] < 0) result[i] += mod;
		}
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

	public static int BinarySearch<T>(T[] array, T value, int left, int right, bool upper = false)
		where T : IComparable<T>
	{
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
			if ((uint)d > 9) break;
			number = number * 10 + d;
		}
		return neg ? -number : number;
	}

	public static long Nl()
	{
		var c = SkipSpaces();
		bool neg = c == '-';
		if (neg) { c = Read(); }

		long number = c - '0';
		while (true)
		{
			var d = Read() - '0';
			if ((uint)d > 9) break;
			number = number * 10 + d;
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