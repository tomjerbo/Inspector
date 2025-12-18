using System;
using UnityEditor;
using UnityEngine;

namespace Jerbo.Inspector {
    [CustomPropertyDrawer(typeof(ShowIf))]
    public class ShowIfDrawer : PropertyDrawer {

        const char ARGUMENT_SPLIT_CHARACTER = ' ';
        ShowIf target;
        Token[] cachedTokens;
        Result parseResult;
        

        enum Result {
            NotParsed,
            Ok,
            InvalidArgumentSyntax,
            InvalidComparisson,
            InvalidType,
        }
        

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            target = attribute as ShowIf;
            if (target == null) {
                return;
            }

            bool isVisible = true;
            if (parseResult == Result.NotParsed) {
                parseResult = ParseArgumentString(target.argument, property, out isVisible);
                if (parseResult != Result.Ok) {
                    Debug.Log($"Parsing failed! {parseResult}");
                }
            }
            else if (parseResult == Result.Ok) {
                Result result = EvaluateTokens(cachedTokens, property, out isVisible);
                if (result != Result.Ok) {
                    Debug.LogError("Failed to EvaluateTokens: " + result);
                }
            }

            if (isVisible) {
                EditorGUI.PropertyField(position, property, label);
            }
        }


        Result ParseArgumentString(string argument, SerializedProperty property, out bool result) {
            result = false;
            if (string.IsNullOrEmpty(argument)) {
                return Result.InvalidArgumentSyntax;
            }
            
            cachedTokens = GenerateTokens(argument);
            return EvaluateTokens(cachedTokens, property, out result);
        }
        

        Token[] GenerateTokens(string argument) {
            string[] argumentSections = argument.Split(ARGUMENT_SPLIT_CHARACTER, StringSplitOptions.RemoveEmptyEntries);
            int argCount = argumentSections.Length;
            Token[] generatedTokens = new Token[argCount];

            for (int i = 0; i < argCount; i++) {
                if (float.TryParse(argumentSections[i], out float floatValue)) {
                    generatedTokens[i].tokenValue = floatValue;
                    generatedTokens[i].type = TokenType.Float;
                }
                else if (int.TryParse(argumentSections[i], out int intValue)) {
                    generatedTokens[i].tokenValue = intValue;
                    generatedTokens[i].type = TokenType.Int;
                }
                else if (bool.TryParse(argumentSections[i], out bool boolValue)) {
                    generatedTokens[i].tokenValue = boolValue;
                    generatedTokens[i].type = TokenType.Bool;
                }
                else {
                    switch (argumentSections[i]) {
                        case "==": generatedTokens[i].type = TokenType.Equals; break;
                        case "!=": generatedTokens[i].type = TokenType.NotEquals; break;
                        case ">": generatedTokens[i].type = TokenType.MoreThan; break;
                        case "<": generatedTokens[i].type = TokenType.LessThan; break;
                        case ">=": generatedTokens[i].type = TokenType.MoreOrEqual; break;
                        case "<=": generatedTokens[i].type = TokenType.LessOrEqual; break;

                        case "&&": generatedTokens[i].type = TokenType.And; break;
                        case "||": generatedTokens[i].type = TokenType.Or; break;
                        case "null":
                        {
                            generatedTokens[i].tokenValue = false;
                            generatedTokens[i].type = TokenType.Bool;
                            break;
                        }

                        default:
                            generatedTokens[i].type = TokenType.VariableName;
                            generatedTokens[i].tokenValue = argumentSections[i];
                            break;
                    }
                }
            }

            return generatedTokens;
        }
        

