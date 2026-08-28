using System;
using System.Globalization;
using System.Text;

namespace HaoFuSurvivor
{
	public sealed class BigCoin : IComparable<BigCoin>, IEquatable<BigCoin>
	{
		public const int MaxDigits = 256;
		private const uint SegmentBase = 1000000000;
		private const int SegmentDigits = 9;
		private readonly uint[] mSegments;

		public static BigCoin Zero { get; } = new BigCoin(new uint[] { 0 });

		public BigCoin(string value)
		{
			if (!TryParse(value, out var parsed)) throw new FormatException("Invalid BigCoin value.");
			mSegments = parsed.mSegments;
		}

		private BigCoin(uint[] segments)
		{
			mSegments = segments;
		}

		public bool IsZero => mSegments.Length == 1 && mSegments[0] == 0;

		public BigCoin AddCoins(BigCoin amount)
		{
			if (amount == null) throw new ArgumentNullException(nameof(amount));
			var result = new uint[Math.Max(mSegments.Length, amount.mSegments.Length) + 1];
			ulong carry = 0;
			for (var i = 0; i < result.Length - 1; i++)
			{
				var sum = carry + (i < mSegments.Length ? mSegments[i] : 0) + (i < amount.mSegments.Length ? amount.mSegments[i] : 0);
				result[i] = (uint)(sum % SegmentBase);
				carry = sum / SegmentBase;
			}
			result[result.Length - 1] = (uint)carry;
			return CreateChecked(result);
		}

		public bool TrySpendCoins(BigCoin amount, out BigCoin remaining)
		{
			if (amount == null) throw new ArgumentNullException(nameof(amount));
			if (CompareTo(amount) < 0)
			{
				remaining = this;
				return false;
			}
			remaining = Subtract(amount);
			return true;
		}

		public string ToDisplayString()
		{
			var digits = ToString();
			if (digits.Length < 5) return digits;
			if (digits.Length > 16) return FormatScientific(digits);
			if (digits.Length >= 13) return FormatScaled(digits, 12, "\u5146");
			if (digits.Length >= 9) return FormatScaled(digits, 8, "\u4ebf");
			return FormatScaled(digits, 4, "\u4e07");
		}

		public override string ToString()
		{
			var builder = new StringBuilder(mSegments.Length * SegmentDigits);
			builder.Append(mSegments[mSegments.Length - 1].ToString(CultureInfo.InvariantCulture));
			for (var i = mSegments.Length - 2; i >= 0; i--)
				builder.Append(mSegments[i].ToString("D9", CultureInfo.InvariantCulture));
			return builder.ToString();
		}

		public int CompareTo(BigCoin other)
		{
			if (ReferenceEquals(other, null)) return 1;
			if (mSegments.Length != other.mSegments.Length) return mSegments.Length.CompareTo(other.mSegments.Length);
			for (var i = mSegments.Length - 1; i >= 0; i--)
				if (mSegments[i] != other.mSegments[i]) return mSegments[i].CompareTo(other.mSegments[i]);
			return 0;
		}

		public bool Equals(BigCoin other) => CompareTo(other) == 0;
		public override bool Equals(object obj) => obj is BigCoin other && Equals(other);

		public override int GetHashCode()
		{
			unchecked
			{
				var hash = 17;
				foreach (var segment in mSegments) hash = hash * 31 + (int)segment;
				return hash;
			}
		}

		public static bool TryParse(string value, out BigCoin result)
		{
			result = null;
			if (string.IsNullOrEmpty(value) || value.Length > MaxDigits || (value.Length > 1 && value[0] == '0')) return false;
			for (var i = 0; i < value.Length; i++)
				if (value[i] < '0' || value[i] > '9') return false;
			var count = (value.Length + SegmentDigits - 1) / SegmentDigits;
			var segments = new uint[count];
			var end = value.Length;
			for (var i = 0; i < count; i++)
			{
				var start = Math.Max(0, end - SegmentDigits);
				uint segment;
				if (!uint.TryParse(value.Substring(start, end - start), NumberStyles.None, CultureInfo.InvariantCulture, out segment)) return false;
				segments[i] = segment;
				end = start;
			}
			result = new BigCoin(segments);
			return true;
		}

		private BigCoin Subtract(BigCoin amount)
		{
			if (CompareTo(amount) < 0) throw new InvalidOperationException("BigCoin cannot become negative.");
			var result = new uint[mSegments.Length];
			long borrow = 0;
			for (var i = 0; i < result.Length; i++)
			{
				var difference = (long)mSegments[i] - borrow - (i < amount.mSegments.Length ? amount.mSegments[i] : 0);
				if (difference < 0)
				{
					difference += SegmentBase;
					borrow = 1;
				}
				else borrow = 0;
				result[i] = (uint)difference;
			}
			return CreateChecked(result);
		}

		private static BigCoin CreateChecked(uint[] segments)
		{
			var last = segments.Length - 1;
			while (last > 0 && segments[last] == 0) last--;
			if (last != segments.Length - 1)
			{
				var normalized = new uint[last + 1];
				Array.Copy(segments, normalized, normalized.Length);
				segments = normalized;
			}
			if (segments[last].ToString(CultureInfo.InvariantCulture).Length + last * SegmentDigits > MaxDigits)
				throw new OverflowException("BigCoin exceeds the maximum digit count.");
			return new BigCoin(segments);
		}

		private static string FormatScaled(string digits, int exponent, string unit)
		{
			var integerPart = digits.Substring(0, digits.Length - exponent);
			var fractional = digits.Substring(digits.Length - exponent, 2).TrimEnd('0');
			return fractional.Length == 0 ? integerPart + unit : integerPart + "." + fractional + unit;
		}

		private static string FormatScientific(string digits)
		{
			var fraction = digits.Substring(1, Math.Min(2, digits.Length - 1)).TrimEnd('0');
			return digits[0] + (fraction.Length == 0 ? string.Empty : "." + fraction) + "e" + (digits.Length - 1);
		}
	}
}
