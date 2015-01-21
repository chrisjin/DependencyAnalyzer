/*
 * Manual Page:
 * 
 * IsStatement
 * IsBreak
 * IsLoopBreak
 * IsDeleate
 * IsMemberDeclaration
 * IsNewStatement
 * IsEnterScope
 * IsLeavingScope
 * IsNamespace
 * IsClass
 * IsInheritance
 * IsFunction
 * IsProperty
 * IsPropertyMember
 * IsOtherScope
 * IsUsingFunction
 * 
 * Maintenance History:
 * Ver 1.0  Oct. 4  2014 created by Shikai Jin 
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeAnalyzer
{
    class DEBUG
    {
        public static int i = 0;
    }
    //detect statement
    public class IsStatement : BaseRule
    {
        public override bool Test(SemiExpression semi)
        {
            int colonindex = semi.FindToken(";");

            if (colonindex != -1)
            {
                goto IsStatement_GOOD;
            }
            return false;
        IsStatement_GOOD:
            return true;
        }
    }
    //detect break
    public class IsBreak : BaseRule
    {
        public override bool Test(SemiExpression semi)
        {
            if (semi.Count > 0)
            {
                if (!semi.Contains("break"))
                    return false;
            }
            return true;
        }
    }
    //detect break in a loop
    public class IsLoopBreak:BaseRule
    {
        public override bool Test(SemiExpression semi)
        {
            Scope cs = TypeRepository.Instance.CurrentScope;
            if (cs.Type != SType.SWTICH)
            {
                //a loop can be placed inside a switch and a switch can also be in a loop
                //To rule out the second case, which is not a loopbreak,
                //we get the nearest switch and loop to compare which one is nearer.
                Predicate<SType> switchfunc=c=>c==SType.SWTICH;
                Predicate<SType> loopfunc = c => (c == SType.FOR || c == SType.FOREACH || c == SType.WHILE);
                Scope nearestswitch=Scope.FindParentWithType(cs,switchfunc);

                if (nearestswitch != null)
                {
                    Scope nearestloop = Scope.FindParentWithType(cs, loopfunc);
                    Scope loopoutsideswitch = Scope.FindParentWithType(nearestswitch, loopfunc);
                    if (loopoutsideswitch != null)
                    {
                        if (nearestloop == loopoutsideswitch)
                            return false;
                    }

                }
                //this argument will be passed to the action 
                //the action will add this scope to the repo tree.
                this.Arg = Scope.MakeKerword("break", semi.LineCount, semi.LineCount);
                return true;
            }
            return false;
        }
    }
    //detect delegate
    public class IsDelegate : BaseRule
    {
        public override bool Test(SemiExpression semi)
        {
            int indexdele = semi.FindToken("delegate");
            if (indexdele>=0)
            {
                int indexpar = semi.FindToken("(");
                if (indexpar == indexdele + 3)
                {
                    this.Arg = Scope.Make(SType.DELEGATE, semi[indexpar-1],semi.LineCount,semi.LineCount);
                    return true;
                }
            }
            return false;
        }
    }
    //detect a member variable
    public class IsMemberDeclaration: BaseRule
    {
        public override bool Test(SemiExpression semi)
        {
            TypeRepository tr = TypeRepository.Instance;
            if (tr.CurrentScope.Type == SType.CLASS || tr.CurrentScope.Type == SType.STRUCT)
            {
                DeclarationAnalyzer dta=new DeclarationAnalyzer();
                dta.Initilize(semi.Tokens);
                dta.Run();
                List<Scope> ls = new List<Scope>();
                for (int i = 0; i < dta.Names.Count; i++)
                {
                    Scope s = Scope.MakeMember(dta.Accessbility, dta.Type, dta.Names[i],
                        semi.LineCount, semi.LineCount, dta.IsStatic);
                    
                    ls.Add(s);
                    
                }
                this.Arg = ls;
                return true;
            }
            return false;
        }
    }
    //detect new statement
    public class IsNewStatement : BaseRule
    {
        static public List<string> VariableName=new List<string>();
        //Get the variable name for the convenience of detecting aggregation relationship
        List<string> GetVariableName(SemiExpression semi)
        {
            List<string> ret=new List<string>();
            SemiExpression nsemi=new SemiExpression();
            for (int i = 0; i < semi.Count; i++)
            {
                string tmp = semi[i].Replace("\n", "");
                tmp = tmp.Replace(" ", "");
                if (tmp.Length > 0)
                {
                    nsemi.Add(tmp);
                }
            }
            for (int i = 0; i < nsemi.Count; i++)
            {
                if (i + 2 < nsemi.Count)
                {
                    if (nsemi[i + 2] == "new")
                    {
                        ret.Add(nsemi[i ]);
                    }
                }
            }
            return ret;
        }
        public override bool Test(SemiExpression semi)
        {
            int indexnew = semi.FindToken("new");
            if (indexnew != -1)
            {
                //this.Arg = Scope.Make(SType.STATEMENT_AGGREGATION, semi[indexnew + 1], semi.LineCount, semi.LineCount);
                TypeRepository tr = TypeRepository.Instance;
                Scope classlocated = Scope.FindParentClass(tr.CurrentScope);
                //Decide which class the semi belongs to.
                if (classlocated != null)
                {
                    List<string> names=GetVariableName(semi);
                    VariableName.AddRange(names);
                }
                goto IsNewStatement_GOOD; 
            }
            return false;
        IsNewStatement_GOOD:
            return true;
        }
    }
    //detect entering scope
    public class IsEnterScope: BaseRule
    {
        public override bool Test(SemiExpression semi)
        {
            int index = semi.FindToken("{");
            //System.Console.ReadLine();
            if (index != -1)
            {
                goto IsEnterScope_GOOD; 
            }
            else
            {
                int forindex = semi.FindToken("for");
                int ifindex = semi.FindToken("if");
                if (forindex != -1||ifindex!=-1)
                {
                    goto IsEnterScope_GOOD;
                }
            }

            return false;

        IsEnterScope_GOOD:
            //DEBUG.i++;
            //System.Console.WriteLine(DEBUG.i);
            return true;
        }
    };
    //detect leaving scope
    public class IsLeavingScope: BaseRule
    {

        public override bool Test(SemiExpression semi)
        {
            int indexend = semi.FindToken("}");
            int indexbegin   = semi.FindToken("{");
            if (indexend != -1)
            {
                goto IsLeavingScope_GOOD;
            }
            else if (indexbegin==-1)
            {
                int forindex = semi.FindToken("for");
                int ifindex = semi.FindToken("if");
                if (forindex != -1 || ifindex != -1)
                {
                    goto IsLeavingScope_GOOD;
                    //semi.Print();
                }
            }
            return false;
        IsLeavingScope_GOOD:
            TypeRepository.Instance.CurrentScope.EndLine = semi.LineCount;
            //DEBUG.i--;
            //System.Console.WriteLine(DEBUG.i);
            return true;
        }
    };
    //detect namespaces
    public class IsNamespace : BaseRule
    {
        public override bool Test(SemiExpression semi)
        {
            int index = semi.FindToken("namespace");
            if (index != -1)
            {
                if (!semi.IsInRange(index + 1)) { return false; }
                this.Arg = this.Arg = Scope.Make(SType.NAMESPACE, semi[index + 1], semi.LineCount, 0); 
                return true;
            }
            return false;
        }
    }
    //detect class
    public class IsClass : BaseRule
    {
        public override bool Test(SemiExpression semi)
        {
            //semi.EraseNewLine();
            int indexCL = semi.FindToken("class");
            int indexIF = semi.FindToken("interface");
            int indexST = semi.FindToken("struct");
            int indexEN = semi.FindToken("enum");
            int index = Math.Max(indexCL, indexIF);
            index = Math.Max(index, indexST);
            index = Math.Max(index, indexEN);
            if (index != -1)
            {
                Scope scope = new Scope();
                if (indexCL != -1)
                    scope.Type = SType.CLASS;
                else if (indexIF != -1)
                    scope.Type = SType.INTERFACE;
                else if (indexST != -1)
                    scope.Type = SType.STRUCT;
                else if (indexEN != -1)
                    scope.Type = SType.ENUM;
                if (!semi.IsInRange(index + 1)) { return false; }
                scope.Name = semi[index + 1];
                scope.BeginLine = semi.LineCount;
                this.Arg = scope;
                return true;
            }
            return false;
        }
    }
    //detect inheritance relationship
    public class IsInheritance : BaseRule
    {
        public override bool Test(SemiExpression semi)
        {
            int indexcol = semi.FindToken(":");
            string parentname="Object";
            if (indexcol != -1)
            {
                if (!semi.IsInRange(indexcol + 1)) { return false; }
                parentname = semi[indexcol + 1];
            }
            this.Arg = Scope.Make(SType.STATEMENT_INHERITANCE, parentname, semi.LineCount, semi.LineCount);
            return true;
        }
    }
    //detect function
    public class IsFunction : BaseRule
    {
        public static bool isSpecialToken(string token)
        {
            string[] SpecialToken = { "if", "for", "foreach", "while", "catch", "using","switch" };
            foreach (string stoken in SpecialToken)
                if (stoken == token)
                    return true;
            return false;
        }
        public override bool Test(SemiExpression semi)
        {
            if (semi[semi.Count - 1] != "{")
                return false;

            int index = semi.FindFirstToken("(");
            if (index > 0 && !isSpecialToken(semi[index - 1]))
            {
                DeclarationAnalyzer da = new DeclarationAnalyzer();
                da.Initilize(semi.Tokens);
                if (false == da.Run())
                    return false;
                this.Arg = Scope.MakeFunction(da.Accessbility, da.Type, 
                    FunctionArgExtractor.GetArgTypeTable(semi.Tokens), da.Names[0], 
                    semi.LineCount,0,da.IsStatic);
                goto IsFunction_GOOD;
            }
            return false;
        IsFunction_GOOD:
            //semi.Print();
            return true;
        }
    }
    //detect getters and setters 
    public class IsProperty : BaseRule
    {
        //public static bool isSpecialToken(string token)
        //{
            
        //    foreach (string stoken in SpecialToken)
        //        if (stoken == token)
        //            return true;
        //    return false;
        //}
        public override bool Test(SemiExpression semi)
        {
            if (!TypeRepository.Instance.CurrentScope.IsGeneralClass())
                return false;
            semi.EraseNewLine();
            if (semi[semi.Count - 1] != "{")
                return false;
            string[] SpecialToken = { "if", "for", "foreach", "while", "catch", "using", "switch", "class", "foreach " };
            foreach (string token in SpecialToken)
            {
                if (semi.Contains(token))
                {
                    return false;
                }
                int indexbegin = semi.FindFirstToken("{");
                if (indexbegin > 0)
                {
                    if (semi[indexbegin - 1] == ")")
                        return false;
                }
                
            }
            DeclarationAnalyzer da = new DeclarationAnalyzer();
            da.Initilize(semi.Tokens);
            if (false == da.Run())
                return false;
            if (da.Names.Count == 0)
                return false;
            this.Arg = Scope.MakeProperty(da.Accessbility, da.Type, da.Names[0],
                        semi.LineCount, semi.LineCount, da.IsStatic);
            return true;
        }
    }
    //detect property like public int A {set;get;}
    //this kind of property is also a member vairable of a class
    public class IsPropertyMember : BaseRule
    {
        public override bool Test(SemiExpression semi)
        {
            semi.EraseNewLine();
            int indexset=semi.FindToken("set");
            if (indexset == -1)
            {
                return false;
            }
            else
            {
                if (semi.Count != 2)
                    return false;
                if (semi[1] != ";")
                    return false;
            }
            TypeRepository tr = TypeRepository.Instance;
            Scope foundproperty = Scope.FindParentWithType(tr.CurrentScope, t => t == SType.PROPERTY);
            if(foundproperty==null)
                return false;
            else
            {
                Scope newsc = foundproperty.Clone();
                newsc.Type = SType.MEMBER;
                if (foundproperty.Parent == null)
                {
                    return false;
                }
                else
                {
                    foundproperty.Parent.AddChild(newsc);
                }
                return true;
            }
        }
    }
    // for, if, switch etc.
    public class IsOtherScope : BaseRule
    {
        public override bool Test(SemiExpression semi)
        {
            string keyword = Scope.HasWhichKeyword(semi.Tokens);
            Scope s = Scope.MakeKerword(keyword, semi.LineCount, 0);
            this.Arg = s;
            return true;
        }
    }
    //detect functions that have arguments
    public class IsUsingFunction : BaseRule
    {
        public override bool Test(SemiExpression semi)
        {
            int firstp = semi.Tokens.FindIndex(0, v => v == "(");
            int secondp = semi.Tokens.FindIndex(firstp, v => v == ")");
            if (secondp == firstp + 1)
                return false;

            string[] types = TypeRepository.Instance.CurrentScope.Property[PROPERTYKEY.ARGSTABLE].Split(',');
            Scope pclass = Scope.FindParentClass(TypeRepository.Instance.CurrentScope);

            if (pclass != null && types.Length > 0)
            {
                foreach (string type in types)
                {
                    pclass.AddChild(Scope.Make(SType.STATEMENT_USING, type, semi.LineCount, semi.LineCount));
                }
                return true;
            }

            return false;
        }
    }
#if(TEST_RULES)
    class TestR
    {
        static void TestOnlyDetectFunction()
        {
            RuleTester rt = new RuleTester();
            IsFunction isf = new IsFunction();
            isf.AddAction(new OnAddScope());
            rt.AddRule(isf);


            SemiExtractor se = new SemiExtractor();
            se.Open("../../Semi.cs");
            rt.Test(se);

            OutputManager.DisplayTypes(TypeRepository.Instance.CurrentScope);  
            TypeRepository.Instance.Reset();
        }
        static void TestOnlyDetectClass()
        {
            RuleTester rt = new RuleTester();
            IsClass isf = new IsClass();
            isf.AddAction(new OnAddScope());
            rt.AddRule(isf);


            SemiExtractor se = new SemiExtractor();
            se.Open("../../Semi.cs");
            rt.Test(se);

            OutputManager.DisplayTypes(TypeRepository.Instance.CurrentScope);
            TypeRepository.Instance.Reset();       
        }
        static void Main(string[] args)
        {
            TestOnlyDetectClass();
            System.Console.WriteLine("");
            System.Console.WriteLine("");
            TestOnlyDetectFunction();
            System.Console.ReadLine();
        }
    }
#endif
}
