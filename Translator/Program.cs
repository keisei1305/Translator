using System;
using System.Text;
using System.Collections.Generic;
using System.IO;

namespace Translator
{
    delegate void State(string s);
    class Scanner
    {
        StringBuilder buffer;
        StringBuilder secondBuffer;
        State currentState;
        public StringBuilder result { get; set; }
        public bool Ready { get; set; }

        Dictionary<string, string> Separators = new Dictionary<string, string>()
        {
            {" ", ""},
            {"\t", ""},
            {".", "R3" },
            {",", "R4" },
            {";", "R5" },
            {"(", "R6" },
            {")", "R7" },
            {"{", "R8" },
            {"}", "R9" },
            {"[", "R10" },
            {"]", "R11" },
            {"\n", "\n" }
        };

        Dictionary<string, string> KeyWords = new Dictionary<string, string>()
        {
            {"int", "W1"},
            {"float", "W2"},
            {"char", "W3" },
            {"return", "W4"},
            {"goto", "W5"},
            {"if", "W6"},
            {"else", "W7"},
            {"while", "W8"},
            {"for", "W9"}
        };

        Dictionary<string, string> Operations = new Dictionary<string, string>()
        {
            {"+", "O1"},
            {"-", "O2"},
            {"*", "O3"},
            {"/", "O4"},
            {"=", "O5"},
            {"<", "O6"},
            {">", "O7"},
            {"!", "O8"},
            {"==", "O9"},
            {"!=", "O10"},
            {"<=", "O11"},
            {">=", "O12"},
            {"*=", "O13"},
            {"-=", "O14"},
            {"/=", "O15"},
            {"+=", "O16"}
        };

        Dictionary<string, string> Identifiers;
        Dictionary<string, string> NumConstants;
        Dictionary<string, string> CharConstants;

        public Scanner()
        {
            currentState = S;
            buffer = new StringBuilder("");
            secondBuffer = new StringBuilder("");
            result = new StringBuilder("");
            Identifiers = new Dictionary<string, string>();
            NumConstants = new Dictionary<string, string>();
            CharConstants = new Dictionary<string, string>();
            Ready = false;
        }

        public void Read(string s)
        {
            currentState(s);
        }

        public string GetNumConst()
        {
            StringBuilder NumConst = new StringBuilder("");
            foreach (var i in NumConstants)
            {
                NumConst.Append($"{i.Key} {i.Value}\n");
            }
            return NumConst.ToString();
        }

        public string GetCharConst()
        {
            StringBuilder CharConst = new StringBuilder("");
            foreach (var i in CharConstants)
            {
                CharConst.Append($"{i.Key} {i.Value}\n");
            }
            return CharConst.ToString();
        }

        public string GetIdentifiers()
        {
            StringBuilder Ident = new StringBuilder("");
            foreach (var i in Identifiers)
            {
                Ident.Append($"{i.Key} {i.Value}\n");
            }
            return Ident.ToString();
        }

        private void S(string s)
        {
            buffer.Append(s);
            if (Char.IsLetter(s, 0)) currentState = W;
            else if (s == "_") currentState = I;
            else if (s == "/") currentState = K1;
            else if (IsLogicOp(s) || IsMathOp(s)) currentState = O1;
            else if (Char.IsDigit(s, 0)) currentState = N1;
            else if (s == ".") currentState = N2;
            else if (s == "\"") currentState = C1;
            else if (s == "'") currentState = C2;
            else if (IsSeparator(s))
            {
                Semant7();
            }
        }

        private void W(string s)
        {
            if (Char.IsLetter(s, 0)) currentState = W;
            else if (Char.IsDigit(s, 0) || s == "_") currentState = I;
            else if (IsSeparator(s) || IsOperation(s))
            {
                Semant1();
                S(s);
                return;
            }
            else currentState = F;
            buffer.Append(s);
        }

        private void I(string s)
        {
            if (Char.IsLetter(s, 0) || Char.IsDigit(s, 0) || s == "_") currentState = I;
            else if (IsSeparator(s) || IsOperation(s))
            {
                Semant2();
                S(s);
                return;
            }
            else currentState = F;
            buffer.Append(s);
        }

        private void K1(string s)
        {
            buffer.Append(s);
            if (s == "/") currentState = K2;
            else if (s == "*") currentState = K3;
            else if (s == "=") currentState = O2;
            else if (IsSeparator(s))
            {
                Semant4();
                S(s);
                return;
            }
            else if (Char.IsLetter(s, 0) || s == "_")
            {
                Semant4();
                I(s);
                return;
            }
            else currentState = F;
        }

        private void K2(string s)
        {
            if (s == "\n") Semant3();
        }

        private void K3(string s)
        {
            if (s == "*") currentState = K4;
        }

        private void K4(string s)
        {
            if (s != "/") currentState = K3;
            else Semant3();
        }

        private void O1(string s)
        {
            if (s == "=") currentState = O2;
            else if (IsSeparator(s))
            {
                Semant4();
                S(s);
                return;
            }
            else if (Char.IsLetter(s, 0) || s == "_")
            {
                Semant4();
                I(s);
                return;
            }
            else currentState = F;
            buffer.Append(s);
        }

        private void O2(string s)
        {
            if (IsSeparator(s))
            {
                Semant4();
                S(s);
                return;
            }
            else if (Char.IsLetter(s, 0) || s == "_" || Char.IsDigit(s, 0))
            {
                Semant4();
                S(s);
                return;
            }
            else currentState = F;
            buffer.Append(s);
        }

