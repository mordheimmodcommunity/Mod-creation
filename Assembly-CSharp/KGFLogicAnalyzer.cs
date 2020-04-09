using System;
using System.Collections.Generic;
using UnityEngine;

public class KGFLogicAnalyzer
{
	public class KGFLogicOperand
	{
		public string itsOperandName = string.Empty;

		private bool? itsValue;

		public List<KGFLogicOperand> itsListOfOperands = new List<KGFLogicOperand>();

		public List<string> itsListOfOperators = new List<string>();

		public void AddOperand(KGFLogicOperand theOperand)
		{
			itsListOfOperands.Add(theOperand);
		}

		public void AddOperator(string theOperator)
		{
			itsListOfOperators.Add(theOperator);
		}

		public void SetName(string theName)
		{
			itsOperandName = theName;
			if (theName.ToLower() == "true")
			{
				itsValue = true;
			}
			else if (theName.ToLower() == "false")
			{
				itsValue = false;
			}
		}

		public string GetName()
		{
			return itsOperandName;
		}

		public void SetValue(bool theValue)
		{
			itsValue = theValue;
		}

		public bool? GetValue()
		{
			bool? flag = itsValue;
			if (!flag.HasValue)
			{
				if (itsOperandName != string.Empty)
				{
					itsValue = GetOperandValue(itsOperandName);
					bool? flag2 = itsValue;
					if (!flag2.HasValue)
					{
						return null;
					}
					return itsValue;
				}
				return Evaluate();
			}
			return itsValue.Value;
		}

		public bool? Evaluate()
		{
			if (itsListOfOperands.Count == 1)
			{
				return itsListOfOperands[0].GetValue();
			}
			bool? flag = false;
			for (int i = 0; i < itsListOfOperands.Count - 1; i++)
			{
				flag = ((i != 0) ? EveluateTwoOperands(flag, itsListOfOperands[i + 1].GetValue(), itsListOfOperators[i]) : EveluateTwoOperands(itsListOfOperands[i].GetValue(), itsListOfOperands[i + 1].GetValue(), itsListOfOperators[i]));
			}
			return flag;
		}

		private bool? EveluateTwoOperands(bool? theValue1, bool? theValue2, string theOperator)
		{
			if (!theValue1.HasValue)
			{
				Debug.LogError("KGFLogicAnalyzer: cannot evaluate because theValue1 is null");
				return null;
			}
			if (!theValue2.HasValue)
			{
				Debug.LogError("KGFLogicAnalyzer: cannot evaluate because theValue2 is null");
				return null;
			}
			if (theOperator == "&&")
			{
				return theValue1.Value && theValue2.Value;
			}
			if (theOperator == "||")
			{
				return theValue1.Value || theValue2.Value;
			}
			Debug.LogError("KGFLogicAnalyzer: wrong operator: " + theOperator);
			return null;
		}
	}

	private static string itsStringAnd = "&&";

	private static string itsStringOr = "||";

	private static Dictionary<string, bool> itsOperandValues = new Dictionary<string, bool>();

	public static bool? Analyze(string theLogicString)
	{
		string theErrorString = string.Empty;
		if (CheckSyntax(theLogicString, out theErrorString))
		{
			if (CheckOperands(theLogicString, out theErrorString))
			{
				int num = 0;
				if (!theLogicString.Contains(")"))
				{
					theLogicString = "(" + theLogicString + ")";
				}
				while (theLogicString.Contains(")"))
				{
					EvaluateBraces(ref theLogicString);
					num++;
					if (num == 30)
					{
						break;
					}
				}
				if (theLogicString.ToLower() == "true")
				{
					return true;
				}
				if (theLogicString.ToLower() == "false")
				{
					return false;
				}
				Debug.LogError("KGFLogicAnalyzer: unexpected result: " + theLogicString);
				return null;
			}
			Debug.LogError("KGFLogicAnalyzer: syntax error: " + theErrorString);
			return null;
		}
		Debug.LogError("KGFLogicAnalyzer: syntax error: " + theErrorString);
		return null;
	}

	private static void EvaluateBraces(ref string theLogicString)
	{
		string text = theLogicString.Replace(" ", string.Empty);
		int num = text.IndexOf(')');
		string text2 = text.Substring(0, num + 1);
		int num2 = text2.LastIndexOf('(');
		int length = num - num2 - 1;
		string theLogicString2 = text.Substring(num2 + 1, length);
		bool? flag = AnalyseLogicBlock(theLogicString2);
		if (!flag.HasValue)
		{
			Debug.LogError("Logic block result is null. Something went wrong!");
			return;
		}
		string str = theLogicString.Substring(0, num2);
		string str2 = theLogicString.Substring(num + 1);
		theLogicString = str + flag.Value.ToString() + str2;
	}

