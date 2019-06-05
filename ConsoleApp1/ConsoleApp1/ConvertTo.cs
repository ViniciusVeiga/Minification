using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AFNToAFD
{
    class ConvertTo
    {
        #region Variables

        public char Empty = '&';
        public string Start = string.Empty;
        public List<string> Final = new List<string>();
        public HashSet<string> Qs = new HashSet<string>();
        public HashSet<char> Alphabets = new HashSet<char>();
        public List<LineTransition> TableTransition = new List<LineTransition>();
        public HashSet<string> StateName = new HashSet<string>();
        public List<State> State = new List<State>();
        public List<QOrder> QOrders = new List<QOrder>();
        public List<LineMinification> TableMinification = new List<LineMinification>();

        #endregion

        public void StartConvert()
        {
            var afn = Menu();
            Qs = GetQs(afn);
            Alphabets = GetAphabets(afn);
            TableTransition = CreateTableTransition(afn);
            //AFeToAFD();
            //Show();

            StartMinification();
            CreateTableMinification();
            SecondPartMinification();

            Console.WriteLine("\nTabela de Marcação:");
            foreach (var item in TableMinification)
            {
                Console.WriteLine($"{item.From.Q} - {item.To.Q} -- {item.X}");
            }

            ShowMinification();
        }

        #region Exercicio Anterior

        #region Menu

        public List<string> Menu()
        {
            var afn = new List<string>();
            var value = string.Empty;

            Console.WriteLine("\nExemplo AFe digitado: ");
            Console.WriteLine("q0 -b-> q1");
            Console.WriteLine("q0 -a-> q2");
            Console.WriteLine("q1 -b-> q0");
            Console.WriteLine("q1 -a-> q1");
            Console.WriteLine("q2 -a-> q4");
            Console.WriteLine("q4 -b-> q2");
            Console.WriteLine("q2 -b-> q5");
            Console.WriteLine("q5 -a-> q2");
            Console.WriteLine("q5 -b-> q3");
            Console.WriteLine("q3 -a-> q5");
            Console.WriteLine("q3 -b-> q4");
            Console.WriteLine("q4 -a-> q3");

            Console.WriteLine("\nIniciais: ");
            Console.WriteLine("q0");

            Console.WriteLine("\nFinais: ");
            Console.WriteLine("q0, q4, q5");

            Console.WriteLine("\nQuando finalizar o AFe, digite 'Finalizado' para encerrar.");
            Console.WriteLine("Digite 'Exemplo' para ver o resultado do exemplo.");
            Console.WriteLine("Digite 'Remover' para remover o a ultima linha.");
            Console.WriteLine("Digite o AFe (Estados: Letra + Numero do estado, exemplo: q0): ");
            var sair = false;

            while (sair == false)
            {
                value = Console.ReadLine();

                switch (value)
                {
                    case "Finalizado":
                        Limpar();
                        sair = true;
                        break;

                    case "Remover":
                        if (afn.Count > 0)
                        {
                            afn.Remove(afn.Last());
                            Console.SetCursorPosition(0, Console.CursorTop - 1);
                            Console.Write(new string(' ', Console.WindowWidth));
                            Console.SetCursorPosition(0, Console.CursorTop - 2);
                            Console.Write(new string(' ', Console.WindowWidth));
                            Console.SetCursorPosition(0, Console.CursorTop - 1);
                        }
                        else
                        {
                            Limpar();
                            Console.WriteLine("AFe Vazio.");
                        }
                        break;

                    case "Exemplo":
                        afn = new List<string>()
                        {
                            "q0 -b-> q1",
                            "q0 -a-> q2",
                            "q1 -b-> q0",
                            "q1 -a-> q1",
                            "q2 -a-> q4",
                            "q4 -b-> q2",
                            "q2 -b-> q5",
                            "q5 -a-> q2",
                            "q5 -b-> q3",
                            "q3 -a-> q5",
                            "q3 -b-> q4",
                            "q4 -a-> q3"
                        };

                        Start = "q0";
                        Final.Add("q0");
                        Final.Add("q4");
                        Final.Add("q5");

                        //afn = new List<string>()
                        //{
                        //    "q0 -b-> q0",
                        //    "q1 -b-> q1",
                        //    "q2 -b-> q2",
                        //    "q3 -b-> q3",
                        //    "q4 -b-> q4",
                        //    "q5 -b-> q5",
                        //    "q0 -a-> q1",
                        //    "q1 -a-> q2",
                        //    "q2 -a-> q3",
                        //    "q3 -a-> q4",
                        //    "q4 -a-> q5",
                        //    "q5 -a-> q0"
                        //};

                        //Start = "q0";
                        //Final.Add("q2");
                        //Final.Add("q5");

                        sair = true;
                        break;
                    default:
                        afn.Add(value);
                        break;
                }

            }

            if (value != "Exemplo")
            {
                Console.WriteLine("\nDigite o inicial: ");
                Start = Console.ReadLine();

                Console.WriteLine("\nDigite os finais, separados por ,: ");
                Final = Console.ReadLine().Split(',').ToList();
            }

            return afn;
        }

        public void Limpar()
        {
            Console.SetCursorPosition(0, Console.CursorTop - 1);
            Console.Write(new string(' ', Console.WindowWidth));
            Console.SetCursorPosition(0, Console.CursorTop - 1);
        }

        #endregion

        #region Show

        public void Show()
        {
            var start = State.Find(s => s.IsStart == true).Name;

            Console.WriteLine("\nAFD  (I: Inicial, F: Final):");
            foreach (var status in State)
            {
                if (status.Name == start)
                {
                    status.Name = $"{status.Name} I";
                }

                if (Final.Any(n => status.Name.Contains(n)))
                {
                    status.Name = $"{status.Name} F";
                }

                foreach (var column in status.Columns)
                {
                    if (column.To == start)
                    {
                        column.To = $"{column.To} I";
                    }

                    if (Final.Any(n => column.To.Contains(n)))
                    {
                        column.To = $"{column.To} F";
                    }

                    Console.WriteLine($"{status.Name} -{column.Alphabet}-> {column.To}");
                }
            }
        }

        #endregion

        #region AFeToAFD

        public void AFeToAFD()
        {
            State.AddRange(Closure());

            for (int i = 0; i < State.Count; i++)
            {
                foreach (var a in Alphabets.Where(e => e != Empty))
                {
                    var state = DFAedge(State[i], a);

                    state.CreateName();

                    if (state.Name != null)
                    {
                        State[i].Columns.Add(new ColumnTransition
                        {
                            To = state.Name,
                            Alphabet = a
                        });

                        if (StateName.Add(state.Name))
                        {
                            State.Add(state);
                        }
                    }
                }
            }
        }

        #region DFAedge

        public State DFAedge(State start, char alphabet)
        {
            var state = new State();
            var names = new HashSet<string>();

            state.Qs = new List<QOrder>();

            foreach (var q in start.Qs)
            {
                var line = TableTransition.Find(t => t.Q == q.Q);

                if (line != null)
                {
                    foreach (var colunm in line.Colunm)
                    {
                        if ($"{alphabet}{Empty}".Contains(colunm.Alphabet))
                        {
                            if (names.Add(colunm.To))
                            {
                                state.Qs.Add(new QOrder { Q = colunm.To });
                            }
                        }
                    }
                }
            }

            return state;
        }

        #endregion

        #region Closure

        public List<State> Closure()
        {
            var list = new List<State>();

            var state = new State { Qs = ConcatState(Start) };
            state.IsStart = true;
            state.CreateName();

            if (StateName.Add(state.Name))
            {
                list.Add(state);
            }

            return list;
        }

        public List<QOrder> ConcatState(string start)
        {
            var list = new List<QOrder> { new QOrder { Q = start } };
            var lineStart = TableTransition.Find(t => t.Q == start);

            if (lineStart != null)
            {
                foreach (var colunm in lineStart.Colunm)
                {
                    if (colunm.Alphabet == Empty)
                    {
                        list.AddRange(ConcatState(colunm.To));
                    }
                }
            }

            return list;
        }

        #endregion

        #endregion

        #region CreateTableTransition

        public List<LineTransition> CreateTableTransition(List<string> afn)
        {
            var list = new List<LineTransition>();

            foreach (var path in afn)
            {
                var sequence = new List<QOrder>();

                foreach (var q in Qs)
                {
                    if (path.Contains(q))
                    {
                        sequence.Add(new QOrder { Q = q, OrderForSequence = path.IndexOf(q) });
                    }
                }
                sequence = sequence.OrderBy(o => o.OrderForSequence).ToList();

                var line = list.Find(t => t.Q == sequence[0].Q);

                if (line != null)
                {
                    line.Colunm.Add(new ColumnTransition
                    {
                        To = (sequence.Count == 1) ? sequence[0].Q : sequence[1].Q,
                        Alphabet = Alphabets.FirstOrDefault(a => path.Contains(a))
                    });
                }
                else
                {
                    line = new LineTransition
                    {
                        Q = sequence[0].Q,
                        Colunm = new List<ColumnTransition>()
                        {
                            new ColumnTransition
                            {
                                To = (sequence.Count == 1) ? sequence[0].Q : sequence[1].Q,
                                Alphabet = Alphabets.FirstOrDefault(a => path.Contains(a))
                            }
                        }
                    };

                    list.Add(line);
                }
            }

            return list;
        }

        #endregion

        #region Get

        #region GetAphabets

        public HashSet<char> GetAphabets(List<string> afn)
        {
            try
            {
                var hashset = new HashSet<char>();

                for (int i = 0; i < afn.Count; i++)
                {
                    var indexOf = afn[i].IndexOf('-');
                    hashset.Add(afn[i][indexOf + 1]);
                }

                return hashset;
            }
            catch (Exception)
            {
                Console.Write("Erro");
                throw;
            }
        }

        #endregion

        #region GetQs

        public HashSet<string> GetQs(List<string> afn)
        {
            try
            {
                var hashset = new HashSet<string>();

                for (int i = 0; i < afn.Count; i++)
                {
                    var variable = afn[i][0];
                    var copy = afn[i];

                    while (afn[i].Contains(variable))
                    {
                        var indexOf = afn[i].IndexOf(variable);
                        var count = 0;
                        var remove = string.Empty;

                        try
                        {
                            while (afn[i][indexOf + count] != ' ')
                            {
                                count++;
                            }
                        }
                        catch (Exception) { }

                        remove = afn[i].Substring(indexOf, count);
                        afn[i] = afn[i].Remove(indexOf, count);

                        hashset.Add(remove);
                    }

                    afn[i] = copy;
                }

                return hashset;
            }
            catch (Exception)
            {
                Console.Write("Erro");
                throw;
            }
        }

        #endregion

        #endregion

        #endregion

        #region Minificação

        #region StartMinification

        public void StartMinification()
        {
            foreach (var q in Qs)
            {
                var qorder = new QOrder { Q = q };

                QOrders.Add(qorder);
            }
        }

        #endregion

        #region CreateTableMinification

        public void CreateTableMinification()
        {
            foreach (var q in QOrders)
            {
                foreach (var qn in QOrders)
                {
                    if (qn.Order > q.Order)
                    {
                        var aux = new LineMinification { To = qn, From = q };

                        if (Final.Any(x => x == q.Q))
                        {
                            if (Final.Any(x => x == qn.Q))
                            {
                                aux.X = false;
                            }
                            else
                            {
                                aux.X = true;
                            }
                        }
                        else
                        {
                            if (!Final.Any(x => x == qn.Q))
                            {
                                aux.X = false;
                            }
                            else
                            {
                                aux.X = true;
                            }
                        }

                        TableMinification.Add(aux);
                    }
                }
            }
        }

        #endregion

        #region SecondPartMinification

        public void SecondPartMinification()
        {
            var aux = TableMinification.Where(x => x.X == false).ToList();

            foreach (var t in aux)
            {
                var result = Check(t.From, t.To, t);

                if (result == true)
                {
                    t.X = result;
                    SecondPartMinification();
                }
            }

        }

        public bool Check(QOrder from, QOrder to, LineMinification t)
        {
            var qs1 = new List<QOrder>();
            var qs2 = new List<QOrder>();
            var c1 = new List<ColumnTransition>();
            var c2 = new List<ColumnTransition>();
            var q1 = TableTransition.First(x => x.Q == from.Q);
            var q2 = TableTransition.First(x => x.Q == to.Q);

            c1.Add(q1.Colunm.Find(x => x.Alphabet == Alphabets.ElementAt(0)));
            c1.Add(q2.Colunm.Find(x => x.Alphabet == Alphabets.ElementAt(0)));
            c2.Add(q1.Colunm.Find(x => x.Alphabet == Alphabets.ElementAt(1)));
            c2.Add(q2.Colunm.Find(x => x.Alphabet == Alphabets.ElementAt(1)));

            c1.ForEach(x => qs1.Add(QOrders.Find(a => x.To == a.Q)));
            c2.ForEach(x => qs2.Add(QOrders.Find(a => x.To == a.Q)));

            qs1 = qs1.OrderBy(x => x.Order).ToList();
            qs2 = qs2.OrderBy(x => x.Order).ToList();

            var t1 = TableMinification.Find(x => x.From.Q == qs1[0].Q && x.To.Q == qs1[1].Q);
            var t2 = TableMinification.Find(x => x.From.Q == qs2[0].Q && x.To.Q == qs2[1].Q);

            if (t1 == t)
            {
                t1 = null;
            }

            if (t2 == t)
            {
                t2 = null;
            }

            if (t1 == t2)
            {
                t2 = null;
            }

            if (t1 != null && t2 != null)
            {
                if (t1.X == true && t2.X == true)
                {
                    return true;
                }
                else if (t1.X == false && t2.X == false)
                {
                    return true;
                }
            }
            else if (t1 != null)
            {
                if (t1.X == true)
                {
                    return true;
                }
            }
            else if (t2 != null)
            {
                if (t2.X == true)
                {
                    return true;
                }
            }

            return false;
        }

        #endregion

        #region ShowMinification

        public void ShowMinification()
        {
            var print = new HashSet<string>();

            Start = Change(Start);

            for (int i = 0; i < Final.Count; i++)
            {
                Final[i] = Change(Final[i]);
            }

            for (int i = 0; i < TableTransition.Count; i++)
            {
                TableTransition.ElementAt(i).Q = Change(TableTransition.ElementAt(i).Q);

                if (TableTransition.ElementAt(i).Q == Start)
                {
                    TableTransition.ElementAt(i).Q += " I";
                }

                if (Final.Any(n => n == TableTransition.ElementAt(i).Q))
                {
                    TableTransition.ElementAt(i).Q += " F";
                }

                foreach (var item in TableTransition.ElementAt(i).Colunm)
                {
                    item.To = Change(item.To);

                    if (item.To == Start)
                    {
                        item.To += " I";
                    }

                    if (Final.Any(n => n == item.To))
                    {
                        item.To += " F";
                    }

                    print.Add($"{TableTransition.ElementAt(i).Q} - {item.Alphabet} -> {item.To}");
                }
            }

            Console.WriteLine("\nResultado:");
            foreach (var item in print)
            {
                Console.WriteLine(item);
            }
        }

        public string Change(string q)
        {
            var aux = TableMinification.Where(x => x.X == false);

            var find = aux.FirstOrDefault(x => x.From.Q == q || x.To.Q == q);

            if (find != null)
            {
                return $"{find.From.Q}{find.To.Order}";
            }

            return q;
        }

        #endregion

        #endregion
    }
}
