using System;
using UnityEditor;
using UnityEngine;

namespace Jerbo.Inspector {
    [CustomPropertyDrawer(typeof(ShowIf))]
    public class ShowIfDrawer : PropertyDrawer {

        const char ARGUMENT_SPLIT_CHARACTER = ' ';
        readonly ShowIf target;
        Token[] cached_tokens;
        Result parse_result;
        bool is_visible;
        

        enum Result {
            NotParsed,
            Ok,
            InvalidArgumentSyntax,
            InvalidComparisson,
            InvalidType,
        }

        public ShowIfDrawer() {
            target = attribute as ShowIf;
        }
        
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            return is_visible ? base.GetPropertyHeight(property, label) : 0f;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            if (parse_result == Result.NotParsed) {
                parse_result = parse_argument_string(target.argument, property, out is_visible);
                if (parse_result != Result.Ok) {
                    Debug.Log($"Parsing failed! {parse_result}");
                }
            }
            else if (parse_result == Result.Ok) {
                Result result = evaluate_tokens(cached_tokens, property, out is_visible);
                if (result != Result.Ok) {
                    Debug.LogError("Failed to EvaluateTokens: " + result);
                }
            }

            if (is_visible) {
                EditorGUI.PropertyField(position, property, label);
            }
        }


        Result parse_argument_string(string argument, SerializedProperty property, out bool result) {
            result = false;
            if (string.IsNullOrEmpty(argument)) {
                return Result.InvalidArgumentSyntax;
            }
            
            cached_tokens = GenerateTokens(argument);
            return evaluate_tokens(cached_tokens, property, out result);
        }
        

        Token[] GenerateTokens(string argument) {
            string[] argument_sections = argument.Split(ARGUMENT_SPLIT_CHARACTER, StringSplitOptions.RemoveEmptyEntries);
            int num_args = argument_sections.Length;
            Token[] tokens = new Token[num_args];

            for (int i = 0; i < num_args; i++) {
                if (float.TryParse(argument_sections[i], out float floatValue)) {
                    tokens[i].tokenValue = floatValue;
                    tokens[i].type = TokenType.Float;
                }
                else if (int.TryParse(argument_sections[i], out int intValue)) {
                    tokens[i].tokenValue = intValue;
                    tokens[i].type = TokenType.Int;
                }
                else if (bool.TryParse(argument_sections[i], out bool boolValue)) {
                    tokens[i].tokenValue = boolValue;
                    tokens[i].type = TokenType.Bool;
                }
                else {
                    switch (argument_sections[i]) {
                        case "==": tokens[i].type = TokenType.Equals; break;
                        case "!=": tokens[i].type = TokenType.NotEquals; break;
                        case ">": tokens[i].type = TokenType.MoreThan; break;
                        case "<": tokens[i].type = TokenType.LessThan; break;
                        case ">=": tokens[i].type = TokenType.MoreOrEqual; break;
                        case "<=": tokens[i].type = TokenType.LessOrEqual; break;

                        case "&&": tokens[i].type = TokenType.And; break;
                        case "||": tokens[i].type = TokenType.Or; break;
                        case "null":
                        {
                            tokens[i].tokenValue = false;
                            tokens[i].type = TokenType.Bool;
                            break;
                        }

                        default:
                            tokens[i].type = TokenType.VariableName;
                            tokens[i].tokenValue = argument_sections[i];
                            break;
                    }
                }
            }

            return tokens;
        }
        

        Result evaluate_tokens(Token[] tokens, SerializedProperty property, out bool value) {
            value = false;

            if (tokens.Length == 1) {
                Result result = resolve_token_value(tokens[0], property, out object tokenValue);
                if (result != Result.Ok) {
                    return result;
                }
                
                value = convert_token_value_to_bool(tokenValue);
                return Result.Ok;
            }

            // Expressions are either just a variable name or 3 tokens 'myTransform', 'A == B', 'myBool == false', 'rigidBody != null'
            if (tokens.Length < 3) {
                return Result.InvalidArgumentSyntax;
            }

            int index = 0;
            Result currentResult = evaluate_comparison(tokens, ref index, property, out bool current);
            if (currentResult != Result.Ok) return currentResult;

            while (index < tokens.Length) {
                TokenType op = tokens[index].type;
                if (op != TokenType.And && op != TokenType.Or) {
                    return Result.InvalidArgumentSyntax;
                }

                index++;

                Result nextResult = evaluate_comparison(tokens, ref index, property, out bool next);
                if (nextResult != Result.Ok) return nextResult;

                if (op == TokenType.And)
                    current = current && next;
                else
                    current = current || next;
            }

            value = current;
            return Result.Ok;
        }

        
        Result evaluate_comparison(Token[] tokens, ref int index, SerializedProperty property, out bool resultValue) {
            resultValue = true;
            
            if (index + 2 >= tokens.Length) {
                return Result.InvalidArgumentSyntax;
            }

            
            Result leftResult = resolve_token_value(tokens[index], property, out object left);
            if (leftResult != Result.Ok) {
                return leftResult;
            }
            
            TokenType op = tokens[index + 1].type;
            
            Result rightResult = resolve_token_value(tokens[index + 2], property, out object right);
            if (rightResult != Result.Ok) {
                return rightResult;
            }
            index += 3;

            return compare(left, op, right, out resultValue);
        }

        
        Result resolve_token_value(Token token, SerializedProperty property, out object tokenValue) {
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
        
        bool convert_token_value_to_bool(object v) {
            switch (v) {
                // Can't really see a good use case for numbers... what should they even evaluate too??
                case int i: return i > 0;
                case float f: return Math.Abs(f) > float.Epsilon;
                case bool b: return b;
                case UnityEngine.Object o: return o != null;
                case null: return false;
            }
            
            return true;
        }
        

        Result compare(object a, TokenType op, object b, out bool evaluation) {
            evaluation = true;
            
            if (a is int ai && b is int bi) {
                evaluation = compare_int(ai, bi, op);
                return Result.Ok;
            }

            if (a is float af && b is float bf) {
                evaluation = compare_float(af, bf, op);
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

        bool compare_float(float a, float b, TokenType op) {
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
        
        bool compare_int(int a, int b, TokenType op)  {
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