	public static void ClearOperandValues()
	{
		itsOperandValues.Clear();
	}

	public static void SetOperandValue(string theOperandName, bool theValue)
	{
		if (itsOperandValues.ContainsKey(theOperandName))
		{
			itsOperandValues[theOperandName] = theValue;
		}
		else
		{
			itsOperandValues.Add(theOperandName, theValue);
		}
	}

	public static bool? GetOperandValue(string theOperandName)
	{
		if (itsOperandValues.ContainsKey(theOperandName))
		{
			return itsOperandValues[theOperandName];
		}
		Debug.LogError("KGFLogicAnalyzer: no operand value for operand: " + theOperandName);
		return null;
	}

	private static bool? AnalyseLogicBlock(string theLogicString)
	{
		KGFLogicOperand kGFLogicOperand = new KGFLogicOperand();
		string text = theLogicString.Replace(" ", string.Empty);
		string[] separator = new string[2]
		{
			itsStringAnd,
			itsStringOr
		};
		string[] array = text.Split(separator, StringSplitOptions.None);
		string[] array2 = array;
		foreach (string name in array2)
		{
			KGFLogicOperand kGFLogicOperand2 = new KGFLogicOperand();
			kGFLogicOperand2.SetName(name);
			kGFLogicOperand.AddOperand(kGFLogicOperand2);
		}
		for (int j = 0; j < array.Length - 1; j++)
		{
			text = text.Remove(0, array[j].Length);
			string theOperator = text.Substring(0, 2);
			kGFLogicOperand.AddOperator(theOperator);
			text = text.Remove(0, 2);
		}
		return kGFLogicOperand.Evaluate();
	}

	public static bool CheckSyntax(string theLogicString, out string theErrorString)
	{
		theErrorString = string.Empty;
		if (theLogicString.IndexOf(itsStringAnd) == 0)
		{
			theErrorString = "condition cannot start with &&";
			return false;
		}
		if (theLogicString.IndexOf(itsStringOr) == 0)
		{
			theErrorString = "condition cannot start with ||";
			return false;
		}
		if (theLogicString.LastIndexOf(itsStringAnd) == theLogicString.Length - 2 && theLogicString.Length != 1)
		{
			theErrorString = "condition cannot end with &&";
			return false;
		}
		if (theLogicString.LastIndexOf(itsStringOr) == theLogicString.Length - 2 && theLogicString.Length != 1)
		{
			theErrorString = "condition cannot end with ||";
			return false;
		}
		string text = theLogicString.Replace(" ", string.Empty);
		int num = text.Split(new char[1]
		{
			'('
		}).Length - 1;
		int num2 = text.Split(new char[1]
		{
			')'
		}).Length - 1;
		if (num > num2)
		{
			theErrorString = "missing closing brace";
			return false;
		}
		if (num2 > num)
		{
			theErrorString = "missing opening brace";
			return false;
		}
		string[] separator = new string[2]
		{
			itsStringAnd,
			itsStringOr
		};
		string text2 = text.Replace("(", string.Empty);
		text2 = text2.Replace(")", string.Empty);
		string[] array = text2.Split(separator, StringSplitOptions.None);
		string[] array2 = array;
		foreach (string text3 in array2)
		{
			if (text3.Contains("&"))
			{
				theErrorString = "condition cannot contain the character &. Use && for logical and.";
				return false;
			}
			if (text3.Contains("|"))
			{
				theErrorString = "condition cannot contain the character |. Use || for logical or.";
				return false;
			}
		}
		return true;
	}

	public static bool CheckOperands(string theLogicString, out string theErrorString)
	{
		theErrorString = string.Empty;
		string[] separator = new string[2]
		{
			itsStringAnd,
			itsStringOr
		};
		string text = theLogicString.Replace(" ", string.Empty);
		string text2 = text.Replace("(", string.Empty);
		text2 = text2.Replace(")", string.Empty);
		string[] array = text2.Split(separator, StringSplitOptions.None);
		string[] array2 = array;
		foreach (string text3 in array2)
		{
			if (!GetOperandValue(text3).HasValue)
			{
				theErrorString = "no operand value for operand: " + text3;
				return false;
			}
		}
		return true;
	}
}
