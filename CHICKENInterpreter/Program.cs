using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CHICKENInterpreter
{
    class Program
    {
        private static Stack<object> m_stack;
        private static int m_pointer { get; set; } = 2;
        private static int lineNumber => m_pointer - 2;

        static void Main(string[] args)
        {
            var path = args[0];

            if (!File.Exists(path))
            {
                Console.WriteLine("File not found!");
                Console.ReadKey();
                return;
            }

            m_stack = new();

            m_stack.Push(m_stack);
            m_stack.Push(string.Join(" ", args[1..]));

            using (var fs = File.OpenRead(path))
            using (var sr = new StreamReader(fs))
            {
                var line = string.Empty;

                do
                {
                    line = sr.ReadLine();

                    var chickens = string.IsNullOrWhiteSpace(line)
                        ? Array.Empty<string>()
                        : line.ToUpper().Split(" ");

                    var impostor = Array.FindIndex(chickens, chick => chick != "CHICKEN");

                    if (impostor > -1)
                    {
                        throw new Exception($"Runtime syntax error. Impostor found in line #{lineNumber}, token #{impostor + 1}");
                    }

                    m_stack.Push(chickens.Length);
                } while (line != null);
            }

            m_stack.Push(0);

            while (m_pointer < m_stack.Count)
            {
                var line = PeekAt(m_pointer);

                switch (line)
                {
                    case 0:
                        m_pointer = m_stack.Count;
                        break;
                    case 1:
                        m_stack.Push("chicken");
                        break;
                    case 2:
                        Add();
                        break;
                    case 3:
                        Sub();
                        break;
                    case 4:
                        Mul();
                        break;
                    case 5:
                        Comp();
                        break;
                    case 6:
                        Load();
                        break;
                    case 7:
                        Store();
                        break;
                    case 8:
                        Jmp();
                        break;
                    case 9:
                        Char();
                        break;
                    default:
                        PushN(line);
                        break;
                }


                m_pointer++;
            }

            Console.WriteLine(m_stack.Pop());

            Console.ReadKey();
        }

        private static void Add()
        {
            var (op1, op2) = (m_stack.Pop(), m_stack.Pop());

            try
            {
                m_stack.Push((int)op2 + (int)op1);
            }
            catch (Exception)
            {
                m_stack.Push($"{op2}{op1}");
            }
        }

        private static void Sub()
        {
            var (op1, op2) = (PopTopInt(), PopTopInt());

            m_stack.Push(op2 - op1);
        }

        private static void Mul()
        {
            var (op1, op2) = (PopTopInt(), PopTopInt());

            m_stack.Push(op1 * op2);
        }

        private static void Comp()
        {
            var (op1, op2) = (m_stack.Pop(), m_stack.Pop());

            m_stack.Push(op1 == op2);
        }

        private static void Load()
        {
            var sourceIdx = PeekAt(++m_pointer);
            var idx = PopTopInt();

            object el;
            if ((int)sourceIdx == 0)
            {
                el = PeekAt(idx);
            }
            else
            {
                try
                {
                    el = PeekAt((int)sourceIdx).ToString()[idx];
                }
                catch (IndexOutOfRangeException)
                {
                    el = "";
                }
            }

            m_stack.Push(el);
        }

        private static void Store()
        {
            var addr = PopTopInt();
            var val = m_stack.Pop();

            if (addr < m_stack.Count)
            {
                var list = m_stack.ToList();
                list.Reverse();
                list[addr] = val;

                m_stack = new Stack<object>(list);
            }
        }

        private static void Jmp()
        {
            var stackOffset = PopTopInt();
            var flag = m_stack.Pop().ToString();

            if (flag != "0" && (bool.TryParse(flag.ToString(), out var res) && res || !string.IsNullOrWhiteSpace(flag)))
            {
                m_pointer += stackOffset;
            }
        }

        private static void Char()
        {
            var top = PopTopInt();

            m_stack.Push((char)top);
        }

        private static void PushN(object n)
        {
            m_stack.Push((int)n - 10);
        }

        private static int PopTopInt()
        {
            var op = m_stack.Pop();

            if (int.TryParse(op.ToString(), out var n))
            {
                return n;
            }

            return 0;
        }

        private static object PeekAt(int index)
        {
            return m_stack.ElementAt(m_stack.Count - index - 1);
        }
    }
}