        private void N1(string s)
        {
            if (Char.IsDigit(s, 0)) currentState = N1;
            else if (s == ".") currentState = N2;
            else if (s == "E") currentState = N3;
            else if (IsSeparator(s) || IsOperation(s))
            {
                Semant5();
                S(s);
                return;
            }
            else currentState = F;
            buffer.Append(s);
        }

        private void N2(string s)
        {
            if (Char.IsDigit(s, 0)) currentState = N4;
            else currentState = F;
            buffer.Append(s);
        }

        private void N3(string s)
        {
            if (s == "+" || s == "-") currentState = N5;
            else if (Char.IsDigit(s, 0)) currentState = N6;
            else currentState = F;
            buffer.Append(s);
        }

        private void N4(string s)
        {
            if (Char.IsDigit(s, 0)) currentState = N4;
            else if (s == "E") currentState = N3;
            else if (IsSeparator(s) || IsOperation(s))
            {
                Semant5();
                S(s);
                return;
            }
            else currentState = F;
            buffer.Append(s);
        }

        private void N5(string s)
        {
            if (Char.IsDigit(s, 0)) currentState = N6;
            else currentState = F;
            buffer.Append(s);
        }

        private void N6(string s)
        {
            if (Char.IsDigit(s, 0)) currentState = N6;
            else if (IsSeparator(s) || IsOperation(s))
            {
                Semant5();
                S(s);
                return;
            }
            else currentState = F;
            buffer.Append(s);
        }

        private void C1(string s)
        {
            buffer.Append(s);
            if (s == "\"") Semant6();
        }

        private void C2(string s)
        {
            buffer.Append(s);
            if (s == "\'") Semant6();
        }

        private void Z(string s)
        {
            Ready = true;
        }

        private void F(string s)
        {

        }

        private void Semant1()
        {
            string lexem = buffer.ToString();
            if (KeyWords.ContainsKey(lexem))
                result.Append($"{KeyWords[lexem]} ");
            else if (Identifiers.ContainsKey(lexem))
                result.Append($"{Identifiers[lexem]} ");
            else
            {
                Identifiers.Add(lexem, $"I{Identifiers.Count + 1}");
                result.Append($"{Identifiers[lexem]} ");
            }
            currentState = S;
            buffer.Clear();
        }

        private void Semant2()
        {
            string lexem = buffer.ToString();
            if (Identifiers.ContainsKey(lexem))
                result.Append($"I{Identifiers[lexem]} ");
            else
            {
                Identifiers.Add(lexem, $"I{Identifiers.Count + 1}");
                result.Append($"{Identifiers[lexem]} ");
            }
            currentState = S;
            buffer.Clear();
        }

        private void Semant3()
        {
            currentState = S;
            buffer.Clear();
        }

        private void Semant4()
        {
            string lexem = buffer.ToString();
            if (Operations.ContainsKey(lexem))
                result.Append($"{Operations[lexem]} ");
            currentState = S;
            buffer.Clear();
        }

        private void Semant5()
        {
            string lexem = buffer.ToString();
            NumConstants.Add($"N{NumConstants.Count + 1}", lexem);
            result.Append($"N{lexem.Length + 1} ");
            currentState = S;
            buffer.Clear();
        }

        private void Semant6()
        {
            string lexem = buffer.ToString();
            CharConstants.Add($"C{CharConstants.Count + 1}", lexem);
            result.Append($"С{lexem.Length + 1} ");
            currentState = S;
            buffer.Clear();
        }

        private void Semant7()
        {
            string lexem = buffer.ToString();
            if (Separators.ContainsKey(lexem))
            {
                string toResult = Separators[lexem];
                result.Append($"{toResult} ");
            }
            currentState = S;
            buffer.Clear();
        }

        private bool IsMathOp(string s)
        {
            return s == "+" || s == "-" || s == "*";
        }

        private bool IsLogicOp(string s)
        {
            return s == "<" || s == ">" || s == "!" || s == "=";
        }

        private bool IsSeparator(string s)
        {
            return Separators.ContainsKey(s);
        }

        private bool IsOperation(string s)
        {
            return Operations.ContainsKey(s);
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            string WriterSrc = @"C:\Users\admin\source\repos\Translator\Translator\OutputData";
            Scanner scanner = new Scanner();
            using (StreamReader s = new StreamReader(@"C:\Users\admin\source\repos\Translator\Translator\InputData.txt"))
            {
                string line = "";
                do
                {
                    line = s.ReadLine();
                    for (int i = 0; i < line.Length; i++)
                    {
                        scanner.Read(line[i].ToString());
                    }
                    scanner.Read("\n");
                } while (!s.EndOfStream);
            }
            using (StreamWriter s = new StreamWriter(WriterSrc + @"\NumConstants.txt"))
            {
                s.Write(scanner.GetNumConst());
            }
            using (StreamWriter s = new StreamWriter(WriterSrc + @"\CharConstants.txt"))
            {
                s.Write(scanner.GetCharConst());
            }
            using (StreamWriter s = new StreamWriter(WriterSrc + @"\Identifiers.txt"))
            {
                s.Write(scanner.GetIdentifiers());
            }
            using (StreamWriter s = new StreamWriter(WriterSrc + @"\Result.txt"))
            {
                s.Write(scanner.result);
            }

            Console.ReadKey();
        }
    }
}
