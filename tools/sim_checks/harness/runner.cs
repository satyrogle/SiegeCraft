using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

public static class Runner
{
    static int pass, fail, skip, inconclusive;

    public static int Main(string[] args)
    {
        Assembly asm = Assembly.LoadFrom(args[0]);
        string filter = args.Length > 1 ? args[1] : "all";
        bool runExplicit = args.Length > 2 && args[2] == "explicit";
        var all = Stopwatch.StartNew();
        foreach (Type type in asm.GetTypes().OrderBy(t => t.FullName))
            foreach (MethodInfo m in type.GetMethods().OrderBy(x => x.Name))
            {
                object[] attrs = m.GetCustomAttributes(true);
                bool isTest = attrs.Any(a => a.GetType().Name == "TestAttribute");
                var cases = attrs.Where(a => a.GetType().Name == "TestCaseAttribute").ToList();
                var sources = attrs.Where(a => a.GetType().Name == "TestCaseSourceAttribute").ToList();
                if (!isTest && cases.Count == 0 && sources.Count == 0) continue;
                string name = type.Name + "." + m.Name;
                if (filter != "all" && !filter.Split(',').Any(f => name.Contains(f))) continue;
                if (attrs.Any(a => a.GetType().Name == "ExplicitAttribute") && !runExplicit) { skip++; continue; }
                if (attrs.Any(a => a.GetType().Name == "IgnoreAttribute")) { skip++; Console.WriteLine("IGNORED " + name); continue; }
                var argSets = new List<object[]>();
                if (isTest && cases.Count == 0 && sources.Count == 0) argSets.Add(null);
                foreach (var c in cases)
                    argSets.Add((object[])c.GetType().GetProperty("Arguments").GetValue(c));
                foreach (var s in sources)
                {
                    string src = (string)s.GetType().GetProperty("SourceName").GetValue(s);
                    Type srcType = (Type)s.GetType().GetProperty("SourceType")?.GetValue(s) ?? type;
                    const BindingFlags F = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
                    object data = srcType.GetField(src, F)?.GetValue(null)
                        ?? srcType.GetProperty(src, F)?.GetValue(null)
                        ?? srcType.GetMethod(src, F)?.Invoke(null, null);
                    foreach (object item in (IEnumerable)data)
                        argSets.Add(item is object[] arr ? arr : new[] { item });
                }
                foreach (object[] a in argSets)
                    RunOne(type, m, a, a == null ? name : name + "(" + string.Join(",", a.Select(x => x?.ToString() ?? "null")) + ")");
            }
        Console.WriteLine("TOTAL pass " + pass + " fail " + fail + " skipped " + skip + " inconclusive " + inconclusive + " in " + all.Elapsed.TotalSeconds.ToString("0") + " s");
        return fail == 0 ? 0 : 1;
    }

    static void RunOne(Type type, MethodInfo m, object[] args, string name)
    {
        var sw = Stopwatch.StartNew();
        try
        {
            m.Invoke(Activator.CreateInstance(type), args);
            pass++;
            Console.WriteLine("PASS " + name + " " + sw.ElapsedMilliseconds + "ms");
        }
        catch (TargetInvocationException e)
        {
            Exception ie = e.InnerException;
            string n = ie.GetType().Name;
            if (n == "SuccessException") { pass++; Console.WriteLine("PASS " + name); }
            else if (n == "InconclusiveException") { inconclusive++; Console.WriteLine("INCONCLUSIVE " + name + ": " + ie.Message.Trim()); }
            else if (n == "IgnoreException") { skip++; Console.WriteLine("IGNORED " + name); }
            else { fail++; Console.WriteLine("FAIL " + name + ": " + n + ": " + ie.Message.Trim() + "\n" + ie.StackTrace); }
        }
    }
}
