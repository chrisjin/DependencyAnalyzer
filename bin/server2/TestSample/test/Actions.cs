/*
 * Manual Page:
 * should be associated with a rule 
 * OnAddAndEnterScope //Add&enter a scope
 * OnAddScope         //add a scope
 * OnAddScopes        //add a list of scope
 * OnLeanvingScope    //leaves a scope
 * Usage: arule.AddAction(new Action)
 * 
 * 
 * Maintenance History:
 * 
 * Ver 1.0  Oct. 4     2014 
 * created by Shikai Jin 
 * 
 * Ver 2.0  Oct. 5
 * add OnAddAndEnterScope, OnAddScope, OnLeavingScope
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeAnalyzer
{
    /*
     * All actions will be executed, when a semi passes the rule related.
     * 
     * 
     * add a list of scope to the current scope
     * 
     */
    class OnAddAndEnterScope : BaseAction
    {
        public override void Do(Object args)
        {
            Scope sc = (Scope)args;
            //System.Console.WriteLine(sc.Type.ToString() + " " + sc.Name + " " + sc.BeginLine.ToString());

            Scope added = TypeRepository.Instance.AddScope(sc);
            TypeRepository.Instance.EnterScope(added);
        }
    }
    /*
     * add a list of scope to the current scope 
     */
    class OnAddScope : BaseAction
    {
        public override void Do(Object args)
        {
            Scope s = (Scope)args;
            TypeRepository.Instance.AddScope(s);
            //System.Console.WriteLine(TypeRepository.Instance.CurrentScope.Name+" Inher " + s.Name);
        }
    }
    /*
     * add a list of scope to the current scope
     * 
     */
    class OnAddScopes : BaseAction
    {
        public override void Do(Object args)
        {
            List<Scope> s = (List<Scope>)args;
            for (int i = 0; i < s.Count; i++)
                TypeRepository.Instance.AddScope(s[i]);
            //System.Console.WriteLine(TypeRepository.Instance.CurrentScope.Name+" Inher " + s.Name);
        }
    }

    /*
     * when leaving a scope calculate the complexities of functions inside this scope
     *
     * 
     */

    class OnLeanvingScope : BaseAction
    {
        void EvalComplexityAndSize()
        {
            TypeRepository tr = TypeRepository.Instance;
            if (tr.CurrentScope.Type == SType.FUNCTION)
            {
                List<Scope> childs = Scope.FindChildWithType(tr.CurrentScope, c => true);
                tr.CurrentScope.Property[PROPERTYKEY.COMPLEXITY] = childs.Count.ToString();
                tr.CurrentScope.Property[PROPERTYKEY.SIZE] = (tr.CurrentScope.EndLine - tr.CurrentScope.BeginLine).ToString();
            }
        }
        void IdentifyAggregationOrComposition()
        {
            TypeRepository tr = TypeRepository.Instance;
            if (tr.CurrentScope.Type == SType.CLASS || tr.CurrentScope.Type == SType.STRUCT)
            {
                List<string> namelist = IsNewStatement.VariableName;
                List<string> AggsToAdd = new List<string>();
                List<string> ComsToAdd = new List<string>();
                foreach (Scope child in tr.CurrentScope.Child)
                {
                    //string[] members = null;
                    if (child.Type == SType.MEMBER)
                    {

                        if (namelist.Contains(child.Name))
                        {
                            AggsToAdd.Add(child.Property[PROPERTYKEY.TYPE]);
                        }
                        else
                        {
                            ComsToAdd.Add(child.Property[PROPERTYKEY.TYPE]);
                        }
                    }
                }
                foreach (string agg in AggsToAdd)
                {
                    tr.CurrentScope.AddChild(Scope.Make(SType.STATEMENT_AGGREGATION,
                        agg, 0, 0));
                }
                foreach (string coms in ComsToAdd)
                {
                    tr.CurrentScope.AddChild(Scope.Make(SType.STATEMENT_COMPOSTION,
                    coms, 0, 0));
                }

                namelist.Clear();
            }
        }
        public override void Do(Object args)
        {

            IdentifyAggregationOrComposition();
            EvalComplexityAndSize();
            TypeRepository.Instance.LeaveScope();
        }
    }

    class TestAction
    {

#if(TEST_ACTION)
        static void Main(string[] args)
        {
            OnAddAndEnterScope addenter = new OnAddAndEnterScope();
            OnAddScope addscope = new OnAddScope();
            OnLeanvingScope leavingscope = new OnLeanvingScope();
            addenter.Do(Scope.Make(SType.NAMESPACE, "anana", 0, 0));
            addenter.Do(Scope.Make(SType.CLASS, "Aname", 0, 0));
            leavingscope.Do(null);
            addenter.Do(Scope.Make(SType.CLASS, "Bname", 0, 0));
                addscope.Do(Scope.Make(SType.FUNCTION, "Func1", 0, 0));
                addscope.Do(Scope.Make(SType.FUNCTION, "Func2", 0, 0));
            leavingscope.Do(null);
            OutputManager.DisplayTypes(TypeRepository.Instance.RootScope);
            System.Console.ReadLine();
        }
#endif
    }
}
