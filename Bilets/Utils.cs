using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bilets
{
    public class ComplexCounter
    {
        IEnumerable<int> _counters { set; get; } = new int[0];
        IEnumerable<int> _maximum { set; get; } = new int[0];

        (IEnumerable<int>, IEnumerable<int>) Counters
        {
            set
            {
                _counters = value.Item1;
                _maximum = value.Item2;
            }
            get => (_counters, _maximum);
        }

        public int StatesCount => _maximum.Aggregate((a, b) => a * b);


    }

    public class Group
    {
        public List<Subgroup> subgroups { set; get; } = new List<Subgroup>();
        public IList<int> positions = new int[0];
        public List<IList<Question>> states { get; } = new List<IList<Question>>();
        public List<IList<Subgroup>> variants { get; } = new List<IList<Subgroup>>();
        public void make_all_variants()
        {

            variants.Clear();
            
            var subgroups_var = new int[positions.Count];
            for (int i = 0; i < positions.Count; ++i)
                subgroups_var[i] = i;

            while (true)
            {
                variants.Add(subgroups_var.Select(x => subgroups[x]).ToArray()); //вариант группы - массив с номерами подгрупп, входящих в вариант.

                if (subgroups_var[0] == subgroups.Count - positions.Count)
                {
                    break;
                }

                for (int i = positions.Count - 1; i >= 0; --i)
                {
                    if (subgroups_var[i] == subgroups.Count - positions.Count + i)
                    {
                        continue;
                    }
                    else
                    {
                        subgroups_var[i]++;
                        for (int j = i + 1; j < positions.Count; ++j)
                        {
                            subgroups_var[j] = subgroups_var[j - 1] + 1;
                        }
                        break;
                    }
                }
            }
            //====================================================================
        }
        public void make_all_states()
        {
            states.Clear();

            //генерация всевозможных состояний группы для каждого варианта.
            for (int _i = 0; _i < variants.Count; ++_i)
            {
                var variant = variants[_i];

                var state = positions.Select(x => 0).ToArray();

                var last_state = variant.Select(x => x.questions.Count - 1);

                while (true)
                {
                    //var st = state.Select((x, i) => subgroups[variant[i]].questions[x]).ToArray<int>();
                    states.Add(state.Select((x, i) => variant[i].questions[x]).ToArray());   //состояние группы - номера вопросов, которые в него входят

                    //Console.WriteLine($"gr{groups.FindIndex(x => x == gr)}| state {state.Select((x,i) => questions_strings[gr.subgroups[sub_var[i]].questions[x]]).Aggregate((a,b) => a + " " + b)}");
                    if (state.SequenceEqual(last_state))
                    {
                        break;
                    }

                    for (int i = state.Length - 1; i >= 0; --i)
                    {
                        if (state[i] == variant[i].questions.Count - 1)
                        {
                            state[i] = 0;
                        }
                        else
                        {
                            state[i]++;
                            break;
                        }
                    }
                    //Console.WriteLine("затянулся ашкьюдишкой со вкусом сладкой бэброчки.......");
                }
            }
        }

    };

    public class Subgroup
    {
        public IList<Question> questions { set; get; } = new List<Question>();
    };

    static class R
    {
        public class RandomProportional : Random
        {
            // The Sample method generates a distribution proportional to the value
            // of the random numbers, in the range [0.0, 1.0].
            protected override double Sample()
            {
                return Math.Sqrt(base.Sample());
            }

            public override int Next(int val)
            {
                //val++;
                return (int)Math.Round(Sample() * val);
            }
        }

        private static Random rng = new Random();

        public static void Shuffle<T>(this IList<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;

                int k = rng.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
    }
    public class Question
    {
        public string Text { set; get; } = string.Empty;
        public IList<int> marks { set; get; } = new int[0];

    }

}
