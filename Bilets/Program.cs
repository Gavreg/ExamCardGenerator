// See https://aka.ms/new-console-template for more information

using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using  Bilets;



string latex_cmd = "pdflatex";
string questions_file = "questions.txt";
string output_dir = "";
string output_file = "card";
bool randomize = false;
bool randomize_in_cars = false;
string latex_args = "";
string pattern = "patern.txt";
int clear_level = 0;



#region ParamsCheck


if (args.Length!=0)
{
    for (int i = 0; i<args.Length; ++i)
    {
        switch (args[i])
        {
            case "-h":
            case "--help": Console.WriteLine("HelpMsg\n"); return;
            case "-l":
            case "--latex":
                latex_cmd = args[i+1];
                i++;
                continue;
            case "-q":
            case "--questions":
                questions_file = args[i+1];
                i++;
                continue;
            case "-d":
            case "--directory":
                output_dir = args[i+1];
                i++;
                continue;
            case "-f":
            case "--filename":
                output_file = args[i+1];
                i++;
                continue;
            case "-r":
            case "--random":
                randomize = true;
                continue;
            case "-R":
                randomize_in_cars = true;
                continue;
            case "-a":
            case "--arguments":
                latex_args = args[i+1];
                i++;
                continue;
            case "-p":
            case "--pattern":
                pattern = args[i + 1];
                i++;
                continue;
            case "-c":
            case "--clear-level":
                clear_level = Int32.Parse(args[++i]);
                continue;
            default: 
                break;
        }
    }
}

if (!File.Exists(pattern))
{
    Console.WriteLine($"Pattern file {pattern} not exists!");
    return;
}

if (!File.Exists(questions_file))
{
    Console.WriteLine($"Pattern file {questions_file} not exists!");
    return;
}





#endregion

var questions_strings = File.ReadAllLines(questions_file).ToList<string>();
var questions = new Bilets.Group();
  

int _curent_list = -1;
Subgroup cur_sub = null;
Bilets.Group cur_gr = null;
var groups = new List<Bilets.Group>();

for (int i = 0; i < questions_strings.Count; ++i)
{
    string s = questions_strings[i];
    
    if (Regex.IsMatch(s, @"^ *\[group=\d+( *, *\d+)*\] *$"))
    {
        cur_gr = new Bilets.Group();
        groups.Add(cur_gr);
        cur_sub = null;
        IList<int> group_position;
        group_position = Regex.Matches(s, @"\d+").Select(x => Int32.Parse(x.Value)).ToArray();
        cur_gr.positions = group_position;
        
    }
    else if(Regex.IsMatch(s, @"^ *\[sub\] *$"))
    {
        cur_sub = new Subgroup();
        cur_gr.subgroups.Add(cur_sub);
    }
    else if  (Regex.IsMatch(s, @"^ *\[/sub\] *$"))
    {
        cur_sub = null;
    }
    else
    {
        string _pattern = @"^ *\[mark=\d+( *, *\d+)*\] *";
        string q_string = s;
        var marks = new int[0];

        Question q = new Question();

        if (Regex.IsMatch(s, _pattern))
        {
            var match = Regex.Match(s, _pattern);
            q_string = s.Substring(match.Captures[0].Index + match.Captures[0].Length);
            var mark_string = match.Value;
            marks = Regex.Matches(mark_string, @"\d+").Select(x => Int32.Parse(x.Value)).ToArray();
        }        
        q.Text = q_string;
        q.marks = marks;

        if (cur_sub == null)
        {
            var _sub = new Subgroup();
            _sub.questions.Add(q);
            cur_gr.subgroups.Add(_sub);
        }
        else
        {
            cur_sub.questions.Add(q);
        }
        
    }
    
}

foreach (var gr in groups)
{
    if (gr.positions.Count > gr.subgroups.Count)
    {
        Console.WriteLine("In groups position count must be bigger then subgroup count!");
        return;
    }
}

var group_states = new Dictionary<Bilets.Group, IList<IList<int>>>();

Dictionary<int, Bilets.Group> positions = new Dictionary<int, Bilets.Group>();
foreach (var gr in groups)
{
    gr.make_all_variants();
    gr.make_all_states();
}


