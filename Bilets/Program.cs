// See https://aka.ms/new-console-template for more information

#define PARALEL

using Bilets;
using System.Diagnostics;
using System.Text.RegularExpressions;

string latex_cmd = "pdflatex";
string questions_file = "questions.txt";
string output_dir = "";
string output_file = "card";
bool randomize = false;
bool randomize_in_cars = false;
string latex_args = "";
string pattern = "patern.txt";
int clear_level = 2;
string seed = "";
int questionCount = 0;
bool showTex = false;
string namesFile = "";
string vars = "";



#region ParamsCheck


if (args.Length!=0)
{
    for (int i = 0; i<args.Length; ++i)
    {
        switch (args[i])
        {
            case "-?":
            case "-h":
            case "--help": 
                Console.WriteLine("BILETS [OPTIONS]");
                Console.WriteLine("");
                Console.WriteLine("OPTIONS:");
                Console.WriteLine("  -l\n" +
                                  "  --latex         - команда вызова компилятора latex)");
                Console.WriteLine("                  (def: pdflatex)");
                Console.WriteLine("  -a \n" +
                                  "  --arguments     - аргументы компилятора latex");
                Console.WriteLine("                  (def: \"\"");
                Console.WriteLine("  -q\n" +
                                  "  --questions     - файл с вопросами");
                Console.WriteLine("                  (def: questions.txt)");
                Console.WriteLine("  -d\n" +
                                 " --directory       - путь вывода результата, \\ на конце!");
                Console.WriteLine("                  (def: ./");
                Console.WriteLine("  -r \n" +
                                  "  --random        - перемешать билеты");
                Console.WriteLine("  -R \n" +
                                  "  --Random        - перемешать вопросы внутри билетов");
                Console.WriteLine("  -p\n" +
                                  "  --pattern       - фалй c Tex-шаблоном для билетов");
                Console.WriteLine("                  (def: patern.txt");

                Console.WriteLine("  -n\n" +
                                  "  --names        - файл c именами вариантов");
                Console.WriteLine("  -v \n" +
                                  "  --vars         - набор параметров для подстановки в документ"+
                                  "                  ПАРАМЕТР1=ЗНАЧЕНИЕ;ПАРАМЕТР2=ЗНАЧЕНИЕ... ");
                

                Console.WriteLine("  -с \n" +
                                  "  --clear-level   - уровень отчистки");
                Console.WriteLine("  -s \n" +
                                  "  --show-Tex      - отобразить окно Latex");
                Console.WriteLine("                  1 - удаление .aux .log .out .dvi");
                Console.WriteLine("                  2 -  1 + удаление .tex");
                Console.WriteLine("                  (def: 2");




                return;
            case "-l":
            case "--latex":
                latex_cmd = args[++i];
                continue;
            case "-q":
            case "--questions":
                questions_file = args[++i];
                continue;
            case "-d":
            case "--directory":
                output_dir = args[++i];
                continue;
            case "-f":
            case "--filename":
                output_file = args[++i];
                continue;
            case "-r":
            case "--random":
                randomize = true;
                continue;
            case "-R":
            case "-Random":
                randomize_in_cars = true;
                continue;
            case "-a":
            case "--arguments":
                latex_args = args[++i];
                continue;
            case "-p":
            case "--pattern":
                pattern = args[++i];
                continue;
            case "-c":
            case "--clear-level":
                clear_level = Int32.Parse(args[++i]);
                continue;
            case "-s":
            case "--show_tex":
                showTex = true;
                continue;
            case "-u":
                questionCount = Int32.Parse(args[++i]);
                continue;
            case "-n":
            case "--names":
                namesFile = args[++i];
                continue;
            case "-v":
            case "--vars":
                vars = args[++i];
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
    Console.WriteLine($"Questions file {questions_file} not exists!");
    return;
}

if (namesFile!="" && !File.Exists(namesFile))
{
    Console.WriteLine($"File with names  {namesFile} not exists!");
    return;
}


#endregion

string[] names = new string[0];

if (namesFile != "")
  names = File.ReadAllLines(namesFile);


Dictionary<string, string> vars_dictionary = new Dictionary<string, string>();
if (vars!="")
{
    var t = vars.Split(';',StringSplitOptions.RemoveEmptyEntries);
    foreach(var x in t)
    {
        var y = x.Split("=", StringSplitOptions.RemoveEmptyEntries);
        vars_dictionary[y[0].Trim()] = y[1].Trim();
    }
};

var questions_strings = File.ReadAllLines(questions_file).ToList<string>();
var questions = new Bilets.Group();
  
int _curent_list = -1;
Subgroup cur_sub = null;
Bilets.Group cur_gr = null;
var groups = new List<Bilets.Group>();
var all_questions = new List<Question>();

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
        all_questions.Add(q);
        
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

    all_bilets.Add(billet_state.Select(x => x).ToArray());
    
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

var exams_copy = exams.Select(x => x).ToList(); //копия, чтобы непохерить сгенереные все билеты

making_variants:

exams = exams_copy.Select(x => x).ToList();

if (questionCount > 0)
{
    Dictionary<Question, int> _counters = new Dictionary<Question, int>(); 
          
    var _exams = new List<IList<Question>>();  //итоговый набор билетов

    for (int i=0; i<questionCount; ++i)
    {
        var __exams = exams.Select(x=>x).ToList();  //временный наблор билетов, из которого мы добавляем новые
        
        //удаляем уже добавленные билеты из общего набора вариантов
        foreach (var e in _exams)
        {
            exams.Remove(e);
        }

        //считаем кол-во вопросов в билете
        foreach (var q in all_questions)
        {
            _counters[q] = 0;            
        }    

        foreach (var e in exams)
        {
            foreach (var q in e)
            {
                _counters[q]++;
            }
        }

        while (__exams.Count!=0)
        {
            var ___eee = __exams.Select(x=>x).ToList();
            R.Shuffle(___eee);
            ___eee.Sort((a, b) => a.Sum(x => _counters[x]).CompareTo(b.Sum(x => _counters[x])));
            var rare = ___eee.First();
            _exams.Add(rare);

            foreach(var q in rare)
            {
                __exams.RemoveAll(x => x.Contains(q));
            }
        }
    }

    again:
    //докидка в набор билетов вопросв, кол-во которых не достаточно

    //считаем кол-во вопросов в новом наборе билетов
    foreach (var q in all_questions)
    {
        _counters[q] = 0;
    }

    foreach (var e in _exams)
    {
        foreach (var q in e)
        {
            _counters[q]++;
        }
    }

    
    foreach(var k in _counters.Keys)
    {
        if (_counters[k] < questionCount)
        {
            //берем набор билетов с нужным вопросом
            var _ex = exams.Where(x => x.Contains(k)).ToList();
            if (_ex.Count == 0)
            {
                Console.WriteLine("Генерация билтов с указаннаыми параметрами невозможна.");
                return;
            }
            //ищем билет с вопросами, которые реже всего встречаются

            R.Shuffle(_ex);
            _ex.Sort((a, b) => a.Sum(x => _counters[x]).CompareTo(b.Sum(x => _counters[x])));
            var rare = _ex.First();
            exams.Remove(rare);
            _exams.Add(rare);
            goto again;
        }
    }

    exams = _exams;
}


if (names.Count()>0)
{
    if (names.Count() > exams.Count)
    {
        questionCount++;
        Console.WriteLine("Увеличиваю кол-во повторений вопроса на 1 и повторяю генерацию...");
        goto making_variants;
    }
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


//foreach (var exam in exams)
{
    //Console.WriteLine(exam.Select(x => x.Text).Aggregate((a, b) => a + " " + b));
}



int counter = 0;

var template_file = File.ReadAllText(pattern);



Console.CursorVisible = false;

Dictionary<Question, int> counters = new Dictionary<Question, int>();

foreach (var q in all_questions)
{
    counters[q]=0;
}

foreach (var e in exams)
{
    foreach(var q in e)
    {
        counters[q]++;
    }
}

var ql = counters.Select(x => x).ToList();
ql.Sort((a,b)=>a.Value.CompareTo(b.Value));


Console.WriteLine();
Console.WriteLine();

foreach (var p in ql)
{
    Console.WriteLine($"{1.0*p.Value } - {p.Key.Text}");
}



//генерим tex

FileInfo pattern_file_info = new FileInfo(pattern);
DirectoryInfo output_dir_info;
if (output_dir != "")
{
   if (output_dir.Last()!='\\' && output_dir.Last() != '/')
    {
        output_dir += "\\";
    }
    output_dir_info = new DirectoryInfo(output_dir);
}    
else
{
    output_dir_info = new DirectoryInfo(".\\");
}
    

if (output_dir_info.Exists == false)
{
    output_dir_info.Create();
}
List<string> outputfiles = new List<string>();

int examsCount = exams.Count;
if (names.Count() > 0)
        examsCount = names.Count();

for (int i = 0; i < examsCount; ++i)
{
    var out_file_name = $"{output_dir_info.FullName}{output_file}{i}";

    var exam = exams[i];
    var _template = template_file;


    _template = _template.Replace("%number%", (i + 1).ToString());

    string name = "";

    if (names.Count() > 0)
    {
        _template = _template.Replace("%name%", names[i]);
        out_file_name = $"{output_dir_info.FullName}{names[i]}";
    }
        

    for (int j = 0; j < exam.Count; ++j)
    {
        _template = _template.Replace($"%q{j + 1}%", exam[j].Text);
    }

    foreach (var v in vars_dictionary)
    {
        _template = _template.Replace($"%{v.Key}%", v.Value);
    }

    File.WriteAllText(out_file_name + ".tex", _template);
    outputfiles.Add(out_file_name + ".tex");
}

int progr = 0;

int max = 0;
if (names.Count() == 0)
    max = exams.Count;
else
    max = names.Count();
//max = 5;
object locker = new object();



#if PARALEL
Parallel.For(0, max, i =>
#else
for (int i = 0; i<max; ++i)
#endif
{
    string name = "";
    if (names.Count() > 0)
        name = names[i];

    //var out_file_name = $"{output_file}{i}";

    Process p = new Process();
    p.StartInfo = new ProcessStartInfo(latex_cmd);
    p.StartInfo.Arguments = $"{latex_args}";
    if (output_dir != "")
        p.StartInfo.Arguments += $" \"-output-directory={output_dir_info.Parent}\\{output_dir_info.Name}\" ";
    
    p.StartInfo.Arguments += $" \"{outputfiles[i]}\"";
    //p.StartInfo.WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory;
    p.StartInfo.WorkingDirectory = pattern_file_info.DirectoryName;
    p.StartInfo.CreateNoWindow = !showTex;
    p.StartInfo.UseShellExecute = showTex;
    p.Start();

    //Console.WriteLine(p.StartInfo.Arguments);
   
    p.WaitForExit();

    
    var __nn = Path.GetFileNameWithoutExtension(outputfiles[i]);
    if (clear_level>=1)
    {
        File.Delete($"{output_dir_info.FullName}{__nn}.aux");
        File.Delete($"{output_dir_info.FullName}{__nn}.log");
        File.Delete($"{output_dir_info.FullName}{__nn}.dvi");
        File.Delete($"{output_dir_info.FullName}{__nn}.out");

    }        
    if (clear_level>=2)
    {
        File.Delete($"{output_dir_info.FullName}{__nn}.tex");
    }
    
    //Console.WriteLine($"Rezult\\var{i + 1}.tex");
    lock (locker)
    {
        counter++;
                
        Console.SetCursorPosition(0, Console.CursorTop);
        Console.Write(new string(' ', Console.WindowWidth-1));
        Console.SetCursorPosition(0, Console.CursorTop);
        if (File.Exists($"{output_dir}{__nn}.pdf")) 
            Console.WriteLine($"{output_dir_info.FullName}{__nn}.pdf - OK!");
        else
            Console.WriteLine($"{output_dir_info.FullName}{__nn}.pdf - ERR!");

        string s = $"{counter}/{max}";
        int w = Console.WindowWidth  - s.Length -2 -1;
        int left = (int)Math.Round(1.0 * counter / max  * w, 0);
        int right = w-left;
        string line = $"{s}[{new string('\u2588',left)}{new string('-',right)}]";
        Console.SetCursorPosition(0, Console.CursorTop);
        Console.Write(line);
        
    }

#if PARALEL
});
#else
}
#endif

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