        Result EvaluateTokens(Token[] tokens, SerializedProperty property, out bool value) {
            value = false;

            if (tokens.Length == 1) {
                Result result = ResolveValue(tokens[0], property, out object tokenValue);
                if (result != Result.Ok) {
                    return result;
                }
                
                value = ConvertToBool(tokenValue);
                return Result.Ok;
            }

            // Expressions are either just a variable name or 3 tokens 'myTransform', 'A == B', 'myBool == false', 'rigidBody != null'
            if (tokens.Length < 3) {
                return Result.InvalidArgumentSyntax;
            }

            int index = 0;
            Result currentResult = EvaluateComparison(tokens, ref index, property, out bool current);
            if (currentResult != Result.Ok) return currentResult;

            while (index < tokens.Length) {
                TokenType op = tokens[index].type;
                if (op != TokenType.And && op != TokenType.Or) {
                    return Result.InvalidArgumentSyntax;
                }

                index++;

                Result nextResult = EvaluateComparison(tokens, ref index, property, out bool next);
                if (nextResult != Result.Ok) return nextResult;

                if (op == TokenType.And)
                    current = current && next;
                else
                    current = current || next;
            }

            value = current;
            return Result.Ok;
        }

        
        Result EvaluateComparison(Token[] tokens, ref int index, SerializedProperty property, out bool resultValue) {
            resultValue = true;
            
            if (index + 2 >= tokens.Length) {
                return Result.InvalidArgumentSyntax;
            }

            
            Result leftResult = ResolveValue(tokens[index], property, out object left);
            if (leftResult != Result.Ok) {
                return leftResult;
            }
            
            TokenType op = tokens[index + 1].type;
            
            Result rightResult = ResolveValue(tokens[index + 2], property, out object right);
            if (rightResult != Result.Ok) {
                return rightResult;
            }
            index += 3;

            return Compare(left, op, right, out resultValue);
        }

        
        Result ResolveValue(Token token, SerializedProperty property, out object tokenValue) {
            if (token.type != TokenType.VariableName) {
                tokenValue = token.tokenValue;
                return Result.Ok;
            }

            string varName = (string)token.tokenValue;
            SerializedProperty declaredProperty = property.serializedObject.FindProperty(varName);
            if (declaredProperty == null) {
                throw new Exception($"Unknown variable '{varName}'");
            }

            switch (declaredProperty.propertyType) {
                case SerializedPropertyType.Integer: 
                    tokenValue = declaredProperty.intValue;
                    return Result.Ok;
                
                case SerializedPropertyType.Float:
                    tokenValue = declaredProperty.floatValue;
                    return Result.Ok;
                
                case SerializedPropertyType.Boolean:
                    tokenValue = declaredProperty.boolValue;
                    return Result.Ok;
                
                case SerializedPropertyType.ObjectReference:
                    tokenValue = declaredProperty.objectReferenceValue != null;
                    return Result.Ok;
                
                case SerializedPropertyType.String:
                    tokenValue = string.IsNullOrEmpty(declaredProperty.stringValue);
                    return Result.Ok;
            }

            tokenValue = null;
            return Result.InvalidType;
        }

        
        bool ConvertToBool(object v) {
            switch (v) {
                case bool b: return b;
                case int i: return i != 0;
                case float f: return Math.Abs(f) > float.Epsilon;
                case UnityEngine.Object o: return o != null;
                case null: return false;
            }
            
            return true;
        }
        

        Result Compare(object a, TokenType op, object b, out bool evaluation) {
            evaluation = true;
            
            if (a is int ai && b is int bi) {
                evaluation = CompareInt(ai, bi, op);
                return Result.Ok;
            }

            if (a is float af && b is float bf) {
                evaluation = CompareFloat(af, bf, op);
                return Result.Ok;
            }

            if (a is bool ab && b is bool bb) {
                switch (op) {
                    case TokenType.Equals:
                    {
                        evaluation = ab == bb;
                        return Result.Ok;
                    }
                    case TokenType.NotEquals:
                    {
                        evaluation = ab != bb;
                        return Result.Ok;
                    }
                }
            }
            
            
            return Result.InvalidComparisson;
        }

        bool CompareFloat(float a, float b, TokenType op) {
            switch (op) {
                case TokenType.Equals: return Mathf.Approximately(a, b);
                case TokenType.NotEquals: return Mathf.Approximately(a, b) == false;
                case TokenType.MoreThan: return a > b;
                case TokenType.LessThan: return a < b;
                case TokenType.MoreOrEqual: return a >= b;
                case TokenType.LessOrEqual: return a <= b;
            }

            throw new Exception($"Invalid operator {op}");
        }
        
        bool CompareInt(int a, int b, TokenType op)  {
            switch (op) {
                case TokenType.Equals: return a == b;
                case TokenType.NotEquals: return a != b;
                case TokenType.MoreThan: return a > b;
                case TokenType.LessThan: return a < b;
                case TokenType.MoreOrEqual: return a >= b;
                case TokenType.LessOrEqual: return a <= b;
            }

            throw new Exception($"Invalid operator {op}");
        }

        struct Token {
            internal TokenType type;
            internal object tokenValue;
        }

        enum TokenType {
            VariableName,

            Equals,
            NotEquals,
            LessThan,
            MoreThan,
            MoreOrEqual,
            LessOrEqual,

            And,
            Or,

            Bool,
            Int,
            Float,
            Null,
        }
    }
}