var last_bilet_state = groups.Select(x => x.states.Count-1);
var billet_state = groups.Select(x => 0).ToArray();

var all_bilets = new List<IList<int>>();

while (true)
{
   
    
    all_bilets.Add(billet_state.Select(x => x ).ToArray());
    
    if (billet_state.SequenceEqual(last_bilet_state) )
    {
        break;
    }

    for (int i = groups.Count-1; i>=0; --i)
    {
        if (billet_state[i] == groups[i].states.Count-1)
        {
            billet_state[i] = 0;
        }
        else
        {
            billet_state[i]++;
            break;
        }
    }
}

foreach (var gr in groups)
{
    foreach (var p in gr.positions)
    {
        positions[p] = gr;
    }
}

List<IList<Question>> exams = new List<IList<Question>>();

foreach (var bilet in all_bilets)
{
    var exam = new Question[positions.Count];
    for (int i=0; i<groups.Count; ++i)
    {
        var gr_state = groups[i].states[bilet[i]];
        for (int j = 0; j < groups[i].positions.Count; ++j)
        {
            exam[groups[i].positions[j]-1] = gr_state[j];
        }
    }

    bool addible = true;
    for (int i=0; i<exam.Length; ++i)
        for (int j=i+1; j<exam.Length; ++j)
        {
            if ( exam[i].marks.Intersect(exam[j].marks).Count() != 0)
            {
                addible = false;
                i = exam.Length;
                break;
            }
        }
    if (addible)
        exams.Add(exam);    
} 

if (randomize)
{
    R.Shuffle(exams);
    R.Shuffle(exams);
    R.Shuffle(exams);
    R.Shuffle(exams);
    R.Shuffle(exams);
}

if (randomize_in_cars)
{
    Parallel.For(0, exams.Count, i =>
    {
        R.Shuffle(exams[i]);
        R.Shuffle(exams[i]);
        R.Shuffle(exams[i]);
        R.Shuffle(exams[i]);
        R.Shuffle(exams[i]);
    });
}


foreach (var exam in exams)
{
    Console.WriteLine(exam.Select(x => x.Text).Aggregate((a, b) => a + " " + b));
}

int progr = 0;
int max = exams.Count;
//max = 5;
object locker = new object();
int counter = 0;

var template_file = File.ReadAllText(pattern);

Directory.CreateDirectory(output_dir);

for (int i=0; i<exams.Count; ++i)
{
    var out_file_name = $"{output_dir}{output_file}{i}";

    var exam = exams[i];
    var _template = template_file;

    _template = _template.Replace("%number%", i.ToString()+1);

    for (int j = 0; j < exam.Count; ++j)
    {
        _template = _template.Replace($"%q{j+1}%", exam[j].Text);
    }
    
    File.WriteAllText(out_file_name + ".tex", _template);
}

Console.CursorVisible = false;
Parallel.For(0, max, i =>
{ 

    var out_file_name = $"{output_dir}{output_file}{i}";

    Process p = new Process();
    p.StartInfo = new ProcessStartInfo(latex_cmd);
    p.StartInfo.Arguments = $" -output-directory=\"{output_dir}\" {out_file_name}.tex {latex_args}";
    p.StartInfo.WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory;
    p.StartInfo.CreateNoWindow = true;
    p.StartInfo.UseShellExecute = false;
    p.Start();

    //var p = System.Diagnostics.Process.Start("pdflatex", $" -output-directory=pdf Rezult/var{i+1}.tex");
    p.WaitForExit();
    if (clear_level>=1)
        File.Delete($"{ out_file_name}.tex");
    if (clear_level>=2)
    {
        File.Delete(out_file_name + ".aux");
        File.Delete(out_file_name + ".log");
    }

    
    //Console.WriteLine($"Rezult\\var{i + 1}.tex");
    lock (locker)
    {
        counter++;


        string s = $"{counter}/{max}";
        int w = Console.WindowWidth  - s.Length;
        int left = (int)Math.Round(1.0 * counter / max  * (w-2), 0);
        
        int right = w-2-left;
        string line = $"{s}[{new string('\u2588',left)}{new string('-',right)}]";
        Console.Write(new string(' ', Console.WindowWidth));
        Console.SetCursorPosition(0, Console.CursorTop);
        if (File.Exists($"{out_file_name}.pdf"))
            Console.WriteLine($"{out_file_name}.pdf - OK!");
        else
            Console.WriteLine($"{out_file_name}.pdf - ERR!");
        Console.Write(line);
        
        Console.SetCursorPosition(0, Console.CursorTop );
    }

});

