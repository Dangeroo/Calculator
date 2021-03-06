﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Calculator
{   
    /// <summary>
    /// Before evaluating expression we are converting inxfix notation to postfix(RPN*) using
    ///"Shunting-yard" algorithm by Edgar Dijkstra that runs in linear time - O(n).
    ///* The postfix notation was brought by Charles Hamblin in 1957, based on Polish or prefix notation, introduced in 1950 by the Polish logician Jan Lukasiewicz,
    ///* It is a method of representing an expression without using parenthesis and still conserving the precedence rules of the original expression.
    /// </summary>
    public class Calculator
    {
        private const string NumberMaker = "#";
        private const string OperatorMarker = "$";
        private const string FunctionMarker = "@";

        private const string Plus = OperatorMarker + "+";
        private const string Minus = OperatorMarker + "-";
        private const string UnPlus = OperatorMarker + "un+";
        private const string UnMinus = OperatorMarker + "un-";
        private const string Multiply = OperatorMarker + "*";
        private const string Divide = OperatorMarker + "/";
        private const string LeftParent = OperatorMarker + "(";
        private const string RightParent = OperatorMarker + ")";

        private readonly Dictionary<string, string> supportedOperators =
            new Dictionary<string, string>
            {
                { "+", Plus },
                { "-", Minus },
                { "*", Multiply },
                { "/", Divide },
                { "(", LeftParent },
                { ")", RightParent }
            };

        public double Parse(string expression)
        {
            try
            {
                return Calculate(ConvertToRPN(FormatString(expression)));
            }
            catch (DivideByZeroException e)
            {
                throw e;
            }
            catch (FormatException e)
            {
                throw e;
            }
            catch (InvalidOperationException e)
            {
                throw e;
            }
            catch (ArgumentOutOfRangeException e)
            {
                throw e;
            }
            catch (ArgumentException e)
            {
                throw e;
            }
            catch (Exception e)
            {
                throw e;
            }

        }

        private string FormatString(string expression)
        {
            if (string.IsNullOrEmpty(expression))
            {
                throw new ArgumentNullException("Expression is null or empty");
            }

            StringBuilder formattedString = new StringBuilder();
            int balanceOfParenth = 0;

            for (int i = 0; i < expression.Length; i++)
            {
                char ch = expression[i];

                if (ch == '(')
                {
                    balanceOfParenth++;
                }
                else if (ch == ')')
                {
                    balanceOfParenth--;
                }

                if (Char.IsWhiteSpace(ch))
                {
                    continue;
                }
                else
                {
                    formattedString.Append(ch);
                }
            }

            if (balanceOfParenth != 0)
            {
                throw new FormatException("Number of left and right parenthesis is not equal");
            }

            return formattedString.ToString();
        }

        private string ConvertToRPN(string expression)
        {
            int pos = 0;
            StringBuilder outputString = new StringBuilder();
            Stack<string> stack = new Stack<string>();

            while (pos < expression.Length)
            {
                string token = LexicalAnalysisInfixNotation(expression, ref pos);

                outputString = SyntaxAnalysisInfixNotation(token, outputString, stack);
            }
        
            while (stack.Count > 0)
            {
                if (stack.Peek()[0] == OperatorMarker[0])
                {
                    outputString.Append(stack.Pop());
                }
                else
                {
                    throw new FormatException("Format exception,"
                    + " there is function without parenthesis");
                }
            }

            return outputString.ToString();
        }

        private string LexicalAnalysisInfixNotation(string expression, ref int pos)
        {
            StringBuilder token = new StringBuilder();
            token.Append(expression[pos]);

            if (supportedOperators.ContainsKey(token.ToString()))
            {
                bool isUnary = pos == 0 || expression[pos - 1] == '(';
                pos++;

                return (token.ToString()) switch
                {
                    "+" => isUnary ? UnPlus : Plus,
                    "-" => isUnary ? UnMinus : Minus,
                    _ => supportedOperators[token.ToString()],
                };
            }
            else if (Char.IsDigit(token[0]))
            {
                if (Char.IsDigit(token[0]))
                {
                    while (++pos < expression.Length
                    && Char.IsDigit(expression[pos]))
                    {
                        token.Append(expression[pos]);
                    }
                }
                else
                {
                    token.Clear();
                }


                return NumberMaker + token.ToString();
            }
            else
            {
                throw new ArgumentException("Unknown token in expression");
            }
        }

        private StringBuilder SyntaxAnalysisInfixNotation(string token, StringBuilder outputString, Stack<string> stack)
        {           
            if (token[0] == NumberMaker[0])
            {
                outputString.Append(token);
            }
            else if (token[0] == FunctionMarker[0])
            {
                stack.Push(token);
            }
            else if (token == LeftParent)
            {
                stack.Push(token);
            }
            else if (token == RightParent)
            {

                string elem;
                while ((elem = stack.Pop()) != LeftParent)
                {
                    outputString.Append(elem);
                }

                if (stack.Count > 0 &&
                    stack.Peek()[0] == FunctionMarker[0])
                {
                    outputString.Append(stack.Pop());
                }
            }
            else
            {
                while (stack.Count > 0 &&
                    Priority(token, stack.Peek()))
                {
                    outputString.Append(stack.Pop());
                }

                stack.Push(token);
            }

            return outputString;
        }

        private bool Priority(string token, string p)
        {
            return IsRightAssociated(token) ?
                GetPriority(token) < GetPriority(p) :
                GetPriority(token) <= GetPriority(p);
        }

        private bool IsRightAssociated(string token)
        {
            token.GetHashCode();
            return true;
        }

        private int GetPriority(string token)
        {
            switch (token)
            {
                case LeftParent:
                    return 0;
                case Plus:
                case Minus:
                    return 2;
                case UnPlus:
                case UnMinus:
                    return 5;
                case Multiply:
                case Divide:
                    return 4;
                default:
                    throw new ArgumentException("Unknown operator");
            }
        }

        private double Calculate(string expression)
        {
            int pos = 0;
            var stack = new Stack<double>();

            while (pos < expression.Length)
            {
                string token = LexicalAnalysisRPN(expression, ref pos);

                stack = SyntaxAnalysisRPN(stack, token);
            }

            if (stack.Count > 1)
            {
                throw new ArgumentException("Excess operand");
            }

            return stack.Pop();
        }

        private string LexicalAnalysisRPN(string expression, ref int pos)
        {
            StringBuilder token = new StringBuilder();

            token.Append(expression[pos++]);

            while (pos < expression.Length && expression[pos] != NumberMaker[0]
                && expression[pos] != OperatorMarker[0]
                && expression[pos] != FunctionMarker[0])
            {
                token.Append(expression[pos++]);
            }

            return token.ToString();
        }

        private Stack<double> SyntaxAnalysisRPN(Stack<double> stack, string token)
        {
            if (token[0] == NumberMaker[0])
            {
                stack.Push(double.Parse(token.Remove(0, 1)));
            }

            else if (NumberOfArguments(token) == 1)
            {
                double arg = stack.Pop();
                var rst = token switch
                {
                    UnPlus => arg,
                    UnMinus => -arg,
                    _ => throw new ArgumentException("Unknown operator"),
                };
                stack.Push(rst);
            }
            else
            {
                double arg2 = stack.Pop();
                double arg1 = stack.Pop();

                double rst;

                switch (token)
                {
                    case Plus:
                        rst = arg1 + arg2;
                        break;
                    case Minus:
                        rst = arg1 - arg2;
                        break;
                    case Multiply:
                        rst = arg1 * arg2;
                        break;
                    case Divide:
                        if (arg2 == 0)
                        {
                            throw new DivideByZeroException("Second argument is zero");
                        }
                        rst = arg1 / arg2;
                        break;
                    default:
                        throw new ArgumentException("Unknown operator");
                }

                stack.Push(rst);
            }

            return stack;
        }

        private int NumberOfArguments(string token)
        {
            switch (token)
            {
                case UnPlus:
                case UnMinus:
                    return 1;
                case Plus:
                case Minus:
                case Multiply:
                case Divide:
                    return 2;
                default:
                    throw new ArgumentException("Unknown operator");
            }
        }

    }
}