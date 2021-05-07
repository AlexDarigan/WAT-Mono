using System;
using Godot.Collections;

namespace WAT
{
	public class Range: Assertion
	{
		public static Dictionary IsInRange(double val, double low, double high, string context)
		{
			var passed = $"{value} is in range {low}-{high}";
			var failed = $"{value} is not in range {low}-{high}";
			var success = val >= low && val < high;
			var result = success ? passed : failed;
			return Result(success, passed, result, context);
		}
		
		public static Dictionary IsNotInRange(double val, double low, double high, string context)
		{
			var passed = $"{value} is not in range {low}-{high}";
			var failed = $"{value} is in range {low}-{high}";
			var success = val < low || val >= high;
			var result = success ? passed : failed;
			return Result(success, passed, result, context);
		}
		
	}
}