#region OldProgram
/*
 
List<string> questions = new List<string>();
List<string> questions1 = new List<string>();

List<(int, int, int)> bils = new List<(int, int, int)>();
List<int> groups = new List<int> { 10, 10, 10, 10, 10, 10 };

var patern = File.ReadAllText("patern.txt");
var q1 = File.ReadAllLines("q1.txt");
var q2 = File.ReadAllLines("q2.txt");

questions = q1.ToList<string>();
questions1 = q2.ToList<string>();

List<int> groupsEndNumbers = new List<int>();
for (int i = 0; i < groups.Count; ++i)
{
    if (i > 0)
        groupsEndNumbers.Add(groupsEndNumbers.Last() + groups[i]);
    else
        groupsEndNumbers.Add(groups[i] - 1);
}


for (int i = 0; i < questions.Count; ++i)
{
    (int, int, int) a;

    a.Item1 = i;
    int itemsLeft = 0;

    for (int j = 0; j < questions1.Count - 1; ++j)
    {
        a.Item2 = j;
        int group_num = 0;



        if (j < groupsEndNumbers[0])
            group_num = 0;
        else
            for (int _i = 1; _i < groups.Count; _i++)
            {
                if (j > groupsEndNumbers[_i - 1] && j <= groupsEndNumbers[_i])
                    group_num = _i;
            }
        for (int k = groupsEndNumbers[group_num] + 1; k < questions1.Count; ++k)
        {
            a.Item3 = k;
            bils.Add(a);
        }
    }
}

R.Shuffle(bils);
R.Shuffle(bils);
R.Shuffle(bils);
R.Shuffle(bils);

int progr = 0;
int max = 300;

object locker = new object();
int counter = 0;

Parallel.For(0, max, new ParallelOptions { MaxDegreeOfParallelism = 7 }, i =>
{
    var b = patern
          .Replace("%number%", (i + 1).ToString())
          .Replace("%q1%", questions[bils[i].Item1])
          .Replace("%q2%", questions1[bils[i].Item2])
          .Replace("%q3%", questions1[bils[i].Item3]);
    File.WriteAllText($"Rezult\\var{i + 1}.tex", b);
    string strCmdText;
    //strCmdText = $"pdflatex.exe C:\\Users\\gavre\\source\\repos\\InformatikaBilets\\InformatikaBilets\\bin\\DebugRezult\\var{i}.tex";
    //string cmd = $"cd \"{AppDomain.CurrentDomain.BaseDirectory}Rezult\" && pdflatex.exe var{i}.tex";
    //System.Diagnostics.Process.Start("cmd.exe", $"/C cd Rezult && pdflatex var{i}.tex");

    Process p = new Process();
    p.StartInfo = new ProcessStartInfo("pdflatex");
    p.StartInfo.Arguments = $" -output-directory=pdf Rezult/var{i + 1}.tex";
    p.StartInfo.WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory;
    p.StartInfo.CreateNoWindow = true;
    p.StartInfo.UseShellExecute = false;
    p.Start();

    //var p = System.Diagnostics.Process.Start("pdflatex", $" -output-directory=pdf Rezult/var{i+1}.tex");
    p.WaitForExit();
    File.Delete($"pdf/var{i + 1}.aux");
    File.Delete($"pdf/var{i + 1}.log");
    //Console.WriteLine($"Rezult\\var{i + 1}.tex");
    Interlocked.Increment(ref counter);
    lock (locker)
    {
        Console.WriteLine(Math.Round(1.0 * counter / max * 100, 5));
        Console.SetCursorPosition(0, Console.CursorTop - 1);
    }

});

#endregion


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

    private static Random rnd = new Random();
    public static void Shuffle<T>(this IList<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = rnd.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }
}

*/

#endregion